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
    public void LoadPlugins_WhenDirectoryContainsNativeLibrary_ShouldSkipItSilently()
    {
        // Arrange
        using TemporaryPluginDirectory temporaryPluginDirectory = new();
        // A native library (like pdfium.dll) is a valid PE image starting with the DOS signature, but it is not a managed assembly.
        temporaryPluginDirectory.CreateFile("native.dll", "MZ\u0001\u0000\u0000\u0000garbage native library content");
        IServiceCollection services = new ServiceCollection();

        // Act
        PluginLoadResultDto result = PluginLoader.LoadPlugins(temporaryPluginDirectory.Path, services);

        // Assert
        Assert.Empty(result.Plugins);
        Assert.Empty(result.Errors);
        Assert.Empty(result.LoadContexts);
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

    [Fact]
    public void LoadPlugins_WhenDirectoryContainsValidPluginAssembly_ShouldRetainTheLoadContextOfThePlugin()
    {
        // Arrange
        using TemporaryPluginDirectory temporaryPluginDirectory = new();
        temporaryPluginDirectory.CopyDll(GetTestPluginAssemblyPath());
        IServiceCollection services = new ServiceCollection();

        // Act
        PluginLoadResultDto result = PluginLoader.LoadPlugins(temporaryPluginDirectory.Path, services);

        // Assert
        Assert.Single(result.Plugins);
        // A collectible load context is only kept alive by a direct reference to the context object itself, so the context of every loaded plugin is retained
        // for the lifetime of the host, keeping the plugin assemblies from being unloaded by the garbage collector.
        Assert.Single(result.LoadContexts);
    }

    [Fact]
    public void LoadPlugins_WhenDirectoryContainsManagedDependencyAssemblyAlongsideAPlugin_ShouldSkipTheDependencyAndLoadOnlyThePlugin()
    {
        // Arrange
        using TemporaryPluginDirectory temporaryPluginDirectory = new();
        temporaryPluginDirectory.CopyDll(GetTestPluginAssemblyPath());
        // A plugin ships its managed dependencies into the plugins directory (like PdfPig.dll and SkiaSharp.dll next to the PDF plugin). A managed dependency is a valid
        // assembly that does not reference the plugin contracts, so it is not a plugin and must be skipped without reflecting over its types, which would otherwise surface
        // spurious load errors for a file that is not a plugin.
        temporaryPluginDirectory.CopyDll(GetDependencyAssemblyPath());
        IServiceCollection services = new ServiceCollection();

        // Act
        PluginLoadResultDto result = PluginLoader.LoadPlugins(temporaryPluginDirectory.Path, services);

        // Assert
        IPlugin loadedPlugin = Assert.Single(result.Plugins);
        Assert.Equal(Guid.Parse("7B5A3E5D-9B7F-4A1E-8D9C-2F3A4B5C6D7E"), loadedPlugin.Id);
        Assert.Equal("Test Plugin", loadedPlugin.Name);
        Assert.Empty(result.Errors);
        Assert.Single(result.LoadContexts);
    }

    [Fact]
    public void LoadPlugins_WhenDirectoryContainsFileShorterThanDosSignature_ShouldRecordInvalidAssemblyError()
    {
        // Arrange
        using TemporaryPluginDirectory temporaryPluginDirectory = new();
        // A single-character file is not long enough to carry the two-byte DOS signature "MZ", so it is not a PE image.
        temporaryPluginDirectory.CreateFile("tiny.dll", "M");
        IServiceCollection services = new ServiceCollection();

        // Act
        PluginLoadResultDto result = PluginLoader.LoadPlugins(temporaryPluginDirectory.Path, services);

        // Assert
        Assert.Empty(result.Plugins);
        PluginLoadErrorDto error = Assert.Single(result.Errors);
        Assert.Equal("tiny", error.AssemblyName);
        Assert.Contains("tiny.dll", error.ErrorMessage);
    }

    [Fact]
    public void LoadPlugins_WhenDirectoryContainsFileWithWrongDosSignature_ShouldRecordInvalidAssemblyError()
    {
        // Arrange
        using TemporaryPluginDirectory temporaryPluginDirectory = new();
        // The file starts with the wrong DOS signature, so it is not a PE image at all.
        temporaryPluginDirectory.CreateFile("notpe.dll", "XY\u0000\u0000\u0000some content that is not a PE image");
        IServiceCollection services = new ServiceCollection();

        // Act
        PluginLoadResultDto result = PluginLoader.LoadPlugins(temporaryPluginDirectory.Path, services);

        // Assert
        Assert.Empty(result.Plugins);
        PluginLoadErrorDto error = Assert.Single(result.Errors);
        Assert.Equal("notpe", error.AssemblyName);
        Assert.Contains("notpe.dll", error.ErrorMessage);
    }

    [Fact]
    public void LoadPlugins_WhenDirectoryContainsLockedNativeLibrary_ShouldNotThrowAndRecordError()
    {
        // Arrange
        using TemporaryPluginDirectory temporaryPluginDirectory = new();
        string nativePath = temporaryPluginDirectory.CreateFileReturningPath("locked.dll", "MZ\u0000\u0000\u0000locked native library content");
        using FileStream lockStream = new(nativePath, FileMode.Open, FileAccess.Read, FileShare.None);
        IServiceCollection services = new ServiceCollection();

        // Act
        PluginLoadResultDto result = PluginLoader.LoadPlugins(temporaryPluginDirectory.Path, services);

        // Assert
        Assert.Empty(result.Plugins);
        // The file cannot be read to verify its PE signature, so it is reported as not loadable rather than crashing the loader.
        PluginLoadErrorDto error = Assert.Single(result.Errors);
        Assert.Equal("locked", error.AssemblyName);
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
    /// Gets the path of a managed dependency assembly from the test output directory, standing in for the managed dependencies
    /// a plugin ships next to itself (like PdfPig.dll and SkiaSharp.dll next to the PDF plugin). The <c>Microsoft.AspNetCore.SignalR.Core</c>
    /// assembly is used because it does not reference the plugin contracts assembly, and reflecting over its types inside a plugin load
    /// context throws, which reproduces the spurious load errors the loader used to record for dependency assemblies.
    /// </summary>
    /// <returns>The path of the managed dependency assembly.</returns>
    private static string GetDependencyAssemblyPath()
    {
        return Path.Combine(AppContext.BaseDirectory, "Microsoft.AspNetCore.SignalR.Core.dll");
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

        public string CreateFileReturningPath(string fileName, string content)
        {
            string filePath = System.IO.Path.Combine(Path, fileName);
            File.WriteAllText(filePath, content);
            return filePath;
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
