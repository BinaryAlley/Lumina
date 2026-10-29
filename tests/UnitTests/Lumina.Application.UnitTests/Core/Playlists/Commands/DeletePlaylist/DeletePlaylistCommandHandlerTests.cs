#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Playlists;
using Lumina.Application.Common.DataAccess.Repositories.Playlists;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.DomainEvents;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Core.Playlists.Commands.DeletePlaylist;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Playlists;
using Lumina.Application.Fixtures.Core.Playlists.Commands.DeletePlaylist;
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

namespace Lumina.Application.UnitTests.Core.Playlists.Commands.DeletePlaylist;

/// <summary>
/// Contains unit tests for the <see cref="DeletePlaylistCommandHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class DeletePlaylistCommandHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly ICurrentUserService _mockCurrentUserService;
    private readonly IDomainEventsQueue _mockDomainEventsQueue;
    private readonly IPlaylistRepository _mockPlaylistRepository;
    private readonly IValidator<DeletePlaylistCommand> _mockValidator;
    private readonly DeletePlaylistCommandHandler _sut;
    private readonly DeletePlaylistCommandFixture _deletePlaylistCommandFixture = new();
    private readonly PlaylistEntityFixture _playlistEntityFixture = new();
    private readonly PlaylistEntryEntityFixture _playlistEntryEntityFixture = new();
    private readonly Guid _userId = Guid.NewGuid();

    /// <summary>
    /// Initializes a new instance of the <see cref="DeletePlaylistCommandHandlerTests"/> class.
    /// </summary>
    public DeletePlaylistCommandHandlerTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockCurrentUserService = Substitute.For<ICurrentUserService>();
        _mockDomainEventsQueue = Substitute.For<IDomainEventsQueue>();
        _mockPlaylistRepository = Substitute.For<IPlaylistRepository>();

        _mockUnitOfWork.PlaylistRepository.Returns(_mockPlaylistRepository);
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Result.Success);
        _mockCurrentUserService.UserId.Returns(_userId);
        _mockPlaylistRepository.GetByIdForUserAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<PlaylistEntity?>(_playlistEntityFixture.Create(userId: _userId)));
        _mockPlaylistRepository.DeleteByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Result.Deleted);

        _mockValidator = Substitute.For<IValidator<DeletePlaylistCommand>>();
        _mockValidator.Validate(Arg.Any<DeletePlaylistCommand>()).Returns([]);

        _sut = new DeletePlaylistCommandHandler(_mockUnitOfWork, _mockCurrentUserService, _mockDomainEventsQueue, _mockValidator);
    }

    [Fact]
    public async Task HandleAsync_WhenValidatorReturnsError_ShouldReturnValidationErrorAndNotTouchTheRepositories()
    {
        // Arrange
        DeletePlaylistCommand command = _deletePlaylistCommandFixture.Create();
        Error error = DomainErrors.Playlists.PlaylistIdCannotBeEmpty;
        _mockValidator.Validate(Arg.Any<DeletePlaylistCommand>()).Returns([error]);

        // Act
        Result<Deleted> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
        _ = _mockCurrentUserService.DidNotReceive().UserId;
        await _mockPlaylistRepository.DidNotReceive().GetByIdForUserAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
        await _mockPlaylistRepository.DidNotReceive().DeleteByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        _mockDomainEventsQueue.DidNotReceive().Enqueue(Arg.Any<IDomainEvent>());
    }

    [Fact]
    public async Task HandleAsync_WhenCurrentUserIsNull_ShouldReturnNotAuthorizedAndNotReadThePlaylist()
    {
        // Arrange
        DeletePlaylistCommand command = _deletePlaylistCommandFixture.Create();
        _mockCurrentUserService.UserId.Returns((Guid?)null);

        // Act
        Result<Deleted> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockPlaylistRepository.DidNotReceive().GetByIdForUserAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
        await _mockPlaylistRepository.DidNotReceive().DeleteByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenThePlaylistReadFails_ShouldReturnThatErrorAndNotDelete()
    {
        // Arrange
        DeletePlaylistCommand command = _deletePlaylistCommandFixture.Create();
        Error error = Error.Failure("Persistence.Failure", "The playlist could not be read.");
        _mockPlaylistRepository.GetByIdForUserAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        Result<Deleted> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
        await _mockPlaylistRepository.DidNotReceive().DeleteByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        _mockDomainEventsQueue.DidNotReceive().Enqueue(Arg.Any<IDomainEvent>());
    }

    [Fact]
    public async Task HandleAsync_WhenThePlaylistDoesNotExistForTheUser_ShouldReturnPlaylistNotFoundAndNotDelete()
    {
        // Arrange
        DeletePlaylistCommand command = _deletePlaylistCommandFixture.Create();
        _mockPlaylistRepository.GetByIdForUserAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<PlaylistEntity?>(null));

        // Act
        Result<Deleted> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Playlists.PlaylistNotFound, result.FirstError);
        await _mockPlaylistRepository.DidNotReceive().DeleteByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenTheStoredPlaylistCannotBeRebuiltAsADomainAggregate_ShouldReturnThatErrorAndNotDelete()
    {
        // Arrange
        DeletePlaylistCommand command = _deletePlaylistCommandFixture.Create();
        List<PlaylistEntryEntity> entries =
        [
            _playlistEntryEntityFixture.Create(position: 512, mediaItemType: PlaylistMediaItemType.MusicTrack),
            _playlistEntryEntityFixture.Create(position: 512, mediaItemType: PlaylistMediaItemType.MusicTrack),
        ];
        _mockPlaylistRepository.GetByIdForUserAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<PlaylistEntity?>(_playlistEntityFixture.Create(userId: _userId, entries: entries)));

        // Act
        Result<Deleted> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Playlists.PlaylistEntryPositionsAreInvalid, result.FirstError);
        await _mockPlaylistRepository.DidNotReceive().DeleteByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        _mockDomainEventsQueue.DidNotReceive().Enqueue(Arg.Any<IDomainEvent>());
    }

    [Fact]
    public async Task HandleAsync_WhenTheDeletionFails_ShouldReturnThatErrorAndNotSaveChanges()
    {
        // Arrange
        DeletePlaylistCommand command = _deletePlaylistCommandFixture.Create();
        Error error = Error.Failure("Persistence.Failure", "The playlist could not be removed.");
        _mockPlaylistRepository.DeleteByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
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
    public async Task HandleAsync_WhenSaveFails_ShouldReturnThatErrorAndNotRaiseTheDeletedEvent()
    {
        // Arrange
        DeletePlaylistCommand command = _deletePlaylistCommandFixture.Create();
        Error error = Error.Failure("Persistence.Failure", "The changes could not be saved.");
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(error);

        // Act
        Result<Deleted> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
        _mockDomainEventsQueue.DidNotReceive().Enqueue(Arg.Any<IDomainEvent>());
    }

    [Fact]
    public async Task HandleAsync_WhenSuccessful_ShouldDeleteSaveAndEnqueueOneDeletedEvent()
    {
        // Arrange
        Guid playlistId = Guid.NewGuid();
        DeletePlaylistCommand command = _deletePlaylistCommandFixture.Create(playlistId: playlistId.ToString());
        _mockPlaylistRepository.GetByIdForUserAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<PlaylistEntity?>(_playlistEntityFixture.Create(id: playlistId, userId: _userId)));

        // Act
        Result<Deleted> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Deleted, result.Value);
        await _mockPlaylistRepository.Received(1).DeleteByIdAsync(playlistId, Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        _mockDomainEventsQueue.Received(1).Enqueue(Arg.Is<IDomainEvent>(domainEvent => domainEvent is PlaylistDeletedDomainEvent));
    }

    [Fact]
    public async Task HandleAsync_WhenCalled_ShouldLoadThePlaylistTrackedAndScopedToTheCurrentUserAndForwardTheCancellationToken()
    {
        // Arrange
        Guid playlistId = Guid.NewGuid();
        DeletePlaylistCommand command = _deletePlaylistCommandFixture.Create(playlistId: playlistId.ToString());
        _mockPlaylistRepository.GetByIdForUserAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<PlaylistEntity?>(_playlistEntityFixture.Create(id: playlistId, userId: _userId)));
        using CancellationTokenSource cancellationTokenSource = new();
        CancellationToken cancellationToken = cancellationTokenSource.Token;

        // Act
        await _sut.HandleAsync(command, cancellationToken);

        // Assert
        await _mockPlaylistRepository.Received(1).GetByIdForUserAsync(playlistId, _userId, false, true, Arg.Is(cancellationToken));
        await _mockPlaylistRepository.Received(1).DeleteByIdAsync(playlistId, Arg.Is(cancellationToken));
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Is(cancellationToken));
    }
}
