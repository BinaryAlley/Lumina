#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Playlists;
using Lumina.Application.Common.DataAccess.Repositories.MusicLibrary;
using Lumina.Application.Common.DataAccess.Repositories.Playlists;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;
using Lumina.Application.Common.DTO.Playlists;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Infrastructure.Authorization.Policies.Common.Base;
using Lumina.Application.Common.Infrastructure.Authorization.Policies.LibraryOwnership;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Core.Playlists.Commands.AddPlaylistEntries;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Playlists;
using Lumina.Application.Fixtures.Common.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;
using Lumina.Application.Fixtures.Common.DTO.Playlists;
using Lumina.Application.Fixtures.Core.Playlists.Commands.AddPlaylistEntries;
using Lumina.Contracts.Responses.Playlists;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.Playlists;
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

namespace Lumina.Application.UnitTests.Core.Playlists.Commands.AddPlaylistEntries;

/// <summary>
/// Contains unit tests for the <see cref="AddPlaylistEntriesCommandHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class AddPlaylistEntriesCommandHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly ICurrentUserService _mockCurrentUserService;
    private readonly IAuthorizationService _mockAuthorizationService;
    private readonly IPlaylistRepository _mockPlaylistRepository;
    private readonly ITrackRepository _mockTrackRepository;
    private readonly IValidator<AddPlaylistEntriesCommand> _mockValidator;
    private readonly AddPlaylistEntriesCommandHandler _sut;
    private readonly AddPlaylistEntriesCommandFixture _addPlaylistEntriesCommandFixture = new();
    private readonly PlaylistEntityFixture _playlistEntityFixture = new();
    private readonly PlaylistBulkAddResultDtoFixture _playlistBulkAddResultDtoFixture = new();
    private readonly TrackAccessSummaryDtoFixture _trackAccessSummaryDtoFixture = new();
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _libraryId = Guid.NewGuid();

    /// <summary>
    /// Initializes a new instance of the <see cref="AddPlaylistEntriesCommandHandlerTests"/> class.
    /// </summary>
    public AddPlaylistEntriesCommandHandlerTests()
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
        _mockTrackRepository.GetAccessInfoByIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Result.From(_trackAccessSummaryDtoFixture.Create(
                trackCount: callInfo.ArgAt<IReadOnlyCollection<Guid>>(0).Count,
                libraryIds: [_libraryId])));
        _mockPlaylistRepository.AddEntriesAsync(
            Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<IReadOnlyList<Guid>>(), Arg.Any<PlaylistMediaItemType>(), Arg.Any<IReadOnlyList<Guid>?>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_playlistBulkAddResultDtoFixture.Create(addedCount: 3)));

        _mockValidator = Substitute.For<IValidator<AddPlaylistEntriesCommand>>();
        _mockValidator.Validate(Arg.Any<AddPlaylistEntriesCommand>()).Returns([]);

        _sut = new AddPlaylistEntriesCommandHandler(_mockUnitOfWork, _mockCurrentUserService, _mockAuthorizationService, _mockValidator);
    }

    [Fact]
    public async Task HandleAsync_WhenValidatorReturnsError_ShouldReturnValidationErrorAndNotTouchTheRepositories()
    {
        // Arrange
        AddPlaylistEntriesCommand command = _addPlaylistEntriesCommandFixture.Create();
        Error error = DomainErrors.Music.TrackIdCannotBeEmpty;
        _mockValidator.Validate(Arg.Any<AddPlaylistEntriesCommand>()).Returns([error]);

        // Act
        Result<PlaylistEntriesAddedResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
        _ = _mockCurrentUserService.DidNotReceive().UserId;
        await _mockPlaylistRepository.DidNotReceive().GetByIdForUserAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
        await _mockTrackRepository.DidNotReceive().GetAccessInfoByIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>());
        await _mockPlaylistRepository.DidNotReceive().AddEntriesAsync(
            Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<IReadOnlyList<Guid>>(), Arg.Any<PlaylistMediaItemType>(), Arg.Any<IReadOnlyList<Guid>?>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenCurrentUserIsNull_ShouldReturnNotAuthorizedAndNotReadThePlaylist()
    {
        // Arrange
        AddPlaylistEntriesCommand command = _addPlaylistEntriesCommandFixture.Create();
        _mockCurrentUserService.UserId.Returns((Guid?)null);

        // Act
        Result<PlaylistEntriesAddedResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockPlaylistRepository.DidNotReceive().GetByIdForUserAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenThePlaylistReadFails_ShouldReturnThatErrorAndNotResolveTheTracks()
    {
        // Arrange
        AddPlaylistEntriesCommand command = _addPlaylistEntriesCommandFixture.Create();
        Error error = Error.Failure("Persistence.Failure", "The playlist could not be read.");
        _mockPlaylistRepository.GetByIdForUserAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        Result<PlaylistEntriesAddedResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
        await _mockTrackRepository.DidNotReceive().GetAccessInfoByIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenThePlaylistDoesNotExistForTheUser_ShouldReturnPlaylistNotFoundAndNotResolveTheTracks()
    {
        // Arrange
        AddPlaylistEntriesCommand command = _addPlaylistEntriesCommandFixture.Create();
        _mockPlaylistRepository.GetByIdForUserAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<PlaylistEntity?>(null));

        // Act
        Result<PlaylistEntriesAddedResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Playlists.PlaylistNotFound, result.FirstError);
        await _mockTrackRepository.DidNotReceive().GetAccessInfoByIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenTheTrackAccessLookupFails_ShouldReturnThatErrorAndNotAuthorize()
    {
        // Arrange
        AddPlaylistEntriesCommand command = _addPlaylistEntriesCommandFixture.Create();
        Error error = Error.Failure("Persistence.Failure", "The tracks could not be read.");
        _mockTrackRepository.GetAccessInfoByIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        Result<PlaylistEntriesAddedResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
        await _mockAuthorizationService.DidNotReceive().EvaluatePolicyAsync<ILibraryOwnershipPolicy>(Arg.Any<Guid>(), Arg.Any<PolicyContext?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenAtLeastOneOfTheTracksDoesNotExist_ShouldReturnTrackNotFoundAndNotAuthorize()
    {
        // Arrange
        AddPlaylistEntriesCommand command = _addPlaylistEntriesCommandFixture.Create(trackCount: 3);
        _mockTrackRepository.GetAccessInfoByIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_trackAccessSummaryDtoFixture.Create(trackCount: 2, libraryIds: [_libraryId])));

        // Act
        Result<PlaylistEntriesAddedResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Music.TrackNotFound, result.FirstError);
        await _mockAuthorizationService.DidNotReceive().EvaluatePolicyAsync<ILibraryOwnershipPolicy>(Arg.Any<Guid>(), Arg.Any<PolicyContext?>(), Arg.Any<CancellationToken>());
        await _mockPlaylistRepository.DidNotReceive().AddEntriesAsync(
            Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<IReadOnlyList<Guid>>(), Arg.Any<PlaylistMediaItemType>(), Arg.Any<IReadOnlyList<Guid>?>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenOneOfTheLibrariesIsNotAccessible_ShouldReturnTrackNotFoundAndNotAppendTheEntries()
    {
        // Arrange
        Guid deniedLibraryId = Guid.NewGuid();
        AddPlaylistEntriesCommand command = _addPlaylistEntriesCommandFixture.Create();
        _mockTrackRepository.GetAccessInfoByIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_trackAccessSummaryDtoFixture.Create(libraryIds: [_libraryId, deniedLibraryId])));
        _mockAuthorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(
            _userId,
            Arg.Is<PolicyContext>(context => context is LibraryOwnershipPolicyContext && ((LibraryOwnershipPolicyContext)context).LibraryId == deniedLibraryId),
            Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        Result<PlaylistEntriesAddedResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Music.TrackNotFound, result.FirstError);
        await _mockPlaylistRepository.DidNotReceive().AddEntriesAsync(
            Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<IReadOnlyList<Guid>>(), Arg.Any<PlaylistMediaItemType>(), Arg.Any<IReadOnlyList<Guid>?>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenThePlaylistCannotContainTracks_ShouldReturnPlaylistDoesNotAcceptMediaItemsAndNotAppendTheEntries()
    {
        // Arrange
        AddPlaylistEntriesCommand command = _addPlaylistEntriesCommandFixture.Create();
        _mockPlaylistRepository.GetByIdForUserAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<PlaylistEntity?>(_playlistEntityFixture.Create(userId: _userId, type: (PlaylistType)999)));

        // Act
        Result<PlaylistEntriesAddedResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Playlists.PlaylistDoesNotAcceptMediaItems, result.FirstError);
        await _mockPlaylistRepository.DidNotReceive().AddEntriesAsync(
            Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<IReadOnlyList<Guid>>(), Arg.Any<PlaylistMediaItemType>(), Arg.Any<IReadOnlyList<Guid>?>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenAppendingTheEntriesFails_ShouldReturnThatError()
    {
        // Arrange
        AddPlaylistEntriesCommand command = _addPlaylistEntriesCommandFixture.Create();
        Error error = Error.Failure("Persistence.Failure", "The entries could not be appended.");
        _mockPlaylistRepository.AddEntriesAsync(
            Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<IReadOnlyList<Guid>>(), Arg.Any<PlaylistMediaItemType>(), Arg.Any<IReadOnlyList<Guid>?>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        Result<PlaylistEntriesAddedResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
    }

    [Fact]
    public async Task HandleAsync_WhenSuccessful_ShouldLookUpDistinctTracksAndAppendEveryRequestedTrackInOrder()
    {
        // Arrange
        Guid playlistId = Guid.NewGuid();
        Guid repeatedTrackId = Guid.NewGuid();
        Guid otherTrackId = Guid.NewGuid();
        List<Guid> trackIds = [repeatedTrackId, otherTrackId, repeatedTrackId];
        AddPlaylistEntriesCommand command = _addPlaylistEntriesCommandFixture.Create(
            playlistId: playlistId.ToString(),
            trackIds: trackIds);
        _mockPlaylistRepository.AddEntriesAsync(
            Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<IReadOnlyList<Guid>>(), Arg.Any<PlaylistMediaItemType>(), Arg.Any<IReadOnlyList<Guid>?>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_playlistBulkAddResultDtoFixture.Create(addedCount: 3)));

        // Act
        Result<PlaylistEntriesAddedResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(playlistId, result.Value.PlaylistId);
        Assert.Equal(3, result.Value.AddedCount);
        Assert.NotNull(result.Value.UpdatedOnUtc);
        Assert.InRange(result.Value.UpdatedOnUtc!.Value, DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow);
        await _mockTrackRepository.Received(1).GetAccessInfoByIdsAsync(
            Arg.Is<IReadOnlyCollection<Guid>>(distinctTrackIds => distinctTrackIds.Count == 2 && distinctTrackIds.Contains(repeatedTrackId) && distinctTrackIds.Contains(otherTrackId)),
            Arg.Any<CancellationToken>());
        await _mockPlaylistRepository.Received(1).AddEntriesAsync(
            playlistId,
            _userId,
            Arg.Is<IReadOnlyList<Guid>>(mediaItemIds => mediaItemIds.SequenceEqual(trackIds)),
            PlaylistMediaItemType.MusicTrack,
            Arg.Is<IReadOnlyList<Guid>?>(entryIds => entryIds == null),
            Arg.Any<DateTime>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenSeveralTracksShareOneLibrary_ShouldEvaluateTheLibraryPolicyOnce()
    {
        // Arrange
        AddPlaylistEntriesCommand command = _addPlaylistEntriesCommandFixture.Create(trackCount: 4);
        _mockTrackRepository.GetAccessInfoByIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_trackAccessSummaryDtoFixture.Create(trackCount: 4, libraryIds: [_libraryId])));

        // Act
        Result<PlaylistEntriesAddedResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        await _mockAuthorizationService.Received(1).EvaluatePolicyAsync<ILibraryOwnershipPolicy>(
            _userId,
            Arg.Is<PolicyContext>(context => context is LibraryOwnershipPolicyContext && ((LibraryOwnershipPolicyContext)context).LibraryId == _libraryId),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenCalled_ShouldScopeEveryRepositoryAndPolicyCallToTheCurrentUserAndForwardTheCancellationToken()
    {
        // Arrange
        Guid playlistId = Guid.NewGuid();
        AddPlaylistEntriesCommand command = _addPlaylistEntriesCommandFixture.Create(playlistId: playlistId.ToString());
        using CancellationTokenSource cancellationTokenSource = new();
        CancellationToken cancellationToken = cancellationTokenSource.Token;

        // Act
        await _sut.HandleAsync(command, cancellationToken);

        // Assert
        await _mockPlaylistRepository.Received(1).GetByIdForUserAsync(playlistId, _userId, false, false, Arg.Is(cancellationToken));
        await _mockTrackRepository.Received(1).GetAccessInfoByIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Is(cancellationToken));
        await _mockAuthorizationService.Received(1).EvaluatePolicyAsync<ILibraryOwnershipPolicy>(_userId, Arg.Any<PolicyContext?>(), Arg.Is(cancellationToken));
        await _mockPlaylistRepository.Received(1).AddEntriesAsync(
            playlistId, _userId, Arg.Any<IReadOnlyList<Guid>>(), PlaylistMediaItemType.MusicTrack, Arg.Any<IReadOnlyList<Guid>?>(), Arg.Any<DateTime>(), Arg.Is(cancellationToken));
    }
}
