#region ========================================================================= USING =====================================================================================
using Lumina.Infrastructure.Common.Models.DTO.Plugins;
using Lumina.Infrastructure.Core.Plugins;
using Lumina.Plugins.Contracts.Core.Plugins;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
#endregion

namespace Lumina.Infrastructure.UnitTests.Core.Plugins;

/// <summary>
/// Contains unit tests for the <see cref="PluginLoader"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class PluginLoaderTests
{
    [Fact]
    public void LoadPlugins_WhenDirectoryDoesNotExist_ShouldReturnNoPluginsAndNoErrors()
    {
        // Arrange
        string nonExistentDirectory = Path.Combine(Path.GetTempPath(), $"lumina-plugins-{Guid.NewGuid()}");
        IServiceCollection services = new ServiceCollection();

        // Act
        PluginLoadResultDto result = PluginLoader.LoadPlugins(nonExistentDirectory, services);

        // Assert
        Assert.Empty(result.Plugins);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void LoadPlugins_WhenDirectoryContainsInvalidAssembly_ShouldRecordError()
    {
        // Arrange
        using TemporaryPluginDirectory temporaryPluginDirectory = new();
        temporaryPluginDirectory.CreateFile("invalid.dll", "this is not a valid assembly");
        IServiceCollection services = new ServiceCollection();

        // Act
        PluginLoadResultDto result = PluginLoader.LoadPlugins(temporaryPluginDirectory.Path, services);

        // Assert
        Assert.Empty(result.Plugins);
        PluginLoadErrorDto error = Assert.Single(result.Errors);
        Assert.Equal("invalid", error.AssemblyName);
        Assert.Contains("invalid.dll", error.ErrorMessage);
    }

    [Fact]
    public void LoadPlugins_WhenDirectoryContainsAssemblyWithoutPluginDescriptors_ShouldReturnNoPluginsAndNoErrors()
    {
        // Arrange
        using TemporaryPluginDirectory temporaryPluginDirectory = new();
        temporaryPluginDirectory.CopyDll(typeof(IPlugin).Assembly.Location);
        IServiceCollection services = new ServiceCollection();

        // Act
        PluginLoadResultDto result = PluginLoader.LoadPlugins(temporaryPluginDirectory.Path, services);

        // Assert
        Assert.Empty(result.Plugins);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void LoadPlugins_WhenDirectoryContainsValidPluginAssembly_ShouldLoadPluginAndRegisterItsServices()
    {
        // Arrange
        using TemporaryPluginDirectory temporaryPluginDirectory = new();
        temporaryPluginDirectory.CopyDll(GetTestPluginAssemblyPath());
        IServiceCollection services = new ServiceCollection();

        // Act
        PluginLoadResultDto result = PluginLoader.LoadPlugins(temporaryPluginDirectory.Path, services);

        // Assert
        IPlugin loadedPlugin = Assert.Single(result.Plugins);
        Assert.Empty(result.Errors);
        Assert.Equal(Guid.Parse("7B5A3E5D-9B7F-4A1E-8D9C-2F3A4B5C6D7E"), loadedPlugin.Id);
        Assert.Equal("Test Plugin", loadedPlugin.Name);
        Assert.Contains(services, descriptor => descriptor.ImplementationInstance as string == "plugin-services-registered");
    }

    /// <summary>
    /// Gets the path of the <c>Lumina.TestPlugin</c> assembly from the test output directory.
    /// The assembly is referenced by the test project but no type from it is used, so it is not loaded
    /// into the default load context, which is what makes the plugin loading pipeline testable.
    /// </summary>
    /// <returns>The path of the test plugin assembly.</returns>
    private static string GetTestPluginAssemblyPath()
    {
        return Path.Combine(AppContext.BaseDirectory, "Lumina.TestPlugin.dll");
    }

    /// <summary>
    /// Test helper managing a temporary plugins directory, deleted when the test finishes.
    /// </summary>
    private sealed class TemporaryPluginDirectory : IDisposable
    {
        private bool _disposed;

        public TemporaryPluginDirectory()
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"lumina-plugin-loader-{Guid.NewGuid()}");
            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public void CreateFile(string fileName, string content)
        {
            File.WriteAllText(System.IO.Path.Combine(Path, fileName), content);
        }

        public void CopyDll(string sourceDllPath)
        {
            File.Copy(sourceDllPath, System.IO.Path.Combine(Path, System.IO.Path.GetFileName(sourceDllPath)));
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                try
                {
                    Directory.Delete(Path, recursive: true);
                }
                catch (IOException)
                {
                    // best effort cleanup of the temporary plugins directory
                }
                catch (UnauthorizedAccessException)
                {
                    // the plugin assemblies are still locked by their load contexts, best effort cleanup only
                }
                _disposed = true;
            }
        }
    }
}
