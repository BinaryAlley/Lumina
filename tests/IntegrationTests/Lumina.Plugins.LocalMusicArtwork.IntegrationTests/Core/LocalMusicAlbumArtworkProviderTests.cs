#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.Common;
using Lumina.Contracts.DTO.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Plugins.LocalMusicArtwork.Core;
using Lumina.Plugins.LocalMusicArtwork.Core.Settings;
using Lumina.Plugins.LocalMusicArtwork.Fixtures.Common.Models.DTO.Settings;
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
/// Contains integration tests for the <see cref="LocalMusicAlbumArtworkProvider"/> class, reading real image and audio files from the file system.
/// </summary>
[ExcludeFromCodeCoverage]
public class LocalMusicAlbumArtworkProviderTests : IDisposable
{
    private static readonly byte[] s_embeddedCoverBytes = [0xFF, 0xD8, 0xFF, 0xE0, 0x01, 0x02, 0x03, 0xFF, 0xD9];

    private readonly AlbumMetadataLookupDtoFixture _albumMetadataLookupDtoFixture = new();
    private readonly LocalMusicArtworkSettingsDtoFixture _localMusicArtworkSettingsDtoFixture = new();
    private readonly string _temporaryDirectory;

    /// <summary>
    /// Initializes a new instance of the <see cref="LocalMusicAlbumArtworkProviderTests"/> class.
    /// </summary>
    public LocalMusicAlbumArtworkProviderTests()
    {
        _temporaryDirectory = Path.Combine(Path.GetTempPath(), $"lumina-local-music-album-artwork-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_temporaryDirectory);
    }

    [Fact]
    public void Name_WhenCalled_ShouldReturnTheProviderDisplayName()
    {
        // Arrange
        LocalMusicAlbumArtworkProvider sut = CreateProvider(shouldExtractEmbeddedCover: true);

        // Act
        string result = sut.Name;

        // Assert
        Assert.Equal("Local Music Artwork", result);
    }

    [Fact]
    public void SupportedLibraryTypes_WhenCalled_ShouldReturnMusic()
    {
        // Arrange
        LocalMusicAlbumArtworkProvider sut = CreateProvider(shouldExtractEmbeddedCover: true);

        // Act
        IReadOnlyList<LibraryType> result = sut.SupportedLibraryTypes;

        // Assert
        Assert.Equal([LibraryType.Music], result);
    }

    [Fact]
    public void RequiresWebAccess_WhenCalled_ShouldReturnFalse()
    {
        // Arrange
        LocalMusicAlbumArtworkProvider sut = CreateProvider(shouldExtractEmbeddedCover: true);

        // Act
        bool result = sut.RequiresWebAccess;

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task GetArtworkAsync_WhenTheAlbumHasDiskImages_ShouldReturnTheDiskArtwork()
    {
        // Arrange
        string albumDirectory = Path.Combine(_temporaryDirectory, "Album");
        TestImageFileFactory.CreateJpeg(Path.Combine(albumDirectory, "cover.jpg"));
        TestImageFileFactory.CreatePng(Path.Combine(albumDirectory, "back.png"));
        TestAudioFileFactory.CreateWav(Path.Combine(albumDirectory, "track.wav"));
        string trackPath = Path.Combine(albumDirectory, "track.wav");
        LocalMusicAlbumArtworkProvider sut = CreateProvider(shouldExtractEmbeddedCover: true);
        AlbumMetadataLookupDto lookup = _albumMetadataLookupDtoFixture.Create(path: trackPath);

        // Act
        IReadOnlyList<ArtworkDto> result = await sut.GetArtworkAsync(lookup, CancellationToken.None);

        // Assert
        List<ArtworkDto> expected =
        [
            new(ArtworkType.Cover, 0, Path.Combine(albumDirectory, "cover.jpg"), null),
            new(ArtworkType.Back, 0, Path.Combine(albumDirectory, "back.png"), null)
        ];
        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task GetArtworkAsync_WhenTheAlbumHasACoverOnDisk_ShouldNotExtractTheEmbeddedCover()
    {
        // Arrange
        string albumDirectory = Path.Combine(_temporaryDirectory, "Album");
        TestImageFileFactory.CreateJpeg(Path.Combine(albumDirectory, "cover.jpg"));
        TestAudioFileFactory.CreateWavWithEmbeddedCover(Path.Combine(albumDirectory, "track.wav"), s_embeddedCoverBytes);
        string trackPath = Path.Combine(albumDirectory, "track.wav");
        LocalMusicAlbumArtworkProvider sut = CreateProvider(shouldExtractEmbeddedCover: true);
        AlbumMetadataLookupDto lookup = _albumMetadataLookupDtoFixture.Create(path: trackPath);

        // Act
        IReadOnlyList<ArtworkDto> result = await sut.GetArtworkAsync(lookup, CancellationToken.None);

        // Assert
        ArtworkDto artwork = Assert.Single(result);
        Assert.Equal(ArtworkType.Cover, artwork.Type);
        Assert.Equal(0, artwork.Ordinal);
        Assert.Equal(Path.Combine(albumDirectory, "cover.jpg"), artwork.LocalPath);
        Assert.Null(artwork.RemoteUrl);
        Assert.False(artwork.IsTemporary);
    }

    [Fact]
    public async Task GetArtworkAsync_WhenTheAlbumHasNoCoverOnDiskAndExtractionIsEnabled_ShouldReturnTheEmbeddedCover()
    {
        // Arrange
        string albumDirectory = Path.Combine(_temporaryDirectory, "Album");
        TestImageFileFactory.CreatePng(Path.Combine(albumDirectory, "back.png"));
        TestAudioFileFactory.CreateWavWithEmbeddedCover(Path.Combine(albumDirectory, "track.wav"), s_embeddedCoverBytes);
        string trackPath = Path.Combine(albumDirectory, "track.wav");
        LocalMusicAlbumArtworkProvider sut = CreateProvider(shouldExtractEmbeddedCover: true);
        AlbumMetadataLookupDto lookup = _albumMetadataLookupDtoFixture.Create(path: trackPath);

        // Act
        IReadOnlyList<ArtworkDto> result = await sut.GetArtworkAsync(lookup, CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal(ArtworkType.Back, result[0].Type);
        Assert.Equal(0, result[0].Ordinal);
        Assert.Equal(Path.Combine(albumDirectory, "back.png"), result[0].LocalPath);
        Assert.Null(result[0].RemoteUrl);
        Assert.False(result[0].IsTemporary);
        Assert.Equal(ArtworkType.Cover, result[1].Type);
        Assert.Equal(0, result[1].Ordinal);
        Assert.NotNull(result[1].LocalPath);
        Assert.EndsWith(".jpg", result[1].LocalPath);
        Assert.True(File.Exists(result[1].LocalPath));
        Assert.Equal(s_embeddedCoverBytes, File.ReadAllBytes(result[1].LocalPath!));
        Assert.Null(result[1].RemoteUrl);
        Assert.True(result[1].IsTemporary);
        File.Delete(result[1].LocalPath!);
    }

    [Fact]
    public async Task GetArtworkAsync_WhenTheAlbumHasNoCoverOnDiskAndExtractionIsDisabled_ShouldReturnOnlyTheDiskArtwork()
    {
        // Arrange
        string albumDirectory = Path.Combine(_temporaryDirectory, "Album");
        TestImageFileFactory.CreatePng(Path.Combine(albumDirectory, "back.png"));
        TestAudioFileFactory.CreateWavWithEmbeddedCover(Path.Combine(albumDirectory, "track.wav"), s_embeddedCoverBytes);
        string trackPath = Path.Combine(albumDirectory, "track.wav");
        LocalMusicAlbumArtworkProvider sut = CreateProvider(shouldExtractEmbeddedCover: false);
        AlbumMetadataLookupDto lookup = _albumMetadataLookupDtoFixture.Create(path: trackPath);

        // Act
        IReadOnlyList<ArtworkDto> result = await sut.GetArtworkAsync(lookup, CancellationToken.None);

        // Assert
        ArtworkDto artwork = Assert.Single(result);
        Assert.Equal(ArtworkType.Back, artwork.Type);
        Assert.Equal(0, artwork.Ordinal);
        Assert.Equal(Path.Combine(albumDirectory, "back.png"), artwork.LocalPath);
        Assert.Null(artwork.RemoteUrl);
        Assert.False(artwork.IsTemporary);
    }

    [Fact]
    public async Task GetArtworkAsync_WhenTheAlbumHasNoCoverOnDiskAndTheTrackHasNoEmbeddedCover_ShouldReturnOnlyTheDiskArtwork()
    {
        // Arrange
        string albumDirectory = Path.Combine(_temporaryDirectory, "Album");
        TestImageFileFactory.CreatePng(Path.Combine(albumDirectory, "back.png"));
        TestAudioFileFactory.CreateWav(Path.Combine(albumDirectory, "track.wav"));
        string trackPath = Path.Combine(albumDirectory, "track.wav");
        LocalMusicAlbumArtworkProvider sut = CreateProvider(shouldExtractEmbeddedCover: true);
        AlbumMetadataLookupDto lookup = _albumMetadataLookupDtoFixture.Create(path: trackPath);

        // Act
        IReadOnlyList<ArtworkDto> result = await sut.GetArtworkAsync(lookup, CancellationToken.None);

        // Assert
        ArtworkDto artwork = Assert.Single(result);
        Assert.Equal(ArtworkType.Back, artwork.Type);
        Assert.Equal(Path.Combine(albumDirectory, "back.png"), artwork.LocalPath);
    }

    [Fact]
    public async Task GetArtworkAsync_WhenTheTrackHasNoImagesAtAll_ShouldReturnEmpty()
    {
        // Arrange
        string albumDirectory = Path.Combine(_temporaryDirectory, "Album");
        TestAudioFileFactory.CreateWav(Path.Combine(albumDirectory, "track.wav"));
        string trackPath = Path.Combine(albumDirectory, "track.wav");
        LocalMusicAlbumArtworkProvider sut = CreateProvider(shouldExtractEmbeddedCover: true);
        AlbumMetadataLookupDto lookup = _albumMetadataLookupDtoFixture.Create(path: trackPath);

        // Act
        IReadOnlyList<ArtworkDto> result = await sut.GetArtworkAsync(lookup, CancellationToken.None);

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
        LocalMusicAlbumArtworkProvider sut = CreateProvider(shouldExtractEmbeddedCover: true);
        AlbumMetadataLookupDto lookup = _albumMetadataLookupDtoFixture.Create(path: trackPath);

        // Act
        IReadOnlyList<ArtworkDto> result = await sut.GetArtworkAsync(lookup, CancellationToken.None);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetArtworkAsync_WhenTheTrackDirectoryDoesNotExist_ShouldReturnEmpty()
    {
        // Arrange
        string trackPath = Path.Combine(_temporaryDirectory, "Missing", "Album", "track.wav");
        LocalMusicAlbumArtworkProvider sut = CreateProvider(shouldExtractEmbeddedCover: true);
        AlbumMetadataLookupDto lookup = _albumMetadataLookupDtoFixture.Create(path: trackPath);

        // Act
        IReadOnlyList<ArtworkDto> result = await sut.GetArtworkAsync(lookup, CancellationToken.None);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetArtworkAsync_WhenTheAlbumHasNoCoverOnDiskAndTheCancellationTokenIsAlreadyCancelled_ShouldCancelTheOperation()
    {
        // Arrange
        using (CancellationTokenSource cancellationTokenSource = new())
        {
            cancellationTokenSource.Cancel();
            string albumDirectory = Path.Combine(_temporaryDirectory, "Album");
            TestImageFileFactory.CreatePng(Path.Combine(albumDirectory, "back.png"));
            TestAudioFileFactory.CreateWav(Path.Combine(albumDirectory, "track.wav"));
            string trackPath = Path.Combine(albumDirectory, "track.wav");
            LocalMusicAlbumArtworkProvider sut = CreateProvider(shouldExtractEmbeddedCover: true);
            AlbumMetadataLookupDto lookup = _albumMetadataLookupDtoFixture.Create(path: trackPath);

            // Act
            Task Act()
            {
                return sut.GetArtworkAsync(lookup, cancellationTokenSource.Token);
            }

            // Assert
            await Assert.ThrowsAnyAsync<OperationCanceledException>(Act);
        }
    }

    /// <summary>
    /// Creates a <see cref="LocalMusicAlbumArtworkProvider"/> whose settings provider carries the provided embedded cover extraction setting.
    /// </summary>
    /// <param name="shouldExtractEmbeddedCover">Whether the cover embedded in the audio files is extracted when the album has no cover image on disk.</param>
    /// <returns>The created provider.</returns>
    private LocalMusicAlbumArtworkProvider CreateProvider(bool shouldExtractEmbeddedCover)
    {
        LocalMusicArtworkSettingsProvider settingsProvider = new(
            null,
            Guid.NewGuid(),
            _localMusicArtworkSettingsDtoFixture.Create(shouldExtractEmbeddedCover: shouldExtractEmbeddedCover));
        return new LocalMusicAlbumArtworkProvider(settingsProvider);
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
