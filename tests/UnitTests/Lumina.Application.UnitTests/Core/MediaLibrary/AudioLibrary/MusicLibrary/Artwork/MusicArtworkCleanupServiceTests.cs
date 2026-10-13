#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Common.DataAccess.Repositories.MusicLibrary;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artwork;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
#endregion

namespace Lumina.Application.UnitTests.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artwork;

/// <summary>
/// Contains unit tests for the <see cref="MusicArtworkCleanupService"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MusicArtworkCleanupServiceTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly IAlbumRepository _mockAlbumRepository;
    private readonly ITrackRepository _mockTrackRepository;
    private readonly IMusicArtworkRepository _mockMusicArtworkRepository;
    private readonly IMusicArtworkService _mockMusicArtworkService;
    private readonly MusicArtworkCleanupService _sut;
    private readonly AlbumEntityFixture _albumEntityFixture = new();
    private readonly TrackEntityFixture _trackEntityFixture = new();
    private readonly MusicArtworkEntityFixture _musicArtworkEntityFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="MusicArtworkCleanupServiceTests"/> class.
    /// </summary>
    public MusicArtworkCleanupServiceTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockAlbumRepository = Substitute.For<IAlbumRepository>();
        _mockTrackRepository = Substitute.For<ITrackRepository>();
        _mockMusicArtworkRepository = Substitute.For<IMusicArtworkRepository>();
        _mockMusicArtworkService = Substitute.For<IMusicArtworkService>();
        _mockUnitOfWork.AlbumRepository.Returns(_mockAlbumRepository);
        _mockUnitOfWork.TrackRepository.Returns(_mockTrackRepository);
        _mockUnitOfWork.MusicArtworkRepository.Returns(_mockMusicArtworkRepository);

        // Default stubs: every owner has no artwork, deleting the records succeeds, deleting a stored file succeeds.
        _mockAlbumRepository.GetByArtistIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<AlbumEntity>>([]));
        _mockTrackRepository.GetByAlbumIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<TrackEntity>>([]));
        _mockMusicArtworkRepository.GetByOwnerAsync(Arg.Any<MusicArtworkOwnerType>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<MusicArtworkEntity>>([]));
        _mockMusicArtworkRepository.DeleteByOwnerAsync(Arg.Any<MusicArtworkOwnerType>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Result.Deleted);
        _mockMusicArtworkService.DeleteStoredArtwork(Arg.Any<string>()).Returns(Result.Deleted);

        _sut = new MusicArtworkCleanupService(_mockUnitOfWork, _mockMusicArtworkService);
    }

    [Fact]
    public async Task DeleteArtistArtworkAsync_WhenArtistHasAlbumsAndTracks_ShouldRemoveTheArtworkOfTheChildrenBeforeTheArtistArtwork()
    {
        // Arrange
        Guid artistId = Guid.NewGuid();
        Guid firstAlbumId = Guid.NewGuid();
        Guid secondAlbumId = Guid.NewGuid();
        Guid firstTrackId = Guid.NewGuid();
        Guid secondTrackId = Guid.NewGuid();
        Guid thirdTrackId = Guid.NewGuid();
        AlbumEntity firstAlbum = _albumEntityFixture.Create(id: firstAlbumId, includeTracks: false);
        AlbumEntity secondAlbum = _albumEntityFixture.Create(id: secondAlbumId, includeTracks: false);
        TrackEntity firstTrack = _trackEntityFixture.Create(id: firstTrackId, albumId: firstAlbumId);
        TrackEntity secondTrack = _trackEntityFixture.Create(id: secondTrackId, albumId: firstAlbumId);
        TrackEntity thirdTrack = _trackEntityFixture.Create(id: thirdTrackId, albumId: secondAlbumId);
        MusicArtworkEntity firstTrackArtwork = _musicArtworkEntityFixture.Create(ownerType: MusicArtworkOwnerType.Track, ownerId: firstTrackId, fileName: "/media/music/first-track.jpg");
        MusicArtworkEntity secondTrackArtwork = _musicArtworkEntityFixture.Create(ownerType: MusicArtworkOwnerType.Track, ownerId: secondTrackId, fileName: "/media/music/second-track.jpg");
        MusicArtworkEntity thirdTrackArtwork = _musicArtworkEntityFixture.Create(ownerType: MusicArtworkOwnerType.Track, ownerId: thirdTrackId, fileName: "/media/music/third-track.jpg");
        MusicArtworkEntity firstAlbumArtwork = _musicArtworkEntityFixture.Create(ownerType: MusicArtworkOwnerType.Album, ownerId: firstAlbumId, fileName: "/media/music/first-album.jpg");
        MusicArtworkEntity secondAlbumArtwork = _musicArtworkEntityFixture.Create(ownerType: MusicArtworkOwnerType.Album, ownerId: secondAlbumId, fileName: "/media/music/second-album.jpg");
        MusicArtworkEntity artistArtwork = _musicArtworkEntityFixture.Create(ownerType: MusicArtworkOwnerType.Artist, ownerId: artistId, fileName: "/media/music/artist.jpg");
        _mockAlbumRepository.GetByArtistIdAsync(artistId, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<AlbumEntity>>([firstAlbum, secondAlbum]));
        _mockTrackRepository.GetByAlbumIdAsync(firstAlbumId, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<TrackEntity>>([firstTrack, secondTrack]));
        _mockTrackRepository.GetByAlbumIdAsync(secondAlbumId, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<TrackEntity>>([thirdTrack]));
        _mockMusicArtworkRepository.GetByOwnerAsync(MusicArtworkOwnerType.Track, firstTrackId, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<MusicArtworkEntity>>([firstTrackArtwork]));
        _mockMusicArtworkRepository.GetByOwnerAsync(MusicArtworkOwnerType.Track, secondTrackId, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<MusicArtworkEntity>>([secondTrackArtwork]));
        _mockMusicArtworkRepository.GetByOwnerAsync(MusicArtworkOwnerType.Track, thirdTrackId, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<MusicArtworkEntity>>([thirdTrackArtwork]));
        _mockMusicArtworkRepository.GetByOwnerAsync(MusicArtworkOwnerType.Album, firstAlbumId, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<MusicArtworkEntity>>([firstAlbumArtwork]));
        _mockMusicArtworkRepository.GetByOwnerAsync(MusicArtworkOwnerType.Album, secondAlbumId, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<MusicArtworkEntity>>([secondAlbumArtwork]));
        _mockMusicArtworkRepository.GetByOwnerAsync(MusicArtworkOwnerType.Artist, artistId, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<MusicArtworkEntity>>([artistArtwork]));

        // Act
        Result<Deleted> result = await _sut.DeleteArtistArtworkAsync(artistId, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        _mockMusicArtworkService.Received(1).DeleteStoredArtwork(firstTrackArtwork.FileName!);
        _mockMusicArtworkService.Received(1).DeleteStoredArtwork(secondTrackArtwork.FileName!);
        _mockMusicArtworkService.Received(1).DeleteStoredArtwork(thirdTrackArtwork.FileName!);
        _mockMusicArtworkService.Received(1).DeleteStoredArtwork(firstAlbumArtwork.FileName!);
        _mockMusicArtworkService.Received(1).DeleteStoredArtwork(secondAlbumArtwork.FileName!);
        _mockMusicArtworkService.Received(1).DeleteStoredArtwork(artistArtwork.FileName!);
        // Every owner of the artwork is removed from the innermost children outwards, so that no artwork is orphaned.
        Received.InOrder(() =>
        {
            _mockMusicArtworkRepository.DeleteByOwnerAsync(MusicArtworkOwnerType.Track, firstTrackId, Arg.Any<CancellationToken>());
            _mockMusicArtworkRepository.DeleteByOwnerAsync(MusicArtworkOwnerType.Track, secondTrackId, Arg.Any<CancellationToken>());
            _mockMusicArtworkRepository.DeleteByOwnerAsync(MusicArtworkOwnerType.Album, firstAlbumId, Arg.Any<CancellationToken>());
            _mockMusicArtworkRepository.DeleteByOwnerAsync(MusicArtworkOwnerType.Track, thirdTrackId, Arg.Any<CancellationToken>());
            _mockMusicArtworkRepository.DeleteByOwnerAsync(MusicArtworkOwnerType.Album, secondAlbumId, Arg.Any<CancellationToken>());
            _mockMusicArtworkRepository.DeleteByOwnerAsync(MusicArtworkOwnerType.Artist, artistId, Arg.Any<CancellationToken>());
        });
    }

    [Fact]
    public async Task DeleteArtistArtworkAsync_WhenGetAlbumsFails_ShouldReturnTheErrorWithoutDeletingAnyArtwork()
    {
        // Arrange
        Guid artistId = Guid.NewGuid();
        _mockAlbumRepository.GetByArtistIdAsync(artistId, Arg.Any<CancellationToken>())
            .Returns(ApplicationErrors.Persistence.UniqueConstraintViolation);

        // Act
        Result<Deleted> result = await _sut.DeleteArtistArtworkAsync(artistId, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Persistence.UniqueConstraintViolation, result.FirstError);
        await _mockMusicArtworkRepository.DidNotReceive().DeleteByOwnerAsync(Arg.Any<MusicArtworkOwnerType>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        _mockMusicArtworkService.DidNotReceive().DeleteStoredArtwork(Arg.Any<string>());
    }

    [Fact]
    public async Task DeleteArtistArtworkAsync_WhenAChildArtworkCleanupFails_ShouldReturnTheErrorWithoutDeletingTheArtistArtwork()
    {
        // Arrange
        Guid artistId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        AlbumEntity album = _albumEntityFixture.Create(id: albumId, includeTracks: false);
        _mockAlbumRepository.GetByArtistIdAsync(artistId, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<AlbumEntity>>([album]));
        _mockMusicArtworkRepository.DeleteByOwnerAsync(MusicArtworkOwnerType.Album, albumId, Arg.Any<CancellationToken>())
            .Returns(ApplicationErrors.Persistence.UniqueConstraintViolation);

        // Act
        Result<Deleted> result = await _sut.DeleteArtistArtworkAsync(artistId, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Persistence.UniqueConstraintViolation, result.FirstError);
        await _mockMusicArtworkRepository.DidNotReceive().DeleteByOwnerAsync(MusicArtworkOwnerType.Artist, Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteAlbumArtworkAsync_WhenAlbumHasTracks_ShouldRemoveTheArtworkOfTheTracksBeforeTheAlbumArtwork()
    {
        // Arrange
        Guid albumId = Guid.NewGuid();
        Guid firstTrackId = Guid.NewGuid();
        Guid secondTrackId = Guid.NewGuid();
        TrackEntity firstTrack = _trackEntityFixture.Create(id: firstTrackId, albumId: albumId);
        TrackEntity secondTrack = _trackEntityFixture.Create(id: secondTrackId, albumId: albumId);
        MusicArtworkEntity firstTrackArtwork = _musicArtworkEntityFixture.Create(ownerType: MusicArtworkOwnerType.Track, ownerId: firstTrackId, fileName: "/media/music/first-track.jpg");
        MusicArtworkEntity secondTrackArtwork = _musicArtworkEntityFixture.Create(ownerType: MusicArtworkOwnerType.Track, ownerId: secondTrackId, fileName: "/media/music/second-track.jpg");
        MusicArtworkEntity albumArtwork = _musicArtworkEntityFixture.Create(ownerType: MusicArtworkOwnerType.Album, ownerId: albumId, fileName: "/media/music/album.jpg");
        _mockTrackRepository.GetByAlbumIdAsync(albumId, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<TrackEntity>>([firstTrack, secondTrack]));
        _mockMusicArtworkRepository.GetByOwnerAsync(MusicArtworkOwnerType.Track, firstTrackId, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<MusicArtworkEntity>>([firstTrackArtwork]));
        _mockMusicArtworkRepository.GetByOwnerAsync(MusicArtworkOwnerType.Track, secondTrackId, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<MusicArtworkEntity>>([secondTrackArtwork]));
        _mockMusicArtworkRepository.GetByOwnerAsync(MusicArtworkOwnerType.Album, albumId, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<MusicArtworkEntity>>([albumArtwork]));

        // Act
        Result<Deleted> result = await _sut.DeleteAlbumArtworkAsync(albumId, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        _mockMusicArtworkService.Received(1).DeleteStoredArtwork(firstTrackArtwork.FileName!);
        _mockMusicArtworkService.Received(1).DeleteStoredArtwork(secondTrackArtwork.FileName!);
        _mockMusicArtworkService.Received(1).DeleteStoredArtwork(albumArtwork.FileName!);
        Received.InOrder(() =>
        {
            _mockMusicArtworkRepository.DeleteByOwnerAsync(MusicArtworkOwnerType.Track, firstTrackId, Arg.Any<CancellationToken>());
            _mockMusicArtworkRepository.DeleteByOwnerAsync(MusicArtworkOwnerType.Track, secondTrackId, Arg.Any<CancellationToken>());
            _mockMusicArtworkRepository.DeleteByOwnerAsync(MusicArtworkOwnerType.Album, albumId, Arg.Any<CancellationToken>());
        });
    }

    [Fact]
    public async Task DeleteAlbumArtworkAsync_WhenGetTracksFails_ShouldReturnTheErrorWithoutDeletingAnyArtwork()
    {
        // Arrange
        Guid albumId = Guid.NewGuid();
        _mockTrackRepository.GetByAlbumIdAsync(albumId, Arg.Any<CancellationToken>())
            .Returns(ApplicationErrors.Persistence.UniqueConstraintViolation);

        // Act
        Result<Deleted> result = await _sut.DeleteAlbumArtworkAsync(albumId, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Persistence.UniqueConstraintViolation, result.FirstError);
        await _mockMusicArtworkRepository.DidNotReceive().DeleteByOwnerAsync(Arg.Any<MusicArtworkOwnerType>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteAlbumArtworkAsync_WhenGetArtworkFails_ShouldReturnTheErrorWithoutDeletingTheRecords()
    {
        // Arrange
        Guid albumId = Guid.NewGuid();
        _mockMusicArtworkRepository.GetByOwnerAsync(MusicArtworkOwnerType.Album, albumId, Arg.Any<CancellationToken>())
            .Returns(ApplicationErrors.Persistence.UniqueConstraintViolation);

        // Act
        Result<Deleted> result = await _sut.DeleteAlbumArtworkAsync(albumId, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Persistence.UniqueConstraintViolation, result.FirstError);
        await _mockMusicArtworkRepository.DidNotReceive().DeleteByOwnerAsync(Arg.Any<MusicArtworkOwnerType>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteAlbumArtworkAsync_WhenDeletingTheArtworkRecordsFails_ShouldReturnTheError()
    {
        // Arrange
        Guid albumId = Guid.NewGuid();
        _mockMusicArtworkRepository.DeleteByOwnerAsync(MusicArtworkOwnerType.Album, albumId, Arg.Any<CancellationToken>())
            .Returns(ApplicationErrors.Persistence.UniqueConstraintViolation);

        // Act
        Result<Deleted> result = await _sut.DeleteAlbumArtworkAsync(albumId, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Persistence.UniqueConstraintViolation, result.FirstError);
    }

    [Fact]
    public async Task DeleteAlbumArtworkAsync_WhenDeletingAStoredArtworkFileFails_ShouldReturnTheErrorWithoutDeletingTheRecords()
    {
        // Arrange
        Guid albumId = Guid.NewGuid();
        MusicArtworkEntity albumArtwork = _musicArtworkEntityFixture.Create(ownerType: MusicArtworkOwnerType.Album, ownerId: albumId, fileName: "/media/music/album.jpg");
        _mockMusicArtworkRepository.GetByOwnerAsync(MusicArtworkOwnerType.Album, albumId, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<MusicArtworkEntity>>([albumArtwork]));
        _mockMusicArtworkService.DeleteStoredArtwork(albumArtwork.FileName!)
            .Returns(ApplicationErrors.Persistence.UniqueConstraintViolation);

        // Act
        Result<Deleted> result = await _sut.DeleteAlbumArtworkAsync(albumId, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Persistence.UniqueConstraintViolation, result.FirstError);
        await _mockMusicArtworkRepository.DidNotReceive().DeleteByOwnerAsync(Arg.Any<MusicArtworkOwnerType>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteAlbumArtworkAsync_WhenArtworkHasNoStoredFile_ShouldSkipTheFileDeletionButDeleteTheRecords()
    {
        // Arrange
        Guid albumId = Guid.NewGuid();
        // An artwork whose file has not been resolved yet has no file to remove, but its record still has to be removed.
        MusicArtworkEntity unresolvedArtwork = _musicArtworkEntityFixture.Create(ownerType: MusicArtworkOwnerType.Album, ownerId: albumId, includeFileName: false);
        _mockMusicArtworkRepository.GetByOwnerAsync(MusicArtworkOwnerType.Album, albumId, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<MusicArtworkEntity>>([unresolvedArtwork]));

        // Act
        Result<Deleted> result = await _sut.DeleteAlbumArtworkAsync(albumId, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        _mockMusicArtworkService.DidNotReceive().DeleteStoredArtwork(Arg.Any<string>());
        await _mockMusicArtworkRepository.Received(1).DeleteByOwnerAsync(MusicArtworkOwnerType.Album, albumId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteTrackArtworkAsync_WhenArtworkExists_ShouldDeleteTheStoredFilesAndTheRecords()
    {
        // Arrange
        Guid trackId = Guid.NewGuid();
        MusicArtworkEntity trackArtwork = _musicArtworkEntityFixture.Create(ownerType: MusicArtworkOwnerType.Track, ownerId: trackId, fileName: "/media/music/track.jpg");
        _mockMusicArtworkRepository.GetByOwnerAsync(MusicArtworkOwnerType.Track, trackId, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<MusicArtworkEntity>>([trackArtwork]));

        // Act
        Result<Deleted> result = await _sut.DeleteTrackArtworkAsync(trackId, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        _mockMusicArtworkService.Received(1).DeleteStoredArtwork(trackArtwork.FileName!);
        await _mockMusicArtworkRepository.Received(1).DeleteByOwnerAsync(MusicArtworkOwnerType.Track, trackId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteTrackArtworkAsync_WhenNoArtworkExists_ShouldStillDeleteTheRecords()
    {
        // Arrange
        Guid trackId = Guid.NewGuid();

        // Act
        Result<Deleted> result = await _sut.DeleteTrackArtworkAsync(trackId, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        _mockMusicArtworkService.DidNotReceive().DeleteStoredArtwork(Arg.Any<string>());
        await _mockMusicArtworkRepository.Received(1).DeleteByOwnerAsync(MusicArtworkOwnerType.Track, trackId, Arg.Any<CancellationToken>());
    }
}
