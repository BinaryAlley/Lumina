#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Scheduling;
using Lumina.Application.Common.Mapping.Scheduling;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Scheduling;
using Lumina.Contracts.Responses.Scheduling;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.SchedulingBoundedContext.ScheduledJobAggregate;
using Lumina.Domain.Core.BoundedContexts.SchedulingBoundedContext.ScheduledJobAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.Scheduling;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.Scheduling;

/// <summary>
/// Contains unit tests for the <see cref="ScheduledJobEntityMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ScheduledJobEntityMappingTests
{
    private readonly ScheduledJobEntityFixture _scheduledJobEntityFixture = new();

    [Fact]
    public void ToDomainEntity_WhenRepositoryEntityHasIntervalSchedule_ShouldMapToIntervalScheduledJob()
    {
        // Arrange
        ScheduledJobEntity repositoryEntity = _scheduledJobEntityFixture.Create(
            scheduleType: ScheduleType.WithIntervalInMinutes,
            status: ScheduledJobStatus.Active);

        // Act
        Result<ScheduledJob> result = repositoryEntity.ToDomainEntity();

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(repositoryEntity.Id, result.Value.Id.Value);
        Assert.Equal(repositoryEntity.Name, result.Value.Name);
        Assert.Equal(repositoryEntity.TaskType, result.Value.TaskType);
        Assert.Equal(repositoryEntity.Status, result.Value.Status);
        Assert.Equal(repositoryEntity.OwnerUserId, result.Value.OwnerUserId.Value);
        IntervalSchedule intervalSchedule = Assert.IsType<IntervalSchedule>(result.Value.Schedule);
        Assert.Equal(repositoryEntity.IntervalMinutes, intervalSchedule.IntervalMinutes);
        if (repositoryEntity.LastStartedOnUtc is not null)
            Assert.Equal(repositoryEntity.LastStartedOnUtc.Value, result.Value.LastStartedOnUtc.Value);
        if (repositoryEntity.LastCompletedOnUtc is not null)
            Assert.Equal(repositoryEntity.LastCompletedOnUtc.Value, result.Value.LastCompletedOnUtc.Value);
    }

    [Fact]
    public void ToDomainEntity_WhenRepositoryEntityHasDailySchedule_ShouldMapToDailyScheduledJob()
    {
        // Arrange
        ScheduledJobEntity repositoryEntity = _scheduledJobEntityFixture.Create(
            scheduleType: ScheduleType.DailyAtHourAndMinute,
            hour: 7,
            minute: 15);

        // Act
        Result<ScheduledJob> result = repositoryEntity.ToDomainEntity();

        // Assert
        Assert.False(result.IsFailure);
        DailySchedule dailySchedule = Assert.IsType<DailySchedule>(result.Value.Schedule);
        Assert.Equal(7, dailySchedule.Hour);
        Assert.Equal(15, dailySchedule.Minute);
    }

    [Fact]
    public void ToDomainEntity_WhenIntervalScheduleHasNonPositiveInterval_ShouldReturnError()
    {
        // Arrange
        ScheduledJobEntity repositoryEntity = _scheduledJobEntityFixture.Create(
            scheduleType: ScheduleType.WithIntervalInMinutes,
            intervalMinutes: 0);

        // Act
        Result<ScheduledJob> result = repositoryEntity.ToDomainEntity();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Lumina.Domain.Common.Errors.Errors.Scheduling.IntervalMinutesMustBePositive, result.FirstError);
    }

    [Fact]
    public void ToDomainEntity_WhenDailyScheduleHasOutOfRangeHour_ShouldReturnError()
    {
        // Arrange
        ScheduledJobEntity repositoryEntity = _scheduledJobEntityFixture.Create(
            scheduleType: ScheduleType.DailyAtHourAndMinute,
            hour: 30,
            minute: 0);

        // Act
        Result<ScheduledJob> result = repositoryEntity.ToDomainEntity();

        // Assert
        Assert.True(result.IsFailure);
    }

    [Fact]
    public void ToDomainEntities_WhenCalledWithCollection_ShouldMapEveryRepositoryEntity()
    {
        // Arrange
        List<ScheduledJobEntity> repositoryEntities =
        [
            _scheduledJobEntityFixture.Create(scheduleType: ScheduleType.WithIntervalInMinutes),
            _scheduledJobEntityFixture.Create(scheduleType: ScheduleType.DailyAtHourAndMinute)
        ];

        // Act
        List<Result<ScheduledJob>> results = repositoryEntities.ToDomainEntities().ToList();

        // Assert
        Assert.Equal(2, results.Count);
        Assert.All(results, result => Assert.False(result.IsFailure));
    }

    [Fact]
    public void ToResponse_WhenRepositoryEntityHasIntervalSchedule_ShouldMapToResponse()
    {
        // Arrange
        ScheduledJobEntity repositoryEntity = _scheduledJobEntityFixture.Create(
            id: Guid.NewGuid(),
            scheduleType: ScheduleType.WithIntervalInMinutes,
            intervalMinutes: 45,
            status: ScheduledJobStatus.Active);

        // Act
        ScheduledJobResponse result = repositoryEntity.ToResponse();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(repositoryEntity.Id, result.Id);
        Assert.Equal(repositoryEntity.Name, result.Name);
        Assert.Equal(repositoryEntity.TaskType, result.TaskType);
        Assert.Equal(ScheduleType.WithIntervalInMinutes, result.ScheduleType);
        Assert.Equal(45, result.IntervalMinutes);
        Assert.Null(result.Hour);
        Assert.Null(result.Minute);
        Assert.Equal(ScheduledJobStatus.Active, result.Status);
        Assert.Equal(repositoryEntity.LastStartedOnUtc, result.LastStartedOnUtc);
        Assert.Equal(repositoryEntity.LastCompletedOnUtc, result.LastCompletedOnUtc);
    }

    [Fact]
    public void ToResponse_WhenRepositoryEntityHasDailySchedule_ShouldMapToResponse()
    {
        // Arrange
        ScheduledJobEntity repositoryEntity = _scheduledJobEntityFixture.Create(
            id: Guid.NewGuid(),
            scheduleType: ScheduleType.DailyAtHourAndMinute,
            hour: 6,
            minute: 30,
            status: ScheduledJobStatus.Added);

        // Act
        ScheduledJobResponse result = repositoryEntity.ToResponse();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(ScheduleType.DailyAtHourAndMinute, result.ScheduleType);
        Assert.Null(result.IntervalMinutes);
        Assert.Equal(6, result.Hour);
        Assert.Equal(30, result.Minute);
    }
}
