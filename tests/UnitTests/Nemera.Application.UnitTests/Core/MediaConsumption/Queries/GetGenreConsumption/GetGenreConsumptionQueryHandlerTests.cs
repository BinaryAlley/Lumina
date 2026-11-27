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
using Lumina.Application.Core.MediaConsumption.Queries.GetGenreConsumption;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Common;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaConsumption;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Fixtures.Core.MediaConsumption.Queries.GetGenreConsumption;
using Lumina.Contracts.Responses.MediaConsumption;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.MediaConsumption;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
using DomainErrors = Lumina.Domain.Common.Errors.Errors;
#endregion

namespace Lumina.Application.UnitTests.Core.MediaConsumption.Queries.GetGenreConsumption;

/// <summary>
/// Contains unit tests for the <see cref="GetGenreConsumptionQueryHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetGenreConsumptionQueryHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly IConsumptionStatisticsRepository _mockConsumptionStatisticsRepository;
    private readonly ITrackRepository _mockTrackRepository;
    private readonly IBookRepository _mockBookRepository;
    private readonly ICurrentUserService _mockCurrentUserService;
    private readonly IAuthorizationService _mockAuthorizationService;
    private readonly IValidator<GetGenreConsumptionQuery> _mockValidator;
    private readonly GetGenreConsumptionQueryHandler _sut;
    private readonly GetGenreConsumptionQueryFixture _getGenreConsumptionQueryFixture = new();
    private readonly MediaConsumptionRecordEntityFixture _mediaConsumptionRecordEntityFixture = new();
    private readonly TrackEntityFixture _trackEntityFixture = new();
    private readonly AlbumEntityFixture _albumEntityFixture = new();
    private readonly ArtistEntityFixture _artistEntityFixture = new();
    private readonly BookEntityFixture _bookEntityFixture = new();
    private readonly GenreEntityFixture _genreEntityFixture = new();
    private readonly Guid _userId = Guid.NewGuid();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetGenreConsumptionQueryHandlerTests"/> class.
    /// </summary>
    public GetGenreConsumptionQueryHandlerTests()
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

        _mockValidator = Substitute.For<IValidator<GetGenreConsumptionQuery>>();
        _mockValidator.Validate(Arg.Any<GetGenreConsumptionQuery>()).Returns([]);

        _sut = new GetGenreConsumptionQueryHandler(_mockUnitOfWork, _mockCurrentUserService, _mockAuthorizationService, _mockValidator);
    }

    [Fact]
    public async Task HandleAsync_WhenValidatorReturnsError_ShouldReturnValidationErrorAndNotReadRecords()
    {
        // Arrange
        GetGenreConsumptionQuery query = _getGenreConsumptionQueryFixture.Create();
        _mockValidator.Validate(Arg.Any<GetGenreConsumptionQuery>())
            .Returns([DomainErrors.MediaConsumption.TargetUserIdIsInvalid]);

        // Act
        Result<IReadOnlyList<GenreConsumptionResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

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
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenCurrentUserIsNull_ShouldReturnNotAuthorizedAndNotReadRecords()
    {
        // Arrange
        GetGenreConsumptionQuery query = _getGenreConsumptionQueryFixture.Create();
        _mockCurrentUserService.UserId.Returns((Guid?)null);

        // Act
        Result<IReadOnlyList<GenreConsumptionResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockAuthorizationService.DidNotReceive().EvaluatePolicyAsync<IConsumptionActivityAccessPolicy>(
            Arg.Any<Guid>(), Arg.Any<ConsumptionActivityAccessPolicyContext>(), Arg.Any<CancellationToken>());
        await _mockConsumptionStatisticsRepository.DidNotReceive().GetForUserAsync(
            Arg.Any<Guid>(), Arg.Any<Guid?>(), Arg.Any<MediaKind?>(), Arg.Any<DateTime?>(), Arg.Any<DateTime?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenPolicyDeniesTheTarget_ShouldReturnNotAuthorizedAndNotReadRecords()
    {
        // Arrange
        GetGenreConsumptionQuery query = _getGenreConsumptionQueryFixture.Create();
        _mockAuthorizationService.EvaluatePolicyAsync<IConsumptionActivityAccessPolicy>(
            Arg.Any<Guid>(), Arg.Any<ConsumptionActivityAccessPolicyContext>(), Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        Result<IReadOnlyList<GenreConsumptionResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockConsumptionStatisticsRepository.DidNotReceive().GetForUserAsync(
            Arg.Any<Guid>(), Arg.Any<Guid?>(), Arg.Any<MediaKind?>(), Arg.Any<DateTime?>(), Arg.Any<DateTime?>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenTheRecordsCannotBeRead_ShouldReturnTheRepositoryError()
    {
        // Arrange
        GetGenreConsumptionQuery query = _getGenreConsumptionQueryFixture.Create();
        Error error = Error.Failure("Persistence.Failure", "The consumption records could not be read.");
        _mockConsumptionStatisticsRepository.GetForUserAsync(
            Arg.Any<Guid>(), Arg.Any<Guid?>(), Arg.Any<MediaKind?>(), Arg.Any<DateTime?>(), Arg.Any<DateTime?>(), Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        Result<IReadOnlyList<GenreConsumptionResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
        await _mockTrackRepository.DidNotReceive().GetByIdsAsync(
            Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
        await _mockBookRepository.DidNotReceive().GetByIdsAsync(
            Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenThereAreNoRecords_ShouldReturnAnEmptyListAndNotReadTheCatalogues()
    {
        // Arrange
        GetGenreConsumptionQuery query = _getGenreConsumptionQueryFixture.Create();

        // Act
        Result<IReadOnlyList<GenreConsumptionResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Empty(result.Value);
        await _mockTrackRepository.DidNotReceive().GetByIdsAsync(
            Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
        await _mockBookRepository.DidNotReceive().GetByIdsAsync(
            Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenTheTargetUserIsNotSupplied_ShouldReadTheStatisticsOfTheCurrentUser()
    {
        // Arrange
        GetGenreConsumptionQuery query = _getGenreConsumptionQueryFixture.Create(includeTargetUserId: false);

        // Act
        await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        await _mockAuthorizationService.Received(1).EvaluatePolicyAsync<IConsumptionActivityAccessPolicy>(
            _userId,
            Arg.Is<ConsumptionActivityAccessPolicyContext>(context => context.TargetUserId == _userId),
            Arg.Any<CancellationToken>());
        await _mockConsumptionStatisticsRepository.Received(1).GetForUserAsync(
            _userId, Arg.Any<Guid?>(), Arg.Any<MediaKind?>(), Arg.Any<DateTime?>(), Arg.Any<DateTime?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenTheTargetUserIsSupplied_ShouldEvaluateThePolicyAndReadTheStatisticsOfTheTargetUser()
    {
        // Arrange
        Guid targetUserId = Guid.NewGuid();
        GetGenreConsumptionQuery query = _getGenreConsumptionQueryFixture.Create(targetUserId: targetUserId.ToString());

        // Act
        await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        await _mockAuthorizationService.Received(1).EvaluatePolicyAsync<IConsumptionActivityAccessPolicy>(
            _userId,
            Arg.Is<ConsumptionActivityAccessPolicyContext>(context => context.TargetUserId == targetUserId),
            Arg.Any<CancellationToken>());
        await _mockConsumptionStatisticsRepository.Received(1).GetForUserAsync(
            targetUserId, Arg.Any<Guid?>(), Arg.Any<MediaKind?>(), Arg.Any<DateTime?>(), Arg.Any<DateTime?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenTheQueryIsFiltered_ShouldForwardTheParsedFiltersToTheRecordsRepository()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        DateTime fromUtc = new(2025, 1, 2, 3, 4, 5, DateTimeKind.Utc);
        DateTime toUtc = new(2025, 2, 3, 4, 5, 6, DateTimeKind.Utc);
        GetGenreConsumptionQuery query = _getGenreConsumptionQueryFixture.Create(
            includeTargetUserId: false,
            libraryId: libraryId.ToString(),
            mediaKind: nameof(MediaKind.Book),
            fromUtc: fromUtc.ToString("o", CultureInfo.InvariantCulture),
            toUtc: toUtc.ToString("o", CultureInfo.InvariantCulture));

        // Act
        await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        await _mockConsumptionStatisticsRepository.Received(1).GetForUserAsync(
            _userId,
            Arg.Is<Guid?>(value => value == libraryId),
            Arg.Is<MediaKind?>(value => value == MediaKind.Book),
            Arg.Is<DateTime?>(value => value == fromUtc),
            Arg.Is<DateTime?>(value => value == toUtc),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenRecordsSpanSeveralLibraries_ShouldEvaluateTheLibraryAccessOncePerDistinctLibrary()
    {
        // Arrange
        Guid firstLibraryId = Guid.NewGuid();
        Guid secondLibraryId = Guid.NewGuid();
        StubRecords(
            _mediaConsumptionRecordEntityFixture.Create(
                userId: _userId,
                mediaKind: MediaKind.MusicTrack,
                mediaItemId: Guid.NewGuid(),
                libraryId: firstLibraryId,
                status: ConsumptionStatus.Completed,
                consumedDurationMilliseconds: 1_000),
            _mediaConsumptionRecordEntityFixture.Create(
                userId: _userId,
                mediaKind: MediaKind.MusicTrack,
                mediaItemId: Guid.NewGuid(),
                libraryId: firstLibraryId,
                status: ConsumptionStatus.Completed,
                consumedDurationMilliseconds: 1_000),
            _mediaConsumptionRecordEntityFixture.Create(
                userId: _userId,
                mediaKind: MediaKind.MusicTrack,
                mediaItemId: Guid.NewGuid(),
                libraryId: secondLibraryId,
                status: ConsumptionStatus.Completed,
                consumedDurationMilliseconds: 1_000));
        GetGenreConsumptionQuery query = _getGenreConsumptionQueryFixture.Create();

        // Act
        await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        await _mockAuthorizationService.Received(1).EvaluatePolicyAsync<ILibraryOwnershipPolicy>(
            _userId,
            Arg.Is<LibraryOwnershipPolicyContext>(context => context.LibraryId == firstLibraryId),
            Arg.Any<CancellationToken>());
        await _mockAuthorizationService.Received(1).EvaluatePolicyAsync<ILibraryOwnershipPolicy>(
            _userId,
            Arg.Is<LibraryOwnershipPolicyContext>(context => context.LibraryId == secondLibraryId),
            Arg.Any<CancellationToken>());
        await _mockAuthorizationService.Received(2).EvaluatePolicyAsync<ILibraryOwnershipPolicy>(
            _userId, Arg.Any<LibraryOwnershipPolicyContext>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenAccessibleItemsExist_ShouldEvaluateTheLibraryAccessBeforeReadingTheCatalogues()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid trackId = Guid.NewGuid();
        TrackEntity track = _trackEntityFixture.Create(id: trackId, libraryId: libraryId, includeMetadata: false);
        track.Genres = [_genreEntityFixture.Create(name: "rock")];
        StubRecords(_mediaConsumptionRecordEntityFixture.Create(
            userId: _userId,
            mediaKind: MediaKind.MusicTrack,
            mediaItemId: trackId,
            libraryId: libraryId,
            status: ConsumptionStatus.Completed,
            consumedDurationMilliseconds: 1_000));
        StubTracks(track);
        GetGenreConsumptionQuery query = _getGenreConsumptionQueryFixture.Create();

        // Act
        await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Received.InOrder(() =>
        {
            _mockAuthorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(
                _userId,
                Arg.Is<LibraryOwnershipPolicyContext>(context => context.LibraryId == libraryId),
                Arg.Any<CancellationToken>());
            _mockTrackRepository.GetByIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), true, Arg.Any<CancellationToken>());
        });
    }

    [Fact]
    public async Task HandleAsync_WhenAnItemIsInAnInaccessibleLibrary_ShouldExcludeItsGenresWithoutReadingThem()
    {
        // Arrange
        Guid accessibleLibraryId = Guid.NewGuid();
        Guid inaccessibleLibraryId = Guid.NewGuid();
        Guid accessibleTrackId = Guid.NewGuid();
        Guid inaccessibleTrackId = Guid.NewGuid();
        TrackEntity accessibleTrack = _trackEntityFixture.Create(id: accessibleTrackId, libraryId: accessibleLibraryId, includeMetadata: false);
        accessibleTrack.Genres = [_genreEntityFixture.Create(name: "jazz")];
        TrackEntity inaccessibleTrack = _trackEntityFixture.Create(id: inaccessibleTrackId, libraryId: inaccessibleLibraryId, includeMetadata: false);
        inaccessibleTrack.Genres = [_genreEntityFixture.Create(name: "rock")];
        StubRecords(
            _mediaConsumptionRecordEntityFixture.Create(
                userId: _userId,
                mediaKind: MediaKind.MusicTrack,
                mediaItemId: accessibleTrackId,
                libraryId: accessibleLibraryId,
                status: ConsumptionStatus.Completed,
                consumedDurationMilliseconds: 1_000),
            _mediaConsumptionRecordEntityFixture.Create(
                userId: _userId,
                mediaKind: MediaKind.MusicTrack,
                mediaItemId: inaccessibleTrackId,
                libraryId: inaccessibleLibraryId,
                status: ConsumptionStatus.Completed,
                consumedDurationMilliseconds: 1_000));
        StubTracks(accessibleTrack, inaccessibleTrack);
        _mockAuthorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(
            _userId, Arg.Is<LibraryOwnershipPolicyContext>(context => context.LibraryId == inaccessibleLibraryId), Arg.Any<CancellationToken>())
            .Returns(false);
        GetGenreConsumptionQuery query = _getGenreConsumptionQueryFixture.Create();

        // Act
        Result<IReadOnlyList<GenreConsumptionResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        GenreConsumptionResponse genre = Assert.Single(result.Value);
        Assert.Equal("jazz", genre.Name);
        await _mockTrackRepository.Received(1).GetByIdsAsync(
            Arg.Is<IReadOnlyCollection<Guid>>(ids => ids.Count == 1 && ids.Contains(accessibleTrackId)),
            true,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenATrackHasGenres_ShouldUseTheTrackGenresInsteadOfTheAlbumAndArtistGenres()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid trackId = Guid.NewGuid();
        ArtistEntity artist = _artistEntityFixture.Create(libraryId: libraryId, includeAlbums: false, includeContributors: false);
        artist.Genres = [_genreEntityFixture.Create(name: "artistgenre")];
        AlbumEntity album = _albumEntityFixture.Create(libraryId: libraryId, includeTracks: false, includeMetadata: false);
        album.Artist = artist;
        album.Genres = [_genreEntityFixture.Create(name: "albumgenre")];
        TrackEntity track = _trackEntityFixture.Create(id: trackId, libraryId: libraryId, includeMetadata: false);
        track.Album = album;
        track.Genres = [_genreEntityFixture.Create(name: "trackgenre")];
        StubRecords(_mediaConsumptionRecordEntityFixture.Create(
            userId: _userId,
            mediaKind: MediaKind.MusicTrack,
            mediaItemId: trackId,
            libraryId: libraryId,
            status: ConsumptionStatus.Completed,
            consumedDurationMilliseconds: 1_000));
        StubTracks(track);
        GetGenreConsumptionQuery query = _getGenreConsumptionQueryFixture.Create();

        // Act
        Result<IReadOnlyList<GenreConsumptionResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        GenreConsumptionResponse genre = Assert.Single(result.Value);
        Assert.Equal("trackgenre", genre.Name);
    }

    [Fact]
    public async Task HandleAsync_WhenATrackHasNoGenresOfItsOwn_ShouldFallBackToItsAlbumGenres()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid trackId = Guid.NewGuid();
        ArtistEntity artist = _artistEntityFixture.Create(libraryId: libraryId, includeAlbums: false, includeContributors: false);
        artist.Genres = [_genreEntityFixture.Create(name: "artistgenre")];
        AlbumEntity album = _albumEntityFixture.Create(libraryId: libraryId, includeTracks: false, includeMetadata: false);
        album.Artist = artist;
        album.Genres = [_genreEntityFixture.Create(name: "albumgenre")];
        TrackEntity track = _trackEntityFixture.Create(id: trackId, libraryId: libraryId, includeMetadata: false);
        track.Album = album;
        StubRecords(_mediaConsumptionRecordEntityFixture.Create(
            userId: _userId,
            mediaKind: MediaKind.MusicTrack,
            mediaItemId: trackId,
            libraryId: libraryId,
            status: ConsumptionStatus.Completed,
            consumedDurationMilliseconds: 1_000));
        StubTracks(track);
        GetGenreConsumptionQuery query = _getGenreConsumptionQueryFixture.Create();

        // Act
        Result<IReadOnlyList<GenreConsumptionResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        GenreConsumptionResponse genre = Assert.Single(result.Value);
        Assert.Equal("albumgenre", genre.Name);
    }

    [Fact]
    public async Task HandleAsync_WhenATrackAndItsAlbumHaveNoGenres_ShouldFallBackToItsArtistGenres()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid trackId = Guid.NewGuid();
        ArtistEntity artist = _artistEntityFixture.Create(libraryId: libraryId, includeAlbums: false, includeContributors: false);
        artist.Genres = [_genreEntityFixture.Create(name: "artistgenre")];
        AlbumEntity album = _albumEntityFixture.Create(libraryId: libraryId, includeTracks: false, includeMetadata: false);
        album.Artist = artist;
        TrackEntity track = _trackEntityFixture.Create(id: trackId, libraryId: libraryId, includeMetadata: false);
        track.Album = album;
        StubRecords(_mediaConsumptionRecordEntityFixture.Create(
            userId: _userId,
            mediaKind: MediaKind.MusicTrack,
            mediaItemId: trackId,
            libraryId: libraryId,
            status: ConsumptionStatus.Completed,
            consumedDurationMilliseconds: 1_000));
        StubTracks(track);
        GetGenreConsumptionQuery query = _getGenreConsumptionQueryFixture.Create();

        // Act
        Result<IReadOnlyList<GenreConsumptionResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        GenreConsumptionResponse genre = Assert.Single(result.Value);
        Assert.Equal("artistgenre", genre.Name);
    }

    [Fact]
    public async Task HandleAsync_WhenATrackHasNoGenresAtAnyLevel_ShouldExcludeTheRecord()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid trackId = Guid.NewGuid();
        TrackEntity track = _trackEntityFixture.Create(id: trackId, libraryId: libraryId, includeMetadata: false);
        StubRecords(_mediaConsumptionRecordEntityFixture.Create(
            userId: _userId,
            mediaKind: MediaKind.MusicTrack,
            mediaItemId: trackId,
            libraryId: libraryId,
            status: ConsumptionStatus.Completed,
            consumedDurationMilliseconds: 1_000));
        StubTracks(track);
        GetGenreConsumptionQuery query = _getGenreConsumptionQueryFixture.Create();

        // Act
        Result<IReadOnlyList<GenreConsumptionResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task HandleAsync_WhenABookHasGenres_ShouldUseTheBookGenres()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid bookId = Guid.NewGuid();
        BookEntity book = _bookEntityFixture.Create(id: bookId, libraryId: libraryId, includeMetadata: false);
        book.Genres = [_genreEntityFixture.Create(name: "fantasy")];
        StubRecords(_mediaConsumptionRecordEntityFixture.Create(
            userId: _userId,
            mediaKind: MediaKind.Book,
            mediaItemId: bookId,
            libraryId: libraryId,
            status: ConsumptionStatus.Completed,
            consumedDurationMilliseconds: 2_500,
            consecutivePagesRead: 0));
        StubBooks(book);
        GetGenreConsumptionQuery query = _getGenreConsumptionQueryFixture.Create();

        // Act
        Result<IReadOnlyList<GenreConsumptionResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        GenreConsumptionResponse genre = Assert.Single(result.Value);
        Assert.Equal("fantasy", genre.Name);
        Assert.Equal(1, genre.ConsumedItemCount);
        Assert.Equal(2.5, genre.ConsumedDurationInSeconds);
    }

    [Fact]
    public async Task HandleAsync_WhenABookHasNoGenres_ShouldExcludeTheRecord()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid bookId = Guid.NewGuid();
        BookEntity book = _bookEntityFixture.Create(id: bookId, libraryId: libraryId, includeMetadata: false);
        StubRecords(_mediaConsumptionRecordEntityFixture.Create(
            userId: _userId,
            mediaKind: MediaKind.Book,
            mediaItemId: bookId,
            libraryId: libraryId,
            status: ConsumptionStatus.Completed,
            consumedDurationMilliseconds: 1_000,
            consecutivePagesRead: 6));
        StubBooks(book);
        GetGenreConsumptionQuery query = _getGenreConsumptionQueryFixture.Create();

        // Act
        Result<IReadOnlyList<GenreConsumptionResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task HandleAsync_WhenGenreNamesDifferInCasingAndWhitespace_ShouldTrimLowercaseAndCountEachNameOncePerItem()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid trackId = Guid.NewGuid();
        TrackEntity track = _trackEntityFixture.Create(id: trackId, libraryId: libraryId, includeMetadata: false);
        track.Genres =
        [
            _genreEntityFixture.Create(name: "rock"),
            _genreEntityFixture.Create(name: "ROCK"),
            _genreEntityFixture.Create(name: " Rock "),
            _genreEntityFixture.Create(name: "jazz")
        ];
        StubRecords(_mediaConsumptionRecordEntityFixture.Create(
            userId: _userId,
            mediaKind: MediaKind.MusicTrack,
            mediaItemId: trackId,
            libraryId: libraryId,
            status: ConsumptionStatus.Completed,
            consumedDurationMilliseconds: 1_000));
        StubTracks(track);
        GetGenreConsumptionQuery query = _getGenreConsumptionQueryFixture.Create();

        // Act
        Result<IReadOnlyList<GenreConsumptionResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

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
    public async Task HandleAsync_WhenGenreNamesAreMissingOrBlank_ShouldSkipThem()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid trackId = Guid.NewGuid();
        TrackEntity track = _trackEntityFixture.Create(id: trackId, libraryId: libraryId, includeMetadata: false);
        track.Genres =
        [
            _genreEntityFixture.Create(includeName: false),
            _genreEntityFixture.Create(name: string.Empty),
            _genreEntityFixture.Create(name: "   "),
            _genreEntityFixture.Create(name: "rock")
        ];
        StubRecords(_mediaConsumptionRecordEntityFixture.Create(
            userId: _userId,
            mediaKind: MediaKind.MusicTrack,
            mediaItemId: trackId,
            libraryId: libraryId,
            status: ConsumptionStatus.Completed,
            consumedDurationMilliseconds: 1_000));
        StubTracks(track);
        GetGenreConsumptionQuery query = _getGenreConsumptionQueryFixture.Create();

        // Act
        Result<IReadOnlyList<GenreConsumptionResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        GenreConsumptionResponse genre = Assert.Single(result.Value);
        Assert.Equal("rock", genre.Name);
    }

    [Fact]
    public async Task HandleAsync_WhenAnItemHasSeveralCountedSessions_ShouldCountItOnceAndSumTheirDurations()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid trackId = Guid.NewGuid();
        TrackEntity track = _trackEntityFixture.Create(id: trackId, libraryId: libraryId, includeMetadata: false);
        track.Genres = [_genreEntityFixture.Create(name: "rock")];
        StubRecords(
            _mediaConsumptionRecordEntityFixture.Create(
                userId: _userId,
                mediaKind: MediaKind.MusicTrack,
                mediaItemId: trackId,
                libraryId: libraryId,
                status: ConsumptionStatus.Completed,
                consumedDurationMilliseconds: 1_000),
            _mediaConsumptionRecordEntityFixture.Create(
                userId: _userId,
                mediaKind: MediaKind.MusicTrack,
                mediaItemId: trackId,
                libraryId: libraryId,
                status: ConsumptionStatus.Completed,
                consumedDurationMilliseconds: 2_000));
        StubTracks(track);
        GetGenreConsumptionQuery query = _getGenreConsumptionQueryFixture.Create();

        // Act
        Result<IReadOnlyList<GenreConsumptionResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        GenreConsumptionResponse genre = Assert.Single(result.Value);
        Assert.Equal("rock", genre.Name);
        Assert.Equal(1, genre.ConsumedItemCount);
        Assert.Equal(3.0, genre.ConsumedDurationInSeconds);
    }

    [Fact]
    public async Task HandleAsync_WhenATrackSessionIsStoppedBelowTheMinimumDuration_ShouldExcludeTheItem()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid trackId = Guid.NewGuid();
        TrackEntity track = _trackEntityFixture.Create(id: trackId, libraryId: libraryId, includeMetadata: false);
        track.Genres = [_genreEntityFixture.Create(name: "rock")];
        StubRecords(_mediaConsumptionRecordEntityFixture.Create(
            userId: _userId,
            mediaKind: MediaKind.MusicTrack,
            mediaItemId: trackId,
            libraryId: libraryId,
            status: ConsumptionStatus.Stopped,
            consumedDurationMilliseconds: 29_999));
        StubTracks(track);
        GetGenreConsumptionQuery query = _getGenreConsumptionQueryFixture.Create();

        // Act
        Result<IReadOnlyList<GenreConsumptionResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task HandleAsync_WhenABookSessionReadsAtMostTheMinimumConsecutivePages_ShouldExcludeTheItem()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid bookId = Guid.NewGuid();
        BookEntity book = _bookEntityFixture.Create(id: bookId, libraryId: libraryId, includeMetadata: false);
        book.Genres = [_genreEntityFixture.Create(name: "fantasy")];
        StubRecords(_mediaConsumptionRecordEntityFixture.Create(
            userId: _userId,
            mediaKind: MediaKind.Book,
            mediaItemId: bookId,
            libraryId: libraryId,
            status: ConsumptionStatus.Stopped,
            consumedDurationMilliseconds: 1_000,
            consecutivePagesRead: 5));
        StubBooks(book);
        GetGenreConsumptionQuery query = _getGenreConsumptionQueryFixture.Create();

        // Act
        Result<IReadOnlyList<GenreConsumptionResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task HandleAsync_WhenASessionIsCompleted_ShouldCountTheItemRegardlessOfTheThreshold()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid trackId = Guid.NewGuid();
        TrackEntity track = _trackEntityFixture.Create(id: trackId, libraryId: libraryId, includeMetadata: false);
        track.Genres = [_genreEntityFixture.Create(name: "rock")];
        StubRecords(_mediaConsumptionRecordEntityFixture.Create(
            userId: _userId,
            mediaKind: MediaKind.MusicTrack,
            mediaItemId: trackId,
            libraryId: libraryId,
            status: ConsumptionStatus.Completed,
            consumedDurationMilliseconds: 0));
        StubTracks(track);
        GetGenreConsumptionQuery query = _getGenreConsumptionQueryFixture.Create();

        // Act
        Result<IReadOnlyList<GenreConsumptionResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        GenreConsumptionResponse genre = Assert.Single(result.Value);
        Assert.Equal("rock", genre.Name);
        Assert.Equal(1, genre.ConsumedItemCount);
        Assert.Equal(0.0, genre.ConsumedDurationInSeconds);
    }

    [Fact]
    public async Task HandleAsync_WhenAnItemIsResolvedInAnotherLibraryThanTheRecord_ShouldExcludeTheItem()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid trackId = Guid.NewGuid();
        TrackEntity track = _trackEntityFixture.Create(id: trackId, libraryId: Guid.NewGuid(), includeMetadata: false);
        track.Genres = [_genreEntityFixture.Create(name: "rock")];
        StubRecords(_mediaConsumptionRecordEntityFixture.Create(
            userId: _userId,
            mediaKind: MediaKind.MusicTrack,
            mediaItemId: trackId,
            libraryId: libraryId,
            status: ConsumptionStatus.Completed,
            consumedDurationMilliseconds: 1_000));
        StubTracks(track);
        GetGenreConsumptionQuery query = _getGenreConsumptionQueryFixture.Create();

        // Act
        Result<IReadOnlyList<GenreConsumptionResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task HandleAsync_WhenSeveralGenresExist_ShouldOrderThemByCountThenDurationThenName()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid firstTrackId = Guid.NewGuid();
        Guid secondTrackId = Guid.NewGuid();
        Guid thirdTrackId = Guid.NewGuid();
        TrackEntity firstTrack = _trackEntityFixture.Create(id: firstTrackId, libraryId: libraryId, includeMetadata: false);
        firstTrack.Genres = [_genreEntityFixture.Create(name: "alpha"), _genreEntityFixture.Create(name: "beta")];
        TrackEntity secondTrack = _trackEntityFixture.Create(id: secondTrackId, libraryId: libraryId, includeMetadata: false);
        secondTrack.Genres = [_genreEntityFixture.Create(name: "alpha")];
        TrackEntity thirdTrack = _trackEntityFixture.Create(id: thirdTrackId, libraryId: libraryId, includeMetadata: false);
        thirdTrack.Genres = [_genreEntityFixture.Create(name: "gamma"), _genreEntityFixture.Create(name: "delta")];
        StubRecords(
            _mediaConsumptionRecordEntityFixture.Create(
                userId: _userId,
                mediaKind: MediaKind.MusicTrack,
                mediaItemId: firstTrackId,
                libraryId: libraryId,
                status: ConsumptionStatus.Completed,
                consumedDurationMilliseconds: 400),
            _mediaConsumptionRecordEntityFixture.Create(
                userId: _userId,
                mediaKind: MediaKind.MusicTrack,
                mediaItemId: secondTrackId,
                libraryId: libraryId,
                status: ConsumptionStatus.Completed,
                consumedDurationMilliseconds: 600),
            _mediaConsumptionRecordEntityFixture.Create(
                userId: _userId,
                mediaKind: MediaKind.MusicTrack,
                mediaItemId: thirdTrackId,
                libraryId: libraryId,
                status: ConsumptionStatus.Completed,
                consumedDurationMilliseconds: 5_000));
        StubTracks(firstTrack, secondTrack, thirdTrack);
        GetGenreConsumptionQuery query = _getGenreConsumptionQueryFixture.Create();

        // Act
        Result<IReadOnlyList<GenreConsumptionResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(["alpha", "delta", "gamma", "beta"], result.Value.Select(genre => genre.Name));
        Assert.Equal([2, 1, 1, 1], result.Value.Select(genre => genre.ConsumedItemCount));
        Assert.Equal([1.0, 5.0, 5.0, 0.4], result.Value.Select(genre => genre.ConsumedDurationInSeconds));
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
                consumedDurationMilliseconds: 1_000),
            _mediaConsumptionRecordEntityFixture.Create(
                userId: _userId,
                mediaKind: MediaKind.MusicTrack,
                mediaItemId: trackId,
                libraryId: accessibleLibraryId,
                status: ConsumptionStatus.Completed,
                consumedDurationMilliseconds: 2_000),
            _mediaConsumptionRecordEntityFixture.Create(
                userId: _userId,
                mediaKind: MediaKind.MusicTrack,
                mediaItemId: Guid.NewGuid(),
                libraryId: inaccessibleLibraryId,
                status: ConsumptionStatus.Completed,
                consumedDurationMilliseconds: 1_000),
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
        GetGenreConsumptionQuery query = _getGenreConsumptionQueryFixture.Create();

        // Act
        await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
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
            consumedDurationMilliseconds: 1_000));
        _mockTrackRepository.GetByIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(error);
        GetGenreConsumptionQuery query = _getGenreConsumptionQueryFixture.Create();

        // Act
        Result<IReadOnlyList<GenreConsumptionResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

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
        GetGenreConsumptionQuery query = _getGenreConsumptionQueryFixture.Create();

        // Act
        Result<IReadOnlyList<GenreConsumptionResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
    }

    [Fact]
    public async Task HandleAsync_WhenSuccessful_ShouldNotSaveChanges()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid trackId = Guid.NewGuid();
        TrackEntity track = _trackEntityFixture.Create(id: trackId, libraryId: libraryId, includeMetadata: false);
        track.Genres = [_genreEntityFixture.Create(name: "rock")];
        StubRecords(_mediaConsumptionRecordEntityFixture.Create(
            userId: _userId,
            mediaKind: MediaKind.MusicTrack,
            mediaItemId: trackId,
            libraryId: libraryId,
            status: ConsumptionStatus.Completed,
            consumedDurationMilliseconds: 1_000));
        StubTracks(track);
        GetGenreConsumptionQuery query = _getGenreConsumptionQueryFixture.Create();

        // Act
        Result<IReadOnlyList<GenreConsumptionResponse>> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Single(result.Value);
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
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
