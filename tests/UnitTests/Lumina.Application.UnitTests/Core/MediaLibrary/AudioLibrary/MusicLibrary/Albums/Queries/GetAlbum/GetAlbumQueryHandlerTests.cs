#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Common.DataAccess.Repositories.MusicLibrary;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Infrastructure.Authorization.Policies.LibraryOwnership;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Albums.Queries.GetAlbum;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Fixtures.Core.MediaLibrary.AudioLibrary.MusicLibrary.Albums.Queries.GetAlbum;
using Lumina.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Albums;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
#endregion

namespace Lumina.Application.UnitTests.Core.MediaLibrary.AudioLibrary.MusicLibrary.Albums.Queries.GetAlbum;

/// <summary>
/// Contains unit tests for the <see cref="GetAlbumQueryHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetAlbumQueryHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly IAlbumRepository _mockAlbumRepository;
    private readonly IMusicArtworkRepository _mockMusicArtworkRepository;
    private readonly IAuthorizationService _mockAuthorizationService;
    private readonly ICurrentUserService _mockCurrentUserService;
    private readonly IValidator<GetAlbumQuery> _mockValidator;
    private readonly GetAlbumQueryHandler _sut;
    private readonly Guid _userId;
    private readonly AlbumEntityFixture _albumEntityFixture = new();
    private readonly MusicArtworkEntityFixture _musicArtworkEntityFixture = new();
    private readonly GetAlbumQueryFixture _getAlbumQueryFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetAlbumQueryHandlerTests"/> class.
    /// </summary>
    public GetAlbumQueryHandlerTests()
    {
        _mockAlbumRepository = Substitute.For<IAlbumRepository>();
        _mockMusicArtworkRepository = Substitute.For<IMusicArtworkRepository>();
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockUnitOfWork.AlbumRepository.Returns(_mockAlbumRepository);
        _mockUnitOfWork.MusicArtworkRepository.Returns(_mockMusicArtworkRepository);
        _mockAuthorizationService = Substitute.For<IAuthorizationService>();
        _mockCurrentUserService = Substitute.For<ICurrentUserService>();
        _mockValidator = Substitute.For<IValidator<GetAlbumQuery>>();
        _userId = Guid.NewGuid();

        // Default stubs: the current user is authenticated, the library ownership policy allows access, the validator passes, and the album has no stored artwork.
        _mockCurrentUserService.UserId.Returns(_userId);
        _mockAuthorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(_userId, Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>())
            .Returns(true);
        _mockValidator.Validate(Arg.Any<GetAlbumQuery>()).Returns([]);
        _mockMusicArtworkRepository.GetByOwnerAsync(Arg.Any<MusicArtworkOwnerType>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<MusicArtworkEntity>>([]));

        _sut = new GetAlbumQueryHandler(_mockUnitOfWork, _mockAuthorizationService, _mockCurrentUserService, _mockValidator);
    }

    [Fact]
    public async Task HandleAsync_WhenAlbumExists_ShouldReturnMappedAlbumResponse()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        AlbumEntity album = _albumEntityFixture.Create(id: albumId, artistId: artistId, libraryId: libraryId, includeTracks: true, includeMetadata: true);
        _mockAlbumRepository.GetByIdAsync(albumId, shouldIncludeNavigationProperties: true, shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<AlbumEntity?>(album));
        List<MusicArtworkEntity> artwork = [
            _musicArtworkEntityFixture.Create(ownerType: MusicArtworkOwnerType.Album, ownerId: albumId, artworkType: ArtworkType.Cover, ordinal: 0, fileName: "/media/cover.jpg"),
            _musicArtworkEntityFixture.Create(ownerType: MusicArtworkOwnerType.Album, ownerId: albumId, artworkType: ArtworkType.Back, ordinal: 0, fileName: "/media/back.jpg"),
            _musicArtworkEntityFixture.Create(ownerType: MusicArtworkOwnerType.Album, ownerId: albumId, artworkType: ArtworkType.Booklet, ordinal: 0, includeFileName: false)
        ];
        _mockMusicArtworkRepository.GetByOwnerAsync(MusicArtworkOwnerType.Album, albumId, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<MusicArtworkEntity>>(artwork));
        GetAlbumQuery query = _getAlbumQueryFixture.Create(libraryId: libraryId.ToString(), artistId: artistId.ToString(), albumId: albumId.ToString());

        // Act
        Result<AlbumResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(albumId, result.Value.Id);
        Assert.Equal(artistId, result.Value.ArtistId);
        Assert.Equal(libraryId, result.Value.LibraryId);
        Assert.Equal(album.Title, result.Value.Metadata.Title);
        Assert.NotNull(result.Value.Tracks);
        Assert.Equal(album.Tracks.Count, result.Value.Tracks!.Count);
        // The cover is exposed on its own, and every stored piece of artwork is exposed in order, while the ones whose file is not stored yet are left out.
        Assert.Equal("/media/cover.jpg", result.Value.CoverPath);
        Assert.Equal(2, result.Value.Artwork.Count);
        Assert.Equal(ArtworkType.Cover, result.Value.Artwork[0].Type);
        Assert.Equal("/media/cover.jpg", result.Value.Artwork[0].Path);
        Assert.Equal(ArtworkType.Back, result.Value.Artwork[1].Type);
        Assert.Equal("/media/back.jpg", result.Value.Artwork[1].Path);
        await _mockAlbumRepository.Received(1).GetByIdAsync(albumId, shouldIncludeNavigationProperties: true, shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>());
        await _mockAuthorizationService.Received(1).EvaluatePolicyAsync<ILibraryOwnershipPolicy>(
            _userId, Arg.Is<LibraryOwnershipPolicyContext>(context => context.LibraryId == libraryId), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenAlbumRepositoryFails_ShouldReturnFailureResultWithoutEvaluatingThePolicy()
    {
        // Arrange
        GetAlbumQuery query = _getAlbumQueryFixture.Create();
        _mockAlbumRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(ApplicationErrors.Persistence.UniqueConstraintViolation);

        // Act
        Result<AlbumResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Persistence.UniqueConstraintViolation, result.FirstError);
        await _mockAuthorizationService.DidNotReceive().EvaluatePolicyAsync<ILibraryOwnershipPolicy>(Arg.Any<Guid>(), Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenAlbumDoesNotExist_ShouldReturnAlbumNotFoundError()
    {
        // Arrange
        GetAlbumQuery query = _getAlbumQueryFixture.Create();
        _mockAlbumRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(_ => Result.From<AlbumEntity?>(null));

        // Act
        Result<AlbumResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.AlbumNotFound, result.FirstError);
        await _mockAuthorizationService.DidNotReceive().EvaluatePolicyAsync<ILibraryOwnershipPolicy>(Arg.Any<Guid>(), Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenAlbumBelongsToAnotherLibrary_ShouldReturnAlbumNotFoundWithoutEvaluatingThePolicy()
    {
        // Arrange
        GetAlbumQuery query = _getAlbumQueryFixture.Create();
        Guid artistId = Guid.Parse(query.ArtistId!);
        Guid albumId = Guid.Parse(query.AlbumId!);
        AlbumEntity album = _albumEntityFixture.Create(id: albumId, artistId: artistId, libraryId: Guid.NewGuid());
        _mockAlbumRepository.GetByIdAsync(albumId, shouldIncludeNavigationProperties: true, shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<AlbumEntity?>(album));

        // Act
        Result<AlbumResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        // The mismatch is reported as not found, without disclosing that the album exists in another library.
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.AlbumNotFound, result.FirstError);
        await _mockAuthorizationService.DidNotReceive().EvaluatePolicyAsync<ILibraryOwnershipPolicy>(Arg.Any<Guid>(), Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenAlbumBelongsToAnotherArtist_ShouldReturnAlbumNotFoundWithoutEvaluatingThePolicy()
    {
        // Arrange
        GetAlbumQuery query = _getAlbumQueryFixture.Create();
        Guid libraryId = Guid.Parse(query.LibraryId!);
        Guid albumId = Guid.Parse(query.AlbumId!);
        AlbumEntity album = _albumEntityFixture.Create(id: albumId, artistId: Guid.NewGuid(), libraryId: libraryId);
        _mockAlbumRepository.GetByIdAsync(albumId, shouldIncludeNavigationProperties: true, shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<AlbumEntity?>(album));

        // Act
        Result<AlbumResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        // The mismatch is reported as not found, without disclosing that the album exists under another artist.
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.AlbumNotFound, result.FirstError);
        await _mockAuthorizationService.DidNotReceive().EvaluatePolicyAsync<ILibraryOwnershipPolicy>(Arg.Any<Guid>(), Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenUserDoesNotOwnTheLibrary_ShouldReturnNotAuthorizedError()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        AlbumEntity album = _albumEntityFixture.Create(id: albumId, artistId: artistId, libraryId: libraryId);
        GetAlbumQuery query = _getAlbumQueryFixture.Create(libraryId: libraryId.ToString(), artistId: artistId.ToString(), albumId: albumId.ToString());
        _mockAlbumRepository.GetByIdAsync(albumId, shouldIncludeNavigationProperties: true, shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<AlbumEntity?>(album));
        _mockAuthorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(_userId, Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        Result<AlbumResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockAuthorizationService.Received(1).EvaluatePolicyAsync<ILibraryOwnershipPolicy>(
            _userId, Arg.Is<LibraryOwnershipPolicyContext>(context => context.LibraryId == libraryId), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenUserIsNotAuthenticated_ShouldReturnNotAuthorizedError()
    {
        // Arrange
        GetAlbumQuery query = _getAlbumQueryFixture.Create();
        _mockCurrentUserService.UserId.Returns((Guid?)null);

        // Act
        Result<AlbumResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockAlbumRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenValidationFails_ShouldReturnValidationErrorsWithoutQuerying()
    {
        // Arrange
        GetAlbumQuery query = _getAlbumQueryFixture.Create();
        _mockValidator.Validate(Arg.Any<GetAlbumQuery>()).Returns([Errors.Music.AlbumIdCannotBeEmpty]);

        // Act
        Result<AlbumResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.AlbumIdCannotBeEmpty, result.FirstError);
        await _mockAlbumRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
        await _mockAuthorizationService.DidNotReceive().EvaluatePolicyAsync<ILibraryOwnershipPolicy>(Arg.Any<Guid>(), Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenArtworkRepositoryFails_ShouldReturnFailureResult()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        AlbumEntity album = _albumEntityFixture.Create(id: albumId, artistId: artistId, libraryId: libraryId);
        GetAlbumQuery query = _getAlbumQueryFixture.Create(libraryId: libraryId.ToString(), artistId: artistId.ToString(), albumId: albumId.ToString());
        _mockAlbumRepository.GetByIdAsync(albumId, shouldIncludeNavigationProperties: true, shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<AlbumEntity?>(album));
        _mockMusicArtworkRepository.GetByOwnerAsync(MusicArtworkOwnerType.Album, albumId, Arg.Any<CancellationToken>())
            .Returns(ApplicationErrors.Persistence.UniqueConstraintViolation);

        // Act
        Result<AlbumResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Persistence.UniqueConstraintViolation, result.FirstError);
    }

    [Fact]
    public async Task HandleAsync_WhenAlbumHasNoArtwork_ShouldReturnNullCoverPathAndEmptyArtwork()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        AlbumEntity album = _albumEntityFixture.Create(id: albumId, artistId: artistId, libraryId: libraryId);
        GetAlbumQuery query = _getAlbumQueryFixture.Create(libraryId: libraryId.ToString(), artistId: artistId.ToString(), albumId: albumId.ToString());
        _mockAlbumRepository.GetByIdAsync(albumId, shouldIncludeNavigationProperties: true, shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<AlbumEntity?>(album));
        _mockMusicArtworkRepository.GetByOwnerAsync(MusicArtworkOwnerType.Album, albumId, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<MusicArtworkEntity>>([]));

        // Act
        Result<AlbumResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Null(result.Value.CoverPath);
        Assert.Empty(result.Value.Artwork);
    }

    [Fact]
    public async Task HandleAsync_WhenMultipleArtworkExist_ShouldMapEveryStoredArtworkAndExcludeUnstoredOnes()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        AlbumEntity album = _albumEntityFixture.Create(id: albumId, artistId: artistId, libraryId: libraryId);
        GetAlbumQuery query = _getAlbumQueryFixture.Create(libraryId: libraryId.ToString(), artistId: artistId.ToString(), albumId: albumId.ToString());
        _mockAlbumRepository.GetByIdAsync(albumId, shouldIncludeNavigationProperties: true, shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<AlbumEntity?>(album));
        List<MusicArtworkEntity> artwork = [
            _musicArtworkEntityFixture.Create(ownerType: MusicArtworkOwnerType.Album, ownerId: albumId, artworkType: ArtworkType.Cover, ordinal: 0, fileName: "/media/cover.jpg"),
            _musicArtworkEntityFixture.Create(ownerType: MusicArtworkOwnerType.Album, ownerId: albumId, artworkType: ArtworkType.Booklet, ordinal: 1, includeFileName: false),
            _musicArtworkEntityFixture.Create(ownerType: MusicArtworkOwnerType.Album, ownerId: albumId, artworkType: ArtworkType.Booklet, ordinal: 2, fileName: string.Empty),
            _musicArtworkEntityFixture.Create(ownerType: MusicArtworkOwnerType.Album, ownerId: albumId, artworkType: ArtworkType.Booklet, ordinal: 3, fileName: "   "),
            _musicArtworkEntityFixture.Create(ownerType: MusicArtworkOwnerType.Album, ownerId: albumId, artworkType: ArtworkType.Back, ordinal: 0, fileName: "/media/back.jpg")
        ];
        _mockMusicArtworkRepository.GetByOwnerAsync(MusicArtworkOwnerType.Album, albumId, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<MusicArtworkEntity>>(artwork));

        // Act
        Result<AlbumResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        // Artwork whose file has not been stored yet (null, empty or whitespace) is left out, and the stored ones keep their order, type and ordinal.
        Assert.Equal(2, result.Value.Artwork.Count);
        Assert.Equal(ArtworkType.Cover, result.Value.Artwork[0].Type);
        Assert.Equal(0, result.Value.Artwork[0].Ordinal);
        Assert.Equal("/media/cover.jpg", result.Value.Artwork[0].Path);
        Assert.Equal(ArtworkType.Back, result.Value.Artwork[1].Type);
        Assert.Equal(0, result.Value.Artwork[1].Ordinal);
        Assert.Equal("/media/back.jpg", result.Value.Artwork[1].Path);
        await _mockMusicArtworkRepository.Received(1).GetByOwnerAsync(MusicArtworkOwnerType.Album, albumId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenMultipleCoversExist_ShouldUseTheFirstCoverFileName()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        AlbumEntity album = _albumEntityFixture.Create(id: albumId, artistId: artistId, libraryId: libraryId);
        GetAlbumQuery query = _getAlbumQueryFixture.Create(libraryId: libraryId.ToString(), artistId: artistId.ToString(), albumId: albumId.ToString());
        _mockAlbumRepository.GetByIdAsync(albumId, shouldIncludeNavigationProperties: true, shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<AlbumEntity?>(album));
        List<MusicArtworkEntity> artwork = [
            _musicArtworkEntityFixture.Create(ownerType: MusicArtworkOwnerType.Album, ownerId: albumId, artworkType: ArtworkType.Cover, ordinal: 0, fileName: "/media/front.jpg"),
            _musicArtworkEntityFixture.Create(ownerType: MusicArtworkOwnerType.Album, ownerId: albumId, artworkType: ArtworkType.Cover, ordinal: 1, fileName: "/media/alternate.jpg")
        ];
        _mockMusicArtworkRepository.GetByOwnerAsync(MusicArtworkOwnerType.Album, albumId, Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<MusicArtworkEntity>>(artwork));

        // Act
        Result<AlbumResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal("/media/front.jpg", result.Value.CoverPath);
    }
}
