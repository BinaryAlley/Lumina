#region ========================================================================= USING =====================================================================================
using Lumina.Infrastructure.Common.Caching;
using Lumina.Infrastructure.Common.Models.DTO.Configuration;
using Lumina.Infrastructure.Core.MediaLibrary.AudioLibrary.MusicLibrary.Playback;
using Lumina.Infrastructure.Fixtures.Common.Models.DTO.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Infrastructure.IntegrationTests.Core.MediaLibrary.AudioLibrary.MusicLibrary.Playback;

/// <summary>
/// Contains integration tests for the <see cref="TranscodingStartupInitializer"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class TranscodingStartupInitializerTests : IDisposable
{
    private readonly string _temporaryDirectory;
    private readonly string _cacheDirectory;
    private readonly TranscodingSettingsDto _settings;
    private readonly TranscodingToolchainStatus _toolchainStatus;
    private readonly ILogger<TranscodingStartupInitializer> _mockLogger;
    private readonly TranscodingStartupInitializer _sut;
    private readonly TranscodingSettingsDtoFixture _transcodingSettingsDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="TranscodingStartupInitializerTests"/> class.
    /// </summary>
    public TranscodingStartupInitializerTests()
    {
        _temporaryDirectory = Path.Combine(Path.GetTempPath(), $"lumina-transcoding-startup-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_temporaryDirectory);
        _cacheDirectory = Path.Combine(_temporaryDirectory, "cache");
        string missingFfmpegPath = Path.Combine(_temporaryDirectory, "missing-ffmpeg-binary");
        _settings = _transcodingSettingsDtoFixture.Create(ffmpegPath: missingFfmpegPath, cacheDirectory: _cacheDirectory);
        _toolchainStatus = new TranscodingToolchainStatus();
        _mockLogger = Substitute.For<ILogger<TranscodingStartupInitializer>>();
        _sut = new TranscodingStartupInitializer(Options.Create(_settings), _toolchainStatus, _mockLogger);
    }

    [Fact]
    public async Task StartAsync_WhenTheConfiguredDirectoryDoesNotExist_ShouldCreateAndMarkItAndRecordTheToolchainUnavailable()
    {
        // Act
        await _sut.StartAsync(CancellationToken.None);

        // Assert
        Assert.True(Directory.Exists(_cacheDirectory));
        Assert.True(TranscodingCachePaths.HasCacheMarker(_cacheDirectory));
        Assert.False(_toolchainStatus.IsTranscoderAvailable);
    }

    [Fact]
    public async Task StartAsync_WhenTheDirectoryExistsButIsEmptyWithoutTheMarker_ShouldAdoptItByWritingTheMarker()
    {
        // Arrange
        Directory.CreateDirectory(_cacheDirectory);
        Assert.False(TranscodingCachePaths.HasCacheMarker(_cacheDirectory));

        // Act
        await _sut.StartAsync(CancellationToken.None);

        // Assert
        Assert.True(Directory.Exists(_cacheDirectory));
        Assert.True(TranscodingCachePaths.HasCacheMarker(_cacheDirectory));
    }

    [Fact]
    public async Task StartAsync_WhenTheDirectoryExistsNonEmptyWithoutTheMarker_ShouldLeaveItUntouchedAndLogAnError()
    {
        // Arrange
        Directory.CreateDirectory(_cacheDirectory);
        string operatorFilePath = Path.Combine(_cacheDirectory, "operator-data.txt");
        File.WriteAllText(operatorFilePath, "operator data");
        string operatorDirectoryPath = Path.Combine(_cacheDirectory, "operator-directory");
        Directory.CreateDirectory(operatorDirectoryPath);
        string nestedFilePath = Path.Combine(operatorDirectoryPath, "nested.txt");
        File.WriteAllText(nestedFilePath, "nested data");

        // Act
        await _sut.StartAsync(CancellationToken.None);

        // Assert
        Assert.True(Directory.Exists(_cacheDirectory));
        Assert.Equal("operator data", File.ReadAllText(operatorFilePath));
        Assert.True(File.Exists(nestedFilePath));
        Assert.False(TranscodingCachePaths.HasCacheMarker(_cacheDirectory));
        Assert.False(_toolchainStatus.IsTranscoderAvailable);
        _mockLogger.Received(1).Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            Arg.Any<Exception?>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task StartAsync_WhenTheDirectoryIsMarked_ShouldRemoveOnlyTheEntriesOfTheCacheNamespace()
    {
        // Arrange
        Directory.CreateDirectory(_cacheDirectory);
        TranscodingCachePaths.WriteCacheMarker(_cacheDirectory);
        string cacheEntryPath = Path.Combine(_cacheDirectory, CreateCacheId());
        Directory.CreateDirectory(Path.Combine(cacheEntryPath, "nested"));
        File.WriteAllText(Path.Combine(cacheEntryPath, "nested", "segment.m4s"), "cache data");
        string temporaryEntryPath = Path.Combine(_cacheDirectory, "tmp-0123456789abcdef");
        Directory.CreateDirectory(temporaryEntryPath);
        string deletionEntryPath = Path.Combine(_cacheDirectory, "del-0123456789abcdef");
        Directory.CreateDirectory(deletionEntryPath);
        string unrelatedDirectoryPath = Path.Combine(_cacheDirectory, "operator-directory");
        Directory.CreateDirectory(unrelatedDirectoryPath);
        File.WriteAllText(Path.Combine(unrelatedDirectoryPath, "keep.txt"), "keep");
        string unrelatedFilePath = Path.Combine(_cacheDirectory, "operator-note.txt");
        File.WriteAllText(unrelatedFilePath, "keep");

        // Act
        await _sut.StartAsync(CancellationToken.None);

        // Assert
        Assert.True(Directory.Exists(_cacheDirectory));
        Assert.False(Directory.Exists(cacheEntryPath));
        Assert.False(Directory.Exists(temporaryEntryPath));
        Assert.False(Directory.Exists(deletionEntryPath));
        Assert.True(Directory.Exists(unrelatedDirectoryPath));
        Assert.Equal("keep", File.ReadAllText(Path.Combine(unrelatedDirectoryPath, "keep.txt")));
        Assert.Equal("keep", File.ReadAllText(unrelatedFilePath));
        Assert.True(TranscodingCachePaths.HasCacheMarker(_cacheDirectory));
    }

    [Fact]
    public async Task StartAsync_WhenTheCacheDirectoryIsAReparsePoint_ShouldNotThrowAndShouldRecordTheToolchainUnavailable()
    {
        // Arrange
        string junctionTargetPath = Path.Combine(_temporaryDirectory, "junction-target");
        Directory.CreateDirectory(junctionTargetPath);
        string targetFilePath = Path.Combine(junctionTargetPath, "keep.txt");
        File.WriteAllText(targetFilePath, "keep");
        if (!TryCreateJunction(_cacheDirectory, junctionTargetPath)) // The platform refused to create a reparse point, so the scenario cannot be exercised here.
            return;
        Assert.NotNull(new DirectoryInfo(_cacheDirectory).LinkTarget);

        // Act
        Exception? exception = await Record.ExceptionAsync(() => _sut.StartAsync(CancellationToken.None));

        // Assert
        Assert.Null(exception);
        Assert.False(_toolchainStatus.IsTranscoderAvailable);
        Assert.True(Directory.Exists(junctionTargetPath));
        Assert.Equal("keep", File.ReadAllText(targetFilePath));
    }

    [Fact]
    public async Task StartAsync_WhenTheProbeFails_ShouldRecordTheOutcomeInTheProvidedToolchainStatusInstance()
    {
        // Act
        await _sut.StartAsync(CancellationToken.None);

        // Assert
        // The instance passed to the initializer is the one that changes. A copied status would leave this instance available.
        Assert.False(_toolchainStatus.IsTranscoderAvailable);
        Assert.All(
            [.. PlaybackFormatCatalog.Formats.Select(format => format.EncoderName).Distinct(StringComparer.OrdinalIgnoreCase)],
            encoderName => Assert.False(_toolchainStatus.IsEncoderAvailable(encoderName)));
    }

    [Fact]
    public async Task StopAsync_WhenCalled_ShouldReturnACompletedTask()
    {
        // Act
        Task result = _sut.StopAsync(CancellationToken.None);

        // Assert
        Assert.True(result.IsCompletedSuccessfully);
        await result;
    }

    /// <summary>
    /// Creates a well formed opaque cache identifier.
    /// </summary>
    /// <returns>A 64 character lowercase hexadecimal identifier.</returns>
    private static string CreateCacheId()
    {
        return $"{Guid.NewGuid():N}{Guid.NewGuid():N}";
    }

    /// <summary>
    /// Creates a file system link that points to the target directory: a junction on Windows, and a symbolic link on Unix-like platforms.
    /// </summary>
    /// <param name="linkPath">The file system path of the link to create.</param>
    /// <param name="targetDirectory">The file system path of the directory the link points to.</param>
    /// <returns><see langword="true"/> when the link was created, <see langword="false"/> otherwise.</returns>
    private static bool TryCreateJunction(string linkPath, string targetDirectory)
    {
        try
        {
            ProcessStartInfo startInfo = OperatingSystem.IsWindows()
                ? new()
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c mklink /J \"{linkPath}\" \"{targetDirectory}\"",
                    CreateNoWindow = true,
                    UseShellExecute = false
                }
                : new()
                {
                    FileName = "ln",
                    CreateNoWindow = true,
                    UseShellExecute = false
                };
            if (!OperatingSystem.IsWindows())
            {
                startInfo.ArgumentList.Add("-s");
                startInfo.ArgumentList.Add(targetDirectory);
                startInfo.ArgumentList.Add(linkPath);
            }

            using (Process process = Process.Start(startInfo)!)
            {
                process.WaitForExit(5000);
                return process.ExitCode == 0;
            }
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>
    /// Removes the temporary directory created for the test.
    /// </summary>
    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_cacheDirectory) && new DirectoryInfo(_cacheDirectory).LinkTarget is not null)
                Directory.Delete(_cacheDirectory);
            if (Directory.Exists(_temporaryDirectory))
                Directory.Delete(_temporaryDirectory, recursive: true);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            // A reparse point may be held by the platform on some systems; the directory is under the process temporary directory.
        }
        GC.SuppressFinalize(this);
    }
}
