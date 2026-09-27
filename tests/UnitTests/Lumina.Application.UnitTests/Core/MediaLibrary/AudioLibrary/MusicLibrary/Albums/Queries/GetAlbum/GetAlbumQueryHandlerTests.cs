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
using NSubstitute;
using System;
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
    private readonly IAuthorizationService _mockAuthorizationService;
    private readonly ICurrentUserService _mockCurrentUserService;
    private readonly IValidator<GetAlbumQuery> _mockValidator;
    private readonly GetAlbumQueryHandler _sut;
    private readonly Guid _userId;
    private readonly AlbumEntityFixture _albumEntityFixture = new();
    private readonly GetAlbumQueryFixture _getAlbumQueryFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetAlbumQueryHandlerTests"/> class.
    /// </summary>
    public GetAlbumQueryHandlerTests()
    {
        _mockAlbumRepository = Substitute.For<IAlbumRepository>();
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockUnitOfWork.AlbumRepository.Returns(_mockAlbumRepository);
        _mockAuthorizationService = Substitute.For<IAuthorizationService>();
        _mockCurrentUserService = Substitute.For<ICurrentUserService>();
        _mockValidator = Substitute.For<IValidator<GetAlbumQuery>>();
        _userId = Guid.NewGuid();

        // Default stubs: the current user is authenticated, the library ownership policy allows access and the validator passes.
        _mockCurrentUserService.UserId.Returns(_userId);
        _mockAuthorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(_userId, Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>())
            .Returns(true);
        _mockValidator.Validate(Arg.Any<GetAlbumQuery>()).Returns([]);

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
}
