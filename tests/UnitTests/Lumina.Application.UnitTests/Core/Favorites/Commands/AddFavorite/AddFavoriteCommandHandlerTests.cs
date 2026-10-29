#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Favorites;
using Lumina.Application.Common.DataAccess.Repositories.Favorites;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.DomainEvents;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Infrastructure.Authorization.Policies.Common.Base;
using Lumina.Application.Common.Infrastructure.Authorization.Policies.LibraryOwnership;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Core.Favorites.Commands.AddFavorite;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Favorites;
using Lumina.Application.Fixtures.Core.Favorites.Commands.AddFavorite;
using Lumina.Contracts.Responses.Favorites;
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

namespace Lumina.Application.UnitTests.Core.Favorites.Commands.AddFavorite;

/// <summary>
/// Contains unit tests for the <see cref="AddFavoriteCommandHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class AddFavoriteCommandHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly ICurrentUserService _mockCurrentUserService;
    private readonly IAuthorizationService _mockAuthorizationService;
    private readonly IDomainEventsQueue _mockDomainEventsQueue;
    private readonly IFavoriteRepository _mockFavoriteRepository;
    private readonly IValidator<AddFavoriteCommand> _mockValidator;
    private readonly AddFavoriteCommandHandler _sut;
    private readonly AddFavoriteCommandFixture _addFavoriteCommandFixture = new();
    private readonly FavoriteEntityFixture _favoriteEntityFixture = new();
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _libraryId = Guid.NewGuid();

    /// <summary>
    /// Initializes a new instance of the <see cref="AddFavoriteCommandHandlerTests"/> class.
    /// </summary>
    public AddFavoriteCommandHandlerTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockCurrentUserService = Substitute.For<ICurrentUserService>();
        _mockAuthorizationService = Substitute.For<IAuthorizationService>();
        _mockDomainEventsQueue = Substitute.For<IDomainEventsQueue>();
        _mockFavoriteRepository = Substitute.For<IFavoriteRepository>();

        _mockUnitOfWork.FavoriteRepository.Returns(_mockFavoriteRepository);
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Result.Success);
        _mockCurrentUserService.UserId.Returns(_userId);
        _mockAuthorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(Arg.Any<Guid>(), Arg.Any<PolicyContext>(), Arg.Any<CancellationToken>())
            .Returns(true);
        _mockFavoriteRepository.GetMediaItemLibraryIdAsync(Arg.Any<FavoriteMediaKind>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<Guid?>(_libraryId));
        _mockFavoriteRepository.GetByMediaItemIdForUserAsync(Arg.Any<Guid>(), Arg.Any<FavoriteMediaKind>(), Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<FavoriteEntity?>(null));
        _mockFavoriteRepository.InsertAsync(Arg.Any<FavoriteEntity>(), Arg.Any<CancellationToken>())
            .Returns(Result.Created);

        _mockValidator = Substitute.For<IValidator<AddFavoriteCommand>>();
        _mockValidator.Validate(Arg.Any<AddFavoriteCommand>()).Returns([]);

        _sut = new AddFavoriteCommandHandler(_mockUnitOfWork, _mockCurrentUserService, _mockAuthorizationService, _mockDomainEventsQueue, _mockValidator);
    }

    [Fact]
    public async Task HandleAsync_WhenValidatorReturnsError_ShouldReturnValidationErrorAndNotTouchTheRepositories()
    {
        // Arrange
        AddFavoriteCommand command = _addFavoriteCommandFixture.Create();
        Error error = DomainErrors.Favorites.FavoriteMediaKindIsInvalid;
        _mockValidator.Validate(Arg.Any<AddFavoriteCommand>()).Returns([error]);

        // Act
        Result<FavoriteResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
        _ = _mockCurrentUserService.DidNotReceive().UserId;
        await _mockFavoriteRepository.DidNotReceive().GetMediaItemLibraryIdAsync(Arg.Any<FavoriteMediaKind>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockFavoriteRepository.DidNotReceive().GetByMediaItemIdForUserAsync(Arg.Any<Guid>(), Arg.Any<FavoriteMediaKind>(), Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
        await _mockFavoriteRepository.DidNotReceive().InsertAsync(Arg.Any<FavoriteEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        _mockDomainEventsQueue.DidNotReceive().Enqueue(Arg.Any<IDomainEvent>());
    }

    [Fact]
    public async Task HandleAsync_WhenCurrentUserIsNull_ShouldReturnNotAuthorizedAndNotReadTheRepositories()
    {
        // Arrange
        AddFavoriteCommand command = _addFavoriteCommandFixture.Create();
        _mockCurrentUserService.UserId.Returns((Guid?)null);

        // Act
        Result<FavoriteResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockFavoriteRepository.DidNotReceive().GetMediaItemLibraryIdAsync(Arg.Any<FavoriteMediaKind>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockFavoriteRepository.DidNotReceive().GetByMediaItemIdForUserAsync(Arg.Any<Guid>(), Arg.Any<FavoriteMediaKind>(), Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
        await _mockFavoriteRepository.DidNotReceive().InsertAsync(Arg.Any<FavoriteEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        _mockDomainEventsQueue.DidNotReceive().Enqueue(Arg.Any<IDomainEvent>());
    }

    [Theory]
    [InlineData(FavoriteMediaKind.None)] // the default value, which is not a kind that can be favorited
    [InlineData(FavoriteMediaKind.Video)] // video libraries are not catalogued yet
    [InlineData(FavoriteMediaKind.Photo)] // photo libraries are not catalogued yet
    public async Task HandleAsync_WhenMediaKindIsNotSupported_ShouldReturnNotSupportedAndNotResolveTheLibrary(FavoriteMediaKind mediaKind)
    {
        // Arrange
        AddFavoriteCommand command = _addFavoriteCommandFixture.Create(mediaKind: mediaKind);

        // Act
        Result<FavoriteResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Favorites.FavoriteMediaKindIsNotSupported, result.FirstError);
        await _mockFavoriteRepository.DidNotReceive().GetMediaItemLibraryIdAsync(Arg.Any<FavoriteMediaKind>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockFavoriteRepository.DidNotReceive().GetByMediaItemIdForUserAsync(Arg.Any<Guid>(), Arg.Any<FavoriteMediaKind>(), Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenTheMediaItemLibraryLookupFails_ShouldReturnThatErrorAndNotAuthorizeOrReadTheFavorite()
    {
        // Arrange
        AddFavoriteCommand command = _addFavoriteCommandFixture.Create();
        Error error = Error.Failure("Persistence.Failure", "The media item library could not be read.");
        _mockFavoriteRepository.GetMediaItemLibraryIdAsync(Arg.Any<FavoriteMediaKind>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        Result<FavoriteResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
        await _mockAuthorizationService.DidNotReceive().EvaluatePolicyAsync<ILibraryOwnershipPolicy>(Arg.Any<Guid>(), Arg.Any<PolicyContext>(), Arg.Any<CancellationToken>());
        await _mockFavoriteRepository.DidNotReceive().GetByMediaItemIdForUserAsync(Arg.Any<Guid>(), Arg.Any<FavoriteMediaKind>(), Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
        await _mockFavoriteRepository.DidNotReceive().InsertAsync(Arg.Any<FavoriteEntity>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(FavoriteMediaKind.MusicArtist, nameof(DomainErrors.Music.ArtistNotFound))] // the artist does not exist
    [InlineData(FavoriteMediaKind.MusicAlbum, nameof(DomainErrors.Music.AlbumNotFound))] // the album does not exist
    [InlineData(FavoriteMediaKind.MusicTrack, nameof(DomainErrors.Music.TrackNotFound))] // the track does not exist
    [InlineData(FavoriteMediaKind.Book, nameof(DomainErrors.WrittenContent.BookNotFound))] // the book does not exist
    public async Task HandleAsync_WhenTheMediaItemHasNoLibrary_ShouldReturnTheMediaKindSpecificNotFoundError(FavoriteMediaKind mediaKind, string expectedErrorDescription)
    {
        // Arrange
        AddFavoriteCommand command = _addFavoriteCommandFixture.Create(mediaKind: mediaKind);
        _mockFavoriteRepository.GetMediaItemLibraryIdAsync(Arg.Any<FavoriteMediaKind>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<Guid?>(null));

        // Act
        Result<FavoriteResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(expectedErrorDescription, result.FirstError.Description);
        await _mockAuthorizationService.DidNotReceive().EvaluatePolicyAsync<ILibraryOwnershipPolicy>(Arg.Any<Guid>(), Arg.Any<PolicyContext>(), Arg.Any<CancellationToken>());
        await _mockFavoriteRepository.DidNotReceive().GetByMediaItemIdForUserAsync(Arg.Any<Guid>(), Arg.Any<FavoriteMediaKind>(), Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenLibraryAccessIsDenied_ShouldReturnTheMediaKindSpecificNotFoundErrorAndNotReadTheFavorite()
    {
        // Arrange
        AddFavoriteCommand command = _addFavoriteCommandFixture.Create(mediaKind: FavoriteMediaKind.MusicTrack);
        _mockAuthorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(Arg.Any<Guid>(), Arg.Any<PolicyContext>(), Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        Result<FavoriteResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(nameof(DomainErrors.Music.TrackNotFound), result.FirstError.Description);
        await _mockFavoriteRepository.DidNotReceive().GetByMediaItemIdForUserAsync(Arg.Any<Guid>(), Arg.Any<FavoriteMediaKind>(), Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
        await _mockFavoriteRepository.DidNotReceive().InsertAsync(Arg.Any<FavoriteEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        _mockDomainEventsQueue.DidNotReceive().Enqueue(Arg.Any<IDomainEvent>());
    }

    [Fact]
    public async Task HandleAsync_WhenTheExistingFavoriteReadFails_ShouldReturnThatErrorAndNotInsert()
    {
        // Arrange
        AddFavoriteCommand command = _addFavoriteCommandFixture.Create();
        Error error = Error.Failure("Persistence.Failure", "The existing favorite could not be read.");
        _mockFavoriteRepository.GetByMediaItemIdForUserAsync(Arg.Any<Guid>(), Arg.Any<FavoriteMediaKind>(), Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        Result<FavoriteResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
        await _mockFavoriteRepository.DidNotReceive().InsertAsync(Arg.Any<FavoriteEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        _mockDomainEventsQueue.DidNotReceive().Enqueue(Arg.Any<IDomainEvent>());
    }

    [Fact]
    public async Task HandleAsync_WhenTheItemIsAlreadyAFavorite_ShouldReturnTheStoredFavoriteWithoutInsertingOrRaisingAnEvent()
    {
        // Arrange
        AddFavoriteCommand command = _addFavoriteCommandFixture.Create();
        DateTime createdOnUtc = new(2024, 5, 6, 7, 8, 9, DateTimeKind.Utc);
        FavoriteEntity existingFavorite = _favoriteEntityFixture.Create(
            userId: _userId,
            mediaKind: command.MediaKind!.Value,
            mediaItemId: command.MediaItemId!.Value,
            createdOnUtc: createdOnUtc);
        _mockFavoriteRepository.GetByMediaItemIdForUserAsync(Arg.Any<Guid>(), Arg.Any<FavoriteMediaKind>(), Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<FavoriteEntity?>(existingFavorite));

        // Act
        Result<FavoriteResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(existingFavorite.Id, result.Value.Id);
        Assert.Equal(createdOnUtc, result.Value.FavoritedOnUtc);
        await _mockFavoriteRepository.DidNotReceive().InsertAsync(Arg.Any<FavoriteEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        _mockDomainEventsQueue.DidNotReceive().Enqueue(Arg.Any<IDomainEvent>());
    }

    [Fact]
    public async Task HandleAsync_WhenInsertFails_ShouldReturnThatErrorAndNotSaveChanges()
    {
        // Arrange
        AddFavoriteCommand command = _addFavoriteCommandFixture.Create();
        Error error = Error.Failure("Persistence.Failure", "The favorite could not be inserted.");
        _mockFavoriteRepository.InsertAsync(Arg.Any<FavoriteEntity>(), Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        Result<FavoriteResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        _mockDomainEventsQueue.DidNotReceive().Enqueue(Arg.Any<IDomainEvent>());
    }

    [Fact]
    public async Task HandleAsync_WhenSaveFailsWithANonUniqueConstraintError_ShouldReturnThatErrorAndNotRaiseAnEvent()
    {
        // Arrange
        AddFavoriteCommand command = _addFavoriteCommandFixture.Create();
        Error error = Error.Failure("Persistence.Failure", "The changes could not be saved.");
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(error);

        // Act
        Result<FavoriteResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
        _mockUnitOfWork.DidNotReceive().ClearTrackedEntities();
        _mockDomainEventsQueue.DidNotReceive().Enqueue(Arg.Any<IDomainEvent>());
    }

    [Fact]
    public async Task HandleAsync_WhenSaveFailsWithAUniqueConstraintViolationAndAConcurrentFavoriteExists_ShouldReturnTheConcurrentFavoriteWithoutRaisingAnEvent()
    {
        // Arrange
        AddFavoriteCommand command = _addFavoriteCommandFixture.Create();
        DateTime createdOnUtc = new(2024, 7, 8, 9, 10, 11, DateTimeKind.Utc);
        FavoriteEntity concurrentFavorite = _favoriteEntityFixture.Create(
            userId: _userId,
            mediaKind: command.MediaKind!.Value,
            mediaItemId: command.MediaItemId!.Value,
            createdOnUtc: createdOnUtc);
        _mockFavoriteRepository.GetByMediaItemIdForUserAsync(Arg.Any<Guid>(), Arg.Any<FavoriteMediaKind>(), Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<FavoriteEntity?>(null), Result.From<FavoriteEntity?>(concurrentFavorite));
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(ApplicationErrors.Persistence.UniqueConstraintViolation);

        // Act
        Result<FavoriteResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(concurrentFavorite.Id, result.Value.Id);
        Assert.Equal(createdOnUtc, result.Value.FavoritedOnUtc);
        _mockUnitOfWork.Received(1).ClearTrackedEntities();
        await _mockFavoriteRepository.Received(2).GetByMediaItemIdForUserAsync(_userId, command.MediaKind!.Value, command.MediaItemId!.Value, Arg.Is<bool>(shouldTrackEntities => !shouldTrackEntities), Arg.Any<CancellationToken>());
        _mockDomainEventsQueue.DidNotReceive().Enqueue(Arg.Any<IDomainEvent>());
    }

    [Fact]
    public async Task HandleAsync_WhenSaveFailsWithAUniqueConstraintViolationAndTheConcurrentFavoriteIsMissing_ShouldReturnTheSaveError()
    {
        // Arrange
        AddFavoriteCommand command = _addFavoriteCommandFixture.Create();
        _mockFavoriteRepository.GetByMediaItemIdForUserAsync(Arg.Any<Guid>(), Arg.Any<FavoriteMediaKind>(), Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<FavoriteEntity?>(null), Result.From<FavoriteEntity?>(null));
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(ApplicationErrors.Persistence.UniqueConstraintViolation);

        // Act
        Result<FavoriteResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Persistence.UniqueConstraintViolation, result.FirstError);
        _mockUnitOfWork.Received(1).ClearTrackedEntities();
        _mockDomainEventsQueue.DidNotReceive().Enqueue(Arg.Any<IDomainEvent>());
    }

    [Fact]
    public async Task HandleAsync_WhenSaveFailsWithAUniqueConstraintViolationAndTheReReadFails_ShouldReturnTheReReadError()
    {
        // Arrange
        AddFavoriteCommand command = _addFavoriteCommandFixture.Create();
        Error error = Error.Failure("Persistence.Failure", "The concurrent favorite could not be read.");
        _mockFavoriteRepository.GetByMediaItemIdForUserAsync(Arg.Any<Guid>(), Arg.Any<FavoriteMediaKind>(), Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<FavoriteEntity?>(null), error);
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(ApplicationErrors.Persistence.UniqueConstraintViolation);

        // Act
        Result<FavoriteResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
        _mockUnitOfWork.Received(1).ClearTrackedEntities();
        _mockDomainEventsQueue.DidNotReceive().Enqueue(Arg.Any<IDomainEvent>());
    }

    [Fact]
    public async Task HandleAsync_WhenSuccessful_ShouldInsertSaveEnqueueOnceAndReturnThePersistedFavorite()
    {
        // Arrange
        AddFavoriteCommand command = _addFavoriteCommandFixture.Create(mediaKind: FavoriteMediaKind.MusicAlbum);
        DateTime createdOnUtc = new(2024, 9, 10, 11, 12, 13, DateTimeKind.Utc);
        FavoriteEntity persistedFavorite = _favoriteEntityFixture.Create(
            userId: _userId,
            mediaKind: command.MediaKind!.Value,
            mediaItemId: command.MediaItemId!.Value,
            createdOnUtc: createdOnUtc);
        _mockFavoriteRepository.GetByMediaItemIdForUserAsync(Arg.Any<Guid>(), Arg.Any<FavoriteMediaKind>(), Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<FavoriteEntity?>(null), Result.From<FavoriteEntity?>(persistedFavorite));
        FavoriteEntity? insertedEntity = null;
        _mockFavoriteRepository.InsertAsync(Arg.Do<FavoriteEntity>(favorite => insertedEntity = favorite), Arg.Any<CancellationToken>())
            .Returns(Result.Created);

        // Act
        Result<FavoriteResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(persistedFavorite.Id, result.Value.Id);
        Assert.Equal(persistedFavorite.MediaKind, result.Value.MediaKind);
        Assert.Equal(persistedFavorite.MediaItemId, result.Value.MediaItemId);
        Assert.Equal(createdOnUtc, result.Value.FavoritedOnUtc);
        Assert.NotNull(insertedEntity);
        Assert.NotEqual(Guid.Empty, insertedEntity!.Id);
        Assert.Equal(_userId, insertedEntity.UserId);
        Assert.Equal(command.MediaKind!.Value, insertedEntity.MediaKind);
        Assert.Equal(command.MediaItemId!.Value, insertedEntity.MediaItemId);
        Assert.Equal(Guid.Empty, insertedEntity.CreatedBy);
        Assert.Null(insertedEntity.UpdatedOnUtc);
        Assert.Null(insertedEntity.UpdatedBy);
        await _mockFavoriteRepository.Received(1).InsertAsync(Arg.Any<FavoriteEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        _mockDomainEventsQueue.Received(1).Enqueue(Arg.Is<IDomainEvent>(domainEvent => domainEvent is FavoriteAddedDomainEvent));
        await _mockFavoriteRepository.Received(2).GetByMediaItemIdForUserAsync(_userId, command.MediaKind!.Value, command.MediaItemId!.Value, Arg.Is<bool>(shouldTrackEntities => !shouldTrackEntities), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenThePersistedFavoriteReadFails_ShouldReturnThatErrorAndNotEnqueueTheAddedEvent()
    {
        // Arrange
        AddFavoriteCommand command = _addFavoriteCommandFixture.Create();
        Error error = Error.Failure("Persistence.Failure", "The persisted favorite could not be read.");
        _mockFavoriteRepository.GetByMediaItemIdForUserAsync(Arg.Any<Guid>(), Arg.Any<FavoriteMediaKind>(), Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<FavoriteEntity?>(null), error);

        // Act
        Result<FavoriteResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
        _mockDomainEventsQueue.DidNotReceive().Enqueue(Arg.Any<IDomainEvent>());
    }

    [Fact]
    public async Task HandleAsync_WhenThePersistedFavoriteIsMissing_ShouldReturnFavoriteNotFoundWithoutEnqueuingTheAddedEvent()
    {
        // Arrange
        AddFavoriteCommand command = _addFavoriteCommandFixture.Create();
        _mockFavoriteRepository.GetByMediaItemIdForUserAsync(Arg.Any<Guid>(), Arg.Any<FavoriteMediaKind>(), Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<FavoriteEntity?>(null), Result.From<FavoriteEntity?>(null));

        // Act
        Result<FavoriteResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Favorites.FavoriteNotFound, result.FirstError);
        _mockDomainEventsQueue.DidNotReceive().Enqueue(Arg.Any<IDomainEvent>());
    }

    [Fact]
    public async Task HandleAsync_WhenCalled_ShouldScopeEveryRepositoryCallToTheCurrentUserTheMediaKindAndTheMediaItemAndForwardTheCancellationToken()
    {
        // Arrange
        AddFavoriteCommand command = _addFavoriteCommandFixture.Create(mediaKind: FavoriteMediaKind.Book);
        using CancellationTokenSource cancellationTokenSource = new();
        CancellationToken cancellationToken = cancellationTokenSource.Token;
        FavoriteEntity persistedFavorite = _favoriteEntityFixture.Create(
            userId: _userId,
            mediaKind: command.MediaKind!.Value,
            mediaItemId: command.MediaItemId!.Value);
        _mockFavoriteRepository.GetByMediaItemIdForUserAsync(Arg.Any<Guid>(), Arg.Any<FavoriteMediaKind>(), Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<FavoriteEntity?>(null), Result.From<FavoriteEntity?>(persistedFavorite));

        // Act
        await _sut.HandleAsync(command, cancellationToken);

        // Assert
        await _mockFavoriteRepository.Received(1).GetMediaItemLibraryIdAsync(command.MediaKind!.Value, command.MediaItemId!.Value, Arg.Is(cancellationToken));
        await _mockAuthorizationService.Received(1).EvaluatePolicyAsync<ILibraryOwnershipPolicy>(
            _userId,
            Arg.Is<PolicyContext>(context => context is LibraryOwnershipPolicyContext && ((LibraryOwnershipPolicyContext)context).LibraryId == _libraryId),
            Arg.Is(cancellationToken));
        await _mockFavoriteRepository.Received(2).GetByMediaItemIdForUserAsync(_userId, command.MediaKind!.Value, command.MediaItemId!.Value, Arg.Is<bool>(shouldTrackEntities => !shouldTrackEntities), Arg.Is(cancellationToken));
        await _mockFavoriteRepository.Received(1).InsertAsync(Arg.Any<FavoriteEntity>(), Arg.Is(cancellationToken));
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Is(cancellationToken));
    }
}
