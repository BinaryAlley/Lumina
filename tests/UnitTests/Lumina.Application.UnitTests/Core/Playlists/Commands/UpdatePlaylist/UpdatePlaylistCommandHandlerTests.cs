#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Playlists;
using Lumina.Application.Common.DataAccess.Repositories.Playlists;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.DomainEvents;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Core.Playlists.Commands.UpdatePlaylist;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Playlists;
using Lumina.Application.Fixtures.Core.Playlists.Commands.UpdatePlaylist;
using Lumina.Contracts.Responses.Playlists;
using Lumina.Domain.Common.Events;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.PlaylistBoundedContext.PlaylistAggregate.Events;
using Lumina.Domain.SharedKernel.Common.Enums.Playlists;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
using DomainErrors = Lumina.Domain.Common.Errors.Errors;
#endregion

namespace Lumina.Application.UnitTests.Core.Playlists.Commands.UpdatePlaylist;

/// <summary>
/// Contains unit tests for the <see cref="UpdatePlaylistCommandHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdatePlaylistCommandHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly ICurrentUserService _mockCurrentUserService;
    private readonly IDomainEventsQueue _mockDomainEventsQueue;
    private readonly IPlaylistRepository _mockPlaylistRepository;
    private readonly IValidator<UpdatePlaylistCommand> _mockValidator;
    private readonly UpdatePlaylistCommandHandler _sut;
    private readonly UpdatePlaylistCommandFixture _updatePlaylistCommandFixture = new();
    private readonly PlaylistEntityFixture _playlistEntityFixture = new();
    private readonly PlaylistEntryEntityFixture _playlistEntryEntityFixture = new();
    private readonly Guid _userId = Guid.NewGuid();

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdatePlaylistCommandHandlerTests"/> class.
    /// </summary>
    public UpdatePlaylistCommandHandlerTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockCurrentUserService = Substitute.For<ICurrentUserService>();
        _mockDomainEventsQueue = Substitute.For<IDomainEventsQueue>();
        _mockPlaylistRepository = Substitute.For<IPlaylistRepository>();

        _mockUnitOfWork.PlaylistRepository.Returns(_mockPlaylistRepository);
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Result.Success);
        _mockCurrentUserService.UserId.Returns(_userId);
        _mockPlaylistRepository.GetByIdForUserAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<PlaylistEntity?>(_playlistEntityFixture.Create(userId: _userId, type: PlaylistType.Music)));
        _mockPlaylistRepository.UpdateAsync(Arg.Any<PlaylistEntity>(), Arg.Any<CancellationToken>())
            .Returns(Result.Updated);

        _mockValidator = Substitute.For<IValidator<UpdatePlaylistCommand>>();
        _mockValidator.Validate(Arg.Any<UpdatePlaylistCommand>()).Returns([]);

        _sut = new UpdatePlaylistCommandHandler(_mockUnitOfWork, _mockCurrentUserService, _mockDomainEventsQueue, _mockValidator);
    }

    [Fact]
    public async Task HandleAsync_WhenValidatorReturnsError_ShouldReturnValidationErrorAndNotTouchTheRepositories()
    {
        // Arrange
        UpdatePlaylistCommand command = _updatePlaylistCommandFixture.Create();
        Error error = DomainErrors.Playlists.PlaylistNameCannotBeEmpty;
        _mockValidator.Validate(Arg.Any<UpdatePlaylistCommand>()).Returns([error]);

        // Act
        Result<PlaylistDetailsResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
        _ = _mockCurrentUserService.DidNotReceive().UserId;
        await _mockPlaylistRepository.DidNotReceive().GetByIdForUserAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
        await _mockPlaylistRepository.DidNotReceive().UpdateAsync(Arg.Any<PlaylistEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        _mockDomainEventsQueue.DidNotReceive().Enqueue(Arg.Any<IDomainEvent>());
    }

    [Fact]
    public async Task HandleAsync_WhenCurrentUserIsNull_ShouldReturnNotAuthorizedAndNotReadThePlaylist()
    {
        // Arrange
        UpdatePlaylistCommand command = _updatePlaylistCommandFixture.Create();
        _mockCurrentUserService.UserId.Returns((Guid?)null);

        // Act
        Result<PlaylistDetailsResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockPlaylistRepository.DidNotReceive().GetByIdForUserAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenThePlaylistReadFails_ShouldReturnThatErrorAndNotUpdate()
    {
        // Arrange
        UpdatePlaylistCommand command = _updatePlaylistCommandFixture.Create();
        Error error = Error.Failure("Persistence.Failure", "The playlist could not be read.");
        _mockPlaylistRepository.GetByIdForUserAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        Result<PlaylistDetailsResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
        await _mockPlaylistRepository.DidNotReceive().UpdateAsync(Arg.Any<PlaylistEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenThePlaylistDoesNotExistForTheUser_ShouldReturnPlaylistNotFoundAndNotUpdate()
    {
        // Arrange
        UpdatePlaylistCommand command = _updatePlaylistCommandFixture.Create();
        _mockPlaylistRepository.GetByIdForUserAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<PlaylistEntity?>(null));

        // Act
        Result<PlaylistDetailsResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Playlists.PlaylistNotFound, result.FirstError);
        await _mockPlaylistRepository.DidNotReceive().UpdateAsync(Arg.Any<PlaylistEntity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenTheStoredPlaylistCannotBeRebuiltAsADomainAggregate_ShouldReturnThatErrorAndNotUpdate()
    {
        // Arrange
        UpdatePlaylistCommand command = _updatePlaylistCommandFixture.Create();
        List<PlaylistEntryEntity> entries =
        [
            _playlistEntryEntityFixture.Create(position: 64, mediaItemType: PlaylistMediaItemType.MusicTrack),
            _playlistEntryEntityFixture.Create(position: 64, mediaItemType: PlaylistMediaItemType.MusicTrack),
        ];
        _mockPlaylistRepository.GetByIdForUserAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<PlaylistEntity?>(_playlistEntityFixture.Create(userId: _userId, entries: entries)));

        // Act
        Result<PlaylistDetailsResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Playlists.PlaylistEntryPositionsAreInvalid, result.FirstError);
        await _mockPlaylistRepository.DidNotReceive().UpdateAsync(Arg.Any<PlaylistEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        _mockDomainEventsQueue.DidNotReceive().Enqueue(Arg.Any<IDomainEvent>());
    }

    [Fact]
    public async Task HandleAsync_WhenTheUpdateFails_ShouldReturnThatErrorAndNotSaveChanges()
    {
        // Arrange
        UpdatePlaylistCommand command = _updatePlaylistCommandFixture.Create();
        Error error = Error.Failure("Persistence.Failure", "The playlist could not be updated.");
        _mockPlaylistRepository.UpdateAsync(Arg.Any<PlaylistEntity>(), Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        Result<PlaylistDetailsResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        _mockDomainEventsQueue.DidNotReceive().Enqueue(Arg.Any<IDomainEvent>());
    }

    [Fact]
    public async Task HandleAsync_WhenSaveFails_ShouldReturnThatErrorAndNotRaiseTheDetailsUpdatedEvent()
    {
        // Arrange
        UpdatePlaylistCommand command = _updatePlaylistCommandFixture.Create();
        Error error = Error.Failure("Persistence.Failure", "The changes could not be saved.");
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(error);

        // Act
        Result<PlaylistDetailsResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
        _mockDomainEventsQueue.DidNotReceive().Enqueue(Arg.Any<IDomainEvent>());
    }

    [Fact]
    public async Task HandleAsync_WhenSuccessful_ShouldPersistTheNewDetailsRaiseOneEventAndReturnTheStoredActivityTime()
    {
        // Arrange
        Guid playlistId = Guid.NewGuid();
        DateTime storedUpdatedOnUtc = new(2024, 8, 9, 10, 11, 12, DateTimeKind.Utc);
        UpdatePlaylistCommand command = _updatePlaylistCommandFixture.Create(
            playlistId: playlistId.ToString(),
            name: "Deep Focus",
            description: "Long sessions");
        _mockPlaylistRepository.GetByIdForUserAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<PlaylistEntity?>(_playlistEntityFixture.Create(
                id: playlistId,
                userId: _userId,
                name: "Focus",
                description: "Deep work",
                type: PlaylistType.Music,
                includeUpdatedOnUtc: true,
                updatedOnUtc: storedUpdatedOnUtc)));
        PlaylistEntity? updatedEntity = null;
        _mockPlaylistRepository.UpdateAsync(Arg.Do<PlaylistEntity>(playlist => updatedEntity = playlist), Arg.Any<CancellationToken>())
            .Returns(Result.Updated);

        // Act
        Result<PlaylistDetailsResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(playlistId, result.Value.Id);
        Assert.Equal("Deep Focus", result.Value.Name);
        Assert.Equal("Long sessions", result.Value.Description);
        Assert.Equal(PlaylistType.Music, result.Value.Type);
        Assert.True(result.Value.UpdatedOnUtc.HasValue);
        Assert.Equal(storedUpdatedOnUtc, result.Value.UpdatedOnUtc!.Value);
        Assert.NotNull(updatedEntity);
        Assert.Equal(playlistId, updatedEntity!.Id);
        Assert.Equal(_userId, updatedEntity.UserId);
        Assert.Equal("Deep Focus", updatedEntity.Name);
        Assert.Equal("Long sessions", updatedEntity.Description);
        await _mockPlaylistRepository.Received(1).UpdateAsync(Arg.Any<PlaylistEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        _mockDomainEventsQueue.Received(1).Enqueue(Arg.Is<IDomainEvent>(domainEvent => domainEvent is PlaylistDetailsUpdatedDomainEvent));
    }

    [Fact]
    public async Task HandleAsync_WhenTheDescriptionIsCleared_ShouldPersistWithoutADescriptionAndRaiseOneEvent()
    {
        // Arrange
        UpdatePlaylistCommand command = _updatePlaylistCommandFixture.Create(
            name: "Focus",
            includeDescription: false);
        _mockPlaylistRepository.GetByIdForUserAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<PlaylistEntity?>(_playlistEntityFixture.Create(
                userId: _userId,
                name: "Focus",
                description: "Deep work")));
        PlaylistEntity? updatedEntity = null;
        _mockPlaylistRepository.UpdateAsync(Arg.Do<PlaylistEntity>(playlist => updatedEntity = playlist), Arg.Any<CancellationToken>())
            .Returns(Result.Updated);

        // Act
        Result<PlaylistDetailsResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Null(result.Value.Description);
        Assert.NotNull(updatedEntity);
        Assert.Null(updatedEntity!.Description);
        _mockDomainEventsQueue.Received(1).Enqueue(Arg.Is<IDomainEvent>(domainEvent => domainEvent is PlaylistDetailsUpdatedDomainEvent));
    }

    [Fact]
    public async Task HandleAsync_WhenNothingChanges_ShouldStillPersistAndRaiseNoEvent()
    {
        // Arrange
        UpdatePlaylistCommand command = _updatePlaylistCommandFixture.Create(
            name: "Focus",
            description: "Deep work");
        _mockPlaylistRepository.GetByIdForUserAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<PlaylistEntity?>(_playlistEntityFixture.Create(
                userId: _userId,
                name: "Focus",
                description: "Deep work")));

        // Act
        Result<PlaylistDetailsResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal("Focus", result.Value.Name);
        _mockDomainEventsQueue.DidNotReceive().Enqueue(Arg.Any<IDomainEvent>());
    }

    [Fact]
    public async Task HandleAsync_WhenCalled_ShouldLoadThePlaylistTrackedAndScopedToTheCurrentUserAndForwardTheCancellationToken()
    {
        // Arrange
        Guid playlistId = Guid.NewGuid();
        UpdatePlaylistCommand command = _updatePlaylistCommandFixture.Create(playlistId: playlistId.ToString());
        using CancellationTokenSource cancellationTokenSource = new();
        CancellationToken cancellationToken = cancellationTokenSource.Token;

        // Act
        await _sut.HandleAsync(command, cancellationToken);

        // Assert
        await _mockPlaylistRepository.Received(1).GetByIdForUserAsync(playlistId, _userId, false, true, Arg.Is(cancellationToken));
        await _mockPlaylistRepository.Received(1).UpdateAsync(Arg.Any<PlaylistEntity>(), Arg.Is(cancellationToken));
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Is(cancellationToken));
    }
}
