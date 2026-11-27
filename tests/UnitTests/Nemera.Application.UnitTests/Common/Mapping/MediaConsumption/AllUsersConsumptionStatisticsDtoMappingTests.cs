#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DTO.MediaConsumption;
using Lumina.Application.Common.Mapping.MediaConsumption;
using Lumina.Application.Fixtures.Common.DTO.MediaConsumption;
using Lumina.Contracts.Responses.MediaConsumption;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.MediaConsumption;

/// <summary>
/// Contains unit tests for the <see cref="AllUsersConsumptionStatisticsDtoMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class AllUsersConsumptionStatisticsDtoMappingTests
{
    private readonly AllUsersConsumptionStatisticsDtoFixture _allUsersConsumptionStatisticsDtoFixture = new();
    private readonly ConsumptionTotalsDtoFixture _consumptionTotalsDtoFixture = new();
    private readonly ConsumptionStatisticsDtoFixture _consumptionStatisticsDtoFixture = new();
    private readonly MediaKindConsumptionStatisticsDtoFixture _mediaKindConsumptionStatisticsDtoFixture = new();

    [Fact]
    public void ToResponse_WhenMappingAllUsersStatistics_ShouldMapTheTotalAndTheUsers()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        MediaKindConsumptionStatisticsDto totalMediaKindSummary = _mediaKindConsumptionStatisticsDtoFixture.Create(
            mediaKind: MediaKind.MusicTrack,
            totalConsumedDurationMilliseconds: 1_000,
            countedSessionCount: 2,
            completedSessionCount: 1,
            stoppedSessionCount: 1,
            completionRate: 0.5);
        ConsumptionTotalsDto total = _consumptionTotalsDtoFixture.Create(
            totalConsumedDurationMilliseconds: 3_000,
            countedSessionCount: 4,
            completedSessionCount: 3,
            stoppedSessionCount: 1,
            completionRate: 0.75,
            mediaKindSummaries: [totalMediaKindSummary]);
        ConsumptionStatisticsDto user = _consumptionStatisticsDtoFixture.Create(
            userId: userId,
            totalConsumedDurationMilliseconds: 2_000,
            countedSessionCount: 2,
            completedSessionCount: 1,
            stoppedSessionCount: 1,
            completionRate: 0.5,
            mediaKindSummaries: [totalMediaKindSummary]);
        AllUsersConsumptionStatisticsDto statistics = _allUsersConsumptionStatisticsDtoFixture.Create(
            total: total,
            users: [user]);

        // Act
        AllUsersConsumptionStatisticsResponse result = statistics.ToResponse();

        // Assert
        Assert.Equal(3.0, result.Total.TotalConsumedDurationInSeconds);
        Assert.Equal(4, result.Total.CountedSessionCount);
        Assert.Equal(3, result.Total.CompletedSessionCount);
        Assert.Equal(1, result.Total.StoppedSessionCount);
        Assert.Equal(0.75, result.Total.CompletionRate);
        Assert.Single(result.Total.MediaKindSummaries);
        Assert.Equal(MediaKind.MusicTrack, result.Total.MediaKindSummaries[0].MediaKind);
        Assert.Equal(1.0, result.Total.MediaKindSummaries[0].TotalConsumedDurationInSeconds);

        ConsumptionSummaryResponse mappedUser = Assert.Single(result.Users);
        Assert.Equal(userId, mappedUser.UserId);
        Assert.Equal(2.0, mappedUser.TotalConsumedDurationInSeconds);
        Assert.Equal(2, mappedUser.CountedSessionCount);
        Assert.Equal(1, mappedUser.CompletedSessionCount);
        Assert.Equal(1, mappedUser.StoppedSessionCount);
        Assert.Equal(0.5, mappedUser.CompletionRate);
        Assert.Single(mappedUser.MediaKindSummaries);
    }

    [Fact]
    public void ToResponse_WhenMappingMultipleUsers_ShouldMapThemInOrder()
    {
        // Arrange
        Guid firstUserId = Guid.NewGuid();
        Guid secondUserId = Guid.NewGuid();
        ConsumptionStatisticsDto firstUser = _consumptionStatisticsDtoFixture.Create(userId: firstUserId, totalConsumedDurationMilliseconds: 1_000);
        ConsumptionStatisticsDto secondUser = _consumptionStatisticsDtoFixture.Create(userId: secondUserId, totalConsumedDurationMilliseconds: 2_000);
        AllUsersConsumptionStatisticsDto statistics = _allUsersConsumptionStatisticsDtoFixture.Create(users: [firstUser, secondUser]);

        // Act
        AllUsersConsumptionStatisticsResponse result = statistics.ToResponse();

        // Assert
        Assert.Equal(2, result.Users.Count);
        Assert.Equal(firstUserId, result.Users[0].UserId);
        Assert.Equal(1.0, result.Users[0].TotalConsumedDurationInSeconds);
        Assert.Equal(secondUserId, result.Users[1].UserId);
        Assert.Equal(2.0, result.Users[1].TotalConsumedDurationInSeconds);
    }

    [Fact]
    public void ToResponse_WhenThereAreNoUsers_ShouldMapAnEmptyList()
    {
        // Arrange
        AllUsersConsumptionStatisticsDto statistics = _allUsersConsumptionStatisticsDtoFixture.Create(includeUsers: false);

        // Act
        AllUsersConsumptionStatisticsResponse result = statistics.ToResponse();

        // Assert
        Assert.NotNull(result.Users);
        Assert.Empty(result.Users);
    }

    [Fact]
    public void ToResponse_WhenThereAreNoUsers_ShouldNotReturnNullCollections()
    {
        // Arrange
        AllUsersConsumptionStatisticsDto statistics = _allUsersConsumptionStatisticsDtoFixture.Create(
            total: _consumptionTotalsDtoFixture.Create(includeMediaKindSummaries: false),
            includeUsers: false);

        // Act
        AllUsersConsumptionStatisticsResponse result = statistics.ToResponse();

        // Assert
        Assert.NotNull(result.Total);
        Assert.NotNull(result.Total.MediaKindSummaries);
        Assert.Empty(result.Total.MediaKindSummaries);
        Assert.NotNull(result.Users);
        Assert.Empty(result.Users);
    }
}
