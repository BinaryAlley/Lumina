#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.Common;
using Lumina.Contracts.DTO.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Plugins.LocalMusicArtwork.Core;
using Lumina.Plugins.LocalMusicArtwork.Fixtures.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Plugins.LocalMusicArtwork.IntegrationTests.Core;

/// <summary>
/// Contains integration tests for the <see cref="LocalMusicArtistArtworkProvider"/> class, reading real image files from the file system.
/// </summary>
[ExcludeFromCodeCoverage]
public class LocalMusicArtistArtworkProviderTests : IDisposable
{
    private readonly ArtistMetadataLookupDtoFixture _artistMetadataLookupDtoFixture = new();
    private readonly LocalMusicArtistArtworkProvider _sut = new();
    private readonly string _temporaryDirectory;

    /// <summary>
    /// Initializes a new instance of the <see cref="LocalMusicArtistArtworkProviderTests"/> class.
    /// </summary>
    public LocalMusicArtistArtworkProviderTests()
    {
        _temporaryDirectory = Path.Combine(Path.GetTempPath(), $"lumina-local-music-artist-artwork-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_temporaryDirectory);
    }

    [Fact]
    public void Name_WhenCalled_ShouldReturnTheProviderDisplayName()
    {
        // Act
        string result = _sut.Name;

        // Assert
        Assert.Equal("Local Music Artwork", result);
    }

    [Fact]
    public void SupportedLibraryTypes_WhenCalled_ShouldReturnMusic()
    {
        // Act
        IReadOnlyList<LibraryType> result = _sut.SupportedLibraryTypes;

        // Assert
        Assert.Equal([LibraryType.Music], result);
    }

    [Fact]
    public void RequiresWebAccess_WhenCalled_ShouldReturnFalse()
    {
        // Act
        bool result = _sut.RequiresWebAccess;

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task GetArtworkAsync_WhenTheArtistDirectoryCarriesImages_ShouldReturnTheArtistArtwork()
    {
        // Arrange
        string artistDirectory = Path.Combine(_temporaryDirectory, "Artist");
        TestImageFileFactory.CreateJpeg(Path.Combine(artistDirectory, "folder.jpg"));
        TestImageFileFactory.CreatePng(Path.Combine(artistDirectory, "banner.png"));
        TestImageFileFactory.CreateWebp(Path.Combine(artistDirectory, "logo.webp"));
        string trackPath = Path.Combine(artistDirectory, "Album", "track.flac");
        ArtistMetadataLookupDto lookup = _artistMetadataLookupDtoFixture.Create(path: trackPath);

        // Act
        IReadOnlyList<ArtworkDto> result = await _sut.GetArtworkAsync(lookup, CancellationToken.None);

        // Assert
        List<ArtworkDto> expected =
        [
            new(ArtworkType.Cover, 0, Path.Combine(artistDirectory, "folder.jpg"), null),
            new(ArtworkType.Banner, 0, Path.Combine(artistDirectory, "banner.png"), null),
            new(ArtworkType.Logo, 0, Path.Combine(artistDirectory, "logo.webp"), null)
        ];
        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task GetArtworkAsync_WhenNoArtistImagesExist_ShouldReturnEmpty()
    {
        // Arrange
        string artistDirectory = Path.Combine(_temporaryDirectory, "Artist");
        TestImageFileFactory.CreateJpeg(Path.Combine(artistDirectory, "cover.jpg"));
        TestImageFileFactory.CreateJpeg(Path.Combine(artistDirectory, "back.jpg"));
        string trackPath = Path.Combine(artistDirectory, "Album", "track.flac");
        ArtistMetadataLookupDto lookup = _artistMetadataLookupDtoFixture.Create(path: trackPath);

        // Act
        IReadOnlyList<ArtworkDto> result = await _sut.GetArtworkAsync(lookup, CancellationToken.None);

        // Assert
        Assert.Empty(result);
    }

    [Theory]
    [InlineData(null)] // a null path contributes no artwork
    [InlineData("")] // an empty path contributes no artwork
    [InlineData("   ")] // a white space path contributes no artwork
    [InlineData("track.flac")] // a path without a directory part contributes no artwork
    public async Task GetArtworkAsync_WhenTheTrackPathDoesNotPointToADirectory_ShouldReturnEmpty(string? trackPath)
    {
        // Arrange
        ArtistMetadataLookupDto lookup = _artistMetadataLookupDtoFixture.Create(path: trackPath);

        // Act
        IReadOnlyList<ArtworkDto> result = await _sut.GetArtworkAsync(lookup, CancellationToken.None);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetArtworkAsync_WhenTheTrackDirectoryDoesNotExist_ShouldReturnEmpty()
    {
        // Arrange
        string trackPath = Path.Combine(_temporaryDirectory, "Missing", "Artist", "track.flac");
        ArtistMetadataLookupDto lookup = _artistMetadataLookupDtoFixture.Create(path: trackPath);

        // Act
        IReadOnlyList<ArtworkDto> result = await _sut.GetArtworkAsync(lookup, CancellationToken.None);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetArtworkAsync_WhenTheCancellationTokenIsAlreadyCancelled_ShouldStillReturnTheDiskArtwork()
    {
        // Arrange
        using (CancellationTokenSource cancellationTokenSource = new())
        {
            cancellationTokenSource.Cancel();
            string artistDirectory = Path.Combine(_temporaryDirectory, "Artist");
            TestImageFileFactory.CreateJpeg(Path.Combine(artistDirectory, "folder.jpg"));
            string trackPath = Path.Combine(artistDirectory, "Album", "track.flac");
            ArtistMetadataLookupDto lookup = _artistMetadataLookupDtoFixture.Create(path: trackPath);

            // Act
            IReadOnlyList<ArtworkDto> result = await _sut.GetArtworkAsync(lookup, cancellationTokenSource.Token);

            // Assert
            ArtworkDto artwork = Assert.Single(result);
            Assert.Equal(ArtworkType.Cover, artwork.Type);
            Assert.Equal(Path.Combine(artistDirectory, "folder.jpg"), artwork.LocalPath);
        }
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
