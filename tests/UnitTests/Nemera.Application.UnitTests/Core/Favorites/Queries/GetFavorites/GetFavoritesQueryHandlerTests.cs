#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Favorites;
using Lumina.Application.Common.DataAccess.Repositories.Favorites;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.DTO.Favorites;
using Lumina.Application.Common.DTO.Pagination;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Infrastructure.Authorization.Policies.Common.Base;
using Lumina.Application.Common.Infrastructure.Authorization.Policies.LibraryOwnership;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Core.Favorites.Queries.GetFavorites;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Favorites;
using Lumina.Application.Fixtures.Common.DTO.Favorites;
using Lumina.Application.Fixtures.Common.DTO.Pagination;
using Lumina.Application.Fixtures.Core.Favorites.Queries.GetFavorites;
using Lumina.Contracts.Responses.Common;
using Lumina.Contracts.Responses.Favorites;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.Favorites;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
using DomainErrors = Lumina.Domain.Common.Errors.Errors;
#endregion

namespace Lumina.Application.UnitTests.Core.Favorites.Queries.GetFavorites;

/// <summary>
/// Contains unit tests for the <see cref="GetFavoritesQueryHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetFavoritesQueryHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly ICurrentUserService _mockCurrentUserService;
    private readonly IAuthorizationService _mockAuthorizationService;
    private readonly IFavoriteRepository _mockFavoriteRepository;
    private readonly IValidator<GetFavoritesQuery> _mockValidator;
    private readonly GetFavoritesQueryHandler _sut;
    private readonly GetFavoritesQueryFixture _getFavoritesQueryFixture = new();
    private readonly FavoriteEntityFixture _favoriteEntityFixture = new();
    private readonly FavoriteItemDtoFixture _favoriteItemDtoFixture = new();
    private readonly FavoriteItemDisplayDtoFixture _favoriteItemDisplayDtoFixture = new();
    private readonly PaginatedResultDtoFixture<FavoriteItemDto> _paginatedResultDtoFixture = new();
    private readonly Guid _userId = Guid.NewGuid();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetFavoritesQueryHandlerTests"/> class.
    /// </summary>
    public GetFavoritesQueryHandlerTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockCurrentUserService = Substitute.For<ICurrentUserService>();
        _mockAuthorizationService = Substitute.For<IAuthorizationService>();
        _mockFavoriteRepository = Substitute.For<IFavoriteRepository>();

        _mockUnitOfWork.FavoriteRepository.Returns(_mockFavoriteRepository);
        _mockCurrentUserService.UserId.Returns(_userId);
        _mockAuthorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(Arg.Any<Guid>(), Arg.Any<PolicyContext>(), Arg.Any<CancellationToken>())
            .Returns(true);
        _mockFavoriteRepository.GetAllForUserAsync(Arg.Any<Guid>(), Arg.Any<FavoriteMediaKind?>(), Arg.Any<PaginationDataDto?>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_paginatedResultDtoFixture.Create()));

        _mockValidator = Substitute.For<IValidator<GetFavoritesQuery>>();
        _mockValidator.Validate(Arg.Any<GetFavoritesQuery>()).Returns([]);

        _sut = new GetFavoritesQueryHandler(_mockUnitOfWork, _mockCurrentUserService, _mockAuthorizationService, _mockValidator);
    }

    [Fact]
    public async Task HandleAsync_WhenValidatorReturnsError_ShouldReturnValidationErrorAndNotReadTheRepositoriesOrAuthorize()
    {
        // Arrange
        GetFavoritesQuery query = _getFavoritesQueryFixture.Create();
        Error error = DomainErrors.Favorites.FavoriteMediaKindIsInvalid;
        _mockValidator.Validate(Arg.Any<GetFavoritesQuery>()).Returns([error]);

        // Act
        Result<PaginatedResponse<FavoriteItemResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
        _ = _mockCurrentUserService.DidNotReceive().UserId;
        await _mockFavoriteRepository.DidNotReceive().GetAllForUserAsync(Arg.Any<Guid>(), Arg.Any<FavoriteMediaKind?>(), Arg.Any<PaginationDataDto?>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
        await _mockAuthorizationService.DidNotReceive().EvaluatePolicyAsync<ILibraryOwnershipPolicy>(Arg.Any<Guid>(), Arg.Any<PolicyContext>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenCurrentUserIsNull_ShouldReturnNotAuthorizedAndNotReadTheRepositories()
    {
        // Arrange
        GetFavoritesQuery query = _getFavoritesQueryFixture.Create();
        _mockCurrentUserService.UserId.Returns((Guid?)null);

        // Act
        Result<PaginatedResponse<FavoriteItemResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockFavoriteRepository.DidNotReceive().GetAllForUserAsync(Arg.Any<Guid>(), Arg.Any<FavoriteMediaKind?>(), Arg.Any<PaginationDataDto?>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenTheRepositoryFails_ShouldReturnThatErrorAndNotAuthorize()
    {
        // Arrange
        GetFavoritesQuery query = _getFavoritesQueryFixture.Create();
        Error error = Error.Failure("Persistence.Failure", "The favorites could not be read.");
        _mockFavoriteRepository.GetAllForUserAsync(Arg.Any<Guid>(), Arg.Any<FavoriteMediaKind?>(), Arg.Any<PaginationDataDto?>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        Result<PaginatedResponse<FavoriteItemResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
        await _mockAuthorizationService.DidNotReceive().EvaluatePolicyAsync<ILibraryOwnershipPolicy>(Arg.Any<Guid>(), Arg.Any<PolicyContext>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenTheItemsHaveNoDisplayData_ShouldKeepEveryRowUnavailableWithEmptyDisplayFields()
    {
        // Arrange
        FavoriteItemDto firstItem = _favoriteItemDtoFixture.Create(includeDisplay: false);
        FavoriteItemDto secondItem = _favoriteItemDtoFixture.Create(includeDisplay: false);
        PaginatedResultDto<FavoriteItemDto> page = _paginatedResultDtoFixture.Create(
            data: [firstItem, secondItem],
            currentPage: 2,
            perPage: 5,
            count: 12,
            numberOfPages: 3);
        GetFavoritesQuery query = _getFavoritesQueryFixture.Create();
        _mockFavoriteRepository.GetAllForUserAsync(Arg.Any<Guid>(), Arg.Any<FavoriteMediaKind?>(), Arg.Any<PaginationDataDto?>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(page));

        // Act
        Result<PaginatedResponse<FavoriteItemResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(2, result.Value.Data.Count);
        Assert.Equal(2, result.Value.CurrentPage);
        Assert.Equal(5, result.Value.PerPage);
        Assert.Equal(12, result.Value.Count);
        Assert.Equal(3, result.Value.NumberOfPages);
        Assert.All(result.Value.Data, item =>
        {
            Assert.False(item.IsAvailable);
            Assert.Equal(Guid.Empty, item.LibraryId);
            Assert.Equal(string.Empty, item.Title);
            Assert.Equal(Guid.Empty, item.CollectionId);
            Assert.Equal(string.Empty, item.CollectionTitle);
            Assert.Equal(Guid.Empty, item.CreatorId);
            Assert.Equal(string.Empty, item.CreatorName);
            Assert.Null(item.CoverPath);
        });
        Assert.Equal<Guid>([firstItem.Favorite.MediaItemId, secondItem.Favorite.MediaItemId], result.Value.Data.Select(item => item.MediaItemId));
        await _mockAuthorizationService.DidNotReceive().EvaluatePolicyAsync<ILibraryOwnershipPolicy>(Arg.Any<Guid>(), Arg.Any<PolicyContext>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenTheItemsHaveDisplayData_ShouldMapEveryDisplayFieldAndPreserveThePaginationMetadata()
    {
        // Arrange
        FavoriteEntity favorite = _favoriteEntityFixture.Create(mediaKind: FavoriteMediaKind.MusicTrack);
        FavoriteItemDisplayDto display = _favoriteItemDisplayDtoFixture.Create();
        FavoriteItemDto item = _favoriteItemDtoFixture.Create(favorite: favorite, display: display);
        PaginatedResultDto<FavoriteItemDto> page = _paginatedResultDtoFixture.Create(
            data: [item],
            currentPage: 4,
            perPage: 10,
            count: 31,
            numberOfPages: 4);
        GetFavoritesQuery query = _getFavoritesQueryFixture.Create();
        _mockFavoriteRepository.GetAllForUserAsync(Arg.Any<Guid>(), Arg.Any<FavoriteMediaKind?>(), Arg.Any<PaginationDataDto?>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(page));

        // Act
        Result<PaginatedResponse<FavoriteItemResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        FavoriteItemResponse response = Assert.Single(result.Value.Data);
        Assert.Equal(favorite.MediaKind, response.MediaKind);
        Assert.Equal(favorite.MediaItemId, response.MediaItemId);
        Assert.True(response.IsAvailable);
        Assert.Equal(display.LibraryId, response.LibraryId);
        Assert.Equal(display.Title, response.Title);
        Assert.Equal(display.CollectionId, response.CollectionId);
        Assert.Equal(display.CollectionTitle, response.CollectionTitle);
        Assert.Equal(display.CreatorId, response.CreatorId);
        Assert.Equal(display.CreatorName, response.CreatorName);
        Assert.Equal(display.CoverPath, response.CoverPath);
        Assert.Equal(4, result.Value.CurrentPage);
        Assert.Equal(10, result.Value.PerPage);
        Assert.Equal(31, result.Value.Count);
        Assert.Equal(4, result.Value.NumberOfPages);
    }

    [Fact]
    public async Task HandleAsync_WhenSeveralItemsShareOneLibrary_ShouldEvaluateTheLibraryPolicyOnceForThatLibrary()
    {
        // Arrange
        Guid sharedLibraryId = Guid.NewGuid();
        FavoriteItemDto firstItem = _favoriteItemDtoFixture.Create(
            display: _favoriteItemDisplayDtoFixture.Create(libraryId: sharedLibraryId));
        FavoriteItemDto secondItem = _favoriteItemDtoFixture.Create(
            display: _favoriteItemDisplayDtoFixture.Create(libraryId: sharedLibraryId));
        PaginatedResultDto<FavoriteItemDto> page = _paginatedResultDtoFixture.Create(data: [firstItem, secondItem]);
        GetFavoritesQuery query = _getFavoritesQueryFixture.Create();
        _mockFavoriteRepository.GetAllForUserAsync(Arg.Any<Guid>(), Arg.Any<FavoriteMediaKind?>(), Arg.Any<PaginationDataDto?>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(page));

        // Act
        Result<PaginatedResponse<FavoriteItemResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        await _mockAuthorizationService.Received(1).EvaluatePolicyAsync<ILibraryOwnershipPolicy>(
            _userId,
            Arg.Is<PolicyContext>(context => context is LibraryOwnershipPolicyContext && ((LibraryOwnershipPolicyContext)context).LibraryId == sharedLibraryId),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenOneLibraryIsDenied_ShouldKeepItsRowsWithoutDisplayDataAndLeaveTheAccessibleRowsUnchanged()
    {
        // Arrange
        Guid accessibleLibraryId = Guid.NewGuid();
        Guid deniedLibraryId = Guid.NewGuid();
        FavoriteItemDisplayDto accessibleDisplay = _favoriteItemDisplayDtoFixture.Create(libraryId: accessibleLibraryId);
        FavoriteItemDto accessibleItem = _favoriteItemDtoFixture.Create(display: accessibleDisplay);
        FavoriteItemDisplayDto deniedDisplay = _favoriteItemDisplayDtoFixture.Create(libraryId: deniedLibraryId, title: "denied title");
        FavoriteItemDto deniedItem = _favoriteItemDtoFixture.Create(display: deniedDisplay);
        FavoriteItemDto itemWithoutDisplay = _favoriteItemDtoFixture.Create(includeDisplay: false);
        PaginatedResultDto<FavoriteItemDto> page = _paginatedResultDtoFixture.Create(
            data: [accessibleItem, deniedItem, itemWithoutDisplay]);
        GetFavoritesQuery query = _getFavoritesQueryFixture.Create();
        _mockFavoriteRepository.GetAllForUserAsync(Arg.Any<Guid>(), Arg.Any<FavoriteMediaKind?>(), Arg.Any<PaginationDataDto?>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(page));
        _mockAuthorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(
            _userId,
            Arg.Is<PolicyContext>(context => context is LibraryOwnershipPolicyContext && ((LibraryOwnershipPolicyContext)context).LibraryId == deniedLibraryId),
            Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        Result<PaginatedResponse<FavoriteItemResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(3, result.Value.Data.Count);
        Assert.Collection(
            result.Value.Data,
            item =>
            {
                Assert.True(item.IsAvailable);
                Assert.Equal(accessibleDisplay.Title, item.Title);
                Assert.Equal(accessibleDisplay.LibraryId, item.LibraryId);
            },
            item =>
            {
                Assert.False(item.IsAvailable);
                Assert.Equal(Guid.Empty, item.LibraryId);
                Assert.Equal(string.Empty, item.Title);
                Assert.Null(item.CoverPath);
            },
            item =>
            {
                Assert.False(item.IsAvailable);
                Assert.Equal(Guid.Empty, item.LibraryId);
                Assert.Equal(string.Empty, item.Title);
            });
        Assert.Equal(deniedItem.Favorite.MediaItemId, result.Value.Data[1].MediaItemId);
    }

    [Fact]
    public async Task HandleAsync_WhenNoRowHasDisplayData_ShouldNotEvaluateAnyLibraryPolicy()
    {
        // Arrange
        PaginatedResultDto<FavoriteItemDto> page = _paginatedResultDtoFixture.Create(
            data: [_favoriteItemDtoFixture.Create(includeDisplay: false)]);
        GetFavoritesQuery query = _getFavoritesQueryFixture.Create();
        _mockFavoriteRepository.GetAllForUserAsync(Arg.Any<Guid>(), Arg.Any<FavoriteMediaKind?>(), Arg.Any<PaginationDataDto?>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(page));

        // Act
        await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        await _mockAuthorizationService.DidNotReceive().EvaluatePolicyAsync<ILibraryOwnershipPolicy>(Arg.Any<Guid>(), Arg.Any<PolicyContext>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenCalled_ShouldPassTheCurrentUserTheMediaKindAndThePaginationDataToTheRepositoryAndForwardTheCancellationToken()
    {
        // Arrange
        PaginationDataDto paginationData = new() { CurrentPage = 3, PerPage = 15 };
        GetFavoritesQuery query = _getFavoritesQueryFixture.Create(
            mediaKind: FavoriteMediaKind.Book,
            paginationData: paginationData);
        using CancellationTokenSource cancellationTokenSource = new();
        CancellationToken cancellationToken = cancellationTokenSource.Token;

        // Act
        await _sut.HandleAsync(query, cancellationToken);

        // Assert
        await _mockFavoriteRepository.Received(1).GetAllForUserAsync(
            _userId,
            FavoriteMediaKind.Book,
            Arg.Is<PaginationDataDto>(data => ReferenceEquals(data, paginationData)),
            Arg.Any<bool>(),
            Arg.Is(cancellationToken));
    }
}
