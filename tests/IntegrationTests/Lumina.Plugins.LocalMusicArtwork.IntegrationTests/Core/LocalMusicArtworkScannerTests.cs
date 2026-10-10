#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.Common;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Plugins.LocalMusicArtwork.Core;
using Lumina.Plugins.LocalMusicArtwork.Fixtures.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
#endregion

namespace Lumina.Plugins.LocalMusicArtwork.IntegrationTests.Core;

/// <summary>
/// Contains integration tests for the <see cref="LocalMusicArtworkScanner"/> class, scanning real image files on the file system.
/// </summary>
[ExcludeFromCodeCoverage]
public class LocalMusicArtworkScannerTests : IDisposable
{
    private readonly string _temporaryDirectory;

    /// <summary>
    /// Initializes a new instance of the <see cref="LocalMusicArtworkScannerTests"/> class.
    /// </summary>
    public LocalMusicArtworkScannerTests()
    {
        _temporaryDirectory = Path.Combine(Path.GetTempPath(), $"lumina-local-music-artwork-scanner-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_temporaryDirectory);
    }

    [Fact]
    public void ScanAlbumArtwork_WhenTheAlbumDirectoryCarriesTheStandardImages_ShouldReturnEachArtworkType()
    {
        // Arrange
        string albumDirectory = Path.Combine(_temporaryDirectory, "Artist", "Album");
        TestImageFileFactory.CreateJpeg(Path.Combine(albumDirectory, "cover.jpg"));
        TestImageFileFactory.CreateJpeg(Path.Combine(albumDirectory, "front.jpg"));
        TestImageFileFactory.CreatePng(Path.Combine(albumDirectory, "back.png"));
        TestImageFileFactory.CreateJpeg(Path.Combine(albumDirectory, "booklet1.jpg"));
        TestImageFileFactory.CreatePng(Path.Combine(albumDirectory, "booklet2.png"));
        TestImageFileFactory.CreatePng(Path.Combine(albumDirectory, "disk.png"));
        TestImageFileFactory.CreateGif(Path.Combine(albumDirectory, "tray.gif"));
        TestImageFileFactory.CreateTiff(Path.Combine(albumDirectory, "spine.tiff"));
        TestImageFileFactory.CreateBmp(Path.Combine(albumDirectory, "obi.bmp"));
        TestImageFileFactory.CreateJpeg(Path.Combine(albumDirectory, "sticker.jpg"));
        TestImageFileFactory.CreatePng(Path.Combine(albumDirectory, "poster.png"));
        TestImageFileFactory.CreateJpeg(Path.Combine(albumDirectory, "liner.jpg"));
        string trackPath = Path.Combine(albumDirectory, "track.flac");

        // Act
        IReadOnlyList<ArtworkDto> result = LocalMusicArtworkScanner.ScanAlbumArtwork(trackPath);

        // Assert
        List<ArtworkDto> expected =
        [
            new(ArtworkType.Cover, 0, Path.Combine(albumDirectory, "cover.jpg"), null),
            new(ArtworkType.Front, 0, Path.Combine(albumDirectory, "front.jpg"), null),
            new(ArtworkType.Back, 0, Path.Combine(albumDirectory, "back.png"), null),
            new(ArtworkType.Booklet, 1, Path.Combine(albumDirectory, "booklet1.jpg"), null),
            new(ArtworkType.Booklet, 2, Path.Combine(albumDirectory, "booklet2.png"), null),
            new(ArtworkType.Medium, 0, Path.Combine(albumDirectory, "disk.png"), null),
            new(ArtworkType.Tray, 0, Path.Combine(albumDirectory, "tray.gif"), null),
            new(ArtworkType.Spine, 0, Path.Combine(albumDirectory, "spine.tiff"), null),
            new(ArtworkType.Obi, 0, Path.Combine(albumDirectory, "obi.bmp"), null),
            new(ArtworkType.Sticker, 0, Path.Combine(albumDirectory, "sticker.jpg"), null),
            new(ArtworkType.Poster, 0, Path.Combine(albumDirectory, "poster.png"), null),
            new(ArtworkType.Liner, 0, Path.Combine(albumDirectory, "liner.jpg"), null)
        ];
        Assert.Equal(12, result.Count);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ScanAlbumArtwork_WhenTheStemCarriesANumber_ShouldUseItAsTheOrdinal()
    {
        // Arrange
        string albumDirectory = Path.Combine(_temporaryDirectory, "Album");
        TestImageFileFactory.CreateJpeg(Path.Combine(albumDirectory, "booklet.jpg"));
        TestImageFileFactory.CreateJpeg(Path.Combine(albumDirectory, "booklet1.jpg"));
        TestImageFileFactory.CreateJpeg(Path.Combine(albumDirectory, "booklet2.jpg"));
        TestImageFileFactory.CreateJpeg(Path.Combine(albumDirectory, "disk1.jpg"));
        TestImageFileFactory.CreateJpeg(Path.Combine(albumDirectory, "disk2.jpg"));
        string trackPath = Path.Combine(albumDirectory, "track.flac");

        // Act
        IReadOnlyList<ArtworkDto> result = LocalMusicArtworkScanner.ScanAlbumArtwork(trackPath);

        // Assert
        List<ArtworkDto> expected =
        [
            new(ArtworkType.Booklet, 0, Path.Combine(albumDirectory, "booklet.jpg"), null),
            new(ArtworkType.Booklet, 1, Path.Combine(albumDirectory, "booklet1.jpg"), null),
            new(ArtworkType.Booklet, 2, Path.Combine(albumDirectory, "booklet2.jpg"), null),
            new(ArtworkType.Medium, 1, Path.Combine(albumDirectory, "disk1.jpg"), null),
            new(ArtworkType.Medium, 2, Path.Combine(albumDirectory, "disk2.jpg"), null)
        ];
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ScanAlbumArtwork_WhenTheStemHasAHyphenOrUnderscoreBeforeTheNumber_ShouldTrimTheSeparator()
    {
        // Arrange
        string albumDirectory = Path.Combine(_temporaryDirectory, "Album");
        TestImageFileFactory.CreateJpeg(Path.Combine(albumDirectory, "booklet - 1.jpg"));
        TestImageFileFactory.CreateJpeg(Path.Combine(albumDirectory, "booklet_2.jpg"));
        string trackPath = Path.Combine(albumDirectory, "track.flac");

        // Act
        IReadOnlyList<ArtworkDto> result = LocalMusicArtworkScanner.ScanAlbumArtwork(trackPath);

        // Assert
        List<ArtworkDto> expected =
        [
            new(ArtworkType.Booklet, 1, Path.Combine(albumDirectory, "booklet - 1.jpg"), null),
            new(ArtworkType.Booklet, 2, Path.Combine(albumDirectory, "booklet_2.jpg"), null)
        ];
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ScanAlbumArtwork_WhenBothCoverAndFrontExist_ShouldReturnThemAsDistinctArtworks()
    {
        // Arrange
        string albumDirectory = Path.Combine(_temporaryDirectory, "Album");
        TestImageFileFactory.CreateJpeg(Path.Combine(albumDirectory, "cover.jpg"));
        TestImageFileFactory.CreateJpeg(Path.Combine(albumDirectory, "front.jpg"));
        string trackPath = Path.Combine(albumDirectory, "track.flac");

        // Act
        IReadOnlyList<ArtworkDto> result = LocalMusicArtworkScanner.ScanAlbumArtwork(trackPath);

        // Assert
        List<ArtworkDto> expected =
        [
            new(ArtworkType.Cover, 0, Path.Combine(albumDirectory, "cover.jpg"), null),
            new(ArtworkType.Front, 0, Path.Combine(albumDirectory, "front.jpg"), null)
        ];
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ScanAlbumArtwork_WhenSeveralNamesMapToTheSameTypeAndOrdinal_ShouldKeepOnlyThePreferredFile()
    {
        // Arrange
        string albumDirectory = Path.Combine(_temporaryDirectory, "Album");
        TestImageFileFactory.CreateJpeg(Path.Combine(albumDirectory, "disk.jpg"));
        TestImageFileFactory.CreateJpeg(Path.Combine(albumDirectory, "disc.jpg"));
        string trackPath = Path.Combine(albumDirectory, "track.flac");

        // Act
        IReadOnlyList<ArtworkDto> result = LocalMusicArtworkScanner.ScanAlbumArtwork(trackPath);

        // Assert
        // Both file names describe the medium of the album, and the one that sorts first by name wins, so that a single image is stored per type and ordinal.
        ArtworkDto artwork = Assert.Single(result);
        Assert.Equal(ArtworkType.Medium, artwork.Type);
        Assert.Equal(0, artwork.Ordinal);
        Assert.Equal(Path.Combine(albumDirectory, "disc.jpg"), artwork.LocalPath);
        Assert.Null(artwork.RemoteUrl);
        Assert.False(artwork.IsTemporary);
    }

    [Fact]
    public void ScanAlbumArtwork_WhenAFileIsNotAnImage_ShouldIgnoreIt()
    {
        // Arrange
        string albumDirectory = Path.Combine(_temporaryDirectory, "Album");
        TestImageFileFactory.CreateJpeg(Path.Combine(albumDirectory, "cover.jpg"));
        TestImageFileFactory.CreateTextFile(Path.Combine(albumDirectory, "notes.txt"));
        string trackPath = Path.Combine(albumDirectory, "track.flac");

        // Act
        IReadOnlyList<ArtworkDto> result = LocalMusicArtworkScanner.ScanAlbumArtwork(trackPath);

        // Assert
        ArtworkDto artwork = Assert.Single(result);
        Assert.Equal(ArtworkType.Cover, artwork.Type);
        Assert.Equal(Path.Combine(albumDirectory, "cover.jpg"), artwork.LocalPath);
    }

    [Fact]
    public void ScanAlbumArtwork_WhenTheImageExtensionIsUppercase_ShouldIgnoreIt()
    {
        // Arrange
        string albumDirectory = Path.Combine(_temporaryDirectory, "Album");
        TestImageFileFactory.CreateJpeg(Path.Combine(albumDirectory, "cover.JPG"));
        TestImageFileFactory.CreateJpeg(Path.Combine(albumDirectory, "back.JPEG"));
        string trackPath = Path.Combine(albumDirectory, "track.flac");

        // Act
        IReadOnlyList<ArtworkDto> result = LocalMusicArtworkScanner.ScanAlbumArtwork(trackPath);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void ScanAlbumArtwork_WhenTheImageNameHasDifferentCasing_ShouldIgnoreIt()
    {
        // Arrange
        string albumDirectory = Path.Combine(_temporaryDirectory, "Album");
        TestImageFileFactory.CreateJpeg(Path.Combine(albumDirectory, "Cover.jpg"));
        TestImageFileFactory.CreateJpeg(Path.Combine(albumDirectory, "BACK.jpg"));
        string trackPath = Path.Combine(albumDirectory, "track.flac");

        // Act
        IReadOnlyList<ArtworkDto> result = LocalMusicArtworkScanner.ScanAlbumArtwork(trackPath);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void ScanAlbumArtwork_WhenTheImageNameIsNotRecognized_ShouldIgnoreIt()
    {
        // Arrange
        string albumDirectory = Path.Combine(_temporaryDirectory, "Album");
        TestImageFileFactory.CreateJpeg(Path.Combine(albumDirectory, "random.jpg"));
        TestImageFileFactory.CreateJpeg(Path.Combine(albumDirectory, "folder.jpg"));
        TestImageFileFactory.CreateJpeg(Path.Combine(albumDirectory, "backdrop.jpg"));
        string trackPath = Path.Combine(albumDirectory, "track.flac");

        // Act
        IReadOnlyList<ArtworkDto> result = LocalMusicArtworkScanner.ScanAlbumArtwork(trackPath);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void ScanAlbumArtwork_WhenTheAlbumIsMultiDiscAndTheParentDirectoryCarriesTheArtwork_ShouldFindIt()
    {
        // Arrange
        string albumDirectory = Path.Combine(_temporaryDirectory, "Album");
        string discDirectory = Path.Combine(albumDirectory, "CD1");
        TestImageFileFactory.CreateJpeg(Path.Combine(albumDirectory, "cover.jpg"));
        TestImageFileFactory.CreateJpeg(Path.Combine(albumDirectory, "back.jpg"));
        Directory.CreateDirectory(discDirectory);
        string trackPath = Path.Combine(discDirectory, "track.flac");

        // Act
        IReadOnlyList<ArtworkDto> result = LocalMusicArtworkScanner.ScanAlbumArtwork(trackPath);

        // Assert
        List<ArtworkDto> expected =
        [
            new(ArtworkType.Cover, 0, Path.Combine(albumDirectory, "cover.jpg"), null),
            new(ArtworkType.Back, 0, Path.Combine(albumDirectory, "back.jpg"), null)
        ];
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ScanAlbumArtwork_WhenBothTheDiscAndTheParentDirectoryCarryTheSameArtwork_ShouldPreferTheNearest()
    {
        // Arrange
        string albumDirectory = Path.Combine(_temporaryDirectory, "Album");
        string discDirectory = Path.Combine(albumDirectory, "CD1");
        TestImageFileFactory.CreateJpeg(Path.Combine(albumDirectory, "cover.jpg"));
        TestImageFileFactory.CreateJpeg(Path.Combine(discDirectory, "cover.jpg"));
        string trackPath = Path.Combine(discDirectory, "track.flac");

        // Act
        IReadOnlyList<ArtworkDto> result = LocalMusicArtworkScanner.ScanAlbumArtwork(trackPath);

        // Assert
        ArtworkDto artwork = Assert.Single(result);
        Assert.Equal(ArtworkType.Cover, artwork.Type);
        Assert.Equal(0, artwork.Ordinal);
        Assert.Equal(Path.Combine(discDirectory, "cover.jpg"), artwork.LocalPath);
    }

    [Fact]
    public void ScanAlbumArtwork_WhenTheDiscAndTheParentDirectoryCarryDifferentArtworks_ShouldReturnBoth()
    {
        // Arrange
        string albumDirectory = Path.Combine(_temporaryDirectory, "Album");
        string discDirectory = Path.Combine(albumDirectory, "CD1");
        TestImageFileFactory.CreateJpeg(Path.Combine(discDirectory, "cover.jpg"));
        TestImageFileFactory.CreateJpeg(Path.Combine(albumDirectory, "back.jpg"));
        string trackPath = Path.Combine(discDirectory, "track.flac");

        // Act
        IReadOnlyList<ArtworkDto> result = LocalMusicArtworkScanner.ScanAlbumArtwork(trackPath);

        // Assert
        List<ArtworkDto> expected =
        [
            new(ArtworkType.Cover, 0, Path.Combine(discDirectory, "cover.jpg"), null),
            new(ArtworkType.Back, 0, Path.Combine(albumDirectory, "back.jpg"), null)
        ];
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ScanAlbumArtwork_WhenTheArtworkIsMoreThanTwoLevelsAboveTheTrack_ShouldNotFindIt()
    {
        // Arrange
        TestImageFileFactory.CreateJpeg(Path.Combine(_temporaryDirectory, "cover.jpg"));
        string trackPath = Path.Combine(_temporaryDirectory, "Album", "CD1", "track.flac");

        // Act
        IReadOnlyList<ArtworkDto> result = LocalMusicArtworkScanner.ScanAlbumArtwork(trackPath);

        // Assert
        Assert.Empty(result);
    }

    [Theory]
    [InlineData(null)] // a null path contributes no artwork
    [InlineData("")] // an empty path contributes no artwork
    [InlineData("   ")] // a white space path contributes no artwork
    [InlineData("track.flac")] // a path without a directory part contributes no artwork
    public void ScanAlbumArtwork_WhenTheTrackPathDoesNotPointToADirectory_ShouldReturnEmpty(string? trackPath)
    {
        // Act
        IReadOnlyList<ArtworkDto> result = LocalMusicArtworkScanner.ScanAlbumArtwork(trackPath);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void ScanAlbumArtwork_WhenTheAlbumDirectoryDoesNotExist_ShouldReturnEmpty()
    {
        // Arrange
        string trackPath = Path.Combine(_temporaryDirectory, "Missing", "Album", "track.flac");

        // Act
        IReadOnlyList<ArtworkDto> result = LocalMusicArtworkScanner.ScanAlbumArtwork(trackPath);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void ScanArtistArtwork_WhenTheArtistDirectoryCarriesTheStandardImages_ShouldReturnEachArtworkType()
    {
        // Arrange
        string artistDirectory = Path.Combine(_temporaryDirectory, "Artist");
        TestImageFileFactory.CreateJpeg(Path.Combine(artistDirectory, "folder.jpg"));
        TestImageFileFactory.CreateJpeg(Path.Combine(artistDirectory, "backdrop.jpg"));
        TestImageFileFactory.CreatePng(Path.Combine(artistDirectory, "banner.png"));
        TestImageFileFactory.CreateWebp(Path.Combine(artistDirectory, "logo.webp"));
        TestImageFileFactory.CreateGif(Path.Combine(artistDirectory, "thumb.gif"));
        string trackPath = Path.Combine(artistDirectory, "Album", "track.flac");

        // Act
        IReadOnlyList<ArtworkDto> result = LocalMusicArtworkScanner.ScanArtistArtwork(trackPath);

        // Assert
        List<ArtworkDto> expected =
        [
            new(ArtworkType.Cover, 0, Path.Combine(artistDirectory, "folder.jpg"), null),
            new(ArtworkType.Backdrop, 0, Path.Combine(artistDirectory, "backdrop.jpg"), null),
            new(ArtworkType.Banner, 0, Path.Combine(artistDirectory, "banner.png"), null),
            new(ArtworkType.Logo, 0, Path.Combine(artistDirectory, "logo.webp"), null),
            new(ArtworkType.Thumb, 0, Path.Combine(artistDirectory, "thumb.gif"), null)
        ];
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ScanArtistArtwork_WhenTheDirectoryCarriesOnlyAlbumImages_ShouldReturnEmpty()
    {
        // Arrange
        string artistDirectory = Path.Combine(_temporaryDirectory, "Artist");
        TestImageFileFactory.CreateJpeg(Path.Combine(artistDirectory, "cover.jpg"));
        TestImageFileFactory.CreateJpeg(Path.Combine(artistDirectory, "back.jpg"));
        TestImageFileFactory.CreateJpeg(Path.Combine(artistDirectory, "disk.jpg"));
        string trackPath = Path.Combine(artistDirectory, "Album", "track.flac");

        // Act
        IReadOnlyList<ArtworkDto> result = LocalMusicArtworkScanner.ScanArtistArtwork(trackPath);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void ScanArtistArtwork_WhenTheArtistImagesAreInAnAncestorDirectory_ShouldFindThem()
    {
        // Arrange
        string artistDirectory = Path.Combine(_temporaryDirectory, "Artist");
        TestImageFileFactory.CreateJpeg(Path.Combine(artistDirectory, "folder.jpg"));
        string trackPath = Path.Combine(artistDirectory, "Album", "CD1", "track.flac");

        // Act
        IReadOnlyList<ArtworkDto> result = LocalMusicArtworkScanner.ScanArtistArtwork(trackPath);

        // Assert
        ArtworkDto artwork = Assert.Single(result);
        Assert.Equal(ArtworkType.Cover, artwork.Type);
        Assert.Equal(0, artwork.Ordinal);
        Assert.Equal(Path.Combine(artistDirectory, "folder.jpg"), artwork.LocalPath);
    }

    [Fact]
    public void ScanArtistArtwork_WhenSeveralAncestorDirectoriesCarryArtistImages_ShouldPreferTheNearest()
    {
        // Arrange
        string artistDirectory = Path.Combine(_temporaryDirectory, "Artist");
        string albumDirectory = Path.Combine(artistDirectory, "Album");
        TestImageFileFactory.CreateJpeg(Path.Combine(artistDirectory, "backdrop.jpg"));
        TestImageFileFactory.CreateJpeg(Path.Combine(albumDirectory, "folder.jpg"));
        TestImageFileFactory.CreateJpeg(Path.Combine(albumDirectory, "cover.jpg"));
        string trackPath = Path.Combine(albumDirectory, "track.flac");

        // Act
        IReadOnlyList<ArtworkDto> result = LocalMusicArtworkScanner.ScanArtistArtwork(trackPath);

        // Assert
        // The nearest ancestor that carries an artist image wins, and the album images of that directory are not part of the artist artwork.
        ArtworkDto artwork = Assert.Single(result);
        Assert.Equal(ArtworkType.Cover, artwork.Type);
        Assert.Equal(0, artwork.Ordinal);
        Assert.Equal(Path.Combine(albumDirectory, "folder.jpg"), artwork.LocalPath);
    }

    [Fact]
    public void ScanArtistArtwork_WhenTheArtistImagesAreBeyondTheSearchDepth_ShouldReturnEmpty()
    {
        // Arrange
        TestImageFileFactory.CreateJpeg(Path.Combine(_temporaryDirectory, "folder.jpg"));
        string trackPath = Path.Combine(_temporaryDirectory, "A", "B", "C", "D", "E", "track.flac");

        // Act
        IReadOnlyList<ArtworkDto> result = LocalMusicArtworkScanner.ScanArtistArtwork(trackPath);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void ScanArtistArtwork_WhenTheImageNameHasDifferentCasing_ShouldIgnoreIt()
    {
        // Arrange
        string artistDirectory = Path.Combine(_temporaryDirectory, "Artist");
        TestImageFileFactory.CreateJpeg(Path.Combine(artistDirectory, "Folder.jpg"));
        TestImageFileFactory.CreateJpeg(Path.Combine(artistDirectory, "Backdrop.jpg"));
        string trackPath = Path.Combine(artistDirectory, "Album", "track.flac");

        // Act
        IReadOnlyList<ArtworkDto> result = LocalMusicArtworkScanner.ScanArtistArtwork(trackPath);

        // Assert
        Assert.Empty(result);
    }

    [Theory]
    [InlineData(null)] // a null path contributes no artwork
    [InlineData("")] // an empty path contributes no artwork
    [InlineData("   ")] // a white space path contributes no artwork
    [InlineData("track.flac")] // a path without a directory part contributes no artwork
    public void ScanArtistArtwork_WhenTheTrackPathDoesNotPointToADirectory_ShouldReturnEmpty(string? trackPath)
    {
        // Act
        IReadOnlyList<ArtworkDto> result = LocalMusicArtworkScanner.ScanArtistArtwork(trackPath);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void ScanArtistArtwork_WhenTheAncestorDirectoryDoesNotExist_ShouldReturnEmpty()
    {
        // Arrange
        string trackPath = Path.Combine(_temporaryDirectory, "Missing", "Artist", "track.flac");

        // Act
        IReadOnlyList<ArtworkDto> result = LocalMusicArtworkScanner.ScanArtistArtwork(trackPath);

        // Assert
        Assert.Empty(result);
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
