#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Favorites;
using Lumina.Application.Common.DataAccess.Repositories.Favorites;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.DomainEvents;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Core.Favorites.Commands.RemoveFavorite;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Favorites;
using Lumina.Application.Fixtures.Core.Favorites.Commands.RemoveFavorite;
using Lumina.Domain.Common.Events;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.FavoriteBoundedContext.FavoriteAggregate.Events;
using Lumina.Domain.SharedKernel.Common.Enums.Favorites;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
using DomainErrors = Lumina.Domain.Common.Errors.Errors;
#endregion

namespace Lumina.Application.UnitTests.Core.Favorites.Commands.RemoveFavorite;

/// <summary>
/// Contains unit tests for the <see cref="RemoveFavoriteCommandHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class RemoveFavoriteCommandHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly ICurrentUserService _mockCurrentUserService;
    private readonly IDomainEventsQueue _mockDomainEventsQueue;
    private readonly IFavoriteRepository _mockFavoriteRepository;
    private readonly IValidator<RemoveFavoriteCommand> _mockValidator;
    private readonly RemoveFavoriteCommandHandler _sut;
    private readonly RemoveFavoriteCommandFixture _removeFavoriteCommandFixture = new();
    private readonly FavoriteEntityFixture _favoriteEntityFixture = new();
    private readonly Guid _userId = Guid.NewGuid();

    /// <summary>
    /// Initializes a new instance of the <see cref="RemoveFavoriteCommandHandlerTests"/> class.
    /// </summary>
    public RemoveFavoriteCommandHandlerTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockCurrentUserService = Substitute.For<ICurrentUserService>();
        _mockDomainEventsQueue = Substitute.For<IDomainEventsQueue>();
        _mockFavoriteRepository = Substitute.For<IFavoriteRepository>();

        _mockUnitOfWork.FavoriteRepository.Returns(_mockFavoriteRepository);
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Result.Success);
        _mockCurrentUserService.UserId.Returns(_userId);
        _mockFavoriteRepository.GetByMediaItemIdForUserAsync(Arg.Any<Guid>(), Arg.Any<FavoriteMediaKind>(), Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<FavoriteEntity?>(null));
        _mockFavoriteRepository.DeleteByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Result.Deleted);

        _mockValidator = Substitute.For<IValidator<RemoveFavoriteCommand>>();
        _mockValidator.Validate(Arg.Any<RemoveFavoriteCommand>()).Returns([]);

        _sut = new RemoveFavoriteCommandHandler(_mockUnitOfWork, _mockCurrentUserService, _mockDomainEventsQueue, _mockValidator);
    }

    [Fact]
    public async Task HandleAsync_WhenValidatorReturnsError_ShouldReturnValidationErrorAndNotTouchTheRepositories()
    {
        // Arrange
        RemoveFavoriteCommand command = _removeFavoriteCommandFixture.Create();
        Error error = DomainErrors.Favorites.FavoriteMediaKindIsInvalid;
        _mockValidator.Validate(Arg.Any<RemoveFavoriteCommand>()).Returns([error]);

        // Act
        Result<Deleted> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
        _ = _mockCurrentUserService.DidNotReceive().UserId;
        await _mockFavoriteRepository.DidNotReceive().GetByMediaItemIdForUserAsync(Arg.Any<Guid>(), Arg.Any<FavoriteMediaKind>(), Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
        await _mockFavoriteRepository.DidNotReceive().DeleteByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        _mockDomainEventsQueue.DidNotReceive().Enqueue(Arg.Any<IDomainEvent>());
    }

    [Fact]
    public async Task HandleAsync_WhenCurrentUserIsNull_ShouldReturnNotAuthorizedAndNotReadTheFavorite()
    {
        // Arrange
        RemoveFavoriteCommand command = _removeFavoriteCommandFixture.Create();
        _mockCurrentUserService.UserId.Returns((Guid?)null);

        // Act
        Result<Deleted> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockFavoriteRepository.DidNotReceive().GetByMediaItemIdForUserAsync(Arg.Any<Guid>(), Arg.Any<FavoriteMediaKind>(), Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenTheFavoriteReadFails_ShouldReturnThatError()
    {
        // Arrange
        RemoveFavoriteCommand command = _removeFavoriteCommandFixture.Create();
        Error error = Error.Failure("Persistence.Failure", "The favorite could not be read.");
        _mockFavoriteRepository.GetByMediaItemIdForUserAsync(Arg.Any<Guid>(), Arg.Any<FavoriteMediaKind>(), Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        Result<Deleted> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
        await _mockFavoriteRepository.DidNotReceive().DeleteByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        _mockDomainEventsQueue.DidNotReceive().Enqueue(Arg.Any<IDomainEvent>());
    }

    [Fact]
    public async Task HandleAsync_WhenTheFavoriteIsAbsent_ShouldReturnDeletedWithoutDeletingOrRaisingAnEvent()
    {
        // Arrange
        RemoveFavoriteCommand command = _removeFavoriteCommandFixture.Create();

        // Act
        Result<Deleted> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Deleted, result.Value);
        await _mockFavoriteRepository.DidNotReceive().DeleteByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        _mockDomainEventsQueue.DidNotReceive().Enqueue(Arg.Any<IDomainEvent>());
    }

    [Fact]
    public async Task HandleAsync_WhenDeleteFails_ShouldReturnThatErrorAndNotSaveChanges()
    {
        // Arrange
        RemoveFavoriteCommand command = _removeFavoriteCommandFixture.Create();
        FavoriteEntity favorite = CreateFavoriteFrom(command);
        Error error = Error.Failure("Persistence.Failure", "The favorite could not be deleted.");
        _mockFavoriteRepository.GetByMediaItemIdForUserAsync(Arg.Any<Guid>(), Arg.Any<FavoriteMediaKind>(), Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<FavoriteEntity?>(favorite));
        _mockFavoriteRepository.DeleteByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        Result<Deleted> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        _mockDomainEventsQueue.DidNotReceive().Enqueue(Arg.Any<IDomainEvent>());
    }

    [Fact]
    public async Task HandleAsync_WhenSaveFailsWithANonConcurrencyError_ShouldReturnThatErrorAndNotRaiseAnEvent()
    {
        // Arrange
        RemoveFavoriteCommand command = _removeFavoriteCommandFixture.Create();
        FavoriteEntity favorite = CreateFavoriteFrom(command);
        Error error = Error.Failure("Persistence.Failure", "The changes could not be saved.");
        _mockFavoriteRepository.GetByMediaItemIdForUserAsync(Arg.Any<Guid>(), Arg.Any<FavoriteMediaKind>(), Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<FavoriteEntity?>(favorite));
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(error);

        // Act
        Result<Deleted> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
        _mockUnitOfWork.DidNotReceive().ClearTrackedEntities();
        _mockDomainEventsQueue.DidNotReceive().Enqueue(Arg.Any<IDomainEvent>());
    }

    [Fact]
    public async Task HandleAsync_WhenSaveFailsWithAConcurrencyConflictAndTheFavoriteStillExists_ShouldReturnTheSaveError()
    {
        // Arrange
        RemoveFavoriteCommand command = _removeFavoriteCommandFixture.Create();
        FavoriteEntity favorite = CreateFavoriteFrom(command);
        _mockFavoriteRepository.GetByMediaItemIdForUserAsync(Arg.Any<Guid>(), Arg.Any<FavoriteMediaKind>(), Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<FavoriteEntity?>(favorite), Result.From<FavoriteEntity?>(favorite));
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(ApplicationErrors.Persistence.ConcurrencyConflict);

        // Act
        Result<Deleted> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Persistence.ConcurrencyConflict, result.FirstError);
        _mockUnitOfWork.Received(1).ClearTrackedEntities();
        _mockDomainEventsQueue.DidNotReceive().Enqueue(Arg.Any<IDomainEvent>());
    }

    [Fact]
    public async Task HandleAsync_WhenSaveFailsWithAConcurrencyConflictAndTheFavoriteIsGone_ShouldReturnDeletedWithoutRaisingAnEvent()
    {
        // Arrange
        RemoveFavoriteCommand command = _removeFavoriteCommandFixture.Create();
        FavoriteEntity favorite = CreateFavoriteFrom(command);
        _mockFavoriteRepository.GetByMediaItemIdForUserAsync(Arg.Any<Guid>(), Arg.Any<FavoriteMediaKind>(), Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<FavoriteEntity?>(favorite), Result.From<FavoriteEntity?>(null));
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(ApplicationErrors.Persistence.ConcurrencyConflict);

        // Act
        Result<Deleted> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Deleted, result.Value);
        _mockUnitOfWork.Received(1).ClearTrackedEntities();
        _mockDomainEventsQueue.DidNotReceive().Enqueue(Arg.Any<IDomainEvent>());
    }

    [Fact]
    public async Task HandleAsync_WhenSaveFailsWithAConcurrencyConflictAndTheReReadFails_ShouldReturnThatError()
    {
        // Arrange
        RemoveFavoriteCommand command = _removeFavoriteCommandFixture.Create();
        FavoriteEntity favorite = CreateFavoriteFrom(command);
        Error error = Error.Failure("Persistence.Failure", "The remaining favorite could not be read.");
        _mockFavoriteRepository.GetByMediaItemIdForUserAsync(Arg.Any<Guid>(), Arg.Any<FavoriteMediaKind>(), Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<FavoriteEntity?>(favorite), error);
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(ApplicationErrors.Persistence.ConcurrencyConflict);

        // Act
        Result<Deleted> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
        _mockUnitOfWork.Received(1).ClearTrackedEntities();
        _mockDomainEventsQueue.DidNotReceive().Enqueue(Arg.Any<IDomainEvent>());
    }

    [Fact]
    public async Task HandleAsync_WhenSuccessful_ShouldDeleteTheFavoriteSaveOnceAndRaiseExactlyOneRemovedEvent()
    {
        // Arrange
        RemoveFavoriteCommand command = _removeFavoriteCommandFixture.Create(mediaKind: nameof(FavoriteMediaKind.Book));
        FavoriteEntity favorite = CreateFavoriteFrom(command);
        _mockFavoriteRepository.GetByMediaItemIdForUserAsync(Arg.Any<Guid>(), Arg.Any<FavoriteMediaKind>(), Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<FavoriteEntity?>(favorite));

        // Act
        Result<Deleted> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Deleted, result.Value);
        await _mockFavoriteRepository.Received(1).GetByMediaItemIdForUserAsync(_userId, FavoriteMediaKind.Book, favorite.MediaItemId, Arg.Is<bool>(shouldTrackEntities => shouldTrackEntities), Arg.Any<CancellationToken>());
        await _mockFavoriteRepository.Received(1).DeleteByIdAsync(favorite.Id, Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        _mockDomainEventsQueue.Received(1).Enqueue(Arg.Is<IDomainEvent>(domainEvent => domainEvent is FavoriteRemovedDomainEvent));
    }

    [Fact]
    public async Task HandleAsync_WhenCalled_ShouldScopeEveryCallToTheStoredFavoriteAndForwardTheCancellationToken()
    {
        // Arrange
        RemoveFavoriteCommand command = _removeFavoriteCommandFixture.Create();
        FavoriteEntity favorite = CreateFavoriteFrom(command);
        using CancellationTokenSource cancellationTokenSource = new();
        CancellationToken cancellationToken = cancellationTokenSource.Token;
        _mockFavoriteRepository.GetByMediaItemIdForUserAsync(Arg.Any<Guid>(), Arg.Any<FavoriteMediaKind>(), Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<FavoriteEntity?>(favorite));

        // Act
        await _sut.HandleAsync(command, cancellationToken);

        // Assert
        await _mockFavoriteRepository.Received(1).GetByMediaItemIdForUserAsync(_userId, favorite.MediaKind, favorite.MediaItemId, Arg.Is<bool>(shouldTrackEntities => shouldTrackEntities), Arg.Is(cancellationToken));
        await _mockFavoriteRepository.Received(1).DeleteByIdAsync(favorite.Id, Arg.Is(cancellationToken));
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Is(cancellationToken));
    }

    /// <summary>
    /// Creates a stored favorite entity that matches the media kind and media item of <paramref name="command"/>.
    /// </summary>
    /// <param name="command">The command whose route values describe the favorite.</param>
    /// <returns>The created <see cref="FavoriteEntity"/>.</returns>
    private FavoriteEntity CreateFavoriteFrom(RemoveFavoriteCommand command)
    {
        return _favoriteEntityFixture.Create(
            userId: _userId,
            mediaKind: Enum.Parse<FavoriteMediaKind>(command.MediaKind!, ignoreCase: true),
            mediaItemId: Guid.Parse(command.MediaItemId!));
    }
}
