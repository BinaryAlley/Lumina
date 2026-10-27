#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaConsumption;
using Lumina.Application.Common.DataAccess.Entities.MediaContributors;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Common.DataAccess.Repositories.BookLibrary;
using Lumina.Application.Common.DataAccess.Repositories.MediaConsumption;
using Lumina.Application.Common.DataAccess.Repositories.MediaContributors;
using Lumina.Application.Common.DataAccess.Repositories.MusicLibrary;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.DTO.Pagination;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Infrastructure.Authorization.Policies.ConsumptionActivity;
using Lumina.Application.Common.Infrastructure.Authorization.Policies.LibraryOwnership;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Core.MediaConsumption.Queries.GetMostConsumed;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaConsumption;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaContributors;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Fixtures.Common.DTO.Pagination;
using Lumina.Application.Fixtures.Core.MediaConsumption.Queries.GetMostConsumed;
using Lumina.Contracts.Responses.Common;
using Lumina.Contracts.Responses.MediaConsumption;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.MediaConsumption;
using Lumina.Domain.SharedKernel.Common.Enums.MediaContributors;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
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

namespace Lumina.Application.UnitTests.Core.MediaConsumption.Queries.GetMostConsumed;

/// <summary>
/// Contains unit tests for the <see cref="GetMostConsumedQueryHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetMostConsumedQueryHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly IConsumptionStatisticsRepository _mockConsumptionStatisticsRepository;
    private readonly ITrackRepository _mockTrackRepository;
    private readonly IMusicArtworkRepository _mockMusicArtworkRepository;
    private readonly IBookRepository _mockBookRepository;
    private readonly IMediaContributorRepository _mockMediaContributorRepository;
    private readonly ICurrentUserService _mockCurrentUserService;
    private readonly IAuthorizationService _mockAuthorizationService;
    private readonly IValidator<GetMostConsumedQuery> _mockValidator;
    private readonly GetMostConsumedQueryHandler _sut;
    private readonly GetMostConsumedQueryFixture _getMostConsumedQueryFixture = new();
    private readonly PaginationDataDtoFixture _paginationDataDtoFixture = new();
    private readonly MediaConsumptionRecordEntityFixture _mediaConsumptionRecordEntityFixture = new();
    private readonly TrackEntityFixture _trackEntityFixture = new();
    private readonly AlbumEntityFixture _albumEntityFixture = new();
    private readonly ArtistEntityFixture _artistEntityFixture = new();
    private readonly MusicArtworkEntityFixture _musicArtworkEntityFixture = new();
    private readonly BookEntityFixture _bookEntityFixture = new();
    private readonly BookContributorEntityFixture _bookContributorEntityFixture = new();
    private readonly BookArtworkEntityFixture _bookArtworkEntityFixture = new();
    private readonly MediaContributorEntityFixture _mediaContributorEntityFixture = new();
    private readonly Guid _userId = Guid.NewGuid();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetMostConsumedQueryHandlerTests"/> class.
    /// </summary>
    public GetMostConsumedQueryHandlerTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockConsumptionStatisticsRepository = Substitute.For<IConsumptionStatisticsRepository>();
        _mockTrackRepository = Substitute.For<ITrackRepository>();
        _mockMusicArtworkRepository = Substitute.For<IMusicArtworkRepository>();
        _mockBookRepository = Substitute.For<IBookRepository>();
        _mockMediaContributorRepository = Substitute.For<IMediaContributorRepository>();
        _mockCurrentUserService = Substitute.For<ICurrentUserService>();
        _mockAuthorizationService = Substitute.For<IAuthorizationService>();

        _mockUnitOfWork.ConsumptionStatisticsRepository.Returns(_mockConsumptionStatisticsRepository);
        _mockUnitOfWork.TrackRepository.Returns(_mockTrackRepository);
        _mockUnitOfWork.MusicArtworkRepository.Returns(_mockMusicArtworkRepository);
        _mockUnitOfWork.BookRepository.Returns(_mockBookRepository);
        _mockUnitOfWork.MediaContributorRepository.Returns(_mockMediaContributorRepository);
        _mockCurrentUserService.UserId.Returns(_userId);
        // Default stubs: the current user can read both the consumption activity and every library, and no display data exists.
        _mockAuthorizationService.EvaluatePolicyAsync<IConsumptionActivityAccessPolicy>(
            Arg.Any<Guid>(), Arg.Any<ConsumptionActivityAccessPolicyContext>(), Arg.Any<CancellationToken>())
            .Returns(true);
        _mockAuthorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(
            Arg.Any<Guid>(), Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>())
            .Returns(true);
        StubRecords();
        StubTracks();
        StubArtwork();
        StubBooks();
        StubAuthors();

        _mockValidator = Substitute.For<IValidator<GetMostConsumedQuery>>();
        _mockValidator.Validate(Arg.Any<GetMostConsumedQuery>()).Returns([]);

        _sut = new GetMostConsumedQueryHandler(_mockUnitOfWork, _mockCurrentUserService, _mockAuthorizationService, _mockValidator);
    }

    [Fact]
    public async Task HandleAsync_WhenValidatorReturnsError_ShouldReturnValidationErrorAndNotReadRecords()
    {
        // Arrange
        GetMostConsumedQuery query = _getMostConsumedQueryFixture.Create();
        _mockValidator.Validate(Arg.Any<GetMostConsumedQuery>())
            .Returns([DomainErrors.MediaConsumption.TargetUserIdIsInvalid]);

        // Act
        Result<PaginatedResponse<ConsumedItemResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.MediaConsumption.TargetUserIdIsInvalid, result.FirstError);
        await _mockConsumptionStatisticsRepository.DidNotReceive().GetForUserAsync(
            Arg.Any<Guid>(), Arg.Any<Guid?>(), Arg.Any<MediaKind?>(), Arg.Any<DateTime?>(), Arg.Any<DateTime?>(), Arg.Any<CancellationToken>());
        await _mockAuthorizationService.DidNotReceive().EvaluatePolicyAsync<IConsumptionActivityAccessPolicy>(
            Arg.Any<Guid>(), Arg.Any<ConsumptionActivityAccessPolicyContext>(), Arg.Any<CancellationToken>());
        await _mockTrackRepository.DidNotReceive().GetByIdsAsync(
            Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
        await _mockMusicArtworkRepository.DidNotReceive().GetByOwnersAsync(
            Arg.Any<MusicArtworkOwnerType>(), Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>());
        await _mockBookRepository.DidNotReceive().GetByIdsAsync(
            Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
        await _mockMediaContributorRepository.DidNotReceive().GetByIdsAsync(
            Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenCurrentUserIsNull_ShouldReturnNotAuthorizedAndNotReadRecords()
    {
        // Arrange
        GetMostConsumedQuery query = _getMostConsumedQueryFixture.Create();
        _mockCurrentUserService.UserId.Returns((Guid?)null);

        // Act
        Result<PaginatedResponse<ConsumedItemResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockConsumptionStatisticsRepository.DidNotReceive().GetForUserAsync(
            Arg.Any<Guid>(), Arg.Any<Guid?>(), Arg.Any<MediaKind?>(), Arg.Any<DateTime?>(), Arg.Any<DateTime?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenPolicyDeniesTheTarget_ShouldReturnNotAuthorizedAndNotReadRecords()
    {
        // Arrange
        GetMostConsumedQuery query = _getMostConsumedQueryFixture.Create();
        _mockAuthorizationService.EvaluatePolicyAsync<IConsumptionActivityAccessPolicy>(
            Arg.Any<Guid>(), Arg.Any<ConsumptionActivityAccessPolicyContext>(), Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        Result<PaginatedResponse<ConsumedItemResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockConsumptionStatisticsRepository.DidNotReceive().GetForUserAsync(
            Arg.Any<Guid>(), Arg.Any<Guid?>(), Arg.Any<MediaKind?>(), Arg.Any<DateTime?>(), Arg.Any<DateTime?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenTheRecordsCannotBeRead_ShouldReturnTheRepositoryError()
    {
        // Arrange
        GetMostConsumedQuery query = _getMostConsumedQueryFixture.Create();
        Error error = Error.Failure("Persistence.Failure", "The consumption records could not be read.");
        _mockConsumptionStatisticsRepository.GetForUserAsync(
            Arg.Any<Guid>(), Arg.Any<Guid?>(), Arg.Any<MediaKind?>(), Arg.Any<DateTime?>(), Arg.Any<DateTime?>(), Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        Result<PaginatedResponse<ConsumedItemResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
    }

    [Fact]
    public async Task HandleAsync_WhenAnItemHasSeveralSessions_ShouldAggregateThemIntoOneRow()
    {
        // Arrange
        Guid itemId = Guid.NewGuid();
        Guid libraryId = Guid.NewGuid();
        DateTime newerActivity = DateTime.UtcNow.AddHours(-1);
        DateTime olderActivity = DateTime.UtcNow.AddHours(-2);
        StubRecords(
            _mediaConsumptionRecordEntityFixture.Create(
                userId: _userId,
                mediaKind: MediaKind.MusicTrack,
                mediaItemId: itemId,
                libraryId: libraryId,
                status: ConsumptionStatus.Completed,
                consumedDurationMilliseconds: 1_000,
                lastActivityOnUtc: olderActivity,
                consecutivePagesRead: 0),
            _mediaConsumptionRecordEntityFixture.Create(
                userId: _userId,
                mediaKind: MediaKind.MusicTrack,
                mediaItemId: itemId,
                libraryId: libraryId,
                status: ConsumptionStatus.Stopped,
                consumedDurationMilliseconds: 29_999,
                lastActivityOnUtc: newerActivity));
        GetMostConsumedQuery query = _getMostConsumedQueryFixture.Create();

        // Act
        Result<PaginatedResponse<ConsumedItemResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        ConsumedItemResponse item = Assert.Single(result.Value.Data);
        Assert.Equal(itemId, item.MediaItemId);
        Assert.Equal(libraryId, item.LibraryId);
        Assert.Equal(1, item.CountedSessionCount);
        Assert.Equal(1, item.CompletedSessionCount);
        Assert.Equal(30.999, item.ConsumedDurationInSeconds);
        Assert.Equal(newerActivity, item.LastConsumedOnUtc);
    }

    [Fact]
    public async Task HandleAsync_WhenSeveralItemsExist_ShouldOrderThemByCountDurationRecencyItemKindAndLibrary()
    {
        // Arrange
        Guid librarySmall = Guid.Parse("00000000-0000-0000-0000-000000000001");
        Guid libraryLarge = Guid.Parse("00000000-0000-0000-0000-000000000002");
        Guid itemA = Guid.Parse("00000000-0000-0000-0000-000000000101");
        Guid itemB = Guid.Parse("00000000-0000-0000-0000-000000000102");
        Guid itemC = Guid.Parse("00000000-0000-0000-0000-000000000103");
        Guid itemD = Guid.Parse("00000000-0000-0000-0000-000000000104");
        Guid itemE = Guid.Parse("00000000-0000-0000-0000-000000000105");
        Guid itemF = Guid.Parse("00000000-0000-0000-0000-000000000106");
        DateTime t1 = DateTime.UtcNow.AddDays(-6);
        DateTime t2 = DateTime.UtcNow.AddDays(-5);
        DateTime t4 = DateTime.UtcNow.AddDays(-3);
        DateTime t5 = DateTime.UtcNow.AddDays(-2);
        DateTime t6 = DateTime.UtcNow.AddDays(-1);
        StubRecords(
            // The highest counted session count always comes first.
            _mediaConsumptionRecordEntityFixture.Create(
                userId: _userId,
                mediaKind: MediaKind.MusicTrack,
                mediaItemId: itemA,
                libraryId: librarySmall,
                status: ConsumptionStatus.Completed,
                consumedDurationMilliseconds: 30,
                lastActivityOnUtc: t1,
                consecutivePagesRead: 0),
            _mediaConsumptionRecordEntityFixture.Create(
                userId: _userId,
                mediaKind: MediaKind.MusicTrack,
                mediaItemId: itemA,
                libraryId: librarySmall,
                status: ConsumptionStatus.Completed,
                consumedDurationMilliseconds: 30,
                lastActivityOnUtc: t1,
                consecutivePagesRead: 0),
            _mediaConsumptionRecordEntityFixture.Create(
                userId: _userId,
                mediaKind: MediaKind.MusicTrack,
                mediaItemId: itemA,
                libraryId: librarySmall,
                status: ConsumptionStatus.Completed,
                consumedDurationMilliseconds: 40,
                lastActivityOnUtc: t1,
                consecutivePagesRead: 0),
            // A longer duration wins over a shorter one when the counted session count is equal.
            _mediaConsumptionRecordEntityFixture.Create(
                userId: _userId,
                mediaKind: MediaKind.MusicTrack,
                mediaItemId: itemB,
                libraryId: librarySmall,
                status: ConsumptionStatus.Completed,
                consumedDurationMilliseconds: 200,
                lastActivityOnUtc: t2,
                consecutivePagesRead: 0),
            _mediaConsumptionRecordEntityFixture.Create(
                userId: _userId,
                mediaKind: MediaKind.MusicTrack,
                mediaItemId: itemB,
                libraryId: librarySmall,
                status: ConsumptionStatus.Completed,
                consumedDurationMilliseconds: 300,
                lastActivityOnUtc: t2,
                consecutivePagesRead: 0),
            _mediaConsumptionRecordEntityFixture.Create(
                userId: _userId,
                mediaKind: MediaKind.MusicTrack,
                mediaItemId: itemC,
                libraryId: librarySmall,
                status: ConsumptionStatus.Completed,
                consumedDurationMilliseconds: 100,
                lastActivityOnUtc: t6,
                consecutivePagesRead: 0),
            _mediaConsumptionRecordEntityFixture.Create(
                userId: _userId,
                mediaKind: MediaKind.MusicTrack,
                mediaItemId: itemC,
                libraryId: librarySmall,
                status: ConsumptionStatus.Completed,
                consumedDurationMilliseconds: 300,
                lastActivityOnUtc: t6,
                consecutivePagesRead: 0),
            // A more recent session wins over an older one when the count and duration are equal.
            _mediaConsumptionRecordEntityFixture.Create(
                userId: _userId,
                mediaKind: MediaKind.MusicTrack,
                mediaItemId: itemD,
                libraryId: librarySmall,
                status: ConsumptionStatus.Completed,
                consumedDurationMilliseconds: 100,
                lastActivityOnUtc: t5,
                consecutivePagesRead: 0),
            _mediaConsumptionRecordEntityFixture.Create(
                userId: _userId,
                mediaKind: MediaKind.MusicTrack,
                mediaItemId: itemE,
                libraryId: librarySmall,
                status: ConsumptionStatus.Completed,
                consumedDurationMilliseconds: 100,
                lastActivityOnUtc: t4,
                consecutivePagesRead: 0),
            // The lower media item Id wins when everything above is equal.
            _mediaConsumptionRecordEntityFixture.Create(
                userId: _userId,
                mediaKind: MediaKind.MusicTrack,
                mediaItemId: itemF,
                libraryId: librarySmall,
                status: ConsumptionStatus.Completed,
                consumedDurationMilliseconds: 100,
                lastActivityOnUtc: t4,
                consecutivePagesRead: 0),
            // The lower media kind wins when the media item Id is equal.
            _mediaConsumptionRecordEntityFixture.Create(
                userId: _userId,
                mediaKind: MediaKind.Movie,
                mediaItemId: itemF,
                libraryId: librarySmall,
                status: ConsumptionStatus.Completed,
                consumedDurationMilliseconds: 100,
                lastActivityOnUtc: t4,
                consecutivePagesRead: 0),
            // The lower library Id wins when everything else is equal.
            _mediaConsumptionRecordEntityFixture.Create(
                userId: _userId,
                mediaKind: MediaKind.Movie,
                mediaItemId: itemF,
                libraryId: libraryLarge,
                status: ConsumptionStatus.Completed,
                consumedDurationMilliseconds: 100,
                lastActivityOnUtc: t4,
                consecutivePagesRead: 0));
        GetMostConsumedQuery query = _getMostConsumedQueryFixture.Create();

        // Act
        Result<PaginatedResponse<ConsumedItemResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(
            [itemA, itemB, itemC, itemD, itemE, itemF, itemF, itemF],
            result.Value.Data.Select(item => item.MediaItemId));
        Assert.Equal(
            [
                MediaKind.MusicTrack,
                MediaKind.MusicTrack,
                MediaKind.MusicTrack,
                MediaKind.MusicTrack,
                MediaKind.MusicTrack,
                MediaKind.MusicTrack,
                MediaKind.Movie,
                MediaKind.Movie
            ],
            result.Value.Data.Select(item => item.MediaKind));
        Assert.Equal(
            [librarySmall, librarySmall, librarySmall, librarySmall, librarySmall, librarySmall, librarySmall, libraryLarge],
            result.Value.Data.Select(item => item.LibraryId));
        Assert.Equal([3, 2, 2, 1, 1, 1, 1, 1], result.Value.Data.Select(item => item.CountedSessionCount));
    }

    [Fact]
    public async Task HandleAsync_WhenPaginationDataIsNotSupplied_ShouldReturnEveryDistinctItem()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        DateTime activity = DateTime.UtcNow.AddHours(-1);
        StubRecords(
            _mediaConsumptionRecordEntityFixture.Create(
                userId: _userId,
                mediaKind: MediaKind.MusicTrack,
                mediaItemId: Guid.NewGuid(),
                libraryId: libraryId,
                status: ConsumptionStatus.Completed,
                consumedDurationMilliseconds: 1_000,
                lastActivityOnUtc: activity,
                consecutivePagesRead: 0),
            _mediaConsumptionRecordEntityFixture.Create(
                userId: _userId,
                mediaKind: MediaKind.MusicTrack,
                mediaItemId: Guid.NewGuid(),
                libraryId: libraryId,
                status: ConsumptionStatus.Completed,
                consumedDurationMilliseconds: 1_000,
                lastActivityOnUtc: activity,
                consecutivePagesRead: 0),
            _mediaConsumptionRecordEntityFixture.Create(
                userId: _userId,
                mediaKind: MediaKind.MusicTrack,
                mediaItemId: Guid.NewGuid(),
                libraryId: libraryId,
                status: ConsumptionStatus.Completed,
                consumedDurationMilliseconds: 1_000,
                lastActivityOnUtc: activity,
                consecutivePagesRead: 0));
        GetMostConsumedQuery query = _getMostConsumedQueryFixture.Create(includePaginationData: false);

        // Act
        Result<PaginatedResponse<ConsumedItemResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(3, result.Value.Data.Count);
        Assert.Equal(1, result.Value.CurrentPage);
        Assert.Equal(3, result.Value.PerPage);
        Assert.Equal(3, result.Value.Count);
        Assert.Equal(1, result.Value.NumberOfPages);
    }

    [Fact]
    public async Task HandleAsync_WhenThereAreNoItemsAndPaginationDataIsNotSupplied_ShouldReturnAnEmptyPage()
    {
        // Arrange
        StubRecords();
        GetMostConsumedQuery query = _getMostConsumedQueryFixture.Create(includePaginationData: false);

        // Act
        Result<PaginatedResponse<ConsumedItemResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Empty(result.Value.Data);
        Assert.Equal(1, result.Value.CurrentPage);
        Assert.Equal(0, result.Value.PerPage);
        Assert.Equal(0, result.Value.Count);
        Assert.Equal(1, result.Value.NumberOfPages);
    }

    [Fact]
    public async Task HandleAsync_WhenTheFirstPageIsRequested_ShouldReturnTheFirstItems()
    {
        // Arrange
        StubOrderedRecords(5);
        GetMostConsumedQuery query = _getMostConsumedQueryFixture.Create(
            includePaginationData: true,
            paginationData: _paginationDataDtoFixture.Create(currentPage: 1, perPage: 2));

        // Act
        Result<PaginatedResponse<ConsumedItemResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(2, result.Value.Data.Count);
        Assert.Equal(1, result.Value.CurrentPage);
        Assert.Equal(2, result.Value.PerPage);
        Assert.Equal(5, result.Value.Count);
        Assert.Equal(3, result.Value.NumberOfPages);
    }

    [Fact]
    public async Task HandleAsync_WhenASpecificPageIsRequested_ShouldReturnThatPageOfItems()
    {
        // Arrange
        StubOrderedRecords(5);
        List<Guid> expectedItemIds = [.. CreateOrderedItemIds(5).Skip(2).Take(2)];
        GetMostConsumedQuery query = _getMostConsumedQueryFixture.Create(
            includePaginationData: true,
            paginationData: _paginationDataDtoFixture.Create(currentPage: 2, perPage: 2));

        // Act
        Result<PaginatedResponse<ConsumedItemResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(expectedItemIds, result.Value.Data.Select(item => item.MediaItemId));
        Assert.Equal(2, result.Value.CurrentPage);
        Assert.Equal(2, result.Value.PerPage);
        Assert.Equal(5, result.Value.Count);
        Assert.Equal(3, result.Value.NumberOfPages);
    }

    [Fact]
    public async Task HandleAsync_WhenTheRequestedPageIsBeyondTheLastPage_ShouldClampToTheLastPage()
    {
        // Arrange
        StubOrderedRecords(5);
        GetMostConsumedQuery query = _getMostConsumedQueryFixture.Create(
            includePaginationData: true,
            paginationData: _paginationDataDtoFixture.Create(currentPage: 99, perPage: 2));

        // Act
        Result<PaginatedResponse<ConsumedItemResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Single(result.Value.Data);
        Assert.Equal(3, result.Value.CurrentPage);
        Assert.Equal(2, result.Value.PerPage);
        Assert.Equal(5, result.Value.Count);
        Assert.Equal(3, result.Value.NumberOfPages);
    }

    [Fact]
    public async Task HandleAsync_WhenAnAccessibleTrackHasAlbumAndCover_ShouldExposeItsDisplayData()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        Guid trackId = Guid.NewGuid();
        ArtistEntity artist = _artistEntityFixture.Create(
            id: artistId, libraryId: libraryId, name: "Artist One", includeAlbums: false, includeContributors: false);
        AlbumEntity album = _albumEntityFixture.Create(
            id: albumId, artistId: artistId, libraryId: libraryId, title: "Album One", includeTracks: false, includeMetadata: false);
        album.Artist = artist;
        TrackEntity track = _trackEntityFixture.Create(
            id: trackId, albumId: albumId, libraryId: libraryId, title: "Track One", includeMetadata: false);
        track.Album = album;
        StubRecords(_mediaConsumptionRecordEntityFixture.Create(
            userId: _userId,
            mediaKind: MediaKind.MusicTrack,
            mediaItemId: trackId,
            libraryId: libraryId,
            status: ConsumptionStatus.Completed,
            consumedDurationMilliseconds: 60_000,
            lastActivityOnUtc: DateTime.UtcNow,
            consecutivePagesRead: 0));
        StubTracks(track);
        StubArtwork(
            _musicArtworkEntityFixture.Create(
                ownerType: MusicArtworkOwnerType.Album, ownerId: albumId, artworkType: ArtworkType.Cover, ordinal: 2, fileName: "cover-high.jpg"),
            _musicArtworkEntityFixture.Create(
                ownerType: MusicArtworkOwnerType.Album, ownerId: albumId, artworkType: ArtworkType.Cover, ordinal: 0, fileName: "cover-low.jpg"),
            _musicArtworkEntityFixture.Create(
                ownerType: MusicArtworkOwnerType.Album, ownerId: albumId, artworkType: ArtworkType.Back, ordinal: 0, fileName: "back.jpg"));
        GetMostConsumedQuery query = _getMostConsumedQueryFixture.Create();

        // Act
        Result<PaginatedResponse<ConsumedItemResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        ConsumedItemResponse item = Assert.Single(result.Value.Data);
        Assert.True(item.IsAvailable);
        Assert.Equal("Track One", item.Title);
        Assert.Equal("Album One", item.CollectionTitle);
        Assert.Equal("Artist One", item.CreatorName);
        Assert.Equal("cover-low.jpg", item.CoverPath);
        await _mockMusicArtworkRepository.Received(1).GetByOwnersAsync(
            MusicArtworkOwnerType.Album,
            Arg.Is<IReadOnlyCollection<Guid>>(ownerIds => ownerIds.Count == 1 && ownerIds.Contains(albumId)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenAnAccessibleTrackNoLongerExists_ShouldReturnBlankDisplayData()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid trackId = Guid.NewGuid();
        StubRecords(_mediaConsumptionRecordEntityFixture.Create(
            userId: _userId,
            mediaKind: MediaKind.MusicTrack,
            mediaItemId: trackId,
            libraryId: libraryId,
            status: ConsumptionStatus.Completed,
            consumedDurationMilliseconds: 60_000,
            lastActivityOnUtc: DateTime.UtcNow,
            consecutivePagesRead: 0));
        StubTracks();
        GetMostConsumedQuery query = _getMostConsumedQueryFixture.Create();

        // Act
        Result<PaginatedResponse<ConsumedItemResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        ConsumedItemResponse item = Assert.Single(result.Value.Data);
        Assert.False(item.IsAvailable);
        Assert.Equal(string.Empty, item.Title);
        Assert.Equal(string.Empty, item.CollectionTitle);
        Assert.Equal(string.Empty, item.CreatorName);
        Assert.Null(item.CoverPath);
        Assert.Equal(1, item.CountedSessionCount);
    }

    [Fact]
    public async Task HandleAsync_WhenATrackBelongsToAnotherLibrary_ShouldReturnBlankDisplayData()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid trackId = Guid.NewGuid();
        TrackEntity track = _trackEntityFixture.Create(
            id: trackId, libraryId: Guid.NewGuid(), title: "Track Elsewhere", includeMetadata: false);
        StubRecords(_mediaConsumptionRecordEntityFixture.Create(
            userId: _userId,
            mediaKind: MediaKind.MusicTrack,
            mediaItemId: trackId,
            libraryId: libraryId,
            status: ConsumptionStatus.Completed,
            consumedDurationMilliseconds: 60_000,
            lastActivityOnUtc: DateTime.UtcNow,
            consecutivePagesRead: 0));
        StubTracks(track);
        GetMostConsumedQuery query = _getMostConsumedQueryFixture.Create();

        // Act
        Result<PaginatedResponse<ConsumedItemResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        ConsumedItemResponse item = Assert.Single(result.Value.Data);
        Assert.False(item.IsAvailable);
        Assert.Equal(string.Empty, item.Title);
    }

    [Fact]
    public async Task HandleAsync_WhenTheLibraryOfATrackIsNotAccessible_ShouldReturnBlankDisplayDataWithoutLoadingTheTrack()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid trackId = Guid.NewGuid();
        StubRecords(_mediaConsumptionRecordEntityFixture.Create(
            userId: _userId,
            mediaKind: MediaKind.MusicTrack,
            mediaItemId: trackId,
            libraryId: libraryId,
            status: ConsumptionStatus.Completed,
            consumedDurationMilliseconds: 60_000,
            lastActivityOnUtc: DateTime.UtcNow,
            consecutivePagesRead: 0));
        _mockAuthorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(
            _userId, Arg.Is<LibraryOwnershipPolicyContext>(context => context.LibraryId == libraryId), Arg.Any<CancellationToken>())
            .Returns(false);
        GetMostConsumedQuery query = _getMostConsumedQueryFixture.Create();

        // Act
        Result<PaginatedResponse<ConsumedItemResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        ConsumedItemResponse item = Assert.Single(result.Value.Data);
        Assert.False(item.IsAvailable);
        Assert.Equal(string.Empty, item.Title);
        await _mockTrackRepository.DidNotReceive().GetByIdsAsync(
            Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenAnAccessibleBookHasAuthorAndCover_ShouldExposeItsDisplayData()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid bookId = Guid.NewGuid();
        Guid authorId = Guid.NewGuid();
        BookEntity book = _bookEntityFixture.Create(id: bookId, libraryId: libraryId, title: "Book One", includeMetadata: false);
        book.Contributors =
        [
            _bookContributorEntityFixture.Create(bookId: bookId, mediaContributorId: authorId, role: MediaContributorRole.Author)
        ];
        book.Artwork =
        [
            _bookArtworkEntityFixture.Create(bookId: bookId, artworkType: ArtworkType.Cover, fileName: "book-cover.jpg")
        ];
        MediaContributorEntity author = _mediaContributorEntityFixture.Create(id: authorId, displayName: "Author One");
        StubRecords(_mediaConsumptionRecordEntityFixture.Create(
            userId: _userId,
            mediaKind: MediaKind.Book,
            mediaItemId: bookId,
            libraryId: libraryId,
            status: ConsumptionStatus.Completed,
            consumedDurationMilliseconds: 60_000,
            lastActivityOnUtc: DateTime.UtcNow,
            consecutivePagesRead: 0));
        StubBooks(book);
        StubAuthors(author);
        GetMostConsumedQuery query = _getMostConsumedQueryFixture.Create();

        // Act
        Result<PaginatedResponse<ConsumedItemResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        ConsumedItemResponse item = Assert.Single(result.Value.Data);
        Assert.True(item.IsAvailable);
        Assert.Equal("Book One", item.Title);
        Assert.Equal(string.Empty, item.CollectionTitle);
        Assert.Equal("Author One", item.CreatorName);
        Assert.Equal("book-cover.jpg", item.CoverPath);
    }

    [Fact]
    public async Task HandleAsync_WhenAnAccessibleBookNoLongerExists_ShouldReturnBlankDisplayData()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid bookId = Guid.NewGuid();
        StubRecords(_mediaConsumptionRecordEntityFixture.Create(
            userId: _userId,
            mediaKind: MediaKind.Book,
            mediaItemId: bookId,
            libraryId: libraryId,
            status: ConsumptionStatus.Completed,
            consumedDurationMilliseconds: 60_000,
            lastActivityOnUtc: DateTime.UtcNow,
            consecutivePagesRead: 0));
        StubBooks();
        GetMostConsumedQuery query = _getMostConsumedQueryFixture.Create();

        // Act
        Result<PaginatedResponse<ConsumedItemResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        ConsumedItemResponse item = Assert.Single(result.Value.Data);
        Assert.False(item.IsAvailable);
        Assert.Equal(string.Empty, item.Title);
        Assert.Equal(string.Empty, item.CreatorName);
        Assert.Null(item.CoverPath);
    }

    [Fact]
    public async Task HandleAsync_WhenABookBelongsToAnotherLibrary_ShouldReturnBlankDisplayData()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid bookId = Guid.NewGuid();
        BookEntity book = _bookEntityFixture.Create(id: bookId, libraryId: Guid.NewGuid(), title: "Book Elsewhere", includeMetadata: false);
        StubRecords(_mediaConsumptionRecordEntityFixture.Create(
            userId: _userId,
            mediaKind: MediaKind.Book,
            mediaItemId: bookId,
            libraryId: libraryId,
            status: ConsumptionStatus.Completed,
            consumedDurationMilliseconds: 60_000,
            lastActivityOnUtc: DateTime.UtcNow,
            consecutivePagesRead: 0));
        StubBooks(book);
        GetMostConsumedQuery query = _getMostConsumedQueryFixture.Create();

        // Act
        Result<PaginatedResponse<ConsumedItemResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        ConsumedItemResponse item = Assert.Single(result.Value.Data);
        Assert.False(item.IsAvailable);
        Assert.Equal(string.Empty, item.Title);
    }

    [Fact]
    public async Task HandleAsync_WhenTheAuthorOfABookNoLongerExists_ShouldReturnAnEmptyCreatorName()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid bookId = Guid.NewGuid();
        Guid authorId = Guid.NewGuid();
        BookEntity book = _bookEntityFixture.Create(id: bookId, libraryId: libraryId, title: "Book One", includeMetadata: false);
        book.Contributors =
        [
            _bookContributorEntityFixture.Create(bookId: bookId, mediaContributorId: authorId, role: MediaContributorRole.Author)
        ];
        StubRecords(_mediaConsumptionRecordEntityFixture.Create(
            userId: _userId,
            mediaKind: MediaKind.Book,
            mediaItemId: bookId,
            libraryId: libraryId,
            status: ConsumptionStatus.Completed,
            consumedDurationMilliseconds: 60_000,
            lastActivityOnUtc: DateTime.UtcNow,
            consecutivePagesRead: 0));
        StubBooks(book);
        StubAuthors();
        GetMostConsumedQuery query = _getMostConsumedQueryFixture.Create();

        // Act
        Result<PaginatedResponse<ConsumedItemResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        ConsumedItemResponse item = Assert.Single(result.Value.Data);
        Assert.True(item.IsAvailable);
        Assert.Equal("Book One", item.Title);
        Assert.Equal(string.Empty, item.CreatorName);
    }

    [Fact]
    public async Task HandleAsync_WhenABookHasSeveralAuthors_ShouldUseTheAlphabeticallyFirstAuthorName()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid bookId = Guid.NewGuid();
        Guid firstAuthorId = Guid.NewGuid();
        Guid secondAuthorId = Guid.NewGuid();
        BookEntity book = _bookEntityFixture.Create(id: bookId, libraryId: libraryId, title: "Book One", includeMetadata: false);
        book.Contributors =
        [
            _bookContributorEntityFixture.Create(bookId: bookId, mediaContributorId: secondAuthorId, role: MediaContributorRole.Author),
            _bookContributorEntityFixture.Create(bookId: bookId, mediaContributorId: firstAuthorId, role: MediaContributorRole.Author)
        ];
        StubRecords(_mediaConsumptionRecordEntityFixture.Create(
            userId: _userId,
            mediaKind: MediaKind.Book,
            mediaItemId: bookId,
            libraryId: libraryId,
            status: ConsumptionStatus.Completed,
            consumedDurationMilliseconds: 60_000,
            lastActivityOnUtc: DateTime.UtcNow,
            consecutivePagesRead: 0));
        StubBooks(book);
        StubAuthors(
            _mediaContributorEntityFixture.Create(id: secondAuthorId, displayName: "Zoe Author"),
            _mediaContributorEntityFixture.Create(id: firstAuthorId, displayName: "Alice Author"));
        GetMostConsumedQuery query = _getMostConsumedQueryFixture.Create();

        // Act
        Result<PaginatedResponse<ConsumedItemResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        ConsumedItemResponse item = Assert.Single(result.Value.Data);
        Assert.Equal("Alice Author", item.CreatorName);
    }

    [Fact]
    public async Task HandleAsync_WhenTheTracksCannotBeRead_ShouldReturnTheRepositoryError()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid trackId = Guid.NewGuid();
        Error error = Error.Failure("Persistence.Failure", "The tracks could not be read.");
        StubRecords(_mediaConsumptionRecordEntityFixture.Create(
            userId: _userId,
            mediaKind: MediaKind.MusicTrack,
            mediaItemId: trackId,
            libraryId: libraryId,
            status: ConsumptionStatus.Completed,
            consumedDurationMilliseconds: 60_000,
            lastActivityOnUtc: DateTime.UtcNow,
            consecutivePagesRead: 0));
        _mockTrackRepository.GetByIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(error);
        GetMostConsumedQuery query = _getMostConsumedQueryFixture.Create();

        // Act
        Result<PaginatedResponse<ConsumedItemResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
    }

    [Fact]
    public async Task HandleAsync_WhenTheArtworkCannotBeRead_ShouldReturnTheRepositoryError()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid trackId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        Error error = Error.Failure("Persistence.Failure", "The artwork could not be read.");
        TrackEntity track = _trackEntityFixture.Create(
            id: trackId, albumId: albumId, libraryId: libraryId, includeMetadata: false);
        StubRecords(_mediaConsumptionRecordEntityFixture.Create(
            userId: _userId,
            mediaKind: MediaKind.MusicTrack,
            mediaItemId: trackId,
            libraryId: libraryId,
            status: ConsumptionStatus.Completed,
            consumedDurationMilliseconds: 60_000,
            lastActivityOnUtc: DateTime.UtcNow,
            consecutivePagesRead: 0));
        StubTracks(track);
        _mockMusicArtworkRepository.GetByOwnersAsync(
            Arg.Any<MusicArtworkOwnerType>(), Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(error);
        GetMostConsumedQuery query = _getMostConsumedQueryFixture.Create();

        // Act
        Result<PaginatedResponse<ConsumedItemResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
    }

    [Fact]
    public async Task HandleAsync_WhenTheBooksCannotBeRead_ShouldReturnTheRepositoryError()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid bookId = Guid.NewGuid();
        Error error = Error.Failure("Persistence.Failure", "The books could not be read.");
        StubRecords(_mediaConsumptionRecordEntityFixture.Create(
            userId: _userId,
            mediaKind: MediaKind.Book,
            mediaItemId: bookId,
            libraryId: libraryId,
            status: ConsumptionStatus.Completed,
            consumedDurationMilliseconds: 60_000,
            lastActivityOnUtc: DateTime.UtcNow,
            consecutivePagesRead: 0));
        _mockBookRepository.GetByIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(error);
        GetMostConsumedQuery query = _getMostConsumedQueryFixture.Create();

        // Act
        Result<PaginatedResponse<ConsumedItemResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
    }

    [Fact]
    public async Task HandleAsync_WhenTheAuthorsCannotBeRead_ShouldReturnTheRepositoryError()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid bookId = Guid.NewGuid();
        Guid authorId = Guid.NewGuid();
        Error error = Error.Failure("Persistence.Failure", "The authors could not be read.");
        BookEntity book = _bookEntityFixture.Create(id: bookId, libraryId: libraryId, includeMetadata: false);
        book.Contributors =
        [
            _bookContributorEntityFixture.Create(bookId: bookId, mediaContributorId: authorId, role: MediaContributorRole.Author)
        ];
        StubRecords(_mediaConsumptionRecordEntityFixture.Create(
            userId: _userId,
            mediaKind: MediaKind.Book,
            mediaItemId: bookId,
            libraryId: libraryId,
            status: ConsumptionStatus.Completed,
            consumedDurationMilliseconds: 60_000,
            lastActivityOnUtc: DateTime.UtcNow,
            consecutivePagesRead: 0));
        StubBooks(book);
        _mockMediaContributorRepository.GetByIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(error);
        GetMostConsumedQuery query = _getMostConsumedQueryFixture.Create();

        // Act
        Result<PaginatedResponse<ConsumedItemResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
    }

    [Fact]
    public async Task HandleAsync_WhenItemsExist_ShouldReadTheDistinctIdsOfAccessibleItemsWithNavigationProperties()
    {
        // Arrange
        Guid accessibleLibraryId = Guid.NewGuid();
        Guid inaccessibleLibraryId = Guid.NewGuid();
        Guid trackId = Guid.NewGuid();
        Guid bookId = Guid.NewGuid();
        DateTime activity = DateTime.UtcNow.AddHours(-1);
        StubRecords(
            _mediaConsumptionRecordEntityFixture.Create(
                userId: _userId,
                mediaKind: MediaKind.MusicTrack,
                mediaItemId: trackId,
                libraryId: accessibleLibraryId,
                status: ConsumptionStatus.Completed,
                consumedDurationMilliseconds: 1_000,
                lastActivityOnUtc: activity,
                consecutivePagesRead: 0),
            _mediaConsumptionRecordEntityFixture.Create(
                userId: _userId,
                mediaKind: MediaKind.MusicTrack,
                mediaItemId: trackId,
                libraryId: accessibleLibraryId,
                status: ConsumptionStatus.Completed,
                consumedDurationMilliseconds: 1_000,
                lastActivityOnUtc: activity,
                consecutivePagesRead: 0),
            _mediaConsumptionRecordEntityFixture.Create(
                userId: _userId,
                mediaKind: MediaKind.MusicTrack,
                mediaItemId: Guid.NewGuid(),
                libraryId: inaccessibleLibraryId,
                status: ConsumptionStatus.Completed,
                consumedDurationMilliseconds: 1_000,
                lastActivityOnUtc: activity,
                consecutivePagesRead: 0),
            _mediaConsumptionRecordEntityFixture.Create(
                userId: _userId,
                mediaKind: MediaKind.Book,
                mediaItemId: bookId,
                libraryId: accessibleLibraryId,
                status: ConsumptionStatus.Completed,
                consumedDurationMilliseconds: 1_000,
                lastActivityOnUtc: activity,
                consecutivePagesRead: 0));
        _mockAuthorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(
            _userId, Arg.Is<LibraryOwnershipPolicyContext>(context => context.LibraryId == inaccessibleLibraryId), Arg.Any<CancellationToken>())
            .Returns(false);
        StubTracks();
        StubBooks();
        GetMostConsumedQuery query = _getMostConsumedQueryFixture.Create();

        // Act
        Result<PaginatedResponse<ConsumedItemResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        await _mockTrackRepository.Received(1).GetByIdsAsync(
            Arg.Is<IReadOnlyCollection<Guid>>(ids => ids.Count == 1 && ids.Contains(trackId)),
            true,
            Arg.Any<CancellationToken>());
        await _mockBookRepository.Received(1).GetByIdsAsync(
            Arg.Is<IReadOnlyCollection<Guid>>(ids => ids.Count == 1 && ids.Contains(bookId)),
            true,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Stubs the consumption statistics repository so that it returns the provided records.
    /// </summary>
    /// <param name="records">The records returned by the repository.</param>
    private void StubRecords(params MediaConsumptionRecordEntity[] records)
    {
        _mockConsumptionStatisticsRepository.GetForUserAsync(
            Arg.Any<Guid>(), Arg.Any<Guid?>(), Arg.Any<MediaKind?>(), Arg.Any<DateTime?>(), Arg.Any<DateTime?>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<MediaConsumptionRecordEntity>>(records));
    }

    /// <summary>
    /// Stubs the track repository so that it returns the provided tracks.
    /// </summary>
    /// <param name="tracks">The tracks returned by the repository.</param>
    private void StubTracks(params TrackEntity[] tracks)
    {
        _mockTrackRepository.GetByIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<TrackEntity>>(tracks));
    }

    /// <summary>
    /// Stubs the artwork repository so that it returns the provided pieces of artwork.
    /// </summary>
    /// <param name="artwork">The artwork returned by the repository.</param>
    private void StubArtwork(params MusicArtworkEntity[] artwork)
    {
        _mockMusicArtworkRepository.GetByOwnersAsync(
            Arg.Any<MusicArtworkOwnerType>(), Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<MusicArtworkEntity>>(artwork));
    }

    /// <summary>
    /// Stubs the book repository so that it returns the provided books.
    /// </summary>
    /// <param name="books">The books returned by the repository.</param>
    private void StubBooks(params BookEntity[] books)
    {
        _mockBookRepository.GetByIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<BookEntity>>(books));
    }

    /// <summary>
    /// Stubs the media contributor repository so that it returns the provided contributors.
    /// </summary>
    /// <param name="authors">The contributors returned by the repository.</param>
    private void StubAuthors(params MediaContributorEntity[] authors)
    {
        _mockMediaContributorRepository.GetByIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<MediaContributorEntity>>(authors));
    }

    /// <summary>
    /// Stubs the consumption statistics repository with the requested number of distinct completed music tracks of the current user.
    /// </summary>
    /// <param name="count">The number of distinct media items to create.</param>
    private void StubOrderedRecords(int count)
    {
        DateTime activity = DateTime.UtcNow.AddHours(-1);
        List<MediaConsumptionRecordEntity> records =
        [
            .. CreateOrderedItemIds(count).Select(itemId => _mediaConsumptionRecordEntityFixture.Create(
                userId: _userId,
                mediaKind: MediaKind.MusicTrack,
                mediaItemId: itemId,
                libraryId: Guid.NewGuid(),
                status: ConsumptionStatus.Completed,
                consumedDurationMilliseconds: 1_000,
                lastActivityOnUtc: activity,
                consecutivePagesRead: 0))
        ];
        StubRecords([.. records]);
    }

    /// <summary>
    /// Creates the Ids of the media items used by the pagination tests, in ascending order.
    /// </summary>
    /// <param name="count">The number of Ids to create.</param>
    /// <returns>The ascending media item Ids.</returns>
    private static List<Guid> CreateOrderedItemIds(int count)
    {
        return [.. Enumerable.Range(1, count).Select(index => Guid.Parse($"00000000-0000-0000-0000-{index:D12}"))];
    }
}
