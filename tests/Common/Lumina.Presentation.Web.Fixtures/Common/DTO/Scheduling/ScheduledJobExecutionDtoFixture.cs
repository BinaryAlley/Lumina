#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Presentation.Web.Common.DTO.Scheduling;
using Lumina.Presentation.Web.Common.Enums.Scheduling;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.DTO.Scheduling;

/// <summary>
/// Fixture class for generating <see cref="ScheduledJobExecutionDto"/> test data.
/// </summary>
[ExcludeFromCodeCoverage]
public class ScheduledJobExecutionDtoFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a new <see cref="ScheduledJobExecutionDto"/> instance with randomized test data.
    /// </summary>
    /// <param name="id">Optional unique identifier of the execution.</param>
    /// <param name="scheduledJobId">Optional unique identifier of the scheduled job whose task was executed.</param>
    /// <param name="taskType">Optional type of the task that was executed.</param>
    /// <param name="isCycleRun">Optional value indicating whether the execution was triggered by the execution cycle of the scheduled job.</param>
    /// <param name="startedOnUtc">Optional date and time when the execution started.</param>
    /// <param name="completedOnUtc">Optional date and time when the execution completed.</param>
    /// <returns>A configured <see cref="ScheduledJobExecutionDto"/> instance.</returns>
    public ScheduledJobExecutionDto Create(
        Guid? id = null,
        Guid? scheduledJobId = null,
        ScheduledTaskType? taskType = null,
        bool? isCycleRun = null,
        DateTime? startedOnUtc = null,
        DateTime? completedOnUtc = null)
    {
        DateTime resolvedStartedOnUtc = startedOnUtc ?? _faker.Date.Recent(10);
        return new ScheduledJobExecutionDto(
            Id: id ?? Guid.NewGuid(),
            ScheduledJobId: scheduledJobId ?? Guid.NewGuid(),
            TaskType: taskType ?? _faker.PickRandom<ScheduledTaskType>(),
            IsCycleRun: isCycleRun ?? _faker.Random.Bool(),
            StartedOnUtc: resolvedStartedOnUtc,
            CompletedOnUtc: completedOnUtc ?? _faker.Date.Soon(1, resolvedStartedOnUtc));
    }

    /// <summary>
    /// Creates multiple <see cref="ScheduledJobExecutionDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="ScheduledJobExecutionDto"/> instances.</returns>
    public List<ScheduledJobExecutionDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
