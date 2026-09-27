#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Common.DataAccess.Repositories.MusicLibrary;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Albums;
using Lumina.Application.Common.DTO.Pagination;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Infrastructure.Authorization.Policies.LibraryOwnership;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artists.Queries.GetArtistAlbumsLite;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Fixtures.Common.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Albums;
using Lumina.Application.Fixtures.Common.DTO.Pagination;
using Lumina.Application.Fixtures.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artists.Queries.GetArtistAlbumsLite;
using Lumina.Contracts.Responses.Common;
using Lumina.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Albums;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
#endregion

namespace Lumina.Application.UnitTests.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artists.Queries.GetArtistAlbumsLite;

/// <summary>
/// Contains unit tests for the <see cref="GetArtistAlbumsLiteQueryHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetArtistAlbumsLiteQueryHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly IArtistRepository _mockArtistRepository;
    private readonly IAlbumRepository _mockAlbumRepository;
    private readonly IAuthorizationService _mockAuthorizationService;
    private readonly ICurrentUserService _mockCurrentUserService;
    private readonly IValidator<GetArtistAlbumsLiteQuery> _mockValidator;
    private readonly GetArtistAlbumsLiteQueryHandler _sut;
    private readonly Guid _userId;
    private readonly ArtistEntityFixture _artistEntityFixture = new();
    private readonly AlbumLiteRowFixture _albumLiteRowFixture = new();
    private readonly GetArtistAlbumsLiteQueryFixture _getArtistAlbumsLiteQueryFixture = new();
    private readonly PaginatedResultDtoFixture<AlbumLiteRow> _paginatedResultDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetArtistAlbumsLiteQueryHandlerTests"/> class.
    /// </summary>
    public GetArtistAlbumsLiteQueryHandlerTests()
    {
        _mockArtistRepository = Substitute.For<IArtistRepository>();
        _mockAlbumRepository = Substitute.For<IAlbumRepository>();
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockUnitOfWork.ArtistRepository.Returns(_mockArtistRepository);
        _mockUnitOfWork.AlbumRepository.Returns(_mockAlbumRepository);
        _mockAuthorizationService = Substitute.For<IAuthorizationService>();
        _mockCurrentUserService = Substitute.For<ICurrentUserService>();
        _mockValidator = Substitute.For<IValidator<GetArtistAlbumsLiteQuery>>();
        _userId = Guid.NewGuid();

        // Default stubs: the current user is authenticated, the library ownership policy allows access and the validator passes.
        _mockCurrentUserService.UserId.Returns(_userId);
        _mockAuthorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(_userId, Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>())
            .Returns(true);
        _mockValidator.Validate(Arg.Any<GetArtistAlbumsLiteQuery>()).Returns([]);

        _sut = new GetArtistAlbumsLiteQueryHandler(_mockUnitOfWork, _mockAuthorizationService, _mockCurrentUserService, _mockValidator);
    }

    [Fact]
    public async Task HandleAsync_WhenArtistExists_ShouldReturnMappedPaginatedAlbumLiteResponses()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        ArtistEntity artist = _artistEntityFixture.Create(id: artistId, libraryId: libraryId, includeAlbums: false, includeContributors: false);
        List<AlbumLiteRow> albumLiteRows = _albumLiteRowFixture.CreateMany(2);
        PaginatedResultDto<AlbumLiteRow> paginatedAlbums = _paginatedResultDtoFixture.Create(data: albumLiteRows, currentPage: 1, perPage: 10, count: 2, numberOfPages: 1);
        _mockArtistRepository.GetByIdAsync(artistId, shouldIncludeNavigationProperties: false, shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<ArtistEntity?>(artist));
        _mockAlbumRepository.GetAlbumsLiteByArtistIdAsync(artistId, Arg.Any<PaginationDataDto?>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(paginatedAlbums));
        GetArtistAlbumsLiteQuery query = _getArtistAlbumsLiteQueryFixture.Create(libraryId: libraryId.ToString(), artistId: artistId.ToString());

        // Act
        Result<PaginatedResponse<AlbumLiteResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(albumLiteRows.Count, result.Value.Data.Count);
        Assert.Equal(albumLiteRows[0].Id, result.Value.Data[0].Id);
        Assert.Equal(albumLiteRows[0].Title, result.Value.Data[0].Title);
        Assert.Equal(albumLiteRows[0].TotalTracks, result.Value.Data[0].TotalTracks);
        Assert.Equal(paginatedAlbums.CurrentPage, result.Value.CurrentPage);
        Assert.Equal(paginatedAlbums.PerPage, result.Value.PerPage);
        Assert.Equal(paginatedAlbums.Count, result.Value.Count);
        Assert.Equal(paginatedAlbums.NumberOfPages, result.Value.NumberOfPages);
    }

    [Fact]
    public async Task HandleAsync_WhenCalled_ShouldForwardTheArtistIdAndPaginationToTheRepository()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        ArtistEntity artist = _artistEntityFixture.Create(id: artistId, libraryId: libraryId, includeAlbums: false, includeContributors: false);
        PaginatedResultDto<AlbumLiteRow> paginatedAlbums = _paginatedResultDtoFixture.Create(data: [], currentPage: 1, perPage: 10, count: 0, numberOfPages: 0);
        _mockArtistRepository.GetByIdAsync(artistId, shouldIncludeNavigationProperties: false, shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<ArtistEntity?>(artist));
        _mockAlbumRepository.GetAlbumsLiteByArtistIdAsync(Arg.Any<Guid>(), Arg.Any<PaginationDataDto?>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(paginatedAlbums));
        GetArtistAlbumsLiteQuery query = _getArtistAlbumsLiteQueryFixture.Create(libraryId: libraryId.ToString(), artistId: artistId.ToString());
        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        await _sut.HandleAsync(query, cancellationToken);

        // Assert
        await _mockAlbumRepository.Received(1).GetAlbumsLiteByArtistIdAsync(
            artistId,
            Arg.Is<PaginationDataDto>(paginationData => paginationData.CurrentPage == query.PaginationData!.CurrentPage &&
                                                        paginationData.PerPage == query.PaginationData.PerPage),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task HandleAsync_WhenArtistRepositoryFails_ShouldReturnFailureResultWithoutReadingAlbums()
    {
        // Arrange
        GetArtistAlbumsLiteQuery query = _getArtistAlbumsLiteQueryFixture.Create();
        _mockArtistRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(ApplicationErrors.Persistence.UniqueConstraintViolation);

        // Act
        Result<PaginatedResponse<AlbumLiteResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Persistence.UniqueConstraintViolation, result.FirstError);
        await _mockAlbumRepository.DidNotReceive().GetAlbumsLiteByArtistIdAsync(Arg.Any<Guid>(), Arg.Any<PaginationDataDto?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenArtistDoesNotExist_ShouldReturnArtistNotFoundError()
    {
        // Arrange
        GetArtistAlbumsLiteQuery query = _getArtistAlbumsLiteQueryFixture.Create();
        _mockArtistRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(_ => Result.From<ArtistEntity?>(null));

        // Act
        Result<PaginatedResponse<AlbumLiteResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.ArtistNotFound, result.FirstError);
        await _mockAuthorizationService.DidNotReceive().EvaluatePolicyAsync<ILibraryOwnershipPolicy>(Arg.Any<Guid>(), Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenArtistBelongsToAnotherLibrary_ShouldReturnArtistNotFoundWithoutEvaluatingThePolicy()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        ArtistEntity artist = _artistEntityFixture.Create(libraryId: Guid.NewGuid(), includeAlbums: false, includeContributors: false);
        GetArtistAlbumsLiteQuery query = _getArtistAlbumsLiteQueryFixture.Create(libraryId: libraryId.ToString(), artistId: artist.Id.ToString());
        _mockArtistRepository.GetByIdAsync(artist.Id, shouldIncludeNavigationProperties: false, shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<ArtistEntity?>(artist));

        // Act
        Result<PaginatedResponse<AlbumLiteResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        // The mismatch is reported as not found, without disclosing that the artist exists in another library.
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.ArtistNotFound, result.FirstError);
        await _mockAuthorizationService.DidNotReceive().EvaluatePolicyAsync<ILibraryOwnershipPolicy>(Arg.Any<Guid>(), Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>());
        await _mockAlbumRepository.DidNotReceive().GetAlbumsLiteByArtistIdAsync(Arg.Any<Guid>(), Arg.Any<PaginationDataDto?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenUserDoesNotOwnTheLibrary_ShouldReturnNotAuthorizedError()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        ArtistEntity artist = _artistEntityFixture.Create(id: artistId, libraryId: libraryId, includeAlbums: false, includeContributors: false);
        GetArtistAlbumsLiteQuery query = _getArtistAlbumsLiteQueryFixture.Create(libraryId: libraryId.ToString(), artistId: artistId.ToString());
        _mockArtistRepository.GetByIdAsync(artistId, shouldIncludeNavigationProperties: false, shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<ArtistEntity?>(artist));
        _mockAuthorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(_userId, Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        Result<PaginatedResponse<AlbumLiteResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockAuthorizationService.Received(1).EvaluatePolicyAsync<ILibraryOwnershipPolicy>(
            _userId, Arg.Is<LibraryOwnershipPolicyContext>(context => context.LibraryId == libraryId), Arg.Any<CancellationToken>());
        await _mockAlbumRepository.DidNotReceive().GetAlbumsLiteByArtistIdAsync(Arg.Any<Guid>(), Arg.Any<PaginationDataDto?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenAlbumRepositoryFails_ShouldReturnFailureResult()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        ArtistEntity artist = _artistEntityFixture.Create(id: artistId, libraryId: libraryId, includeAlbums: false, includeContributors: false);
        GetArtistAlbumsLiteQuery query = _getArtistAlbumsLiteQueryFixture.Create(libraryId: libraryId.ToString(), artistId: artistId.ToString());
        _mockArtistRepository.GetByIdAsync(artistId, shouldIncludeNavigationProperties: false, shouldTrackEntities: false, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<ArtistEntity?>(artist));
        _mockAlbumRepository.GetAlbumsLiteByArtistIdAsync(artistId, Arg.Any<PaginationDataDto?>(), Arg.Any<CancellationToken>())
            .Returns(ApplicationErrors.Persistence.UniqueConstraintViolation);

        // Act
        Result<PaginatedResponse<AlbumLiteResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Persistence.UniqueConstraintViolation, result.FirstError);
    }

    [Fact]
    public async Task HandleAsync_WhenUserIsNotAuthenticated_ShouldReturnNotAuthorizedError()
    {
        // Arrange
        GetArtistAlbumsLiteQuery query = _getArtistAlbumsLiteQueryFixture.Create();
        _mockCurrentUserService.UserId.Returns((Guid?)null);

        // Act
        Result<PaginatedResponse<AlbumLiteResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockArtistRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenValidationFails_ShouldReturnValidationErrorsWithoutQuerying()
    {
        // Arrange
        GetArtistAlbumsLiteQuery query = _getArtistAlbumsLiteQueryFixture.Create();
        _mockValidator.Validate(Arg.Any<GetArtistAlbumsLiteQuery>()).Returns([Errors.Music.ArtistIdCannotBeEmpty]);

        // Act
        Result<PaginatedResponse<AlbumLiteResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.ArtistIdCannotBeEmpty, result.FirstError);
        await _mockArtistRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
        await _mockAlbumRepository.DidNotReceive().GetAlbumsLiteByArtistIdAsync(Arg.Any<Guid>(), Arg.Any<PaginationDataDto?>(), Arg.Any<CancellationToken>());
    }
}
