#region ========================================================================= USING =====================================================================================
using Nemera.Contracts.DTO.Common;
using Nemera.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Nemera.Plugins.LocalMusicArtwork.Core;
using Nemera.Plugins.LocalMusicArtwork.Fixtures.Core;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
#endregion

namespace Nemera.Plugins.LocalMusicArtwork.IntegrationTests.Core;

/// <summary>
/// Contains integration tests for the <see cref="LocalMusicArtworkEmbeddedCoverExtractor"/> class, reading real audio containers that carry embedded pictures.
/// </summary>
[ExcludeFromCodeCoverage]
public class LocalMusicArtworkEmbeddedCoverExtractorTests : IDisposable
{
    private static readonly byte[] s_coverBytes = [0xFF, 0xD8, 0xFF, 0xE0, 0x11, 0x22, 0x33, 0xFF, 0xD9];

    private readonly string _temporaryDirectory;

    /// <summary>
    /// Initializes a new instance of the <see cref="LocalMusicArtworkEmbeddedCoverExtractorTests"/> class.
    /// </summary>
    public LocalMusicArtworkEmbeddedCoverExtractorTests()
    {
        _temporaryDirectory = Path.Combine(Path.GetTempPath(), $"nemera-local-music-embedded-cover-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_temporaryDirectory);
    }

    [Fact]
    public void Extract_WhenTheTrackCarriesAnEmbeddedCover_ShouldReturnATemporaryCoverArtwork()
    {
        // Arrange
        string trackPath = Path.Combine(_temporaryDirectory, "track.wav");
        TestAudioFileFactory.CreateWavWithEmbeddedCover(trackPath, s_coverBytes, "image/jpeg");

        // Act
        ArtworkDto? result = LocalMusicArtworkEmbeddedCoverExtractor.Extract(trackPath);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(ArtworkType.Cover, result.Type);
        Assert.Equal(0, result.Ordinal);
        Assert.NotNull(result.LocalPath);
        Assert.StartsWith(Path.GetTempPath(), result.LocalPath);
        Assert.EndsWith(".jpg", result.LocalPath);
        Assert.True(File.Exists(result.LocalPath));
        Assert.Equal(s_coverBytes, File.ReadAllBytes(result.LocalPath!));
        Assert.Null(result.RemoteUrl);
        Assert.True(result.IsTemporary);
        File.Delete(result.LocalPath!);
    }

    [Fact]
    public void Extract_WhenTheTrackHasNoEmbeddedCover_ShouldReturnNull()
    {
        // Arrange
        string trackPath = Path.Combine(_temporaryDirectory, "track.wav");
        TestAudioFileFactory.CreateWav(trackPath);

        // Act
        ArtworkDto? result = LocalMusicArtworkEmbeddedCoverExtractor.Extract(trackPath);

        // Assert
        Assert.Null(result);
    }

    [Theory]
    [InlineData(null)] // A null path contributes no cover.
    [InlineData("")] // An empty path contributes no cover.
    [InlineData("   ")] // A white space path contributes no cover.
    public void Extract_WhenTheTrackPathIsNullOrEmpty_ShouldReturnNull(string? trackPath)
    {
        // Act
        ArtworkDto? result = LocalMusicArtworkEmbeddedCoverExtractor.Extract(trackPath);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Extract_WhenTheTrackFileDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        string trackPath = Path.Combine(_temporaryDirectory, "missing.wav");

        // Act
        ArtworkDto? result = LocalMusicArtworkEmbeddedCoverExtractor.Extract(trackPath);

        // Assert
        Assert.Null(result);
    }

    [Theory]
    [InlineData("image/jpeg", ".jpg")]
    [InlineData("image/png", ".png")]
    [InlineData("image/webp", ".webp")]
    [InlineData("image/bmp", ".bmp")]
    [InlineData("image/x-ms-bmp", ".bmp")]
    [InlineData("image/gif", ".gif")]
    [InlineData("image/tiff", ".tiff")]
    [InlineData("IMAGE/PNG", ".png")] // The MIME type is matched without regard to casing.
    [InlineData("  image/png  ", ".png")] // The MIME type is trimmed before it is matched.
    [InlineData("application/octet-stream", ".jpg")] // An unknown MIME type falls back to the JPEG extension.
    [InlineData("", ".jpg")] // An empty MIME type falls back to the JPEG extension.
    [InlineData(null, ".jpg")] // A missing MIME type falls back to the JPEG extension.
    public void Extract_WhenTheMimeTypeIsProvided_ShouldUseTheMatchingFileExtension(string? mimeType, string expectedExtension)
    {
        // Arrange
        string trackPath = Path.Combine(_temporaryDirectory, "track.wav");
        TestAudioFileFactory.CreateWavWithEmbeddedCover(trackPath, s_coverBytes, mimeType);

        // Act
        ArtworkDto? result = LocalMusicArtworkEmbeddedCoverExtractor.Extract(trackPath);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.LocalPath);
        Assert.EndsWith(expectedExtension, result.LocalPath);
        Assert.Equal(s_coverBytes, File.ReadAllBytes(result.LocalPath!));
        Assert.True(result.IsTemporary);
        File.Delete(result.LocalPath!);
    }

    [Fact]
    public void Extract_WhenTheEmbeddedCoverIsExactlyTheMaximumSize_ShouldReturnTheCover()
    {
        // Arrange
        byte[] maximumCover = new byte[10 * 1024 * 1024];
        string trackPath = Path.Combine(_temporaryDirectory, "track.wav");
        TestAudioFileFactory.CreateWavWithEmbeddedCover(trackPath, maximumCover, "image/jpeg");

        // Act
        ArtworkDto? result = LocalMusicArtworkEmbeddedCoverExtractor.Extract(trackPath);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.LocalPath);
        Assert.Equal(maximumCover, File.ReadAllBytes(result.LocalPath!));
        Assert.True(result.IsTemporary);
        File.Delete(result.LocalPath!);
    }

    [Fact]
    public void Extract_WhenTheEmbeddedCoverIsLargerThanTheMaximumSize_ShouldReturnNull()
    {
        // Arrange
        byte[] oversizedCover = new byte[10 * 1024 * 1024 + 1];
        string trackPath = Path.Combine(_temporaryDirectory, "track.wav");
        TestAudioFileFactory.CreateWavWithEmbeddedCover(trackPath, oversizedCover, "image/jpeg");

        // Act
        ArtworkDto? result = LocalMusicArtworkEmbeddedCoverExtractor.Extract(trackPath);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Extract_WhenTheTrackIsNotAnAudioFile_ShouldReturnNull()
    {
        // Arrange
        string trackPath = Path.Combine(_temporaryDirectory, "track.txt");
        TestAudioFileFactory.CreateUnsupportedFile(trackPath);

        // Act
        ArtworkDto? result = LocalMusicArtworkEmbeddedCoverExtractor.Extract(trackPath);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Extract_WhenTheAudioFileIsCorrupt_ShouldReturnNull()
    {
        // Arrange
        string trackPath = Path.Combine(_temporaryDirectory, "track.mp3");
        TestAudioFileFactory.CreateUnsupportedFile(trackPath);

        // Act
        ArtworkDto? result = LocalMusicArtworkEmbeddedCoverExtractor.Extract(trackPath);

        // Assert
        Assert.Null(result);
    }

    /// <summary>
    /// Removes the temporary directory created for the test.
    /// </summary>
    public void Dispose()
    {
        if (Directory.Exists(_temporaryDirectory))
            Directory.Delete(_temporaryDirectory, recursive: true);
        GC.SuppressFinalize(this);
    }
}
