#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Repositories.Favorites;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Core.Favorites.Queries.GetFavoriteStatus;
using Lumina.Application.Fixtures.Core.Favorites.Queries.GetFavoriteStatus;
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

namespace Lumina.Application.UnitTests.Core.Favorites.Queries.GetFavoriteStatus;

/// <summary>
/// Contains unit tests for the <see cref="GetFavoriteStatusQueryHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetFavoriteStatusQueryHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly ICurrentUserService _mockCurrentUserService;
    private readonly IFavoriteRepository _mockFavoriteRepository;
    private readonly IValidator<GetFavoriteStatusQuery> _mockValidator;
    private readonly GetFavoriteStatusQueryHandler _sut;
    private readonly GetFavoriteStatusQueryFixture _getFavoriteStatusQueryFixture = new();
    private readonly Guid _userId = Guid.NewGuid();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetFavoriteStatusQueryHandlerTests"/> class.
    /// </summary>
    public GetFavoriteStatusQueryHandlerTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockCurrentUserService = Substitute.For<ICurrentUserService>();
        _mockFavoriteRepository = Substitute.For<IFavoriteRepository>();

        _mockUnitOfWork.FavoriteRepository.Returns(_mockFavoriteRepository);
        _mockCurrentUserService.UserId.Returns(_userId);
        _mockFavoriteRepository.GetFavoritedMediaItemIdsForUserAsync(Arg.Any<Guid>(), Arg.Any<FavoriteMediaKind>(), Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<Guid>>([]));

        _mockValidator = Substitute.For<IValidator<GetFavoriteStatusQuery>>();
        _mockValidator.Validate(Arg.Any<GetFavoriteStatusQuery>()).Returns([]);

        _sut = new GetFavoriteStatusQueryHandler(_mockUnitOfWork, _mockCurrentUserService, _mockValidator);
    }

    [Fact]
    public async Task HandleAsync_WhenValidatorReturnsError_ShouldReturnValidationErrorAndNotReadTheRepositories()
    {
        // Arrange
        GetFavoriteStatusQuery query = _getFavoriteStatusQueryFixture.Create();
        Error error = DomainErrors.Favorites.FavoriteMediaItemIdsCannotBeEmpty;
        _mockValidator.Validate(Arg.Any<GetFavoriteStatusQuery>()).Returns([error]);

        // Act
        Result<FavoriteStatusResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
        _ = _mockCurrentUserService.DidNotReceive().UserId;
        await _mockFavoriteRepository.DidNotReceive().GetFavoritedMediaItemIdsForUserAsync(Arg.Any<Guid>(), Arg.Any<FavoriteMediaKind>(), Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenCurrentUserIsNull_ShouldReturnNotAuthorizedAndNotReadTheRepositories()
    {
        // Arrange
        GetFavoriteStatusQuery query = _getFavoriteStatusQueryFixture.Create();
        _mockCurrentUserService.UserId.Returns((Guid?)null);

        // Act
        Result<FavoriteStatusResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockFavoriteRepository.DidNotReceive().GetFavoritedMediaItemIdsForUserAsync(Arg.Any<Guid>(), Arg.Any<FavoriteMediaKind>(), Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenTheRepositoryFails_ShouldReturnThatError()
    {
        // Arrange
        GetFavoriteStatusQuery query = _getFavoriteStatusQueryFixture.Create();
        Error error = Error.Failure("Persistence.Failure", "The favorite status could not be read.");
        _mockFavoriteRepository.GetFavoritedMediaItemIdsForUserAsync(Arg.Any<Guid>(), Arg.Any<FavoriteMediaKind>(), Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        Result<FavoriteStatusResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
    }

    [Fact]
    public async Task HandleAsync_WhenTheQueryContainsDuplicateIds_ShouldCheckEveryDistinctIdAndReturnTheFavoritedIds()
    {
        // Arrange
        Guid firstId = Guid.NewGuid();
        Guid secondId = Guid.NewGuid();
        Guid thirdId = Guid.NewGuid();
        List<Guid> requestedIds = [firstId, secondId, firstId, thirdId, secondId];
        GetFavoriteStatusQuery query = _getFavoriteStatusQueryFixture.Create(
            mediaKind: FavoriteMediaKind.MusicAlbum,
            mediaItemIds: requestedIds);
        List<Guid> favoritedIds = [firstId, thirdId];
        _mockFavoriteRepository.GetFavoritedMediaItemIdsForUserAsync(Arg.Any<Guid>(), Arg.Any<FavoriteMediaKind>(), Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<Guid>>(favoritedIds));

        // Act
        Result<FavoriteStatusResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(favoritedIds, result.Value.FavoritedMediaItemIds);
        await _mockFavoriteRepository.Received(1).GetFavoritedMediaItemIdsForUserAsync(
            _userId,
            FavoriteMediaKind.MusicAlbum,
            Arg.Is<IReadOnlyCollection<Guid>>(ids => ids.Count == 3 && ids.Contains(firstId) && ids.Contains(secondId) && ids.Contains(thirdId)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenNoRequestedItemIsFavorited_ShouldReturnAnEmptyStatusList()
    {
        // Arrange
        GetFavoriteStatusQuery query = _getFavoriteStatusQueryFixture.Create();

        // Act
        Result<FavoriteStatusResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Empty(result.Value.FavoritedMediaItemIds);
    }

    [Fact]
    public async Task HandleAsync_WhenCalled_ShouldForwardTheCurrentUserTheMediaKindAndTheCancellationToken()
    {
        // Arrange
        GetFavoriteStatusQuery query = _getFavoriteStatusQueryFixture.Create(mediaKind: FavoriteMediaKind.Book);
        using CancellationTokenSource cancellationTokenSource = new();
        CancellationToken cancellationToken = cancellationTokenSource.Token;

        // Act
        await _sut.HandleAsync(query, cancellationToken);

        // Assert
        await _mockFavoriteRepository.Received(1).GetFavoritedMediaItemIdsForUserAsync(
            _userId,
            FavoriteMediaKind.Book,
            Arg.Any<IReadOnlyCollection<Guid>>(),
            Arg.Is(cancellationToken));
    }
}
