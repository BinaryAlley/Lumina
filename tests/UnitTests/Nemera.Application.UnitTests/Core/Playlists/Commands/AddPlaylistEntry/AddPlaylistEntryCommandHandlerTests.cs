#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Common.DataAccess.Entities.Playlists;
using Lumina.Application.Common.DataAccess.Repositories.MusicLibrary;
using Lumina.Application.Common.DataAccess.Repositories.Playlists;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.DTO.Playlists;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Infrastructure.Authorization.Policies.Common.Base;
using Lumina.Application.Common.Infrastructure.Authorization.Policies.LibraryOwnership;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Core.Playlists.Commands.AddPlaylistEntry;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Playlists;
using Lumina.Application.Fixtures.Common.DTO.Playlists;
using Lumina.Application.Fixtures.Core.Playlists.Commands.AddPlaylistEntry;
using Lumina.Contracts.Responses.Playlists;
using Lumina.Domain.Common.Primitives;
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

namespace Lumina.Application.UnitTests.Core.Playlists.Commands.AddPlaylistEntry;

/// <summary>
/// Contains unit tests for the <see cref="AddPlaylistEntryCommandHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class AddPlaylistEntryCommandHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly ICurrentUserService _mockCurrentUserService;
    private readonly IAuthorizationService _mockAuthorizationService;
    private readonly IPlaylistRepository _mockPlaylistRepository;
    private readonly ITrackRepository _mockTrackRepository;
    private readonly IValidator<AddPlaylistEntryCommand> _mockValidator;
    private readonly AddPlaylistEntryCommandHandler _sut;
    private readonly AddPlaylistEntryCommandFixture _addPlaylistEntryCommandFixture = new();
    private readonly PlaylistEntityFixture _playlistEntityFixture = new();
    private readonly PlaylistEntryDtoFixture _playlistEntryDtoFixture = new();
    private readonly PlaylistBulkAddResultDtoFixture _playlistBulkAddResultDtoFixture = new();
    private readonly TrackEntityFixture _trackEntityFixture = new();
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _libraryId = Guid.NewGuid();

    /// <summary>
    /// Initializes a new instance of the <see cref="AddPlaylistEntryCommandHandlerTests"/> class.
    /// </summary>
    public AddPlaylistEntryCommandHandlerTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockCurrentUserService = Substitute.For<ICurrentUserService>();
        _mockAuthorizationService = Substitute.For<IAuthorizationService>();
        _mockPlaylistRepository = Substitute.For<IPlaylistRepository>();
        _mockTrackRepository = Substitute.For<ITrackRepository>();

        _mockUnitOfWork.PlaylistRepository.Returns(_mockPlaylistRepository);
        _mockUnitOfWork.TrackRepository.Returns(_mockTrackRepository);
        _mockCurrentUserService.UserId.Returns(_userId);
        _mockAuthorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(Arg.Any<Guid>(), Arg.Any<PolicyContext?>(), Arg.Any<CancellationToken>())
            .Returns(true);
        _mockPlaylistRepository.GetByIdForUserAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<PlaylistEntity?>(_playlistEntityFixture.Create(userId: _userId, type: PlaylistType.Music)));
        _mockTrackRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<TrackEntity?>(_trackEntityFixture.Create(libraryId: _libraryId)));
        _mockPlaylistRepository.AddEntriesAsync(
            Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<IReadOnlyList<Guid>>(), Arg.Any<PlaylistMediaItemType>(), Arg.Any<IReadOnlyList<Guid>?>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_playlistBulkAddResultDtoFixture.Create(addedCount: 1)));
        _mockPlaylistRepository.GetAppendedEntryDetailsForUserAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<PlaylistEntryDto?>(_playlistEntryDtoFixture.Create(displayPosition: 0)));

        _mockValidator = Substitute.For<IValidator<AddPlaylistEntryCommand>>();
        _mockValidator.Validate(Arg.Any<AddPlaylistEntryCommand>()).Returns([]);

        _sut = new AddPlaylistEntryCommandHandler(_mockUnitOfWork, _mockCurrentUserService, _mockAuthorizationService, _mockValidator);
    }

    [Fact]
    public async Task HandleAsync_WhenValidatorReturnsError_ShouldReturnValidationErrorAndNotTouchTheRepositories()
    {
        // Arrange
        AddPlaylistEntryCommand command = _addPlaylistEntryCommandFixture.Create();
        Error error = DomainErrors.Playlists.PlaylistIdCannotBeEmpty;
        _mockValidator.Validate(Arg.Any<AddPlaylistEntryCommand>()).Returns([error]);

        // Act
        Result<PlaylistEntryAddedResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
        _ = _mockCurrentUserService.DidNotReceive().UserId;
        await _mockPlaylistRepository.DidNotReceive().GetByIdForUserAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
        await _mockTrackRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
        await _mockAuthorizationService.DidNotReceive().EvaluatePolicyAsync<ILibraryOwnershipPolicy>(Arg.Any<Guid>(), Arg.Any<PolicyContext?>(), Arg.Any<CancellationToken>());
        await _mockPlaylistRepository.DidNotReceive().AddEntriesAsync(
            Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<IReadOnlyList<Guid>>(), Arg.Any<PlaylistMediaItemType>(), Arg.Any<IReadOnlyList<Guid>?>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenCurrentUserIsNull_ShouldReturnNotAuthorizedAndNotReadThePlaylist()
    {
        // Arrange
        AddPlaylistEntryCommand command = _addPlaylistEntryCommandFixture.Create();
        _mockCurrentUserService.UserId.Returns((Guid?)null);

        // Act
        Result<PlaylistEntryAddedResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockPlaylistRepository.DidNotReceive().GetByIdForUserAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
        await _mockTrackRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenThePlaylistReadFails_ShouldReturnThatErrorAndNotReadTheTrack()
    {
        // Arrange
        AddPlaylistEntryCommand command = _addPlaylistEntryCommandFixture.Create();
        Error error = Error.Failure("Persistence.Failure", "The playlist could not be read.");
        _mockPlaylistRepository.GetByIdForUserAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        Result<PlaylistEntryAddedResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
        await _mockTrackRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
        await _mockPlaylistRepository.DidNotReceive().AddEntriesAsync(
            Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<IReadOnlyList<Guid>>(), Arg.Any<PlaylistMediaItemType>(), Arg.Any<IReadOnlyList<Guid>?>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenThePlaylistDoesNotExistForTheUser_ShouldReturnPlaylistNotFoundAndNotReadTheTrack()
    {
        // Arrange
        AddPlaylistEntryCommand command = _addPlaylistEntryCommandFixture.Create();
        _mockPlaylistRepository.GetByIdForUserAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<PlaylistEntity?>(null));

        // Act
        Result<PlaylistEntryAddedResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Playlists.PlaylistNotFound, result.FirstError);
        await _mockTrackRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenTheTrackReadFails_ShouldReturnThatErrorAndNotAuthorize()
    {
        // Arrange
        AddPlaylistEntryCommand command = _addPlaylistEntryCommandFixture.Create();
        Error error = Error.Failure("Persistence.Failure", "The track could not be read.");
        _mockTrackRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        Result<PlaylistEntryAddedResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
        await _mockAuthorizationService.DidNotReceive().EvaluatePolicyAsync<ILibraryOwnershipPolicy>(Arg.Any<Guid>(), Arg.Any<PolicyContext?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenTheTrackDoesNotExist_ShouldReturnTrackNotFoundAndNotAuthorize()
    {
        // Arrange
        AddPlaylistEntryCommand command = _addPlaylistEntryCommandFixture.Create();
        _mockTrackRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<TrackEntity?>(null));

        // Act
        Result<PlaylistEntryAddedResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Music.TrackNotFound, result.FirstError);
        await _mockAuthorizationService.DidNotReceive().EvaluatePolicyAsync<ILibraryOwnershipPolicy>(Arg.Any<Guid>(), Arg.Any<PolicyContext?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenTheTrackLibraryIsNotAccessible_ShouldReturnTrackNotFoundAndNotAppendTheEntry()
    {
        // Arrange
        AddPlaylistEntryCommand command = _addPlaylistEntryCommandFixture.Create();
        _mockAuthorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(Arg.Any<Guid>(), Arg.Any<PolicyContext?>(), Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        Result<PlaylistEntryAddedResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Music.TrackNotFound, result.FirstError);
        await _mockPlaylistRepository.DidNotReceive().AddEntriesAsync(
            Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<IReadOnlyList<Guid>>(), Arg.Any<PlaylistMediaItemType>(), Arg.Any<IReadOnlyList<Guid>?>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>());
        await _mockPlaylistRepository.DidNotReceive().GetAppendedEntryDetailsForUserAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenThePlaylistCannotContainTracks_ShouldReturnPlaylistDoesNotAcceptMediaItemsAndNotAppendTheEntry()
    {
        // Arrange
        AddPlaylistEntryCommand command = _addPlaylistEntryCommandFixture.Create();
        _mockPlaylistRepository.GetByIdForUserAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<PlaylistEntity?>(_playlistEntityFixture.Create(userId: _userId, type: (PlaylistType)999)));

        // Act
        Result<PlaylistEntryAddedResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Playlists.PlaylistDoesNotAcceptMediaItems, result.FirstError);
        await _mockPlaylistRepository.DidNotReceive().AddEntriesAsync(
            Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<IReadOnlyList<Guid>>(), Arg.Any<PlaylistMediaItemType>(), Arg.Any<IReadOnlyList<Guid>?>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenAppendingTheEntryFails_ShouldReturnThatErrorAndNotReadTheAppendedEntry()
    {
        // Arrange
        AddPlaylistEntryCommand command = _addPlaylistEntryCommandFixture.Create();
        Error error = Error.Failure("Persistence.Failure", "The entry could not be appended.");
        _mockPlaylistRepository.AddEntriesAsync(
            Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<IReadOnlyList<Guid>>(), Arg.Any<PlaylistMediaItemType>(), Arg.Any<IReadOnlyList<Guid>?>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        Result<PlaylistEntryAddedResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
        await _mockPlaylistRepository.DidNotReceive().GetAppendedEntryDetailsForUserAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenTheAppendedEntryReadFails_ShouldReturnThatError()
    {
        // Arrange
        AddPlaylistEntryCommand command = _addPlaylistEntryCommandFixture.Create();
        Error error = Error.Failure("Persistence.Failure", "The appended entry could not be read.");
        _mockPlaylistRepository.GetAppendedEntryDetailsForUserAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        Result<PlaylistEntryAddedResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
    }

    [Fact]
    public async Task HandleAsync_WhenTheAppendedEntryIsMissing_ShouldReturnPlaylistEntryNotFound()
    {
        // Arrange
        AddPlaylistEntryCommand command = _addPlaylistEntryCommandFixture.Create();
        _mockPlaylistRepository.GetAppendedEntryDetailsForUserAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<PlaylistEntryDto?>(null));

        // Act
        Result<PlaylistEntryAddedResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Playlists.PlaylistEntryNotFound, result.FirstError);
    }

    [Fact]
    public async Task HandleAsync_WhenSuccessful_ShouldAppendOneTrackEntryAndReturnTheReadBackEntryAndActivityTime()
    {
        // Arrange
        Guid playlistId = Guid.NewGuid();
        Guid trackId = Guid.NewGuid();
        AddPlaylistEntryCommand command = _addPlaylistEntryCommandFixture.Create(
            playlistId: playlistId.ToString(),
            trackId: trackId);
        PlaylistEntryDto appendedEntry = _playlistEntryDtoFixture.Create(displayPosition: 4);
        _mockPlaylistRepository.GetAppendedEntryDetailsForUserAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<PlaylistEntryDto?>(appendedEntry));
        Guid generatedEntryId = Guid.Empty;
        _mockPlaylistRepository.AddEntriesAsync(
            Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<IReadOnlyList<Guid>>(), Arg.Any<PlaylistMediaItemType>(), Arg.Do<IReadOnlyList<Guid>?>(entryIds => generatedEntryId = entryIds![0]), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_playlistBulkAddResultDtoFixture.Create(addedCount: 1)));

        // Act
        Result<PlaylistEntryAddedResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(playlistId, result.Value.PlaylistId);
        Assert.NotEqual(Guid.Empty, generatedEntryId);
        Assert.Equal(appendedEntry.Entry.Id, result.Value.Entry.Id);
        Assert.Equal(appendedEntry.DisplayPosition, result.Value.Entry.Position);
        Assert.NotNull(result.Value.UpdatedOnUtc);
        Assert.InRange(result.Value.UpdatedOnUtc!.Value, DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow);
        await _mockPlaylistRepository.Received(1).AddEntriesAsync(
            playlistId,
            _userId,
            Arg.Is<IReadOnlyList<Guid>>(mediaItemIds => mediaItemIds.Count == 1 && mediaItemIds[0] == trackId),
            PlaylistMediaItemType.MusicTrack,
            Arg.Is<IReadOnlyList<Guid>?>(entryIds => entryIds != null && entryIds.Count == 1 && entryIds[0] == generatedEntryId),
            Arg.Any<DateTime>(),
            Arg.Any<CancellationToken>());
        await _mockPlaylistRepository.Received(1).GetAppendedEntryDetailsForUserAsync(playlistId, _userId, generatedEntryId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenCalled_ShouldScopeEveryRepositoryAndPolicyCallToTheCurrentUserAndForwardTheCancellationToken()
    {
        // Arrange
        Guid playlistId = Guid.NewGuid();
        Guid trackId = Guid.NewGuid();
        AddPlaylistEntryCommand command = _addPlaylistEntryCommandFixture.Create(
            playlistId: playlistId.ToString(),
            trackId: trackId);
        using CancellationTokenSource cancellationTokenSource = new();
        CancellationToken cancellationToken = cancellationTokenSource.Token;

        // Act
        await _sut.HandleAsync(command, cancellationToken);

        // Assert
        await _mockPlaylistRepository.Received(1).GetByIdForUserAsync(playlistId, _userId, false, false, Arg.Is(cancellationToken));
        await _mockTrackRepository.Received(1).GetByIdAsync(trackId, false, false, Arg.Is(cancellationToken));
        await _mockAuthorizationService.Received(1).EvaluatePolicyAsync<ILibraryOwnershipPolicy>(
            _userId,
            Arg.Is<PolicyContext>(context => context is LibraryOwnershipPolicyContext && ((LibraryOwnershipPolicyContext)context).LibraryId == _libraryId),
            Arg.Is(cancellationToken));
        await _mockPlaylistRepository.Received(1).AddEntriesAsync(
            playlistId, _userId, Arg.Any<IReadOnlyList<Guid>>(), PlaylistMediaItemType.MusicTrack, Arg.Any<IReadOnlyList<Guid>?>(), Arg.Any<DateTime>(), Arg.Is(cancellationToken));
        await _mockPlaylistRepository.Received(1).GetAppendedEntryDetailsForUserAsync(playlistId, _userId, Arg.Any<Guid>(), Arg.Is(cancellationToken));
    }
}
