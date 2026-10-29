#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Repositories.Playlists;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Core.Playlists.Commands.RemovePlaylistEntry;
using Lumina.Application.Fixtures.Core.Playlists.Commands.RemovePlaylistEntry;
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

namespace Lumina.Application.UnitTests.Core.Playlists.Commands.RemovePlaylistEntry;

/// <summary>
/// Contains unit tests for the <see cref="RemovePlaylistEntryCommandHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class RemovePlaylistEntryCommandHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly ICurrentUserService _mockCurrentUserService;
    private readonly IPlaylistRepository _mockPlaylistRepository;
    private readonly IValidator<RemovePlaylistEntryCommand> _mockValidator;
    private readonly RemovePlaylistEntryCommandHandler _sut;
    private readonly RemovePlaylistEntryCommandFixture _removePlaylistEntryCommandFixture = new();
    private readonly Guid _userId = Guid.NewGuid();

    /// <summary>
    /// Initializes a new instance of the <see cref="RemovePlaylistEntryCommandHandlerTests"/> class.
    /// </summary>
    public RemovePlaylistEntryCommandHandlerTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockCurrentUserService = Substitute.For<ICurrentUserService>();
        _mockPlaylistRepository = Substitute.For<IPlaylistRepository>();

        _mockUnitOfWork.PlaylistRepository.Returns(_mockPlaylistRepository);
        _mockCurrentUserService.UserId.Returns(_userId);
        _mockPlaylistRepository.RemoveEntryAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(0));

        _mockValidator = Substitute.For<IValidator<RemovePlaylistEntryCommand>>();
        _mockValidator.Validate(Arg.Any<RemovePlaylistEntryCommand>()).Returns([]);

        _sut = new RemovePlaylistEntryCommandHandler(_mockUnitOfWork, _mockCurrentUserService, _mockValidator);
    }

    [Fact]
    public async Task HandleAsync_WhenValidatorReturnsError_ShouldReturnValidationErrorAndNotTouchTheRepository()
    {
        // Arrange
        RemovePlaylistEntryCommand command = _removePlaylistEntryCommandFixture.Create();
        Error error = DomainErrors.Playlists.PlaylistEntryIdCannotBeEmpty;
        _mockValidator.Validate(Arg.Any<RemovePlaylistEntryCommand>()).Returns([error]);

        // Act
        Result<PlaylistEntryRemovedResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
        _ = _mockCurrentUserService.DidNotReceive().UserId;
        await _mockPlaylistRepository.DidNotReceive().RemoveEntryAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenCurrentUserIsNull_ShouldReturnNotAuthorizedAndNotRemoveTheEntry()
    {
        // Arrange
        RemovePlaylistEntryCommand command = _removePlaylistEntryCommandFixture.Create();
        _mockCurrentUserService.UserId.Returns((Guid?)null);

        // Act
        Result<PlaylistEntryRemovedResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockPlaylistRepository.DidNotReceive().RemoveEntryAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenRemovingTheEntryFails_ShouldReturnThatError()
    {
        // Arrange
        RemovePlaylistEntryCommand command = _removePlaylistEntryCommandFixture.Create();
        Error error = Error.Failure("Persistence.Failure", "The entry could not be removed.");
        _mockPlaylistRepository.RemoveEntryAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        Result<PlaylistEntryRemovedResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
    }

    [Fact]
    public async Task HandleAsync_WhenSuccessful_ShouldReturnTheRemovedEntryWithNoRepositionedEntries()
    {
        // Arrange
        Guid playlistId = Guid.NewGuid();
        Guid playlistEntryId = Guid.NewGuid();
        RemovePlaylistEntryCommand command = _removePlaylistEntryCommandFixture.Create(
            playlistId: playlistId.ToString(),
            playlistEntryId: playlistEntryId.ToString());

        // Act
        Result<PlaylistEntryRemovedResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(playlistId, result.Value.PlaylistId);
        Assert.Equal(playlistEntryId, result.Value.RemovedEntryId);
        Assert.Equal(0, result.Value.RepositionedCount);
        Assert.NotNull(result.Value.UpdatedOnUtc);
        Assert.InRange(result.Value.UpdatedOnUtc!.Value, DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow);
    }

    [Fact]
    public async Task HandleAsync_WhenTheRepositoryReportsARepositionedCount_ShouldPreserveIt()
    {
        // Arrange
        RemovePlaylistEntryCommand command = _removePlaylistEntryCommandFixture.Create();
        _mockPlaylistRepository.RemoveEntryAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(4));

        // Act
        Result<PlaylistEntryRemovedResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(4, result.Value.RepositionedCount);
    }

    [Fact]
    public async Task HandleAsync_WhenCalled_ShouldPassTheRouteIdentifiersToTheRepositoryAndForwardTheCancellationToken()
    {
        // Arrange
        Guid playlistId = Guid.NewGuid();
        Guid playlistEntryId = Guid.NewGuid();
        RemovePlaylistEntryCommand command = _removePlaylistEntryCommandFixture.Create(
            playlistId: playlistId.ToString(),
            playlistEntryId: playlistEntryId.ToString());
        using CancellationTokenSource cancellationTokenSource = new();
        CancellationToken cancellationToken = cancellationTokenSource.Token;

        // Act
        await _sut.HandleAsync(command, cancellationToken);

        // Assert
        await _mockPlaylistRepository.Received(1).RemoveEntryAsync(
            playlistId,
            _userId,
            playlistEntryId,
            Arg.Any<DateTime>(),
            Arg.Is(cancellationToken));
    }
}
