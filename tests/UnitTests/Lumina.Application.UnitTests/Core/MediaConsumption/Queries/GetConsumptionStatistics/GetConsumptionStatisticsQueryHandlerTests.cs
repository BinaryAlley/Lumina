#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaConsumption;
using Lumina.Application.Common.DataAccess.Repositories.MediaConsumption;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Infrastructure.Authorization.Policies.ConsumptionActivity;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Core.MediaConsumption.Queries.GetConsumptionStatistics;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaConsumption;
using Lumina.Application.Fixtures.Core.MediaConsumption.Queries.GetConsumptionStatistics;
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

namespace Lumina.Application.UnitTests.Core.MediaConsumption.Queries.GetConsumptionStatistics;

/// <summary>
/// Contains unit tests for the <see cref="GetConsumptionStatisticsQueryHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetConsumptionStatisticsQueryHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly IConsumptionStatisticsRepository _mockConsumptionStatisticsRepository;
    private readonly ICurrentUserService _mockCurrentUserService;
    private readonly IAuthorizationService _mockAuthorizationService;
    private readonly IValidator<GetConsumptionStatisticsQuery> _mockValidator;
    private readonly GetConsumptionStatisticsQueryHandler _sut;
    private readonly GetConsumptionStatisticsQueryFixture _getConsumptionStatisticsQueryFixture = new();
    private readonly MediaConsumptionRecordEntityFixture _mediaConsumptionRecordEntityFixture = new();
    private readonly Guid _userId = Guid.NewGuid();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetConsumptionStatisticsQueryHandlerTests"/> class.
    /// </summary>
    public GetConsumptionStatisticsQueryHandlerTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockConsumptionStatisticsRepository = Substitute.For<IConsumptionStatisticsRepository>();
        _mockCurrentUserService = Substitute.For<ICurrentUserService>();
        _mockAuthorizationService = Substitute.For<IAuthorizationService>();

        _mockUnitOfWork.ConsumptionStatisticsRepository.Returns(_mockConsumptionStatisticsRepository);
        _mockCurrentUserService.UserId.Returns(_userId);
        _mockAuthorizationService.EvaluatePolicyAsync<IConsumptionActivityAccessPolicy>(
            Arg.Any<Guid>(), Arg.Any<ConsumptionActivityAccessPolicyContext>(), Arg.Any<CancellationToken>())
            .Returns(true);
        _mockConsumptionStatisticsRepository.GetForUserAsync(
            Arg.Any<Guid>(), Arg.Any<Guid?>(), Arg.Any<MediaKind?>(), Arg.Any<DateTime?>(), Arg.Any<DateTime?>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<MediaConsumptionRecordEntity>>([]));

        _mockValidator = Substitute.For<IValidator<GetConsumptionStatisticsQuery>>();
        _mockValidator.Validate(Arg.Any<GetConsumptionStatisticsQuery>()).Returns([]);

        _sut = new GetConsumptionStatisticsQueryHandler(_mockUnitOfWork, _mockCurrentUserService, _mockAuthorizationService, _mockValidator);
    }

    [Fact]
    public async Task HandleAsync_WhenValidatorReturnsError_ShouldReturnValidationErrorAndNotReadRecords()
    {
        // Arrange
        GetConsumptionStatisticsQuery query = _getConsumptionStatisticsQueryFixture.Create();
        _mockValidator.Validate(Arg.Any<GetConsumptionStatisticsQuery>())
            .Returns([DomainErrors.MediaConsumption.TargetUserIdIsInvalid]);

        // Act
        Result<ConsumptionSummaryResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.MediaConsumption.TargetUserIdIsInvalid, result.FirstError);
        await _mockAuthorizationService.DidNotReceive().EvaluatePolicyAsync<IConsumptionActivityAccessPolicy>(
            Arg.Any<Guid>(), Arg.Any<ConsumptionActivityAccessPolicyContext>(), Arg.Any<CancellationToken>());
        await _mockConsumptionStatisticsRepository.DidNotReceive().GetForUserAsync(
            Arg.Any<Guid>(), Arg.Any<Guid?>(), Arg.Any<MediaKind?>(), Arg.Any<DateTime?>(), Arg.Any<DateTime?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenCurrentUserIsNull_ShouldReturnNotAuthorizedAndNotReadRecords()
    {
        // Arrange
        GetConsumptionStatisticsQuery query = _getConsumptionStatisticsQueryFixture.Create();
        _mockCurrentUserService.UserId.Returns((Guid?)null);

        // Act
        Result<ConsumptionSummaryResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

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
        GetConsumptionStatisticsQuery query = _getConsumptionStatisticsQueryFixture.Create();
        _mockAuthorizationService.EvaluatePolicyAsync<IConsumptionActivityAccessPolicy>(
            Arg.Any<Guid>(), Arg.Any<ConsumptionActivityAccessPolicyContext>(), Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        Result<ConsumptionSummaryResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Authorization.NotAuthorized, result.FirstError);
        await _mockConsumptionStatisticsRepository.DidNotReceive().GetForUserAsync(
            Arg.Any<Guid>(), Arg.Any<Guid?>(), Arg.Any<MediaKind?>(), Arg.Any<DateTime?>(), Arg.Any<DateTime?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenTargetUserIdIsNotSupplied_ShouldEvaluateThePolicyForTheCurrentUser()
    {
        // Arrange
        GetConsumptionStatisticsQuery query = _getConsumptionStatisticsQueryFixture.Create(includeTargetUserId: false);

        // Act
        Result<ConsumptionSummaryResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        await _mockAuthorizationService.Received(1).EvaluatePolicyAsync<IConsumptionActivityAccessPolicy>(
            _userId,
            Arg.Is<ConsumptionActivityAccessPolicyContext>(context => context.TargetUserId == _userId),
            Arg.Any<CancellationToken>());
        await _mockConsumptionStatisticsRepository.Received(1).GetForUserAsync(
            _userId, Arg.Any<Guid?>(), Arg.Any<MediaKind?>(), Arg.Any<DateTime?>(), Arg.Any<DateTime?>(), Arg.Any<CancellationToken>());
        Assert.Equal(_userId, result.Value.UserId);
    }

    [Fact]
    public async Task HandleAsync_WhenTargetUserIdIsSupplied_ShouldReadTheRecordsOfTheParsedTarget()
    {
        // Arrange
        Guid targetUserId = Guid.NewGuid();
        GetConsumptionStatisticsQuery query = _getConsumptionStatisticsQueryFixture.Create(targetUserId: targetUserId.ToString());

        // Act
        Result<ConsumptionSummaryResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(targetUserId, result.Value.UserId);
        await _mockAuthorizationService.Received(1).EvaluatePolicyAsync<IConsumptionActivityAccessPolicy>(
            _userId,
            Arg.Is<ConsumptionActivityAccessPolicyContext>(context => context.TargetUserId == targetUserId),
            Arg.Any<CancellationToken>());
        await _mockConsumptionStatisticsRepository.Received(1).GetForUserAsync(
            targetUserId, Arg.Any<Guid?>(), Arg.Any<MediaKind?>(), Arg.Any<DateTime?>(), Arg.Any<DateTime?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenLibraryIdIsSupplied_ShouldForwardTheParsedLibraryId()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        GetConsumptionStatisticsQuery query = _getConsumptionStatisticsQueryFixture.Create(libraryId: libraryId.ToString());

        // Act
        Result<ConsumptionSummaryResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        await _mockConsumptionStatisticsRepository.Received(1).GetForUserAsync(
            Arg.Any<Guid>(), libraryId, Arg.Any<MediaKind?>(), Arg.Any<DateTime?>(), Arg.Any<DateTime?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenLibraryIdIsNotSupplied_ShouldForwardNoLibraryFilter()
    {
        // Arrange
        GetConsumptionStatisticsQuery query = _getConsumptionStatisticsQueryFixture.Create(includeLibraryId: false);

        // Act
        Result<ConsumptionSummaryResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        await _mockConsumptionStatisticsRepository.Received(1).GetForUserAsync(
            Arg.Any<Guid>(), null, Arg.Any<MediaKind?>(), Arg.Any<DateTime?>(), Arg.Any<DateTime?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenFiltersAreSupplied_ShouldForwardThemUnchanged()
    {
        // Arrange
        MediaKind mediaKind = MediaKind.Book;
        DateTime fromUtc = DateTime.UtcNow.AddDays(-10);
        DateTime toUtc = DateTime.UtcNow.AddDays(-1);
        GetConsumptionStatisticsQuery query = _getConsumptionStatisticsQueryFixture.Create(
            includeLibraryId: false,
            mediaKind: mediaKind,
            fromUtc: fromUtc,
            toUtc: toUtc);

        // Act
        Result<ConsumptionSummaryResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        await _mockConsumptionStatisticsRepository.Received(1).GetForUserAsync(
            Arg.Any<Guid>(), null, mediaKind, fromUtc, toUtc, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenTheRecordsCannotBeRead_ShouldReturnTheRepositoryError()
    {
        // Arrange
        GetConsumptionStatisticsQuery query = _getConsumptionStatisticsQueryFixture.Create();
        Error error = Error.Failure("Persistence.Failure", "The consumption records could not be read.");
        _mockConsumptionStatisticsRepository.GetForUserAsync(
            Arg.Any<Guid>(), Arg.Any<Guid?>(), Arg.Any<MediaKind?>(), Arg.Any<DateTime?>(), Arg.Any<DateTime?>(), Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        Result<ConsumptionSummaryResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
    }

    [Fact]
    public async Task HandleAsync_WhenThereAreNoRecords_ShouldReturnZeroTotalsAndNoMediaKindSummaries()
    {
        // Arrange
        GetConsumptionStatisticsQuery query = _getConsumptionStatisticsQueryFixture.Create(includeTargetUserId: false);

        // Act
        Result<ConsumptionSummaryResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(_userId, result.Value.UserId);
        Assert.Equal(0, result.Value.TotalConsumedDurationInSeconds);
        Assert.Equal(0, result.Value.CountedSessionCount);
        Assert.Equal(0, result.Value.CompletedSessionCount);
        Assert.Equal(0, result.Value.StoppedSessionCount);
        Assert.Equal(0, result.Value.CompletionRate);
        Assert.Empty(result.Value.MediaKindSummaries);
    }

    [Fact]
    public async Task HandleAsync_WhenRecordsOfSeveralMediaKindsExist_ShouldApplyTheRulesAndOrderTheSummariesByMediaKind()
    {
        // Arrange
        MediaConsumptionRecordEntity[] records =
        [
            // A music track stopped exactly at the 30 second threshold counts, and its duration still adds up.
            _mediaConsumptionRecordEntityFixture.Create(
                userId: _userId,
                mediaKind: MediaKind.MusicTrack,
                status: ConsumptionStatus.Stopped,
                consumedDurationMilliseconds: 30_000),
            // A music track stopped just below the threshold does not count, but its duration still adds up.
            _mediaConsumptionRecordEntityFixture.Create(
                userId: _userId,
                mediaKind: MediaKind.MusicTrack,
                status: ConsumptionStatus.Stopped,
                consumedDurationMilliseconds: 29_999),
            // A completed music track always counts, regardless of its duration.
            _mediaConsumptionRecordEntityFixture.Create(
                userId: _userId,
                mediaKind: MediaKind.MusicTrack,
                status: ConsumptionStatus.Completed,
                consumedDurationMilliseconds: 5_000),
            // A music track that is still in progress counts once it reaches the threshold.
            _mediaConsumptionRecordEntityFixture.Create(
                userId: _userId,
                mediaKind: MediaKind.MusicTrack,
                status: ConsumptionStatus.InProgress,
                consumedDurationMilliseconds: 30_001),
            // A book stopped after more than five consecutive pages counts.
            _mediaConsumptionRecordEntityFixture.Create(
                userId: _userId,
                mediaKind: MediaKind.Book,
                status: ConsumptionStatus.Stopped,
                consumedDurationMilliseconds: 1_000,
                consecutivePagesRead: 6),
            // A book stopped at exactly five consecutive pages does not count, because the rule is strictly above the threshold.
            _mediaConsumptionRecordEntityFixture.Create(
                userId: _userId,
                mediaKind: MediaKind.Book,
                status: ConsumptionStatus.Stopped,
                consumedDurationMilliseconds: 2_000,
                consecutivePagesRead: 5),
            // A completed book always counts, regardless of the pages read.
            _mediaConsumptionRecordEntityFixture.Create(
                userId: _userId,
                mediaKind: MediaKind.Book,
                status: ConsumptionStatus.Completed,
                consumedDurationMilliseconds: 3_000,
                consecutivePagesRead: 0)
        ];
        _mockConsumptionStatisticsRepository.GetForUserAsync(
            Arg.Any<Guid>(), Arg.Any<Guid?>(), Arg.Any<MediaKind?>(), Arg.Any<DateTime?>(), Arg.Any<DateTime?>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<MediaConsumptionRecordEntity>>(records));
        GetConsumptionStatisticsQuery query = _getConsumptionStatisticsQueryFixture.Create(includeTargetUserId: false);

        // Act
        Result<ConsumptionSummaryResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(2, result.Value.MediaKindSummaries.Count);

        MediaKindConsumptionSummaryResponse musicTracks = result.Value.MediaKindSummaries[0];
        Assert.Equal(MediaKind.MusicTrack, musicTracks.MediaKind);
        Assert.Equal(95.0, musicTracks.TotalConsumedDurationInSeconds);
        Assert.Equal(3, musicTracks.CountedSessionCount);
        Assert.Equal(1, musicTracks.CompletedSessionCount);
        Assert.Equal(1, musicTracks.StoppedSessionCount);
        Assert.Equal(1.0 / 3.0, musicTracks.CompletionRate);

        MediaKindConsumptionSummaryResponse books = result.Value.MediaKindSummaries[1];
        Assert.Equal(MediaKind.Book, books.MediaKind);
        Assert.Equal(6.0, books.TotalConsumedDurationInSeconds);
        Assert.Equal(2, books.CountedSessionCount);
        Assert.Equal(1, books.CompletedSessionCount);
        Assert.Equal(1, books.StoppedSessionCount);
        Assert.Equal(0.5, books.CompletionRate);

        Assert.Equal(101.0, result.Value.TotalConsumedDurationInSeconds);
        Assert.Equal(5, result.Value.CountedSessionCount);
        Assert.Equal(2, result.Value.CompletedSessionCount);
        Assert.Equal(2, result.Value.StoppedSessionCount);
        Assert.Equal(0.4, result.Value.CompletionRate);
    }

    [Fact]
    public async Task HandleAsync_WhenRecordsExist_ShouldConvertTheTotalDurationToSeconds()
    {
        // Arrange
        MediaConsumptionRecordEntity record = _mediaConsumptionRecordEntityFixture.Create(
            userId: _userId,
            mediaKind: MediaKind.MusicTrack,
            status: ConsumptionStatus.Completed,
            consumedDurationMilliseconds: 1_500);
        _mockConsumptionStatisticsRepository.GetForUserAsync(
            Arg.Any<Guid>(), Arg.Any<Guid?>(), Arg.Any<MediaKind?>(), Arg.Any<DateTime?>(), Arg.Any<DateTime?>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<MediaConsumptionRecordEntity>>([record]));
        GetConsumptionStatisticsQuery query = _getConsumptionStatisticsQueryFixture.Create(includeTargetUserId: false);

        // Act
        Result<ConsumptionSummaryResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(1.5, result.Value.TotalConsumedDurationInSeconds);
    }

    [Fact]
    public async Task HandleAsync_WhenDurationExceedsTheRepresentableTimeSpanRange_ShouldClampWithoutThrowing()
    {
        // Arrange
        long maximumMilliseconds = TimeSpan.MaxValue.Ticks / TimeSpan.TicksPerMillisecond;
        double expectedClampedSeconds = TimeSpan.FromMilliseconds(maximumMilliseconds).TotalSeconds;
        MediaConsumptionRecordEntity record = _mediaConsumptionRecordEntityFixture.Create(
            userId: _userId,
            mediaKind: MediaKind.MusicTrack,
            status: ConsumptionStatus.Completed,
            consumedDurationMilliseconds: maximumMilliseconds + 1);
        _mockConsumptionStatisticsRepository.GetForUserAsync(
            Arg.Any<Guid>(), Arg.Any<Guid?>(), Arg.Any<MediaKind?>(), Arg.Any<DateTime?>(), Arg.Any<DateTime?>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<MediaConsumptionRecordEntity>>([record]));
        GetConsumptionStatisticsQuery query = _getConsumptionStatisticsQueryFixture.Create(includeTargetUserId: false);

        // Act
        Result<ConsumptionSummaryResponse> result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(expectedClampedSeconds, result.Value.TotalConsumedDurationInSeconds);
        Assert.Single(result.Value.MediaKindSummaries);
        Assert.Equal(expectedClampedSeconds, result.Value.MediaKindSummaries[0].TotalConsumedDurationInSeconds);
    }
}
