#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Models.DTO.Configuration;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Infrastructure.Core.Plugins;
using Lumina.Infrastructure.Fixtures.Common.Models.DTO.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Infrastructure.IntegrationTests.Core.Plugins;

/// <summary>
/// Contains integration tests for the <see cref="PluginInstaller"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class PluginInstallerTests : IDisposable
{
    private readonly string _pluginsDirectory;
    private readonly PluginInstaller _sut;
    private readonly PluginsSettingsDtoFixture _pluginsSettingsDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="PluginInstallerTests"/> class.
    /// </summary>
    public PluginInstallerTests()
    {
        _pluginsDirectory = Path.Combine(AppContext.BaseDirectory, $".test-plugins-{Guid.NewGuid():N}");
        ILogger<PluginInstaller> mockLogger = Substitute.For<ILogger<PluginInstaller>>();
        IOptions<PluginsSettingsDto> mockOptions = Substitute.For<IOptions<PluginsSettingsDto>>();
        mockOptions.Value.Returns(_pluginsSettingsDtoFixture.Create(directory: Path.GetFileName(_pluginsDirectory)));
        _sut = new PluginInstaller(mockOptions, mockLogger);
    }

    [Fact]
    public async Task InstallAsync_WhenUploadingSingleAssembly_ShouldCopyAssemblyToPluginsDirectory()
    {
        // Arrange
        string assemblyName = "Lumina.TestPlugin.dll";
        await using FileStream stream = File.OpenRead(Path.Combine(AppContext.BaseDirectory, assemblyName));

        // Act
        Result<Success> result = await _sut.InstallAsync(stream, assemblyName, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.True(File.Exists(Path.Combine(_pluginsDirectory, assemblyName)));
    }

    [Fact]
    public async Task InstallAsync_WhenUploadingZipWithAssemblies_ShouldExtractAssembliesFlattenedToPluginsDirectory()
    {
        // Arrange
        await using MemoryStream zipStream = new();
        CreateZipArchive(zipStream, [("nested/plugin/Lumina.TestPlugin.dll", ReadTestAssembly()), ("readme.txt", Encoding.UTF8.GetBytes("not an assembly"))]);
        zipStream.Position = 0;

        // Act
        Result<Success> result = await _sut.InstallAsync(zipStream, "plugin.zip", CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.True(File.Exists(Path.Combine(_pluginsDirectory, "Lumina.TestPlugin.dll")));
        Assert.False(File.Exists(Path.Combine(_pluginsDirectory, "readme.txt")));
    }

    [Fact]
    public async Task InstallAsync_WhenUploadingZipWithBackslashTraversalEntry_ShouldFlattenItInsidePluginsDirectory()
    {
        // Arrange
        await using MemoryStream zipStream = new();
        // A backslash is not a directory separator on non-Windows platforms, so it must be neutralized explicitly.
        CreateZipArchive(zipStream, [(@"..\evil.dll", ReadTestAssembly())]);
        zipStream.Position = 0;

        // Act
        Result<Success> result = await _sut.InstallAsync(zipStream, "plugin.zip", CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.True(File.Exists(Path.Combine(_pluginsDirectory, "evil.dll")));
        Assert.False(File.Exists(Path.Combine(AppContext.BaseDirectory, "evil.dll")));
    }

    [Fact]
    public async Task InstallAsync_WhenUploadingSingleAssemblyWithTraversalFileName_ShouldFlattenItInsidePluginsDirectory()
    {
        // Arrange
        const string TRAVERSAL_FILE_NAME = "traversal-test-plugin.dll";
        await using FileStream stream = File.OpenRead(Path.Combine(AppContext.BaseDirectory, "Lumina.TestPlugin.dll"));

        // Act
        Result<Success> result = await _sut.InstallAsync(stream, $@"..\{TRAVERSAL_FILE_NAME}", CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.True(File.Exists(Path.Combine(_pluginsDirectory, TRAVERSAL_FILE_NAME)));
        Assert.False(File.Exists(Path.Combine(AppContext.BaseDirectory, TRAVERSAL_FILE_NAME)));
    }

    [Fact]
    public async Task InstallAsync_WhenUploadingZipWithoutAssemblies_ShouldReturnError()
    {
        // Arrange
        await using MemoryStream zipStream = new();
        CreateZipArchive(zipStream, [("readme.txt", Encoding.UTF8.GetBytes("not an assembly"))]);
        zipStream.Position = 0;

        // Act
        Result<Success> result = await _sut.InstallAsync(zipStream, "plugin.zip", CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Plugins.PluginArchiveContainsNoAssemblies, result.FirstError);
    }

    /// <summary>
    /// Deletes the temporary plugins directory created by the tests.
    /// </summary>
    public void Dispose()
    {
        if (Directory.Exists(_pluginsDirectory))
            Directory.Delete(_pluginsDirectory, recursive: true);
    }

    /// <summary>
    /// Creates an in-memory ZIP archive containing the provided entries.
    /// </summary>
    /// <param name="zipStream">The stream to write the archive into.</param>
    /// <param name="entries">The entries to add, as pairs of entry name and content.</param>
    private static void CreateZipArchive(MemoryStream zipStream, IEnumerable<(string Name, byte[] Content)> entries)
    {
        using ZipArchive zipArchive = new(zipStream, ZipArchiveMode.Create, leaveOpen: true);
        foreach ((string name, byte[] content) in entries)
        {
            ZipArchiveEntry entry = zipArchive.CreateEntry(name);
            using Stream entryStream = entry.Open();
            entryStream.Write(content);
        }
    }

    /// <summary>
    /// Reads the bytes of the <c>Lumina.TestPlugin.dll</c> assembly from the test output directory.
    /// </summary>
    /// <returns>The assembly bytes.</returns>
    private static byte[] ReadTestAssembly()
    {
        return File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "Lumina.TestPlugin.dll"));
    }
}
