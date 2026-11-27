#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Scheduling;
using Lumina.Application.Common.Mapping.Scheduling;
using Lumina.Contracts.Responses.Scheduling;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.SchedulingBoundedContext.ScheduledJobAggregate;
using Lumina.Domain.Core.BoundedContexts.SchedulingBoundedContext.ScheduledJobAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.SchedulingBoundedContext.ScheduledJobAggregate;
using Lumina.Domain.SharedKernel.Common.Enums.Scheduling;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.Scheduling;

/// <summary>
/// Contains unit tests for the <see cref="ScheduledJobMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ScheduledJobMappingTests
{
    private readonly ScheduledJobFixture _scheduledJobFixture = new();

    [Fact]
    public void ToRepositoryEntity_WhenDomainEntityHasIntervalSchedule_ShouldMapToRepositoryEntity()
    {
        // Arrange
        Guid ownerUserId = Guid.NewGuid();
        Result<IntervalSchedule> intervalScheduleResult = IntervalSchedule.Create(30);
        ScheduledJob domainEntity = _scheduledJobFixture.Create(
            name: "Interval job",
            taskType: ScheduledTaskType.ScanMediaLibraries,
            schedule: intervalScheduleResult.Value,
            ownerUserId: ownerUserId);

        // Act
        ScheduledJobEntity result = domainEntity.ToRepositoryEntity();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(domainEntity.Id.Value, result.Id);
        Assert.Equal(domainEntity.Name, result.Name);
        Assert.Equal(domainEntity.TaskType, result.TaskType);
        Assert.Equal(ScheduleType.WithIntervalInMinutes, result.ScheduleType);
        Assert.Equal(30, result.IntervalMinutes);
        Assert.Null(result.Hour);
        Assert.Null(result.Minute);
        Assert.Equal(domainEntity.Status, result.Status);
        Assert.Equal(ownerUserId, result.OwnerUserId);
        Assert.False(result.LastStartedOnUtc.HasValue);
        Assert.False(result.LastCompletedOnUtc.HasValue);
    }

    [Fact]
    public void ToRepositoryEntity_WhenDomainEntityHasDailySchedule_ShouldMapToRepositoryEntity()
    {
        // Arrange
        Result<DailySchedule> dailyScheduleResult = DailySchedule.Create(9, 45);
        ScheduledJob domainEntity = _scheduledJobFixture.Create(
            name: "Daily job",
            taskType: ScheduledTaskType.CleanTemporaryFiles,
            schedule: dailyScheduleResult.Value);

        // Act
        ScheduledJobEntity result = domainEntity.ToRepositoryEntity();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(ScheduleType.DailyAtHourAndMinute, result.ScheduleType);
        Assert.Null(result.IntervalMinutes);
        Assert.Equal(9, result.Hour);
        Assert.Equal(45, result.Minute);
    }

    [Fact]
    public void ToResponse_WhenDomainEntityHasIntervalSchedule_ShouldMapToResponse()
    {
        // Arrange
        Result<IntervalSchedule> intervalScheduleResult = IntervalSchedule.Create(120);
        ScheduledJob domainEntity = _scheduledJobFixture.Create(
            name: "Interval job",
            schedule: intervalScheduleResult.Value);

        // Act
        ScheduledJobResponse result = domainEntity.ToResponse();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(domainEntity.Id.Value, result.Id);
        Assert.Equal(domainEntity.Name, result.Name);
        Assert.Equal(ScheduleType.WithIntervalInMinutes, result.ScheduleType);
        Assert.Equal(120, result.IntervalMinutes);
        Assert.Null(result.Hour);
        Assert.Null(result.Minute);
        Assert.Equal(domainEntity.Status, result.Status);
    }

    [Fact]
    public void ToResponse_WhenDomainEntityHasDailySchedule_ShouldMapToResponse()
    {
        // Arrange
        Result<DailySchedule> dailyScheduleResult = DailySchedule.Create(5, 0);
        ScheduledJob domainEntity = _scheduledJobFixture.Create(
            name: "Daily job",
            schedule: dailyScheduleResult.Value);

        // Act
        ScheduledJobResponse result = domainEntity.ToResponse();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(ScheduleType.DailyAtHourAndMinute, result.ScheduleType);
        Assert.Null(result.IntervalMinutes);
        Assert.Equal(5, result.Hour);
        Assert.Equal(0, result.Minute);
    }
}
