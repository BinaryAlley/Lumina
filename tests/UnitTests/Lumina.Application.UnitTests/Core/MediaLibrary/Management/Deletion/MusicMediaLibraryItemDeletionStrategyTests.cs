#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Common.DataAccess.Repositories.MusicLibrary;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Core.MediaLibrary.Management.Deletion;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.UnitTests.Core.MediaLibrary.Management.Deletion;

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
    private readonly MusicMediaLibraryItemDeletionStrategy _sut;
    private readonly LibraryIdFixture _libraryIdFixture = new();
    private readonly TrackEntityFixture _trackEntityFixture = new();
    private readonly AlbumEntityFixture _albumEntityFixture = new();

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
        _mockUnitOfWork.TrackRepository.Returns(_mockTrackRepository);
        _mockUnitOfWork.AlbumRepository.Returns(_mockAlbumRepository);
        _mockUnitOfWork.ArtistRepository.Returns(_mockArtistRepository);
        _mockUnitOfWork.MusicArtworkRepository.Returns(_mockMusicArtworkRepository);

        _sut = new MusicMediaLibraryItemDeletionStrategy(_mockUnitOfWork);
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
        _mockAlbumRepository.GetByIdAsync(albumId, Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<AlbumEntity?>(album));
        _mockTrackRepository.GetByAlbumIdAsync(albumId, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<TrackEntity>>([otherTrack]));
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Result.Success);

        // Act
        Result<Success> result = await _sut.DeleteItemAsync(libraryId.Value, track.Path, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        await _mockMusicArtworkRepository.Received(1).DeleteByOwnerAsync(MusicArtworkOwnerType.Track, track.Id, Arg.Any<CancellationToken>());
        await _mockTrackRepository.Received(1).DeleteByIdAsync(track.Id, Arg.Any<CancellationToken>());
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
        _mockAlbumRepository.GetByIdAsync(albumId, Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<AlbumEntity?>(album));
        _mockTrackRepository.GetByAlbumIdAsync(albumId, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<TrackEntity>>([track]));
        _mockMusicArtworkRepository.DeleteByOwnerAsync(MusicArtworkOwnerType.Album, albumId, Arg.Any<CancellationToken>())
            .Returns(Result.Deleted);
        _mockAlbumRepository.DeleteByIdAsync(albumId, Arg.Any<CancellationToken>())
            .Returns(Result.From(Result.Deleted));
        _mockAlbumRepository.GetByArtistIdAsync(artistId, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<AlbumEntity>>([otherAlbum]));
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Result.Success);

        // Act
        Result<Success> result = await _sut.DeleteItemAsync(libraryId.Value, track.Path, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        await _mockMusicArtworkRepository.Received(1).DeleteByOwnerAsync(MusicArtworkOwnerType.Album, albumId, Arg.Any<CancellationToken>());
        await _mockAlbumRepository.Received(1).DeleteByIdAsync(albumId, Arg.Any<CancellationToken>());
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
        _mockAlbumRepository.GetByIdAsync(albumId, Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<AlbumEntity?>(album));
        _mockTrackRepository.GetByAlbumIdAsync(albumId, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<TrackEntity>>([track]));
        _mockMusicArtworkRepository.DeleteByOwnerAsync(MusicArtworkOwnerType.Album, albumId, Arg.Any<CancellationToken>())
            .Returns(Result.Deleted);
        _mockAlbumRepository.DeleteByIdAsync(albumId, Arg.Any<CancellationToken>())
            .Returns(Result.From(Result.Deleted));
        _mockAlbumRepository.GetByArtistIdAsync(artistId, Arg.Any<CancellationToken>())
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
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
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
        _mockAlbumRepository.GetByIdAsync(albumId, Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<AlbumEntity?>(null));
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Result.Success);

        // Act
        Result<Success> result = await _sut.DeleteItemAsync(libraryId.Value, track.Path, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        await _mockTrackRepository.Received(1).DeleteByIdAsync(track.Id, Arg.Any<CancellationToken>());
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
        _mockAlbumRepository.GetByIdAsync(albumId, Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<AlbumEntity?>(album));
        _mockTrackRepository.GetByAlbumIdAsync(albumId, Arg.Any<CancellationToken>())
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
        _mockAlbumRepository.GetByIdAsync(albumId, Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<AlbumEntity?>(album));
        _mockTrackRepository.GetByAlbumIdAsync(albumId, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<TrackEntity>>([track]));
        _mockMusicArtworkRepository.DeleteByOwnerAsync(MusicArtworkOwnerType.Album, albumId, Arg.Any<CancellationToken>())
            .Returns(Result.Deleted);
        _mockAlbumRepository.DeleteByIdAsync(albumId, Arg.Any<CancellationToken>())
            .Returns(Result.From(Result.Deleted));
        _mockAlbumRepository.GetByArtistIdAsync(artistId, Arg.Any<CancellationToken>())
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
