#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Common.DataAccess.Repositories.MusicLibrary;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Services;
using Lumina.Infrastructure.Core.MediaLibrary.Management.Scanning.Jobs.Audio.Music;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Infrastructure.UnitTests.Core.MediaLibrary.Management.Scanning.Jobs.Audio.Music;

/// <summary>
/// Contains unit tests for the <see cref="MusicMediaLibraryScanItemMaterializer"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MusicMediaLibraryScanItemMaterializerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly IArtistRepository _mockArtistRepository;
    private readonly IAlbumRepository _mockAlbumRepository;
    private readonly ITrackRepository _mockTrackRepository;
    private readonly IMusicLibraryScanItemMetadataRepository _mockStagedMetadataRepository;
    private readonly IPathService _mockPathService;
    private readonly MusicMediaLibraryScanItemMaterializer _sut;
    private readonly MusicLibraryScanItemMetadataEntityFixture _musicLibraryScanItemMetadataEntityFixture = new();
    private readonly List<ArtistEntity> _insertedArtists = [];
    private readonly Guid _libraryId = Guid.NewGuid();
    private readonly Guid _scanId = Guid.NewGuid();

    /// <summary>
    /// Initializes a new instance of the <see cref="MusicMediaLibraryScanItemMaterializerTests"/> class.
    /// </summary>
    public MusicMediaLibraryScanItemMaterializerTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockArtistRepository = Substitute.For<IArtistRepository>();
        _mockAlbumRepository = Substitute.For<IAlbumRepository>();
        _mockTrackRepository = Substitute.For<ITrackRepository>();
        _mockStagedMetadataRepository = Substitute.For<IMusicLibraryScanItemMetadataRepository>();
        _mockUnitOfWork.ArtistRepository.Returns(_mockArtistRepository);
        _mockUnitOfWork.AlbumRepository.Returns(_mockAlbumRepository);
        _mockUnitOfWork.TrackRepository.Returns(_mockTrackRepository);
        _mockUnitOfWork.MusicLibraryScanItemMetadataRepository.Returns(_mockStagedMetadataRepository);

        _mockArtistRepository.GetByNameAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<ArtistEntity?>(null));
        _mockAlbumRepository.GetByTitleAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<AlbumEntity?>(null));
        _mockArtistRepository.InsertAsync(Arg.Do<ArtistEntity>(artist => _insertedArtists.Add(artist)), Arg.Any<CancellationToken>())
            .Returns(Result.From(new Created()));
        _mockAlbumRepository.InsertAsync(Arg.Any<AlbumEntity>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(new Created()));
        _mockTrackRepository.InsertAsync(Arg.Any<TrackEntity>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(new Created()));
        _mockTrackRepository.GetExistingPathsAsync(Arg.Any<Guid>(), Arg.Any<IReadOnlyCollection<string>>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyCollection<string>>([]));
        _mockStagedMetadataRepository.DeleteByScanIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(new Deleted()));

        _mockPathService = Substitute.For<IPathService>();
        _mockPathService.PathSeparator.Returns('/');
        _mockPathService.GetFileNameWithoutExtension(Arg.Any<string>()).Returns("track");

        _sut = new MusicMediaLibraryScanItemMaterializer(new MusicLibraryPathStructure(_mockPathService), _mockPathService);
    }

    [Fact]
    public async Task MaterializeItemsAsync_WhenTheTracksLiveInDifferentDirectories_ShouldMaterializeOneAlbumPerDirectory()
    {
        // Arrange
        Guid releaseArtistId = Guid.NewGuid();
        Guid firstReleaseId = Guid.NewGuid();
        Guid secondReleaseId = Guid.NewGuid();
        Guid firstRecordingId = Guid.NewGuid();
        Guid secondRecordingId = Guid.NewGuid();
        MusicLibraryScanItemMetadataEntity firstStagedItem = _musicLibraryScanItemMetadataEntityFixture.Create(
            libraryId: _libraryId,
            artistName: "Shared Artist Name",
            releaseName: "Shared Release Title",
            path: "/music/shared-artist/release-one/first.mp3",
            musicBrainzArtistId: Guid.NewGuid(),
            musicBrainzReleaseArtistId: releaseArtistId,
            musicBrainzReleaseId: firstReleaseId,
            musicBrainzRecordingId: firstRecordingId);
        MusicLibraryScanItemMetadataEntity secondStagedItem = _musicLibraryScanItemMetadataEntityFixture.Create(
            libraryId: _libraryId,
            artistName: "Shared Artist Name",
            releaseName: "Shared Release Title",
            path: "/music/shared-artist/release-two/second.mp3",
            musicBrainzArtistId: Guid.NewGuid(),
            musicBrainzReleaseArtistId: releaseArtistId,
            musicBrainzReleaseId: secondReleaseId,
            musicBrainzRecordingId: secondRecordingId);
        _mockStagedMetadataRepository.GetByScanIdAsync(_scanId, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<MusicLibraryScanItemMetadataEntity>>([firstStagedItem, secondStagedItem]));

        // Act
        Result<Success> result = await _sut.MaterializeItemsAsync(_mockUnitOfWork, _libraryId, _scanId, [firstStagedItem.Path, secondStagedItem.Path], CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        // The same artist name groups the items under a single artist, whose identifier is the one of the release artist.
        ArtistEntity artist = Assert.Single(_insertedArtists);
        Assert.Equal(releaseArtistId, artist.MusicBrainzArtistId);
        // The distinct release identifiers keep the identically titled releases apart.
        Assert.Equal(2, artist.Albums.Count);
        AlbumEntity firstAlbum = artist.Albums.Single(album => album.MusicBrainzReleaseId == firstReleaseId);
        AlbumEntity secondAlbum = artist.Albums.Single(album => album.MusicBrainzReleaseId == secondReleaseId);
        Assert.Equal(firstRecordingId, Assert.Single(firstAlbum.Tracks).MusicBrainzRecordingId);
        Assert.Equal(secondRecordingId, Assert.Single(secondAlbum.Tracks).MusicBrainzRecordingId);
    }

    [Fact]
    public async Task MaterializeItemsAsync_WhenCompilationTracksHaveDifferentTrackArtists_ShouldMaterializeASingleAlbumArtist()
    {
        // Arrange
        Guid variousArtistsId = Guid.NewGuid();
        Guid compilationReleaseId = Guid.NewGuid();
        MusicLibraryScanItemMetadataEntity firstStagedItem = _musicLibraryScanItemMetadataEntityFixture.Create(
            libraryId: _libraryId,
            artistName: "Various Artists",
            releaseName: "Now That Is Music",
            path: "/music/various-artists/now-that-is-music/first.mp3",
            musicBrainzArtistId: Guid.NewGuid(),
            musicBrainzReleaseArtistId: variousArtistsId,
            musicBrainzReleaseId: compilationReleaseId);
        MusicLibraryScanItemMetadataEntity secondStagedItem = _musicLibraryScanItemMetadataEntityFixture.Create(
            libraryId: _libraryId,
            artistName: "Various Artists",
            releaseName: "Now That Is Music",
            path: "/music/various-artists/now-that-is-music/second.mp3",
            musicBrainzArtistId: Guid.NewGuid(),
            musicBrainzReleaseArtistId: variousArtistsId,
            musicBrainzReleaseId: compilationReleaseId);
        _mockStagedMetadataRepository.GetByScanIdAsync(_scanId, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<MusicLibraryScanItemMetadataEntity>>([firstStagedItem, secondStagedItem]));

        // Act
        Result<Success> result = await _sut.MaterializeItemsAsync(_mockUnitOfWork, _libraryId, _scanId, [firstStagedItem.Path, secondStagedItem.Path], CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        // The different track artists of the compilation do not split the release artist into several identically named artists.
        ArtistEntity artist = Assert.Single(_insertedArtists);
        Assert.Equal("Various Artists", artist.Name);
        Assert.Equal(variousArtistsId, artist.MusicBrainzArtistId);
        AlbumEntity album = Assert.Single(artist.Albums);
        Assert.Equal(2, album.Tracks.Count);
    }

    [Fact]
    public async Task MaterializeItemsAsync_WhenTheTagsDoNotCarryMusicBrainzIds_ShouldGroupByTheNameAndTitle()
    {
        // Arrange
        MusicLibraryScanItemMetadataEntity firstStagedItem = _musicLibraryScanItemMetadataEntityFixture.Create(
            libraryId: _libraryId,
            artistName: "Shared Artist Name",
            releaseName: "Shared Release Title",
            path: "/music/shared-artist/shared-release/first.mp3",
            includeMusicBrainzTags: false);
        MusicLibraryScanItemMetadataEntity secondStagedItem = _musicLibraryScanItemMetadataEntityFixture.Create(
            libraryId: _libraryId,
            artistName: "Shared Artist Name",
            releaseName: "Shared Release Title",
            path: "/music/shared-artist/shared-release/second.mp3",
            includeMusicBrainzTags: false);
        _mockStagedMetadataRepository.GetByScanIdAsync(_scanId, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<MusicLibraryScanItemMetadataEntity>>([firstStagedItem, secondStagedItem]));

        // Act
        Result<Success> result = await _sut.MaterializeItemsAsync(_mockUnitOfWork, _libraryId, _scanId, [firstStagedItem.Path, secondStagedItem.Path], CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        // Without identifiers, the items fall back to the name and the title, so they collapse into a single artist with a single album.
        ArtistEntity artist = Assert.Single(_insertedArtists);
        AlbumEntity album = Assert.Single(artist.Albums);
        Assert.Null(artist.MusicBrainzArtistId);
        Assert.Null(album.MusicBrainzReleaseId);
        Assert.Equal(2, album.Tracks.Count);
    }
}
