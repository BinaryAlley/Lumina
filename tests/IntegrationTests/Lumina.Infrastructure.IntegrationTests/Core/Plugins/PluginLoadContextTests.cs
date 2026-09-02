#region ========================================================================= USING =====================================================================================
using Lumina.Infrastructure.Core.Plugins;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
#endregion

namespace Lumina.Infrastructure.IntegrationTests.Core.Plugins;

/// <summary>
/// Contains integration tests for the <see cref="PluginLoadContext"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class PluginLoadContextTests
{
    [Fact]
    public void LoadFromAssemblyName_WhenAssemblyIsResolvableFromDefaultContext_ShouldReturnTheSharedAssembly()
    {
        // Arrange
        using TemporaryPluginDirectory temporaryPluginDirectory = new();
        PluginLoadContext sut = new(temporaryPluginDirectory.Path, "test-context");

        // Act
        Assembly result = sut.LoadFromAssemblyName(typeof(object).Assembly.GetName());

        // Assert
        Assert.NotNull(result);
        Assert.Equal(typeof(object).Assembly.GetName().Name, result.GetName().Name);
    }

    [Fact]
    public void Load_WhenAssemblyExistsOnlyInPluginDirectory_ShouldLoadItFromThePluginDirectory()
    {
        // Arrange
        using TemporaryPluginDirectory temporaryPluginDirectory = new();
        string destinationPath = temporaryPluginDirectory.CopyDll(Path.Combine(AppContext.BaseDirectory, "Lumina.TestPlugin.dll"), "PluginLocalDependency.dll");
        PluginLoadContext sut = new(temporaryPluginDirectory.Path, "test-context");
        MethodInfo loadMethod = typeof(PluginLoadContext).GetMethod("Load", BindingFlags.Instance | BindingFlags.NonPublic)!;

        // Act
        Assembly? result = (Assembly?)loadMethod.Invoke(sut, [new AssemblyName("PluginLocalDependency")]);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Lumina.TestPlugin", result!.GetName().Name);
        Assert.Equal(destinationPath, result.Location);
    }

    [Fact]
    public void LoadFromAssemblyName_WhenAssemblyIsNotFoundAnywhere_ShouldThrowFileNotFoundException()
    {
        // Arrange
        using TemporaryPluginDirectory temporaryPluginDirectory = new();
        PluginLoadContext sut = new(temporaryPluginDirectory.Path, "test-context");

        // Act
        Action act = () => sut.LoadFromAssemblyName(new AssemblyName($"Definitely.Not.Resolvable.{Guid.NewGuid()}"));

        // Assert
        Assert.Throws<FileNotFoundException>(act);
    }

    /// <summary>
    /// Test helper managing a temporary plugins directory, deleted when the test finishes.
    /// </summary>
    private sealed class TemporaryPluginDirectory : IDisposable
    {
        private bool _disposed;

        public TemporaryPluginDirectory()
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"lumina-plugin-load-context-{Guid.NewGuid()}");
            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public string CopyDll(string sourceDllPath, string destinationFileName)
        {
            string destinationPath = System.IO.Path.Combine(Path, destinationFileName);
            File.Copy(sourceDllPath, destinationPath);
            return destinationPath;
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
                    // Best effort cleanup of the temporary plugins directory.
                }
                catch (UnauthorizedAccessException)
                {
                    // The plugin assemblies are still locked by their load contexts, best effort cleanup only.
                }
                _disposed = true;
            }
        }
    }
}
