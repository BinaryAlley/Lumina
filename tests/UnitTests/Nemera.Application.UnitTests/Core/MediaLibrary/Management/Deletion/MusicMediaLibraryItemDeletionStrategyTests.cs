#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Nemera.Application.Common.DataAccess.Repositories.MusicLibrary;
using Nemera.Application.Common.DataAccess.UoW;
using Nemera.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artwork;
using Nemera.Application.Core.MediaLibrary.Management.Deletion;
using Nemera.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Nemera.Domain.Common.Errors;
using Nemera.Domain.Common.Primitives;
using Nemera.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using Nemera.Domain.Fixtures.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using Nemera.Domain.SharedKernel.Common.Enums.AudioLibrary;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Nemera.Application.UnitTests.Core.MediaLibrary.Management.Deletion;

/// <summary>
/// Contains unit tests for the <see cref="MusicMediaLibraryItemDeletionStrategy"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MusicMediaLibraryItemDeletionStrategyTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly ITrackRepository _mockTrackRepository;
    private readonly IAlbumRepository _mockAlbumRepository;
    private readonly IArtistRepository _mockArtistRepository;
    private readonly IMusicArtworkRepository _mockMusicArtworkRepository;
    private readonly IMusicArtworkService _mockMusicArtworkService;
    private readonly ILogger<MusicMediaLibraryItemDeletionStrategy> _mockLogger;
    private readonly MusicMediaLibraryItemDeletionStrategy _sut;
    private readonly LibraryIdFixture _libraryIdFixture = new();
    private readonly TrackEntityFixture _trackEntityFixture = new();
    private readonly AlbumEntityFixture _albumEntityFixture = new();
    private readonly MusicArtworkEntityFixture _musicArtworkEntityFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="MusicMediaLibraryItemDeletionStrategyTests"/> class.
    /// </summary>
    public MusicMediaLibraryItemDeletionStrategyTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockTrackRepository = Substitute.For<ITrackRepository>();
        _mockAlbumRepository = Substitute.For<IAlbumRepository>();
        _mockArtistRepository = Substitute.For<IArtistRepository>();
        _mockMusicArtworkRepository = Substitute.For<IMusicArtworkRepository>();
        _mockMusicArtworkService = Substitute.For<IMusicArtworkService>();
        _mockLogger = Substitute.For<ILogger<MusicMediaLibraryItemDeletionStrategy>>();
        _mockLogger.IsEnabled(Arg.Any<LogLevel>()).Returns(true);
        _mockUnitOfWork.TrackRepository.Returns(_mockTrackRepository);
        _mockUnitOfWork.AlbumRepository.Returns(_mockAlbumRepository);
        _mockUnitOfWork.ArtistRepository.Returns(_mockArtistRepository);
        _mockUnitOfWork.MusicArtworkRepository.Returns(_mockMusicArtworkRepository);
        _mockMusicArtworkRepository.GetByOwnerAsync(Arg.Any<MusicArtworkOwnerType>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<MusicArtworkEntity>>([]));
        _mockMusicArtworkService.DeleteStoredArtwork(Arg.Any<string>())
            .Returns(Result.Deleted);

        _sut = new MusicMediaLibraryItemDeletionStrategy(_mockUnitOfWork, _mockMusicArtworkService, _mockLogger);
    }

    [Fact]
    public async Task DeleteItemAsync_WhenAlbumHasOtherTracks_ShouldDeleteOnlyTheTrackAndItsArtwork()
    {
        // Arrange
        LibraryId libraryId = _libraryIdFixture.Create();
        Guid albumId = Guid.NewGuid();
        TrackEntity track = _trackEntityFixture.Create(albumId: albumId, path: "/music/deleted.flac");
        AlbumEntity album = _albumEntityFixture.Create(id: albumId, includeTracks: false);
        TrackEntity otherTrack = _trackEntityFixture.Create(albumId: albumId);
        _mockTrackRepository.GetByPathAsync(libraryId.Value, track.Path, Arg.Any<CancellationToken>())
            .Returns(Result.From<TrackEntity?>(track));
        _mockMusicArtworkRepository.DeleteByOwnerAsync(MusicArtworkOwnerType.Track, track.Id, Arg.Any<CancellationToken>())
            .Returns(Result.Deleted);
        _mockTrackRepository.DeleteByIdAsync(track.Id, Arg.Any<CancellationToken>())
            .Returns(Result.From(Result.Deleted));
        _mockAlbumRepository.GetIncludingDeletedByIdAsync(albumId, Arg.Any<CancellationToken>())
            .Returns(Result.From<AlbumEntity?>(album));
        _mockTrackRepository.GetByAlbumIdIncludingDeletedAsync(albumId, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<TrackEntity>>([otherTrack]));
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Result.Success);

        // Act
        Result<Success> result = await _sut.DeleteItemAsync(libraryId.Value, track.Path, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        await _mockMusicArtworkRepository.Received(1).DeleteByOwnerAsync(MusicArtworkOwnerType.Track, track.Id, Arg.Any<CancellationToken>());
        await _mockTrackRepository.Received(1).DeleteByIdAsync(track.Id, Arg.Any<CancellationToken>());
        // The album and its remaining tracks are read including the soft deleted ones, so a hidden track still keeps the album alive.
        await _mockAlbumRepository.Received(1).GetIncludingDeletedByIdAsync(albumId, Arg.Any<CancellationToken>());
        await _mockTrackRepository.Received(1).GetByAlbumIdIncludingDeletedAsync(albumId, Arg.Any<CancellationToken>());
        await _mockAlbumRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
        await _mockTrackRepository.DidNotReceive().GetByAlbumIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockMusicArtworkRepository.DidNotReceive().DeleteByOwnerAsync(MusicArtworkOwnerType.Album, Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockAlbumRepository.DidNotReceive().DeleteByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockArtistRepository.DidNotReceive().DeleteByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteItemAsync_WhenAlbumHasNoOtherTracksButArtistHasOtherAlbums_ShouldDeleteTheAlbumAndItsArtwork()
    {
        // Arrange
        LibraryId libraryId = _libraryIdFixture.Create();
        Guid albumId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        TrackEntity track = _trackEntityFixture.Create(albumId: albumId, path: "/music/deleted.flac");
        AlbumEntity album = _albumEntityFixture.Create(id: albumId, artistId: artistId, includeTracks: false);
        AlbumEntity otherAlbum = _albumEntityFixture.Create(artistId: artistId, includeTracks: false);
        _mockTrackRepository.GetByPathAsync(libraryId.Value, track.Path, Arg.Any<CancellationToken>())
            .Returns(Result.From<TrackEntity?>(track));
        _mockMusicArtworkRepository.DeleteByOwnerAsync(MusicArtworkOwnerType.Track, track.Id, Arg.Any<CancellationToken>())
            .Returns(Result.Deleted);
        _mockTrackRepository.DeleteByIdAsync(track.Id, Arg.Any<CancellationToken>())
            .Returns(Result.From(Result.Deleted));
        _mockAlbumRepository.GetIncludingDeletedByIdAsync(albumId, Arg.Any<CancellationToken>())
            .Returns(Result.From<AlbumEntity?>(album));
        _mockTrackRepository.GetByAlbumIdIncludingDeletedAsync(albumId, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<TrackEntity>>([track]));
        _mockMusicArtworkRepository.DeleteByOwnerAsync(MusicArtworkOwnerType.Album, albumId, Arg.Any<CancellationToken>())
            .Returns(Result.Deleted);
        _mockAlbumRepository.DeleteByIdAsync(albumId, Arg.Any<CancellationToken>())
            .Returns(Result.From(Result.Deleted));
        _mockAlbumRepository.GetByArtistIdIncludingDeletedAsync(artistId, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<AlbumEntity>>([otherAlbum]));
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Result.Success);

        // Act
        Result<Success> result = await _sut.DeleteItemAsync(libraryId.Value, track.Path, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        await _mockMusicArtworkRepository.Received(1).DeleteByOwnerAsync(MusicArtworkOwnerType.Album, albumId, Arg.Any<CancellationToken>());
        await _mockAlbumRepository.Received(1).DeleteByIdAsync(albumId, Arg.Any<CancellationToken>());
        // The remaining albums of the artist are read including the soft deleted ones, so a hidden album still keeps the artist alive.
        await _mockAlbumRepository.Received(1).GetByArtistIdIncludingDeletedAsync(artistId, Arg.Any<CancellationToken>());
        await _mockAlbumRepository.DidNotReceive().GetByArtistIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockMusicArtworkRepository.DidNotReceive().DeleteByOwnerAsync(MusicArtworkOwnerType.Artist, Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockArtistRepository.DidNotReceive().DeleteByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteItemAsync_WhenAlbumAndArtistAreLeftWithNoChildren_ShouldDeleteTheTrackAlbumAndArtist()
    {
        // Arrange
        LibraryId libraryId = _libraryIdFixture.Create();
        Guid albumId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        TrackEntity track = _trackEntityFixture.Create(albumId: albumId, path: "/music/deleted.flac");
        AlbumEntity album = _albumEntityFixture.Create(id: albumId, artistId: artistId, includeTracks: false);
        _mockTrackRepository.GetByPathAsync(libraryId.Value, track.Path, Arg.Any<CancellationToken>())
            .Returns(Result.From<TrackEntity?>(track));
        _mockMusicArtworkRepository.DeleteByOwnerAsync(MusicArtworkOwnerType.Track, track.Id, Arg.Any<CancellationToken>())
            .Returns(Result.Deleted);
        _mockTrackRepository.DeleteByIdAsync(track.Id, Arg.Any<CancellationToken>())
            .Returns(Result.From(Result.Deleted));
        _mockAlbumRepository.GetIncludingDeletedByIdAsync(albumId, Arg.Any<CancellationToken>())
            .Returns(Result.From<AlbumEntity?>(album));
        _mockTrackRepository.GetByAlbumIdIncludingDeletedAsync(albumId, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<TrackEntity>>([track]));
        _mockMusicArtworkRepository.DeleteByOwnerAsync(MusicArtworkOwnerType.Album, albumId, Arg.Any<CancellationToken>())
            .Returns(Result.Deleted);
        _mockAlbumRepository.DeleteByIdAsync(albumId, Arg.Any<CancellationToken>())
            .Returns(Result.From(Result.Deleted));
        _mockAlbumRepository.GetByArtistIdIncludingDeletedAsync(artistId, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<AlbumEntity>>([album]));
        _mockMusicArtworkRepository.DeleteByOwnerAsync(MusicArtworkOwnerType.Artist, artistId, Arg.Any<CancellationToken>())
            .Returns(Result.Deleted);
        _mockArtistRepository.DeleteByIdAsync(artistId, Arg.Any<CancellationToken>())
            .Returns(Result.From(Result.Deleted));
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Result.Success);

        // Act
        Result<Success> result = await _sut.DeleteItemAsync(libraryId.Value, track.Path, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        await _mockMusicArtworkRepository.Received(1).DeleteByOwnerAsync(MusicArtworkOwnerType.Album, albumId, Arg.Any<CancellationToken>());
        await _mockMusicArtworkRepository.Received(1).DeleteByOwnerAsync(MusicArtworkOwnerType.Artist, artistId, Arg.Any<CancellationToken>());
        await _mockAlbumRepository.Received(1).DeleteByIdAsync(albumId, Arg.Any<CancellationToken>());
        await _mockArtistRepository.Received(1).DeleteByIdAsync(artistId, Arg.Any<CancellationToken>());
        // Every scoping read includes the soft deleted rows, so hidden siblings are never missed when deciding what to remove.
        await _mockAlbumRepository.Received(1).GetIncludingDeletedByIdAsync(albumId, Arg.Any<CancellationToken>());
        await _mockTrackRepository.Received(1).GetByAlbumIdIncludingDeletedAsync(albumId, Arg.Any<CancellationToken>());
        await _mockAlbumRepository.Received(1).GetByArtistIdIncludingDeletedAsync(artistId, Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteItemAsync_WhenEveryItemHasStoredArtwork_ShouldDeleteEveryStoredFileAndRow()
    {
        // Arrange
        LibraryId libraryId = _libraryIdFixture.Create();
        Guid albumId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        TrackEntity track = _trackEntityFixture.Create(albumId: albumId, path: "/music/deleted.flac");
        AlbumEntity album = _albumEntityFixture.Create(id: albumId, artistId: artistId, includeTracks: false);
        MusicArtworkEntity trackArtwork = _musicArtworkEntityFixture.Create(ownerType: MusicArtworkOwnerType.Track, ownerId: track.Id, fileName: "/media/music/track-cover.jpg");
        MusicArtworkEntity albumArtwork = _musicArtworkEntityFixture.Create(ownerType: MusicArtworkOwnerType.Album, ownerId: albumId, fileName: "/media/music/album-cover.jpg");
        MusicArtworkEntity artistArtwork = _musicArtworkEntityFixture.Create(ownerType: MusicArtworkOwnerType.Artist, ownerId: artistId, fileName: "/media/music/artist-cover.jpg");
        _mockTrackRepository.GetByPathAsync(libraryId.Value, track.Path, Arg.Any<CancellationToken>())
            .Returns(Result.From<TrackEntity?>(track));
        _mockMusicArtworkRepository.GetByOwnerAsync(MusicArtworkOwnerType.Track, track.Id, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<MusicArtworkEntity>>([trackArtwork]));
        _mockMusicArtworkRepository.DeleteByOwnerAsync(MusicArtworkOwnerType.Track, track.Id, Arg.Any<CancellationToken>())
            .Returns(Result.Deleted);
        _mockTrackRepository.DeleteByIdAsync(track.Id, Arg.Any<CancellationToken>())
            .Returns(Result.From(Result.Deleted));
        _mockAlbumRepository.GetIncludingDeletedByIdAsync(albumId, Arg.Any<CancellationToken>())
            .Returns(Result.From<AlbumEntity?>(album));
        _mockTrackRepository.GetByAlbumIdIncludingDeletedAsync(albumId, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<TrackEntity>>([track]));
        _mockMusicArtworkRepository.GetByOwnerAsync(MusicArtworkOwnerType.Album, albumId, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<MusicArtworkEntity>>([albumArtwork]));
        _mockMusicArtworkRepository.DeleteByOwnerAsync(MusicArtworkOwnerType.Album, albumId, Arg.Any<CancellationToken>())
            .Returns(Result.Deleted);
        _mockAlbumRepository.DeleteByIdAsync(albumId, Arg.Any<CancellationToken>())
            .Returns(Result.From(Result.Deleted));
        _mockAlbumRepository.GetByArtistIdIncludingDeletedAsync(artistId, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<AlbumEntity>>([album]));
        _mockMusicArtworkRepository.GetByOwnerAsync(MusicArtworkOwnerType.Artist, artistId, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<MusicArtworkEntity>>([artistArtwork]));
        _mockMusicArtworkRepository.DeleteByOwnerAsync(MusicArtworkOwnerType.Artist, artistId, Arg.Any<CancellationToken>())
            .Returns(Result.Deleted);
        _mockArtistRepository.DeleteByIdAsync(artistId, Arg.Any<CancellationToken>())
            .Returns(Result.From(Result.Deleted));
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Result.Success);

        // Act
        Result<Success> result = await _sut.DeleteItemAsync(libraryId.Value, track.Path, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        _mockMusicArtworkService.Received(1).DeleteStoredArtwork(trackArtwork.FileName!);
        _mockMusicArtworkService.Received(1).DeleteStoredArtwork(albumArtwork.FileName!);
        _mockMusicArtworkService.Received(1).DeleteStoredArtwork(artistArtwork.FileName!);
        await _mockMusicArtworkRepository.Received(1).DeleteByOwnerAsync(MusicArtworkOwnerType.Track, track.Id, Arg.Any<CancellationToken>());
        await _mockMusicArtworkRepository.Received(1).DeleteByOwnerAsync(MusicArtworkOwnerType.Album, albumId, Arg.Any<CancellationToken>());
        await _mockMusicArtworkRepository.Received(1).DeleteByOwnerAsync(MusicArtworkOwnerType.Artist, artistId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteItemAsync_WhenDeletingAStoredArtworkFileFails_ShouldLogAWarningAndStillDeleteTheRow()
    {
        // Arrange
        LibraryId libraryId = _libraryIdFixture.Create();
        Guid albumId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        TrackEntity track = _trackEntityFixture.Create(albumId: albumId, path: "/music/deleted.flac");
        AlbumEntity album = _albumEntityFixture.Create(id: albumId, artistId: artistId, includeTracks: false);
        AlbumEntity otherAlbum = _albumEntityFixture.Create(artistId: artistId, includeTracks: false);
        MusicArtworkEntity trackArtwork = _musicArtworkEntityFixture.Create(ownerType: MusicArtworkOwnerType.Track, ownerId: track.Id, fileName: "/media/music/track-cover.jpg");
        _mockTrackRepository.GetByPathAsync(libraryId.Value, track.Path, Arg.Any<CancellationToken>())
            .Returns(Result.From<TrackEntity?>(track));
        _mockMusicArtworkRepository.GetByOwnerAsync(MusicArtworkOwnerType.Track, track.Id, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<MusicArtworkEntity>>([trackArtwork]));
        _mockMusicArtworkService.DeleteStoredArtwork(trackArtwork.FileName!)
            .Returns(Errors.FileSystemManagement.FileNotFound);
        _mockMusicArtworkRepository.DeleteByOwnerAsync(MusicArtworkOwnerType.Track, track.Id, Arg.Any<CancellationToken>())
            .Returns(Result.Deleted);
        _mockTrackRepository.DeleteByIdAsync(track.Id, Arg.Any<CancellationToken>())
            .Returns(Result.From(Result.Deleted));
        _mockAlbumRepository.GetIncludingDeletedByIdAsync(albumId, Arg.Any<CancellationToken>())
            .Returns(Result.From<AlbumEntity?>(album));
        _mockTrackRepository.GetByAlbumIdIncludingDeletedAsync(albumId, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<TrackEntity>>([track]));
        _mockMusicArtworkRepository.DeleteByOwnerAsync(MusicArtworkOwnerType.Album, albumId, Arg.Any<CancellationToken>())
            .Returns(Result.Deleted);
        _mockAlbumRepository.DeleteByIdAsync(albumId, Arg.Any<CancellationToken>())
            .Returns(Result.From(Result.Deleted));
        _mockAlbumRepository.GetByArtistIdIncludingDeletedAsync(artistId, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<AlbumEntity>>([otherAlbum]));
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Result.Success);

        // Act
        Result<Success> result = await _sut.DeleteItemAsync(libraryId.Value, track.Path, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        await _mockMusicArtworkRepository.Received(1).DeleteByOwnerAsync(MusicArtworkOwnerType.Track, track.Id, Arg.Any<CancellationToken>());
        _mockLogger.Received(1).Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Is<object>(state => state.ToString()!.Contains("Failed to delete the stored")),
            Arg.Any<Exception?>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task DeleteItemAsync_WhenLoggingIsDisabledAndDeletingAStoredArtworkFileFails_ShouldNotLogAWarningButStillDeleteTheRow()
    {
        // Arrange
        _mockLogger.IsEnabled(Arg.Any<LogLevel>()).Returns(false);
        LibraryId libraryId = _libraryIdFixture.Create();
        Guid albumId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        TrackEntity track = _trackEntityFixture.Create(albumId: albumId, path: "/music/deleted.flac");
        AlbumEntity album = _albumEntityFixture.Create(id: albumId, artistId: artistId, includeTracks: false);
        AlbumEntity otherAlbum = _albumEntityFixture.Create(artistId: artistId, includeTracks: false);
        MusicArtworkEntity trackArtwork = _musicArtworkEntityFixture.Create(ownerType: MusicArtworkOwnerType.Track, ownerId: track.Id, fileName: "/media/music/track-cover.jpg");
        _mockTrackRepository.GetByPathAsync(libraryId.Value, track.Path, Arg.Any<CancellationToken>())
            .Returns(Result.From<TrackEntity?>(track));
        _mockMusicArtworkRepository.GetByOwnerAsync(MusicArtworkOwnerType.Track, track.Id, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<MusicArtworkEntity>>([trackArtwork]));
        _mockMusicArtworkService.DeleteStoredArtwork(trackArtwork.FileName!)
            .Returns(Errors.FileSystemManagement.FileNotFound);
        _mockMusicArtworkRepository.DeleteByOwnerAsync(MusicArtworkOwnerType.Track, track.Id, Arg.Any<CancellationToken>())
            .Returns(Result.Deleted);
        _mockTrackRepository.DeleteByIdAsync(track.Id, Arg.Any<CancellationToken>())
            .Returns(Result.From(Result.Deleted));
        _mockAlbumRepository.GetIncludingDeletedByIdAsync(albumId, Arg.Any<CancellationToken>())
            .Returns(Result.From<AlbumEntity?>(album));
        _mockTrackRepository.GetByAlbumIdIncludingDeletedAsync(albumId, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<TrackEntity>>([track]));
        _mockMusicArtworkRepository.DeleteByOwnerAsync(MusicArtworkOwnerType.Album, albumId, Arg.Any<CancellationToken>())
            .Returns(Result.Deleted);
        _mockAlbumRepository.DeleteByIdAsync(albumId, Arg.Any<CancellationToken>())
            .Returns(Result.From(Result.Deleted));
        _mockAlbumRepository.GetByArtistIdIncludingDeletedAsync(artistId, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<AlbumEntity>>([otherAlbum]));
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Result.Success);

        // Act
        Result<Success> result = await _sut.DeleteItemAsync(libraryId.Value, track.Path, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        await _mockMusicArtworkRepository.Received(1).DeleteByOwnerAsync(MusicArtworkOwnerType.Track, track.Id, Arg.Any<CancellationToken>());
        _mockLogger.DidNotReceive().Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            Arg.Any<Exception?>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task DeleteItemAsync_WhenAlbumWasAlreadyRemoved_ShouldPersistTheTrackDeletion()
    {
        // Arrange
        LibraryId libraryId = _libraryIdFixture.Create();
        Guid albumId = Guid.NewGuid();
        TrackEntity track = _trackEntityFixture.Create(albumId: albumId, path: "/music/deleted.flac");
        _mockTrackRepository.GetByPathAsync(libraryId.Value, track.Path, Arg.Any<CancellationToken>())
            .Returns(Result.From<TrackEntity?>(track));
        _mockMusicArtworkRepository.DeleteByOwnerAsync(MusicArtworkOwnerType.Track, track.Id, Arg.Any<CancellationToken>())
            .Returns(Result.Deleted);
        _mockTrackRepository.DeleteByIdAsync(track.Id, Arg.Any<CancellationToken>())
            .Returns(Result.From(Result.Deleted));
        _mockAlbumRepository.GetIncludingDeletedByIdAsync(albumId, Arg.Any<CancellationToken>())
            .Returns(Result.From<AlbumEntity?>(null));
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Result.Success);

        // Act
        Result<Success> result = await _sut.DeleteItemAsync(libraryId.Value, track.Path, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        await _mockTrackRepository.Received(1).DeleteByIdAsync(track.Id, Arg.Any<CancellationToken>());
        // A soft deleted album is still read, because its own tracks have to keep it alive until their files disappear too.
        await _mockAlbumRepository.Received(1).GetIncludingDeletedByIdAsync(albumId, Arg.Any<CancellationToken>());
        await _mockAlbumRepository.DidNotReceive().DeleteByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteItemAsync_WhenNoTrackExistsAtPath_ShouldDoNothing()
    {
        // Arrange
        LibraryId libraryId = _libraryIdFixture.Create();
        _mockTrackRepository.GetByPathAsync(libraryId.Value, "/music/deleted.flac", Arg.Any<CancellationToken>())
            .Returns(Result.From<TrackEntity?>(null));

        // Act
        Result<Success> result = await _sut.DeleteItemAsync(libraryId.Value, "/music/deleted.flac", CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        await _mockMusicArtworkRepository.DidNotReceive().DeleteByOwnerAsync(Arg.Any<MusicArtworkOwnerType>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockTrackRepository.DidNotReceive().DeleteByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteItemAsync_WhenGetTrackFails_ShouldReturnFailure()
    {
        // Arrange
        LibraryId libraryId = _libraryIdFixture.Create();
        Error error = Error.Failure("Database.Error", "Failed to get the track");
        _mockTrackRepository.GetByPathAsync(libraryId.Value, "/music/deleted.flac", Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        Result<Success> result = await _sut.DeleteItemAsync(libraryId.Value, "/music/deleted.flac", CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
        await _mockTrackRepository.DidNotReceive().DeleteByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteItemAsync_WhenDeleteTrackArtworkFails_ShouldReturnFailure()
    {
        // Arrange
        LibraryId libraryId = _libraryIdFixture.Create();
        Guid albumId = Guid.NewGuid();
        TrackEntity track = _trackEntityFixture.Create(albumId: albumId, path: "/music/deleted.flac");
        Error error = Error.Failure("Database.Error", "Failed to delete the track artwork");
        _mockTrackRepository.GetByPathAsync(libraryId.Value, track.Path, Arg.Any<CancellationToken>())
            .Returns(Result.From<TrackEntity?>(track));
        _mockMusicArtworkRepository.DeleteByOwnerAsync(MusicArtworkOwnerType.Track, track.Id, Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        Result<Success> result = await _sut.DeleteItemAsync(libraryId.Value, track.Path, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
        await _mockTrackRepository.DidNotReceive().DeleteByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteItemAsync_WhenDeleteAlbumArtworkFails_ShouldReturnFailure()
    {
        // Arrange
        LibraryId libraryId = _libraryIdFixture.Create();
        Guid albumId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        TrackEntity track = _trackEntityFixture.Create(albumId: albumId, path: "/music/deleted.flac");
        AlbumEntity album = _albumEntityFixture.Create(id: albumId, artistId: artistId, includeTracks: false);
        Error error = Error.Failure("Database.Error", "Failed to delete the album artwork");
        _mockTrackRepository.GetByPathAsync(libraryId.Value, track.Path, Arg.Any<CancellationToken>())
            .Returns(Result.From<TrackEntity?>(track));
        _mockMusicArtworkRepository.DeleteByOwnerAsync(MusicArtworkOwnerType.Track, track.Id, Arg.Any<CancellationToken>())
            .Returns(Result.Deleted);
        _mockTrackRepository.DeleteByIdAsync(track.Id, Arg.Any<CancellationToken>())
            .Returns(Result.From(Result.Deleted));
        _mockAlbumRepository.GetIncludingDeletedByIdAsync(albumId, Arg.Any<CancellationToken>())
            .Returns(Result.From<AlbumEntity?>(album));
        _mockTrackRepository.GetByAlbumIdIncludingDeletedAsync(albumId, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<TrackEntity>>([track]));
        _mockMusicArtworkRepository.DeleteByOwnerAsync(MusicArtworkOwnerType.Album, albumId, Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        Result<Success> result = await _sut.DeleteItemAsync(libraryId.Value, track.Path, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
        await _mockAlbumRepository.DidNotReceive().DeleteByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteItemAsync_WhenDeleteArtistArtworkFails_ShouldReturnFailure()
    {
        // Arrange
        LibraryId libraryId = _libraryIdFixture.Create();
        Guid albumId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        TrackEntity track = _trackEntityFixture.Create(albumId: albumId, path: "/music/deleted.flac");
        AlbumEntity album = _albumEntityFixture.Create(id: albumId, artistId: artistId, includeTracks: false);
        Error error = Error.Failure("Database.Error", "Failed to delete the artist artwork");
        _mockTrackRepository.GetByPathAsync(libraryId.Value, track.Path, Arg.Any<CancellationToken>())
            .Returns(Result.From<TrackEntity?>(track));
        _mockMusicArtworkRepository.DeleteByOwnerAsync(MusicArtworkOwnerType.Track, track.Id, Arg.Any<CancellationToken>())
            .Returns(Result.Deleted);
        _mockTrackRepository.DeleteByIdAsync(track.Id, Arg.Any<CancellationToken>())
            .Returns(Result.From(Result.Deleted));
        _mockAlbumRepository.GetIncludingDeletedByIdAsync(albumId, Arg.Any<CancellationToken>())
            .Returns(Result.From<AlbumEntity?>(album));
        _mockTrackRepository.GetByAlbumIdIncludingDeletedAsync(albumId, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<TrackEntity>>([track]));
        _mockMusicArtworkRepository.DeleteByOwnerAsync(MusicArtworkOwnerType.Album, albumId, Arg.Any<CancellationToken>())
            .Returns(Result.Deleted);
        _mockAlbumRepository.DeleteByIdAsync(albumId, Arg.Any<CancellationToken>())
            .Returns(Result.From(Result.Deleted));
        _mockAlbumRepository.GetByArtistIdIncludingDeletedAsync(artistId, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<AlbumEntity>>([album]));
        _mockMusicArtworkRepository.DeleteByOwnerAsync(MusicArtworkOwnerType.Artist, artistId, Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        Result<Success> result = await _sut.DeleteItemAsync(libraryId.Value, track.Path, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
        await _mockArtistRepository.DidNotReceive().DeleteByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
