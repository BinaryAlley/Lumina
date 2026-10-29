#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Playlists;
using Lumina.Application.Common.DataAccess.Repositories.Playlists;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.DTO.Pagination;
using Lumina.Application.Common.DTO.Playlists;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Infrastructure.Authorization.Policies.Common.Base;
using Lumina.Application.Common.Infrastructure.Authorization.Policies.LibraryOwnership;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Core.Playlists.Queries.GetPlaylist;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Playlists;
using Lumina.Application.Fixtures.Common.DTO.Pagination;
using Lumina.Application.Fixtures.Common.DTO.Playlists;
using Lumina.Application.Fixtures.Core.Playlists.Queries.GetPlaylist;
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

namespace Lumina.Application.UnitTests.Core.Playlists.Queries.GetPlaylist;

/// <summary>
/// Contains unit tests for the <see cref="GetPlaylistQueryHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetPlaylistQueryHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly ICurrentUserService _mockCurrentUserService;
    private readonly IAuthorizationService _mockAuthorizationService;
    private readonly IPlaylistRepository _mockPlaylistRepository;
    private readonly IValidator<GetPlaylistQuery> _mockValidator;
    private readonly GetPlaylistQueryHandler _sut;
    private readonly GetPlaylistQueryFixture _getPlaylistQueryFixture = new();
    private readonly PlaylistEntityFixture _playlistEntityFixture = new();
    private readonly PlaylistEntryDtoFixture _playlistEntryDtoFixture = new();
    private readonly PlaylistEntryDisplayDtoFixture _playlistEntryDisplayDtoFixture = new();
    private readonly PlaylistDetailsDtoFixture _playlistDetailsDtoFixture = new();
    private readonly PaginatedResultDtoFixture<PlaylistEntryDto> _paginatedResultDtoFixture = new();
    private readonly Guid _userId = Guid.NewGuid();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetPlaylistQueryHandlerTests"/> class.
    /// </summary>
    public GetPlaylistQueryHandlerTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockCurrentUserService = Substitute.For<ICurrentUserService>();
        _mockAuthorizationService = Substitute.For<IAuthorizationService>();
        _mockPlaylistRepository = Substitute.For<IPlaylistRepository>();

        _mockUnitOfWork.PlaylistRepository.Returns(_mockPlaylistRepository);
        _mockCurrentUserService.UserId.Returns(_userId);
        _mockAuthorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(Arg.Any<Guid>(), Arg.Any<PolicyContext>(), Arg.Any<CancellationToken>())
            .Returns(true);
        _mockPlaylistRepository.GetDetailsForUserAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<PaginationDataDto?>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<PlaylistDetailsDto?>(_playlistDetailsDtoFixture.Create()));

        _mockValidator = Substitute.For<IValidator<GetPlaylistQuery>>();
        _mockValidator.Validate(Arg.Any<GetPlaylistQuery>()).Returns([]);

        _sut = new GetPlaylistQueryHandler(_mockUnitOfWork, _mockCurrentUserService, _mockAuthorizationService, _mockValidator);
    }

    [Fact]
    public async Task HandleAsync_WhenValidatorReturnsError_ShouldReturnValidationErrorAndNotReadTheRepositoriesOrAuthorize()
    {
        // Arrange
        GetPlaylistQuery query = _getPlaylistQueryFixture.Create();
        Error error = DomainErrors.Playlists.PlaylistIdCannotBeEmpty;
        _mockValidator.Validate(Arg.Any<GetPlaylistQuery>()).Returns([error]);

        // Act
        Result<PlaylistResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
        _ = _mockCurrentUserService.DidNotReceive().UserId;
        await _mockPlaylistRepository.DidNotReceive().GetDetailsForUserAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<PaginationDataDto?>(), Arg.Any<CancellationToken>());
        await _mockAuthorizationService.DidNotReceive().EvaluatePolicyAsync<ILibraryOwnershipPolicy>(Arg.Any<Guid>(), Arg.Any<PolicyContext>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenCurrentUserIsNull_ShouldReturnNotAuthorizedAndNotReadTheRepositories()
    {
        // Arrange
        GetPlaylistQuery query = _getPlaylistQueryFixture.Create();
        _mockCurrentUserService.UserId.Returns((Guid?)null);

        // Act
        Result<PlaylistResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockPlaylistRepository.DidNotReceive().GetDetailsForUserAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<PaginationDataDto?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenTheRepositoryFails_ShouldReturnThatErrorAndNotAuthorize()
    {
        // Arrange
        GetPlaylistQuery query = _getPlaylistQueryFixture.Create();
        Error error = Error.Failure("Persistence.Failure", "The playlist could not be read.");
        _mockPlaylistRepository.GetDetailsForUserAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<PaginationDataDto?>(), Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        Result<PlaylistResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
        await _mockAuthorizationService.DidNotReceive().EvaluatePolicyAsync<ILibraryOwnershipPolicy>(Arg.Any<Guid>(), Arg.Any<PolicyContext>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenThePlaylistDoesNotExist_ShouldReturnPlaylistNotFound()
    {
        // Arrange
        GetPlaylistQuery query = _getPlaylistQueryFixture.Create();
        _mockPlaylistRepository.GetDetailsForUserAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<PaginationDataDto?>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<PlaylistDetailsDto?>(null));

        // Act
        Result<PlaylistResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Playlists.PlaylistNotFound, result.FirstError);
    }

    [Fact]
    public async Task HandleAsync_WhenTheEntriesHaveNoDisplayData_ShouldKeepEveryRowWithEmptyDisplayFields()
    {
        // Arrange
        PlaylistEntity playlist = _playlistEntityFixture.Create(userId: _userId);
        PlaylistEntryDto entry = _playlistEntryDtoFixture.Create(displayPosition: 4, includeDisplay: false);
        PaginatedResultDto<PlaylistEntryDto> entries = _paginatedResultDtoFixture.Create(
            data: [entry],
            currentPage: 2,
            perPage: 10,
            count: 15,
            numberOfPages: 2);
        PlaylistDetailsDto details = _playlistDetailsDtoFixture.Create(playlist: playlist, entries: entries);
        GetPlaylistQuery query = _getPlaylistQueryFixture.Create();
        _mockPlaylistRepository.GetDetailsForUserAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<PaginationDataDto?>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<PlaylistDetailsDto?>(details));

        // Act
        Result<PlaylistResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(playlist.Id, result.Value.Id);
        Assert.Equal(2, result.Value.Entries.CurrentPage);
        Assert.Equal(10, result.Value.Entries.PerPage);
        Assert.Equal(15, result.Value.Entries.Count);
        Assert.Equal(2, result.Value.Entries.NumberOfPages);
        PlaylistEntryResponse response = Assert.Single(result.Value.Entries.Data);
        Assert.Equal(entry.Entry.Id, response.Id);
        Assert.Equal(entry.Entry.MediaItemId, response.MediaItemId);
        Assert.Equal(entry.Entry.MediaItemType, response.MediaItemType);
        Assert.Equal(4, response.Position);
        Assert.Equal(Guid.Empty, response.LibraryId);
        Assert.Equal(Guid.Empty, response.CollectionId);
        Assert.Equal(Guid.Empty, response.CreatorId);
        Assert.Equal(string.Empty, response.MediaItemTitle);
        Assert.Equal(string.Empty, response.CollectionTitle);
        Assert.Equal(string.Empty, response.CreatorName);
        Assert.Equal(0, response.DurationInSeconds);
        Assert.Null(response.CoverPath);
        await _mockAuthorizationService.DidNotReceive().EvaluatePolicyAsync<ILibraryOwnershipPolicy>(Arg.Any<Guid>(), Arg.Any<PolicyContext>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenTheEntriesHaveDisplayData_ShouldMapEveryDisplayFieldAndPreserveThePaginationMetadata()
    {
        // Arrange
        PlaylistEntity playlist = _playlistEntityFixture.Create(
            userId: _userId,
            name: "Focus",
            description: "Deep work",
            type: PlaylistType.Music);
        PlaylistEntryDisplayDto display = _playlistEntryDisplayDtoFixture.Create();
        PlaylistEntryDto entry = _playlistEntryDtoFixture.Create(display: display, displayPosition: 7);
        PaginatedResultDto<PlaylistEntryDto> entries = _paginatedResultDtoFixture.Create(
            data: [entry],
            currentPage: 1,
            perPage: 200,
            count: 1,
            numberOfPages: 1);
        PlaylistDetailsDto details = _playlistDetailsDtoFixture.Create(playlist: playlist, entries: entries);
        GetPlaylistQuery query = _getPlaylistQueryFixture.Create();
        _mockPlaylistRepository.GetDetailsForUserAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<PaginationDataDto?>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<PlaylistDetailsDto?>(details));

        // Act
        Result<PlaylistResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(playlist.Id, result.Value.Id);
        Assert.Equal("Focus", result.Value.Name);
        Assert.Equal("Deep work", result.Value.Description);
        Assert.Equal(PlaylistType.Music, result.Value.Type);
        Assert.Equal(playlist.CreatedOnUtc, result.Value.CreatedOnUtc);
        Assert.Equal(playlist.UpdatedOnUtc, result.Value.UpdatedOnUtc);
        PlaylistEntryResponse response = Assert.Single(result.Value.Entries.Data);
        Assert.Equal(entry.Entry.Id, response.Id);
        Assert.Equal(entry.Entry.MediaItemId, response.MediaItemId);
        Assert.Equal(entry.Entry.MediaItemType, response.MediaItemType);
        Assert.Equal(7, response.Position);
        Assert.Equal(display.LibraryId, response.LibraryId);
        Assert.Equal(display.AlbumId, response.CollectionId);
        Assert.Equal(display.ArtistId, response.CreatorId);
        Assert.Equal(display.TrackTitle, response.MediaItemTitle);
        Assert.Equal(display.AlbumTitle, response.CollectionTitle);
        Assert.Equal(display.ArtistName, response.CreatorName);
        Assert.Equal(display.DurationInSeconds, response.DurationInSeconds);
        Assert.Equal(display.CoverPath, response.CoverPath);
        Assert.Equal(1, result.Value.Entries.CurrentPage);
        Assert.Equal(200, result.Value.Entries.PerPage);
        Assert.Equal(1, result.Value.Entries.Count);
        Assert.Equal(1, result.Value.Entries.NumberOfPages);
    }

    [Fact]
    public async Task HandleAsync_WhenSeveralEntriesShareOneLibrary_ShouldEvaluateTheLibraryPolicyOnceForThatLibrary()
    {
        // Arrange
        Guid sharedLibraryId = Guid.NewGuid();
        PlaylistEntryDto firstEntry = _playlistEntryDtoFixture.Create(
            display: _playlistEntryDisplayDtoFixture.Create(libraryId: sharedLibraryId),
            displayPosition: 0);
        PlaylistEntryDto secondEntry = _playlistEntryDtoFixture.Create(
            display: _playlistEntryDisplayDtoFixture.Create(libraryId: sharedLibraryId),
            displayPosition: 1);
        PlaylistDetailsDto details = _playlistDetailsDtoFixture.Create(
            entries: _paginatedResultDtoFixture.Create(data: [firstEntry, secondEntry]));
        GetPlaylistQuery query = _getPlaylistQueryFixture.Create();
        _mockPlaylistRepository.GetDetailsForUserAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<PaginationDataDto?>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<PlaylistDetailsDto?>(details));

        // Act
        Result<PlaylistResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        await _mockAuthorizationService.Received(1).EvaluatePolicyAsync<ILibraryOwnershipPolicy>(
            _userId,
            Arg.Is<PolicyContext>(context => context is LibraryOwnershipPolicyContext && ((LibraryOwnershipPolicyContext)context).LibraryId == sharedLibraryId),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenOneLibraryIsDenied_ShouldKeepItsRowsWithoutDisplayDataAndLeaveTheAccessibleRowsUnchanged()
    {
        // Arrange
        Guid accessibleLibraryId = Guid.NewGuid();
        Guid deniedLibraryId = Guid.NewGuid();
        PlaylistEntryDisplayDto accessibleDisplay = _playlistEntryDisplayDtoFixture.Create(libraryId: accessibleLibraryId);
        PlaylistEntryDto accessibleEntry = _playlistEntryDtoFixture.Create(display: accessibleDisplay, displayPosition: 0);
        PlaylistEntryDisplayDto deniedDisplay = _playlistEntryDisplayDtoFixture.Create(libraryId: deniedLibraryId, trackTitle: "denied title");
        PlaylistEntryDto deniedEntry = _playlistEntryDtoFixture.Create(display: deniedDisplay, displayPosition: 1);
        PlaylistEntryDto entryWithoutDisplay = _playlistEntryDtoFixture.Create(includeDisplay: false, displayPosition: 2);
        PlaylistDetailsDto details = _playlistDetailsDtoFixture.Create(
            entries: _paginatedResultDtoFixture.Create(data: [accessibleEntry, deniedEntry, entryWithoutDisplay]));
        GetPlaylistQuery query = _getPlaylistQueryFixture.Create();
        _mockPlaylistRepository.GetDetailsForUserAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<PaginationDataDto?>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<PlaylistDetailsDto?>(details));
        _mockAuthorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(
            _userId,
            Arg.Is<PolicyContext>(context => context is LibraryOwnershipPolicyContext && ((LibraryOwnershipPolicyContext)context).LibraryId == deniedLibraryId),
            Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        Result<PlaylistResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(3, result.Value.Entries.Data.Count);
        Assert.Collection(
            result.Value.Entries.Data,
            entryResponse =>
            {
                Assert.Equal(accessibleDisplay.TrackTitle, entryResponse.MediaItemTitle);
                Assert.Equal(accessibleDisplay.LibraryId, entryResponse.LibraryId);
            },
            entryResponse =>
            {
                Assert.Equal(deniedEntry.Entry.MediaItemId, entryResponse.MediaItemId);
                Assert.Equal(Guid.Empty, entryResponse.LibraryId);
                Assert.Equal(string.Empty, entryResponse.MediaItemTitle);
                Assert.Null(entryResponse.CoverPath);
            },
            entryResponse =>
            {
                Assert.Equal(entryWithoutDisplay.Entry.MediaItemId, entryResponse.MediaItemId);
                Assert.Equal(Guid.Empty, entryResponse.LibraryId);
                Assert.Equal(string.Empty, entryResponse.MediaItemTitle);
            });
        Assert.Equal(1, result.Value.Entries.Data[1].Position);
    }

    [Fact]
    public async Task HandleAsync_WhenNoEntryHasDisplayData_ShouldNotEvaluateAnyLibraryPolicy()
    {
        // Arrange
        PlaylistDetailsDto details = _playlistDetailsDtoFixture.Create(
            entries: _paginatedResultDtoFixture.Create(data: [_playlistEntryDtoFixture.Create(includeDisplay: false)]));
        GetPlaylistQuery query = _getPlaylistQueryFixture.Create();
        _mockPlaylistRepository.GetDetailsForUserAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<PaginationDataDto?>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<PlaylistDetailsDto?>(details));

        // Act
        await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        await _mockAuthorizationService.DidNotReceive().EvaluatePolicyAsync<ILibraryOwnershipPolicy>(Arg.Any<Guid>(), Arg.Any<PolicyContext>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenCalled_ShouldPassThePlaylistTheCurrentUserAndThePaginationDataToTheRepositoryAndForwardTheCancellationToken()
    {
        // Arrange
        Guid playlistId = Guid.NewGuid();
        PaginationDataDto paginationData = new() { CurrentPage = 5, PerPage = 20 };
        GetPlaylistQuery query = _getPlaylistQueryFixture.Create(
            playlistId: playlistId.ToString(),
            paginationData: paginationData);
        using CancellationTokenSource cancellationTokenSource = new();
        CancellationToken cancellationToken = cancellationTokenSource.Token;

        // Act
        await _sut.HandleAsync(query, cancellationToken);

        // Assert
        await _mockPlaylistRepository.Received(1).GetDetailsForUserAsync(
            playlistId,
            _userId,
            Arg.Is<PaginationDataDto>(data => ReferenceEquals(data, paginationData)),
            Arg.Is(cancellationToken));
    }
}
