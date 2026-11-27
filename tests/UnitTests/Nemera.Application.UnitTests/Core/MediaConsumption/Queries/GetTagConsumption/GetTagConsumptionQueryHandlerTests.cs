#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaConsumption;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Common.DataAccess.Repositories.BookLibrary;
using Lumina.Application.Common.DataAccess.Repositories.MediaConsumption;
using Lumina.Application.Common.DataAccess.Repositories.MusicLibrary;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Infrastructure.Authorization.Policies.ConsumptionActivity;
using Lumina.Application.Common.Infrastructure.Authorization.Policies.LibraryOwnership;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Core.MediaConsumption.Queries.GetTagConsumption;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Common;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaConsumption;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Fixtures.Core.MediaConsumption.Queries.GetTagConsumption;
using Lumina.Contracts.Responses.MediaConsumption;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.MediaConsumption;
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

namespace Lumina.Application.UnitTests.Core.MediaConsumption.Queries.GetTagConsumption;

/// <summary>
/// Contains unit tests for the <see cref="GetTagConsumptionQueryHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetTagConsumptionQueryHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly IConsumptionStatisticsRepository _mockConsumptionStatisticsRepository;
    private readonly ITrackRepository _mockTrackRepository;
    private readonly IBookRepository _mockBookRepository;
    private readonly ICurrentUserService _mockCurrentUserService;
    private readonly IAuthorizationService _mockAuthorizationService;
    private readonly IValidator<GetTagConsumptionQuery> _mockValidator;
    private readonly GetTagConsumptionQueryHandler _sut;
    private readonly GetTagConsumptionQueryFixture _getTagConsumptionQueryFixture = new();
    private readonly MediaConsumptionRecordEntityFixture _mediaConsumptionRecordEntityFixture = new();
    private readonly TrackEntityFixture _trackEntityFixture = new();
    private readonly AlbumEntityFixture _albumEntityFixture = new();
    private readonly ArtistEntityFixture _artistEntityFixture = new();
    private readonly BookEntityFixture _bookEntityFixture = new();
    private readonly TagEntityFixture _tagEntityFixture = new();
    private readonly GenreEntityFixture _genreEntityFixture = new();
    private readonly Guid _userId = Guid.NewGuid();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetTagConsumptionQueryHandlerTests"/> class.
    /// </summary>
    public GetTagConsumptionQueryHandlerTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockConsumptionStatisticsRepository = Substitute.For<IConsumptionStatisticsRepository>();
        _mockTrackRepository = Substitute.For<ITrackRepository>();
        _mockBookRepository = Substitute.For<IBookRepository>();
        _mockCurrentUserService = Substitute.For<ICurrentUserService>();
        _mockAuthorizationService = Substitute.For<IAuthorizationService>();

        _mockUnitOfWork.ConsumptionStatisticsRepository.Returns(_mockConsumptionStatisticsRepository);
        _mockUnitOfWork.TrackRepository.Returns(_mockTrackRepository);
        _mockUnitOfWork.BookRepository.Returns(_mockBookRepository);
        _mockCurrentUserService.UserId.Returns(_userId);
        // Default stubs: the current user can read both the consumption activity and every library, and no item exists.
        _mockAuthorizationService.EvaluatePolicyAsync<IConsumptionActivityAccessPolicy>(
            Arg.Any<Guid>(), Arg.Any<ConsumptionActivityAccessPolicyContext>(), Arg.Any<CancellationToken>())
            .Returns(true);
        _mockAuthorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(
            Arg.Any<Guid>(), Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>())
            .Returns(true);
        StubRecords();
        StubTracks();
        StubBooks();

        _mockValidator = Substitute.For<IValidator<GetTagConsumptionQuery>>();
        _mockValidator.Validate(Arg.Any<GetTagConsumptionQuery>()).Returns([]);

        _sut = new GetTagConsumptionQueryHandler(_mockUnitOfWork, _mockCurrentUserService, _mockAuthorizationService, _mockValidator);
    }

    [Fact]
    public async Task HandleAsync_WhenValidatorReturnsError_ShouldReturnValidationErrorAndNotReadRecords()
    {
        // Arrange
        GetTagConsumptionQuery query = _getTagConsumptionQueryFixture.Create();
        _mockValidator.Validate(Arg.Any<GetTagConsumptionQuery>())
            .Returns([DomainErrors.MediaConsumption.TargetUserIdIsInvalid]);

        // Act
        Result<IReadOnlyList<TagConsumptionResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.MediaConsumption.TargetUserIdIsInvalid, result.FirstError);
        await _mockAuthorizationService.DidNotReceive().EvaluatePolicyAsync<IConsumptionActivityAccessPolicy>(
            Arg.Any<Guid>(), Arg.Any<ConsumptionActivityAccessPolicyContext>(), Arg.Any<CancellationToken>());
        await _mockConsumptionStatisticsRepository.DidNotReceive().GetForUserAsync(
            Arg.Any<Guid>(), Arg.Any<Guid?>(), Arg.Any<MediaKind?>(), Arg.Any<DateTime?>(), Arg.Any<DateTime?>(), Arg.Any<CancellationToken>());
        await _mockTrackRepository.DidNotReceive().GetByIdsAsync(
            Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
        await _mockBookRepository.DidNotReceive().GetByIdsAsync(
            Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenCurrentUserIsNull_ShouldReturnNotAuthorizedAndNotReadRecords()
    {
        // Arrange
        GetTagConsumptionQuery query = _getTagConsumptionQueryFixture.Create();
        _mockCurrentUserService.UserId.Returns((Guid?)null);

        // Act
        Result<IReadOnlyList<TagConsumptionResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

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
        GetTagConsumptionQuery query = _getTagConsumptionQueryFixture.Create();
        _mockAuthorizationService.EvaluatePolicyAsync<IConsumptionActivityAccessPolicy>(
            Arg.Any<Guid>(), Arg.Any<ConsumptionActivityAccessPolicyContext>(), Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        Result<IReadOnlyList<TagConsumptionResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

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
        GetTagConsumptionQuery query = _getTagConsumptionQueryFixture.Create();
        Error error = Error.Failure("Persistence.Failure", "The consumption records could not be read.");
        _mockConsumptionStatisticsRepository.GetForUserAsync(
            Arg.Any<Guid>(), Arg.Any<Guid?>(), Arg.Any<MediaKind?>(), Arg.Any<DateTime?>(), Arg.Any<DateTime?>(), Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        Result<IReadOnlyList<TagConsumptionResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
        await _mockTrackRepository.DidNotReceive().GetByIdsAsync(
            Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
        await _mockBookRepository.DidNotReceive().GetByIdsAsync(
            Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenThereAreNoRecords_ShouldReturnAnEmptyList()
    {
        // Arrange
        GetTagConsumptionQuery query = _getTagConsumptionQueryFixture.Create();

        // Act
        Result<IReadOnlyList<TagConsumptionResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task HandleAsync_WhenATrackHasTagsWithDifferentCasingAndDuplicates_ShouldCountEachNameOnce()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid trackId = Guid.NewGuid();
        TrackEntity track = _trackEntityFixture.Create(id: trackId, libraryId: libraryId, includeMetadata: false);
        track.Tags =
        [
            _tagEntityFixture.Create(name: "rock"),
            _tagEntityFixture.Create(name: "ROCK"),
            _tagEntityFixture.Create(name: " Rock "),
            _tagEntityFixture.Create(name: "jazz")
        ];
        StubRecords(_mediaConsumptionRecordEntityFixture.Create(
            userId: _userId,
            mediaKind: MediaKind.MusicTrack,
            mediaItemId: trackId,
            libraryId: libraryId,
            status: ConsumptionStatus.Completed,
            consumedDurationMilliseconds: 1_000,
            consecutivePagesRead: 0));
        StubTracks(track);
        GetTagConsumptionQuery query = _getTagConsumptionQueryFixture.Create();

        // Act
        Result<IReadOnlyList<TagConsumptionResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(2, result.Value.Count);
        Assert.Equal("jazz", result.Value[0].Name);
        Assert.Equal(1, result.Value[0].ConsumedItemCount);
        Assert.Equal(1.0, result.Value[0].ConsumedDurationInSeconds);
        Assert.Equal("rock", result.Value[1].Name);
        Assert.Equal(1, result.Value[1].ConsumedItemCount);
        Assert.Equal(1.0, result.Value[1].ConsumedDurationInSeconds);
    }

    [Fact]
    public async Task HandleAsync_WhenTwoItemsCarryTheSameTag_ShouldCountBothAndSumTheirDurations()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid firstTrackId = Guid.NewGuid();
        Guid secondTrackId = Guid.NewGuid();
        TrackEntity firstTrack = _trackEntityFixture.Create(id: firstTrackId, libraryId: libraryId, includeMetadata: false);
        firstTrack.Tags = [_tagEntityFixture.Create(name: "shared")];
        TrackEntity secondTrack = _trackEntityFixture.Create(id: secondTrackId, libraryId: libraryId, includeMetadata: false);
        secondTrack.Tags = [_tagEntityFixture.Create(name: "Shared")];
        StubRecords(
            _mediaConsumptionRecordEntityFixture.Create(
                userId: _userId,
                mediaKind: MediaKind.MusicTrack,
                mediaItemId: firstTrackId,
                libraryId: libraryId,
                status: ConsumptionStatus.Completed,
                consumedDurationMilliseconds: 1_000,
                consecutivePagesRead: 0),
            _mediaConsumptionRecordEntityFixture.Create(
                userId: _userId,
                mediaKind: MediaKind.MusicTrack,
                mediaItemId: secondTrackId,
                libraryId: libraryId,
                status: ConsumptionStatus.Completed,
                consumedDurationMilliseconds: 2_000,
                consecutivePagesRead: 0));
        StubTracks(firstTrack, secondTrack);
        GetTagConsumptionQuery query = _getTagConsumptionQueryFixture.Create();

        // Act
        Result<IReadOnlyList<TagConsumptionResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        TagConsumptionResponse tag = Assert.Single(result.Value);
        Assert.Equal("shared", tag.Name);
        Assert.Equal(2, tag.ConsumedItemCount);
        Assert.Equal(3.0, tag.ConsumedDurationInSeconds);
    }

    [Fact]
    public async Task HandleAsync_WhenAnItemHasSeveralSessions_ShouldCountItsTagsOnceAndSumItsSessions()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid trackId = Guid.NewGuid();
        TrackEntity track = _trackEntityFixture.Create(id: trackId, libraryId: libraryId, includeMetadata: false);
        track.Tags = [_tagEntityFixture.Create(name: "rock")];
        StubRecords(
            _mediaConsumptionRecordEntityFixture.Create(
                userId: _userId,
                mediaKind: MediaKind.MusicTrack,
                mediaItemId: trackId,
                libraryId: libraryId,
                status: ConsumptionStatus.Completed,
                consumedDurationMilliseconds: 1_000,
                consecutivePagesRead: 0),
            _mediaConsumptionRecordEntityFixture.Create(
                userId: _userId,
                mediaKind: MediaKind.MusicTrack,
                mediaItemId: trackId,
                libraryId: libraryId,
                status: ConsumptionStatus.Completed,
                consumedDurationMilliseconds: 2_000,
                consecutivePagesRead: 0));
        StubTracks(track);
        GetTagConsumptionQuery query = _getTagConsumptionQueryFixture.Create();

        // Act
        Result<IReadOnlyList<TagConsumptionResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        TagConsumptionResponse tag = Assert.Single(result.Value);
        Assert.Equal("rock", tag.Name);
        Assert.Equal(1, tag.ConsumedItemCount);
        Assert.Equal(3.0, tag.ConsumedDurationInSeconds);
    }

    [Fact]
    public async Task HandleAsync_WhenATrackHasTags_ShouldUseTheTrackTagsInsteadOfTheAlbumAndArtistTags()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid trackId = Guid.NewGuid();
        ArtistEntity artist = _artistEntityFixture.Create(libraryId: libraryId, includeAlbums: false, includeContributors: false);
        artist.Tags = [_tagEntityFixture.Create(name: "artisttag")];
        AlbumEntity album = _albumEntityFixture.Create(libraryId: libraryId, includeTracks: false, includeMetadata: false);
        album.Artist = artist;
        album.Tags = [_tagEntityFixture.Create(name: "albumtag")];
        TrackEntity track = _trackEntityFixture.Create(id: trackId, albumId: album.Id, libraryId: libraryId, includeMetadata: false);
        track.Album = album;
        track.Tags = [_tagEntityFixture.Create(name: "tracktag")];
        StubRecords(_mediaConsumptionRecordEntityFixture.Create(
            userId: _userId,
            mediaKind: MediaKind.MusicTrack,
            mediaItemId: trackId,
            libraryId: libraryId,
            status: ConsumptionStatus.Completed,
            consumedDurationMilliseconds: 1_000,
            consecutivePagesRead: 0));
        StubTracks(track);
        GetTagConsumptionQuery query = _getTagConsumptionQueryFixture.Create();

        // Act
        Result<IReadOnlyList<TagConsumptionResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        TagConsumptionResponse tag = Assert.Single(result.Value);
        Assert.Equal("tracktag", tag.Name);
    }

    [Fact]
    public async Task HandleAsync_WhenATrackHasNoTagsOfItsOwn_ShouldFallBackToItsAlbumTags()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid trackId = Guid.NewGuid();
        AlbumEntity album = _albumEntityFixture.Create(libraryId: libraryId, includeTracks: false, includeMetadata: false);
        album.Tags = [_tagEntityFixture.Create(name: "albumtag")];
        TrackEntity track = _trackEntityFixture.Create(id: trackId, albumId: album.Id, libraryId: libraryId, includeMetadata: false);
        track.Album = album;
        StubRecords(_mediaConsumptionRecordEntityFixture.Create(
            userId: _userId,
            mediaKind: MediaKind.MusicTrack,
            mediaItemId: trackId,
            libraryId: libraryId,
            status: ConsumptionStatus.Completed,
            consumedDurationMilliseconds: 1_000,
            consecutivePagesRead: 0));
        StubTracks(track);
        GetTagConsumptionQuery query = _getTagConsumptionQueryFixture.Create();

        // Act
        Result<IReadOnlyList<TagConsumptionResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        TagConsumptionResponse tag = Assert.Single(result.Value);
        Assert.Equal("albumtag", tag.Name);
    }

    [Fact]
    public async Task HandleAsync_WhenATrackAndItsAlbumHaveNoTags_ShouldFallBackToItsArtistTags()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid trackId = Guid.NewGuid();
        ArtistEntity artist = _artistEntityFixture.Create(libraryId: libraryId, includeAlbums: false, includeContributors: false);
        artist.Tags = [_tagEntityFixture.Create(name: "artisttag")];
        AlbumEntity album = _albumEntityFixture.Create(libraryId: libraryId, includeTracks: false, includeMetadata: false);
        album.Artist = artist;
        TrackEntity track = _trackEntityFixture.Create(id: trackId, albumId: album.Id, libraryId: libraryId, includeMetadata: false);
        track.Album = album;
        StubRecords(_mediaConsumptionRecordEntityFixture.Create(
            userId: _userId,
            mediaKind: MediaKind.MusicTrack,
            mediaItemId: trackId,
            libraryId: libraryId,
            status: ConsumptionStatus.Completed,
            consumedDurationMilliseconds: 1_000,
            consecutivePagesRead: 0));
        StubTracks(track);
        GetTagConsumptionQuery query = _getTagConsumptionQueryFixture.Create();

        // Act
        Result<IReadOnlyList<TagConsumptionResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        TagConsumptionResponse tag = Assert.Single(result.Value);
        Assert.Equal("artisttag", tag.Name);
    }

    [Fact]
    public async Task HandleAsync_WhenAnItemOnlyHasGenres_ShouldNotContributeThem()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid trackId = Guid.NewGuid();
        Guid bookId = Guid.NewGuid();
        TrackEntity track = _trackEntityFixture.Create(id: trackId, libraryId: libraryId, includeMetadata: false);
        track.Genres = [_genreEntityFixture.Create(name: "rock")];
        BookEntity book = _bookEntityFixture.Create(id: bookId, libraryId: libraryId, includeMetadata: false);
        book.Genres = [_genreEntityFixture.Create(name: "fantasy")];
        StubRecords(
            _mediaConsumptionRecordEntityFixture.Create(
                userId: _userId,
                mediaKind: MediaKind.MusicTrack,
                mediaItemId: trackId,
                libraryId: libraryId,
                status: ConsumptionStatus.Completed,
                consumedDurationMilliseconds: 1_000,
                consecutivePagesRead: 0),
            _mediaConsumptionRecordEntityFixture.Create(
                userId: _userId,
                mediaKind: MediaKind.Book,
                mediaItemId: bookId,
                libraryId: libraryId,
                status: ConsumptionStatus.Completed,
                consumedDurationMilliseconds: 1_000,
                consecutivePagesRead: 0));
        StubTracks(track);
        StubBooks(book);
        GetTagConsumptionQuery query = _getTagConsumptionQueryFixture.Create();

        // Act
        Result<IReadOnlyList<TagConsumptionResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task HandleAsync_WhenAllSessionsOfAnItemAreBelowItsCountingThreshold_ShouldIgnoreTheItem()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid trackId = Guid.NewGuid();
        Guid bookId = Guid.NewGuid();
        TrackEntity track = _trackEntityFixture.Create(id: trackId, libraryId: libraryId, includeMetadata: false);
        track.Tags = [_tagEntityFixture.Create(name: "rock")];
        BookEntity book = _bookEntityFixture.Create(id: bookId, libraryId: libraryId, includeMetadata: false);
        book.Tags = [_tagEntityFixture.Create(name: "fantasy")];
        StubRecords(
            _mediaConsumptionRecordEntityFixture.Create(
                userId: _userId,
                mediaKind: MediaKind.MusicTrack,
                mediaItemId: trackId,
                libraryId: libraryId,
                status: ConsumptionStatus.Stopped,
                consumedDurationMilliseconds: 29_999),
            _mediaConsumptionRecordEntityFixture.Create(
                userId: _userId,
                mediaKind: MediaKind.Book,
                mediaItemId: bookId,
                libraryId: libraryId,
                status: ConsumptionStatus.Stopped,
                consumedDurationMilliseconds: 1_000,
                consecutivePagesRead: 5));
        StubTracks(track);
        StubBooks(book);
        GetTagConsumptionQuery query = _getTagConsumptionQueryFixture.Create();

        // Act
        Result<IReadOnlyList<TagConsumptionResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task HandleAsync_WhenAnItemIsInAnInaccessibleLibrary_ShouldIgnoreTheItem()
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
            consumedDurationMilliseconds: 1_000,
            consecutivePagesRead: 0));
        _mockAuthorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(
            _userId, Arg.Is<LibraryOwnershipPolicyContext>(context => context.LibraryId == libraryId), Arg.Any<CancellationToken>())
            .Returns(false);
        GetTagConsumptionQuery query = _getTagConsumptionQueryFixture.Create();

        // Act
        Result<IReadOnlyList<TagConsumptionResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Empty(result.Value);
        await _mockTrackRepository.DidNotReceive().GetByIdsAsync(
            Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenAnItemNoLongerExists_ShouldIgnoreTheRecord()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        StubRecords(_mediaConsumptionRecordEntityFixture.Create(
            userId: _userId,
            mediaKind: MediaKind.MusicTrack,
            mediaItemId: Guid.NewGuid(),
            libraryId: libraryId,
            status: ConsumptionStatus.Completed,
            consumedDurationMilliseconds: 1_000,
            consecutivePagesRead: 0));
        StubTracks();
        GetTagConsumptionQuery query = _getTagConsumptionQueryFixture.Create();

        // Act
        Result<IReadOnlyList<TagConsumptionResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task HandleAsync_WhenAnItemMovedToAnotherLibrary_ShouldIgnoreTheRecord()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid trackId = Guid.NewGuid();
        TrackEntity track = _trackEntityFixture.Create(id: trackId, libraryId: Guid.NewGuid(), includeMetadata: false);
        track.Tags = [_tagEntityFixture.Create(name: "rock")];
        StubRecords(_mediaConsumptionRecordEntityFixture.Create(
            userId: _userId,
            mediaKind: MediaKind.MusicTrack,
            mediaItemId: trackId,
            libraryId: libraryId,
            status: ConsumptionStatus.Completed,
            consumedDurationMilliseconds: 1_000,
            consecutivePagesRead: 0));
        StubTracks(track);
        GetTagConsumptionQuery query = _getTagConsumptionQueryFixture.Create();

        // Act
        Result<IReadOnlyList<TagConsumptionResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task HandleAsync_WhenTheMediaKindHasNoConsumptionRule_ShouldIgnoreTheRecord()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        StubRecords(_mediaConsumptionRecordEntityFixture.Create(
            userId: _userId,
            mediaKind: MediaKind.None,
            mediaItemId: Guid.NewGuid(),
            libraryId: libraryId,
            status: ConsumptionStatus.Completed,
            consumedDurationMilliseconds: 1_000,
            consecutivePagesRead: 0));
        GetTagConsumptionQuery query = _getTagConsumptionQueryFixture.Create();

        // Act
        Result<IReadOnlyList<TagConsumptionResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task HandleAsync_WhenTagNamesAreBlank_ShouldSkipThem()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid trackId = Guid.NewGuid();
        TrackEntity track = _trackEntityFixture.Create(id: trackId, libraryId: libraryId, includeMetadata: false);
        track.Tags =
        [
            _tagEntityFixture.Create(name: string.Empty),
            _tagEntityFixture.Create(name: "   "),
            _tagEntityFixture.Create(name: "rock")
        ];
        StubRecords(_mediaConsumptionRecordEntityFixture.Create(
            userId: _userId,
            mediaKind: MediaKind.MusicTrack,
            mediaItemId: trackId,
            libraryId: libraryId,
            status: ConsumptionStatus.Completed,
            consumedDurationMilliseconds: 1_000,
            consecutivePagesRead: 0));
        StubTracks(track);
        GetTagConsumptionQuery query = _getTagConsumptionQueryFixture.Create();

        // Act
        Result<IReadOnlyList<TagConsumptionResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        TagConsumptionResponse tag = Assert.Single(result.Value);
        Assert.Equal("rock", tag.Name);
    }

    [Fact]
    public async Task HandleAsync_WhenABookHasTags_ShouldAggregateThemByTheirLowercaseTrimmedName()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid bookId = Guid.NewGuid();
        BookEntity book = _bookEntityFixture.Create(id: bookId, libraryId: libraryId, includeMetadata: false);
        book.Tags =
        [
            _tagEntityFixture.Create(name: " Fantasy "),
            _tagEntityFixture.Create(name: "FANTASY")
        ];
        StubRecords(_mediaConsumptionRecordEntityFixture.Create(
            userId: _userId,
            mediaKind: MediaKind.Book,
            mediaItemId: bookId,
            libraryId: libraryId,
            status: ConsumptionStatus.Completed,
            consumedDurationMilliseconds: 2_500,
            consecutivePagesRead: 6));
        StubBooks(book);
        GetTagConsumptionQuery query = _getTagConsumptionQueryFixture.Create();

        // Act
        Result<IReadOnlyList<TagConsumptionResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        TagConsumptionResponse tag = Assert.Single(result.Value);
        Assert.Equal("fantasy", tag.Name);
        Assert.Equal(1, tag.ConsumedItemCount);
        Assert.Equal(2.5, tag.ConsumedDurationInSeconds);
    }

    [Fact]
    public async Task HandleAsync_WhenSeveralTagsExist_ShouldOrderThemByCountThenDurationThenName()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid firstTrackId = Guid.NewGuid();
        Guid secondTrackId = Guid.NewGuid();
        Guid thirdTrackId = Guid.NewGuid();
        TrackEntity firstTrack = _trackEntityFixture.Create(id: firstTrackId, libraryId: libraryId, includeMetadata: false);
        firstTrack.Tags = [_tagEntityFixture.Create(name: "alpha"), _tagEntityFixture.Create(name: "beta")];
        TrackEntity secondTrack = _trackEntityFixture.Create(id: secondTrackId, libraryId: libraryId, includeMetadata: false);
        secondTrack.Tags = [_tagEntityFixture.Create(name: "alpha")];
        TrackEntity thirdTrack = _trackEntityFixture.Create(id: thirdTrackId, libraryId: libraryId, includeMetadata: false);
        thirdTrack.Tags = [_tagEntityFixture.Create(name: "gamma"), _tagEntityFixture.Create(name: "delta")];
        StubRecords(
            _mediaConsumptionRecordEntityFixture.Create(
                userId: _userId,
                mediaKind: MediaKind.MusicTrack,
                mediaItemId: firstTrackId,
                libraryId: libraryId,
                status: ConsumptionStatus.Completed,
                consumedDurationMilliseconds: 400,
                consecutivePagesRead: 0),
            _mediaConsumptionRecordEntityFixture.Create(
                userId: _userId,
                mediaKind: MediaKind.MusicTrack,
                mediaItemId: secondTrackId,
                libraryId: libraryId,
                status: ConsumptionStatus.Completed,
                consumedDurationMilliseconds: 600,
                consecutivePagesRead: 0),
            _mediaConsumptionRecordEntityFixture.Create(
                userId: _userId,
                mediaKind: MediaKind.MusicTrack,
                mediaItemId: thirdTrackId,
                libraryId: libraryId,
                status: ConsumptionStatus.Completed,
                consumedDurationMilliseconds: 5_000,
                consecutivePagesRead: 0));
        StubTracks(firstTrack, secondTrack, thirdTrack);
        GetTagConsumptionQuery query = _getTagConsumptionQueryFixture.Create();

        // Act
        Result<IReadOnlyList<TagConsumptionResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(["alpha", "delta", "gamma", "beta"], result.Value.Select(tag => tag.Name));
        Assert.Equal([2, 1, 1, 1], result.Value.Select(tag => tag.ConsumedItemCount));
        Assert.Equal([1.0, 5.0, 5.0, 0.4], result.Value.Select(tag => tag.ConsumedDurationInSeconds));
    }

    [Fact]
    public async Task HandleAsync_WhenItemsExist_ShouldReadTheDistinctIdsOfAccessibleItemsWithNavigationProperties()
    {
        // Arrange
        Guid accessibleLibraryId = Guid.NewGuid();
        Guid inaccessibleLibraryId = Guid.NewGuid();
        Guid trackId = Guid.NewGuid();
        Guid bookId = Guid.NewGuid();
        StubRecords(
            _mediaConsumptionRecordEntityFixture.Create(
                userId: _userId,
                mediaKind: MediaKind.MusicTrack,
                mediaItemId: trackId,
                libraryId: accessibleLibraryId,
                status: ConsumptionStatus.Completed,
                consumedDurationMilliseconds: 1_000,
                consecutivePagesRead: 0),
            _mediaConsumptionRecordEntityFixture.Create(
                userId: _userId,
                mediaKind: MediaKind.MusicTrack,
                mediaItemId: trackId,
                libraryId: accessibleLibraryId,
                status: ConsumptionStatus.Completed,
                consumedDurationMilliseconds: 2_000,
                consecutivePagesRead: 0),
            _mediaConsumptionRecordEntityFixture.Create(
                userId: _userId,
                mediaKind: MediaKind.MusicTrack,
                mediaItemId: Guid.NewGuid(),
                libraryId: inaccessibleLibraryId,
                status: ConsumptionStatus.Completed,
                consumedDurationMilliseconds: 1_000,
                consecutivePagesRead: 0),
            _mediaConsumptionRecordEntityFixture.Create(
                userId: _userId,
                mediaKind: MediaKind.Book,
                mediaItemId: bookId,
                libraryId: accessibleLibraryId,
                status: ConsumptionStatus.Completed,
                consumedDurationMilliseconds: 1_000,
                consecutivePagesRead: 6));
        _mockAuthorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(
            _userId, Arg.Is<LibraryOwnershipPolicyContext>(context => context.LibraryId == inaccessibleLibraryId), Arg.Any<CancellationToken>())
            .Returns(false);
        StubTracks();
        StubBooks();
        GetTagConsumptionQuery query = _getTagConsumptionQueryFixture.Create();

        // Act
        Result<IReadOnlyList<TagConsumptionResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

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
            consumedDurationMilliseconds: 1_000,
            consecutivePagesRead: 0));
        _mockTrackRepository.GetByIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(error);
        GetTagConsumptionQuery query = _getTagConsumptionQueryFixture.Create();

        // Act
        Result<IReadOnlyList<TagConsumptionResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
        await _mockBookRepository.DidNotReceive().GetByIdsAsync(
            Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
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
            consumedDurationMilliseconds: 1_000,
            consecutivePagesRead: 6));
        _mockBookRepository.GetByIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(error);
        GetTagConsumptionQuery query = _getTagConsumptionQueryFixture.Create();

        // Act
        Result<IReadOnlyList<TagConsumptionResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
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
    /// Stubs the book repository so that it returns the provided books.
    /// </summary>
    /// <param name="books">The books returned by the repository.</param>
    private void StubBooks(params BookEntity[] books)
    {
        _mockBookRepository.GetByIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<BookEntity>>(books));
    }
}
