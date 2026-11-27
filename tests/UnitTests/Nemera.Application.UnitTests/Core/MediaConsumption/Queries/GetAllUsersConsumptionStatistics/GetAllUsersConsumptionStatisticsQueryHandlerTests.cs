#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaConsumption;
using Lumina.Application.Common.DataAccess.Repositories.MediaConsumption;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Core.MediaConsumption.Queries.GetAllUsersConsumptionStatistics;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaConsumption;
using Lumina.Application.Fixtures.Core.MediaConsumption.Queries.GetAllUsersConsumptionStatistics;
using Lumina.Contracts.Responses.MediaConsumption;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.MediaConsumption;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
using DomainErrors = Lumina.Domain.Common.Errors.Errors;
#endregion

namespace Lumina.Application.UnitTests.Core.MediaConsumption.Queries.GetAllUsersConsumptionStatistics;

/// <summary>
/// Contains unit tests for the <see cref="GetAllUsersConsumptionStatisticsQueryHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetAllUsersConsumptionStatisticsQueryHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly IConsumptionStatisticsRepository _mockConsumptionStatisticsRepository;
    private readonly ICurrentUserService _mockCurrentUserService;
    private readonly IAuthorizationService _mockAuthorizationService;
    private readonly IValidator<GetAllUsersConsumptionStatisticsQuery> _mockValidator;
    private readonly GetAllUsersConsumptionStatisticsQueryHandler _sut;
    private readonly GetAllUsersConsumptionStatisticsQueryFixture _getAllUsersConsumptionStatisticsQueryFixture = new();
    private readonly MediaConsumptionRecordEntityFixture _mediaConsumptionRecordEntityFixture = new();
    private readonly Guid _userId = Guid.NewGuid();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetAllUsersConsumptionStatisticsQueryHandlerTests"/> class.
    /// </summary>
    public GetAllUsersConsumptionStatisticsQueryHandlerTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockConsumptionStatisticsRepository = Substitute.For<IConsumptionStatisticsRepository>();
        _mockCurrentUserService = Substitute.For<ICurrentUserService>();
        _mockAuthorizationService = Substitute.For<IAuthorizationService>();

        _mockUnitOfWork.ConsumptionStatisticsRepository.Returns(_mockConsumptionStatisticsRepository);
        _mockCurrentUserService.UserId.Returns(_userId);
        // Default stubs: the current user is an administrator and the records of every user can be read.
        _mockAuthorizationService.IsInRoleAsync(_userId, AuthorizationRoles.ADMIN, Arg.Any<CancellationToken>())
            .Returns(true);
        _mockConsumptionStatisticsRepository.GetForAllUsersAsync(
            Arg.Any<Guid?>(), Arg.Any<MediaKind?>(), Arg.Any<DateTime?>(), Arg.Any<DateTime?>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<MediaConsumptionRecordEntity>>([]));

        _mockValidator = Substitute.For<IValidator<GetAllUsersConsumptionStatisticsQuery>>();
        _mockValidator.Validate(Arg.Any<GetAllUsersConsumptionStatisticsQuery>()).Returns([]);

        _sut = new GetAllUsersConsumptionStatisticsQueryHandler(_mockUnitOfWork, _mockCurrentUserService, _mockAuthorizationService, _mockValidator);
    }

    [Fact]
    public async Task HandleAsync_WhenValidatorReturnsError_ShouldReturnValidationErrorAndNotReadRecords()
    {
        // Arrange
        GetAllUsersConsumptionStatisticsQuery query = _getAllUsersConsumptionStatisticsQueryFixture.Create();
        _mockValidator.Validate(Arg.Any<GetAllUsersConsumptionStatisticsQuery>())
            .Returns([DomainErrors.MediaConsumption.UnsupportedMediaKind]);

        // Act
        Result<AllUsersConsumptionStatisticsResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.MediaConsumption.UnsupportedMediaKind, result.FirstError);
        await _mockAuthorizationService.DidNotReceive().IsInRoleAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _mockConsumptionStatisticsRepository.DidNotReceive().GetForAllUsersAsync(
            Arg.Any<Guid?>(), Arg.Any<MediaKind?>(), Arg.Any<DateTime?>(), Arg.Any<DateTime?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenCurrentUserIsNull_ShouldReturnNotAuthorizedAndNotReadRecords()
    {
        // Arrange
        GetAllUsersConsumptionStatisticsQuery query = _getAllUsersConsumptionStatisticsQueryFixture.Create();
        _mockCurrentUserService.UserId.Returns((Guid?)null);

        // Act
        Result<AllUsersConsumptionStatisticsResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockConsumptionStatisticsRepository.DidNotReceive().GetForAllUsersAsync(
            Arg.Any<Guid?>(), Arg.Any<MediaKind?>(), Arg.Any<DateTime?>(), Arg.Any<DateTime?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenTheUserIsNotAnAdministrator_ShouldReturnNotAuthorizedAndNotReadRecords()
    {
        // Arrange
        GetAllUsersConsumptionStatisticsQuery query = _getAllUsersConsumptionStatisticsQueryFixture.Create();
        _mockAuthorizationService.IsInRoleAsync(_userId, AuthorizationRoles.ADMIN, Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        Result<AllUsersConsumptionStatisticsResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockAuthorizationService.Received(1).IsInRoleAsync(_userId, AuthorizationRoles.ADMIN, Arg.Any<CancellationToken>());
        await _mockConsumptionStatisticsRepository.DidNotReceive().GetForAllUsersAsync(
            Arg.Any<Guid?>(), Arg.Any<MediaKind?>(), Arg.Any<DateTime?>(), Arg.Any<DateTime?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenTheUserIsAnAdministrator_ShouldReadTheRecordsOfAllUsersWithTheForwardedFilters()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        string mediaKind = nameof(MediaKind.MusicTrack);
        string fromUtc = "2026-03-01T00:00:00Z";
        string toUtc = "2026-03-02T00:00:00Z";
        GetAllUsersConsumptionStatisticsQuery query = _getAllUsersConsumptionStatisticsQueryFixture.Create(
            libraryId: libraryId.ToString(),
            mediaKind: mediaKind,
            fromUtc: fromUtc,
            toUtc: toUtc);

        // Act
        Result<AllUsersConsumptionStatisticsResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        await _mockConsumptionStatisticsRepository.Received(1).GetForAllUsersAsync(
            libraryId,
            MediaKind.MusicTrack,
            Arg.Is<DateTime?>(value => value == new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc)),
            Arg.Is<DateTime?>(value => value == new DateTime(2026, 3, 2, 0, 0, 0, DateTimeKind.Utc)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenLibraryIdIsNotSupplied_ShouldForwardNoLibraryFilter()
    {
        // Arrange
        GetAllUsersConsumptionStatisticsQuery query = _getAllUsersConsumptionStatisticsQueryFixture.Create(includeLibraryId: false);

        // Act
        Result<AllUsersConsumptionStatisticsResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        await _mockConsumptionStatisticsRepository.Received(1).GetForAllUsersAsync(
            null, Arg.Any<MediaKind?>(), Arg.Any<DateTime?>(), Arg.Any<DateTime?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenTheRecordsCannotBeRead_ShouldReturnTheRepositoryError()
    {
        // Arrange
        GetAllUsersConsumptionStatisticsQuery query = _getAllUsersConsumptionStatisticsQueryFixture.Create();
        Error error = Error.Failure("Persistence.Failure", "The consumption records could not be read.");
        _mockConsumptionStatisticsRepository.GetForAllUsersAsync(
            Arg.Any<Guid?>(), Arg.Any<MediaKind?>(), Arg.Any<DateTime?>(), Arg.Any<DateTime?>(), Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        Result<AllUsersConsumptionStatisticsResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
    }

    [Fact]
    public async Task HandleAsync_WhenThereAreNoRecords_ShouldReturnZeroTotalsAndNoUsers()
    {
        // Arrange
        GetAllUsersConsumptionStatisticsQuery query = _getAllUsersConsumptionStatisticsQueryFixture.Create();

        // Act
        Result<AllUsersConsumptionStatisticsResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(0, result.Value.Total.TotalConsumedDurationInSeconds);
        Assert.Equal(0, result.Value.Total.CountedSessionCount);
        Assert.Equal(0, result.Value.Total.CompletedSessionCount);
        Assert.Equal(0, result.Value.Total.StoppedSessionCount);
        Assert.Equal(0, result.Value.Total.CompletionRate);
        Assert.Empty(result.Value.Total.MediaKindSummaries);
        Assert.Empty(result.Value.Users);
    }

    [Fact]
    public async Task HandleAsync_WhenSeveralUsersHaveRecords_ShouldBuildOneOrderedSummaryPerUserAndMergeTheirTotals()
    {
        // Arrange
        Guid firstUserId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        Guid secondUserId = Guid.Parse("00000000-0000-0000-0000-000000000002");
        MediaConsumptionRecordEntity[] records =
        [
            // The first user has two counted music tracks, one stopped and one completed.
            _mediaConsumptionRecordEntityFixture.Create(
                userId: firstUserId,
                mediaKind: MediaKind.MusicTrack,
                status: ConsumptionStatus.Stopped,
                consumedDurationMilliseconds: 30_000),
            _mediaConsumptionRecordEntityFixture.Create(
                userId: firstUserId,
                mediaKind: MediaKind.MusicTrack,
                status: ConsumptionStatus.Completed,
                consumedDurationMilliseconds: 10_000),
            // The second user has a music track below the threshold, which still contributes its duration but is not counted.
            _mediaConsumptionRecordEntityFixture.Create(
                userId: secondUserId,
                mediaKind: MediaKind.MusicTrack,
                status: ConsumptionStatus.Stopped,
                consumedDurationMilliseconds: 29_999),
            // The second user also has two counted books, one stopped above the page threshold and one completed.
            _mediaConsumptionRecordEntityFixture.Create(
                userId: secondUserId,
                mediaKind: MediaKind.Book,
                status: ConsumptionStatus.Stopped,
                consumedDurationMilliseconds: 1_000,
                consecutivePagesRead: 6),
            _mediaConsumptionRecordEntityFixture.Create(
                userId: secondUserId,
                mediaKind: MediaKind.Book,
                status: ConsumptionStatus.Completed,
                consumedDurationMilliseconds: 4_000,
                consecutivePagesRead: 0)
        ];
        _mockConsumptionStatisticsRepository.GetForAllUsersAsync(
            Arg.Any<Guid?>(), Arg.Any<MediaKind?>(), Arg.Any<DateTime?>(), Arg.Any<DateTime?>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<MediaConsumptionRecordEntity>>(records));
        GetAllUsersConsumptionStatisticsQuery query = _getAllUsersConsumptionStatisticsQueryFixture.Create();

        // Act
        Result<AllUsersConsumptionStatisticsResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);

        Assert.Equal(2, result.Value.Users.Count);

        ConsumptionSummaryResponse firstUser = result.Value.Users[0];
        Assert.Equal(firstUserId, firstUser.UserId);
        Assert.Equal(40.0, firstUser.TotalConsumedDurationInSeconds);
        Assert.Equal(2, firstUser.CountedSessionCount);
        Assert.Equal(1, firstUser.CompletedSessionCount);
        Assert.Equal(1, firstUser.StoppedSessionCount);
        Assert.Equal(0.5, firstUser.CompletionRate);
        Assert.Single(firstUser.MediaKindSummaries);
        Assert.Equal(MediaKind.MusicTrack, firstUser.MediaKindSummaries[0].MediaKind);

        ConsumptionSummaryResponse secondUser = result.Value.Users[1];
        Assert.Equal(secondUserId, secondUser.UserId);
        Assert.Equal(34.999, secondUser.TotalConsumedDurationInSeconds);
        Assert.Equal(2, secondUser.CountedSessionCount);
        Assert.Equal(1, secondUser.CompletedSessionCount);
        Assert.Equal(1, secondUser.StoppedSessionCount);
        Assert.Equal(0.5, secondUser.CompletionRate);
        Assert.Equal(2, secondUser.MediaKindSummaries.Count);
        Assert.Equal(MediaKind.MusicTrack, secondUser.MediaKindSummaries[0].MediaKind);
        Assert.Equal(29.999, secondUser.MediaKindSummaries[0].TotalConsumedDurationInSeconds);
        Assert.Equal(0, secondUser.MediaKindSummaries[0].CountedSessionCount);
        Assert.Equal(0, secondUser.MediaKindSummaries[0].CompletionRate);
        Assert.Equal(MediaKind.Book, secondUser.MediaKindSummaries[1].MediaKind);
        Assert.Equal(5.0, secondUser.MediaKindSummaries[1].TotalConsumedDurationInSeconds);
        Assert.Equal(2, secondUser.MediaKindSummaries[1].CountedSessionCount);

        Assert.Equal(2, result.Value.Total.MediaKindSummaries.Count);
        Assert.Equal(MediaKind.MusicTrack, result.Value.Total.MediaKindSummaries[0].MediaKind);
        Assert.Equal(69.999, result.Value.Total.MediaKindSummaries[0].TotalConsumedDurationInSeconds);
        Assert.Equal(2, result.Value.Total.MediaKindSummaries[0].CountedSessionCount);
        Assert.Equal(1, result.Value.Total.MediaKindSummaries[0].CompletedSessionCount);
        Assert.Equal(1, result.Value.Total.MediaKindSummaries[0].StoppedSessionCount);
        Assert.Equal(0.5, result.Value.Total.MediaKindSummaries[0].CompletionRate);
        Assert.Equal(MediaKind.Book, result.Value.Total.MediaKindSummaries[1].MediaKind);
        Assert.Equal(5.0, result.Value.Total.MediaKindSummaries[1].TotalConsumedDurationInSeconds);

        Assert.Equal(74.999, result.Value.Total.TotalConsumedDurationInSeconds);
        Assert.Equal(4, result.Value.Total.CountedSessionCount);
        Assert.Equal(2, result.Value.Total.CompletedSessionCount);
        Assert.Equal(2, result.Value.Total.StoppedSessionCount);
        Assert.Equal(0.5, result.Value.Total.CompletionRate);

        await _mockConsumptionStatisticsRepository.Received(1).GetForAllUsersAsync(
            Arg.Any<Guid?>(), Arg.Any<MediaKind?>(), Arg.Any<DateTime?>(), Arg.Any<DateTime?>(), Arg.Any<CancellationToken>());
    }
}
