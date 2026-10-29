#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Repositories.Playlists;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Core.Playlists.Commands.MovePlaylistEntry;
using Lumina.Application.Fixtures.Core.Playlists.Commands.MovePlaylistEntry;
using Lumina.Contracts.Responses.Playlists;
using Lumina.Domain.Common.Primitives;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
using DomainErrors = Lumina.Domain.Common.Errors.Errors;
#endregion

namespace Lumina.Application.UnitTests.Core.Playlists.Commands.MovePlaylistEntry;

/// <summary>
/// Contains unit tests for the <see cref="MovePlaylistEntryCommandHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MovePlaylistEntryCommandHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly ICurrentUserService _mockCurrentUserService;
    private readonly IPlaylistRepository _mockPlaylistRepository;
    private readonly IValidator<MovePlaylistEntryCommand> _mockValidator;
    private readonly MovePlaylistEntryCommandHandler _sut;
    private readonly MovePlaylistEntryCommandFixture _movePlaylistEntryCommandFixture = new();
    private readonly Guid _userId = Guid.NewGuid();

    /// <summary>
    /// Initializes a new instance of the <see cref="MovePlaylistEntryCommandHandlerTests"/> class.
    /// </summary>
    public MovePlaylistEntryCommandHandlerTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockCurrentUserService = Substitute.For<ICurrentUserService>();
        _mockPlaylistRepository = Substitute.For<IPlaylistRepository>();

        _mockUnitOfWork.PlaylistRepository.Returns(_mockPlaylistRepository);
        _mockCurrentUserService.UserId.Returns(_userId);
        _mockPlaylistRepository.MoveEntryAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<int>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(Result.Updated);

        _mockValidator = Substitute.For<IValidator<MovePlaylistEntryCommand>>();
        _mockValidator.Validate(Arg.Any<MovePlaylistEntryCommand>()).Returns([]);

        _sut = new MovePlaylistEntryCommandHandler(_mockUnitOfWork, _mockCurrentUserService, _mockValidator);
    }

    [Fact]
    public async Task HandleAsync_WhenValidatorReturnsError_ShouldReturnValidationErrorAndNotTouchTheRepository()
    {
        // Arrange
        MovePlaylistEntryCommand command = _movePlaylistEntryCommandFixture.Create();
        Error error = DomainErrors.Playlists.PlaylistMoveTargetPositionIsInvalid;
        _mockValidator.Validate(Arg.Any<MovePlaylistEntryCommand>()).Returns([error]);

        // Act
        Result<PlaylistEntryMovedResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
        _ = _mockCurrentUserService.DidNotReceive().UserId;
        await _mockPlaylistRepository.DidNotReceive().MoveEntryAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<int>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenCurrentUserIsNull_ShouldReturnNotAuthorizedAndNotMoveTheEntry()
    {
        // Arrange
        MovePlaylistEntryCommand command = _movePlaylistEntryCommandFixture.Create();
        _mockCurrentUserService.UserId.Returns((Guid?)null);

        // Act
        Result<PlaylistEntryMovedResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockPlaylistRepository.DidNotReceive().MoveEntryAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<int>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenMovingTheEntryFails_ShouldReturnThatError()
    {
        // Arrange
        MovePlaylistEntryCommand command = _movePlaylistEntryCommandFixture.Create();
        Error error = Error.Failure("Persistence.Failure", "The entry could not be moved.");
        _mockPlaylistRepository.MoveEntryAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<int>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        Result<PlaylistEntryMovedResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
    }

    [Fact]
    public async Task HandleAsync_WhenSuccessful_ShouldReturnTheMovedEntryAndItsTargetPosition()
    {
        // Arrange
        Guid playlistId = Guid.NewGuid();
        Guid playlistEntryId = Guid.NewGuid();
        MovePlaylistEntryCommand command = _movePlaylistEntryCommandFixture.Create(
            playlistId: playlistId.ToString(),
            playlistEntryId: playlistEntryId.ToString(),
            targetPosition: 3);

        // Act
        Result<PlaylistEntryMovedResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(playlistId, result.Value.PlaylistId);
        Assert.Equal(playlistEntryId, result.Value.EntryId);
        Assert.Equal(3, result.Value.Position);
        Assert.NotNull(result.Value.UpdatedOnUtc);
        Assert.InRange(result.Value.UpdatedOnUtc!.Value, DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow);
    }

    [Fact]
    public async Task HandleAsync_WhenCalled_ShouldPassTheRouteIdentifiersAndTheTargetPositionToTheRepositoryAndForwardTheCancellationToken()
    {
        // Arrange
        Guid playlistId = Guid.NewGuid();
        Guid playlistEntryId = Guid.NewGuid();
        MovePlaylistEntryCommand command = _movePlaylistEntryCommandFixture.Create(
            playlistId: playlistId.ToString(),
            playlistEntryId: playlistEntryId.ToString(),
            targetPosition: 12);
        using CancellationTokenSource cancellationTokenSource = new();
        CancellationToken cancellationToken = cancellationTokenSource.Token;

        // Act
        await _sut.HandleAsync(command, cancellationToken);

        // Assert
        await _mockPlaylistRepository.Received(1).MoveEntryAsync(
            playlistId,
            _userId,
            playlistEntryId,
            12,
            Arg.Any<DateTime>(),
            Arg.Is(cancellationToken));
    }
}
