#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Playlists;
using Lumina.Application.Common.DataAccess.Repositories.Playlists;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.DTO.Playlists;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Infrastructure.Authorization.Policies.Common.Base;
using Lumina.Application.Common.Infrastructure.Authorization.Policies.LibraryOwnership;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Core.Playlists.Commands.AddPlaylistScopeEntries;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Playlists;
using Lumina.Application.Fixtures.Common.DTO.Playlists;
using Lumina.Application.Fixtures.Core.Playlists.Commands.AddPlaylistScopeEntries;
using Lumina.Contracts.Responses.Playlists;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.Playlists;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
using DomainErrors = Lumina.Domain.Common.Errors.Errors;
#endregion

namespace Lumina.Application.UnitTests.Core.Playlists.Commands.AddPlaylistScopeEntries;

/// <summary>
/// Contains unit tests for the <see cref="AddPlaylistScopeEntriesCommandHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class AddPlaylistScopeEntriesCommandHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly ICurrentUserService _mockCurrentUserService;
    private readonly IAuthorizationService _mockAuthorizationService;
    private readonly IPlaylistRepository _mockPlaylistRepository;
    private readonly IValidator<AddPlaylistScopeEntriesCommand> _mockValidator;
    private readonly AddPlaylistScopeEntriesCommandHandler _sut;
    private readonly AddPlaylistScopeEntriesCommandFixture _addPlaylistScopeEntriesCommandFixture = new();
    private readonly PlaylistEntityFixture _playlistEntityFixture = new();
    private readonly PlaylistBulkAddResultDtoFixture _playlistBulkAddResultDtoFixture = new();
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _libraryId = Guid.NewGuid();

    /// <summary>
    /// Initializes a new instance of the <see cref="AddPlaylistScopeEntriesCommandHandlerTests"/> class.
    /// </summary>
    public AddPlaylistScopeEntriesCommandHandlerTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockCurrentUserService = Substitute.For<ICurrentUserService>();
        _mockAuthorizationService = Substitute.For<IAuthorizationService>();
        _mockPlaylistRepository = Substitute.For<IPlaylistRepository>();

        _mockUnitOfWork.PlaylistRepository.Returns(_mockPlaylistRepository);
        _mockCurrentUserService.UserId.Returns(_userId);
        _mockAuthorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(Arg.Any<Guid>(), Arg.Any<PolicyContext?>(), Arg.Any<CancellationToken>())
            .Returns(true);
        _mockPlaylistRepository.GetByIdForUserAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<PlaylistEntity?>(_playlistEntityFixture.Create(userId: _userId, type: PlaylistType.Music)));
        _mockPlaylistRepository.AddScopeEntriesAsync(
            Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid?>(), Arg.Any<Guid?>(), Arg.Any<PlaylistMediaItemType>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_playlistBulkAddResultDtoFixture.Create(addedCount: 5)));

        _mockValidator = Substitute.For<IValidator<AddPlaylistScopeEntriesCommand>>();
        _mockValidator.Validate(Arg.Any<AddPlaylistScopeEntriesCommand>()).Returns([]);

        _sut = new AddPlaylistScopeEntriesCommandHandler(_mockUnitOfWork, _mockCurrentUserService, _mockAuthorizationService, _mockValidator);
    }

    [Fact]
    public async Task HandleAsync_WhenValidatorReturnsError_ShouldReturnValidationErrorAndNotTouchTheRepositories()
    {
        // Arrange
        AddPlaylistScopeEntriesCommand command = _addPlaylistScopeEntriesCommandFixture.Create();
        Error error = DomainErrors.Library.LibraryIdCannotBeEmpty;
        _mockValidator.Validate(Arg.Any<AddPlaylistScopeEntriesCommand>()).Returns([error]);

        // Act
        Result<PlaylistEntriesAddedResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
        _ = _mockCurrentUserService.DidNotReceive().UserId;
        await _mockPlaylistRepository.DidNotReceive().GetByIdForUserAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
        await _mockAuthorizationService.DidNotReceive().EvaluatePolicyAsync<ILibraryOwnershipPolicy>(Arg.Any<Guid>(), Arg.Any<PolicyContext?>(), Arg.Any<CancellationToken>());
        await _mockPlaylistRepository.DidNotReceive().AddScopeEntriesAsync(
            Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid?>(), Arg.Any<Guid?>(), Arg.Any<PlaylistMediaItemType>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenCurrentUserIsNull_ShouldReturnNotAuthorizedAndNotReadThePlaylist()
    {
        // Arrange
        AddPlaylistScopeEntriesCommand command = _addPlaylistScopeEntriesCommandFixture.Create();
        _mockCurrentUserService.UserId.Returns((Guid?)null);

        // Act
        Result<PlaylistEntriesAddedResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockPlaylistRepository.DidNotReceive().GetByIdForUserAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenThePlaylistReadFails_ShouldReturnThatErrorAndNotAuthorize()
    {
        // Arrange
        AddPlaylistScopeEntriesCommand command = _addPlaylistScopeEntriesCommandFixture.Create();
        Error error = Error.Failure("Persistence.Failure", "The playlist could not be read.");
        _mockPlaylistRepository.GetByIdForUserAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        Result<PlaylistEntriesAddedResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
        await _mockAuthorizationService.DidNotReceive().EvaluatePolicyAsync<ILibraryOwnershipPolicy>(Arg.Any<Guid>(), Arg.Any<PolicyContext?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenThePlaylistDoesNotExistForTheUser_ShouldReturnPlaylistNotFoundAndNotAuthorize()
    {
        // Arrange
        AddPlaylistScopeEntriesCommand command = _addPlaylistScopeEntriesCommandFixture.Create();
        _mockPlaylistRepository.GetByIdForUserAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<PlaylistEntity?>(null));

        // Act
        Result<PlaylistEntriesAddedResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Playlists.PlaylistNotFound, result.FirstError);
        await _mockAuthorizationService.DidNotReceive().EvaluatePolicyAsync<ILibraryOwnershipPolicy>(Arg.Any<Guid>(), Arg.Any<PolicyContext?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenTheLibraryIsNotAccessible_ShouldReturnNotAuthorizedAndNotAppendTheScope()
    {
        // Arrange
        AddPlaylistScopeEntriesCommand command = _addPlaylistScopeEntriesCommandFixture.Create();
        _mockAuthorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(Arg.Any<Guid>(), Arg.Any<PolicyContext?>(), Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        Result<PlaylistEntriesAddedResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockPlaylistRepository.DidNotReceive().AddScopeEntriesAsync(
            Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid?>(), Arg.Any<Guid?>(), Arg.Any<PlaylistMediaItemType>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenThePlaylistCannotContainTracks_ShouldReturnPlaylistDoesNotAcceptMediaItemsAndNotAppendTheScope()
    {
        // Arrange
        AddPlaylistScopeEntriesCommand command = _addPlaylistScopeEntriesCommandFixture.Create();
        _mockPlaylistRepository.GetByIdForUserAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<PlaylistEntity?>(_playlistEntityFixture.Create(userId: _userId, type: (PlaylistType)999)));

        // Act
        Result<PlaylistEntriesAddedResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Playlists.PlaylistDoesNotAcceptMediaItems, result.FirstError);
        await _mockPlaylistRepository.DidNotReceive().AddScopeEntriesAsync(
            Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid?>(), Arg.Any<Guid?>(), Arg.Any<PlaylistMediaItemType>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenAppendingTheScopeFails_ShouldReturnThatError()
    {
        // Arrange
        AddPlaylistScopeEntriesCommand command = _addPlaylistScopeEntriesCommandFixture.Create();
        Error error = Error.Failure("Persistence.Failure", "The scope could not be appended.");
        _mockPlaylistRepository.AddScopeEntriesAsync(
            Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid?>(), Arg.Any<Guid?>(), Arg.Any<PlaylistMediaItemType>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        Result<PlaylistEntriesAddedResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
    }

    [Fact]
    public async Task HandleAsync_WhenTheScopeAddedEntries_ShouldReturnTheAddedCountAndTheActivityTime()
    {
        // Arrange
        Guid playlistId = Guid.NewGuid();
        AddPlaylistScopeEntriesCommand command = _addPlaylistScopeEntriesCommandFixture.Create(
            playlistId: playlistId.ToString(),
            libraryId: _libraryId);
        _mockPlaylistRepository.AddScopeEntriesAsync(
            Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid?>(), Arg.Any<Guid?>(), Arg.Any<PlaylistMediaItemType>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_playlistBulkAddResultDtoFixture.Create(addedCount: 7)));

        // Act
        Result<PlaylistEntriesAddedResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(playlistId, result.Value.PlaylistId);
        Assert.Equal(7, result.Value.AddedCount);
        Assert.NotNull(result.Value.UpdatedOnUtc);
        Assert.InRange(result.Value.UpdatedOnUtc!.Value, DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow);
    }

    [Fact]
    public async Task HandleAsync_WhenTheScopeIsEmptyForTheUser_ShouldReturnZeroAddedEntriesWithoutAnActivityTime()
    {
        // Arrange
        AddPlaylistScopeEntriesCommand command = _addPlaylistScopeEntriesCommandFixture.Create(libraryId: _libraryId);
        _mockPlaylistRepository.AddScopeEntriesAsync(
            Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid?>(), Arg.Any<Guid?>(), Arg.Any<PlaylistMediaItemType>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_playlistBulkAddResultDtoFixture.Create(addedCount: 0)));

        // Act
        Result<PlaylistEntriesAddedResponse> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(0, result.Value.AddedCount);
        Assert.Null(result.Value.UpdatedOnUtc);
    }

    [Fact]
    public async Task HandleAsync_WhenCalled_ShouldPassTheResolvedScopeToTheRepositoryAndForwardTheCancellationToken()
    {
        // Arrange
        Guid playlistId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        AddPlaylistScopeEntriesCommand command = _addPlaylistScopeEntriesCommandFixture.Create(
            playlistId: playlistId.ToString(),
            libraryId: _libraryId,
            artistId: artistId,
            albumId: albumId,
            includeArtistId: true,
            includeAlbumId: true);
        using CancellationTokenSource cancellationTokenSource = new();
        CancellationToken cancellationToken = cancellationTokenSource.Token;

        // Act
        await _sut.HandleAsync(command, cancellationToken);

        // Assert
        await _mockPlaylistRepository.Received(1).GetByIdForUserAsync(playlistId, _userId, false, false, Arg.Is(cancellationToken));
        await _mockAuthorizationService.Received(1).EvaluatePolicyAsync<ILibraryOwnershipPolicy>(
            _userId,
            Arg.Is<PolicyContext>(context => context is LibraryOwnershipPolicyContext && ((LibraryOwnershipPolicyContext)context).LibraryId == _libraryId),
            Arg.Is(cancellationToken));
        await _mockPlaylistRepository.Received(1).AddScopeEntriesAsync(
            playlistId,
            _userId,
            _libraryId,
            artistId,
            albumId,
            PlaylistMediaItemType.MusicTrack,
            Arg.Any<DateTime>(),
            Arg.Is(cancellationToken));
    }
}
