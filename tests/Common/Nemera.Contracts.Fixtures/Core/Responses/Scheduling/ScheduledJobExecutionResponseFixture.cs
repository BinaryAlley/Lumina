#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.Responses.Scheduling;
using Lumina.Domain.SharedKernel.Common.Enums.Scheduling;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.Responses.Scheduling;

/// <summary>
/// Fixture class for the <see cref="ScheduledJobExecutionResponse"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ScheduledJobExecutionResponseFixture
{
    /// <summary>
    /// Creates a random valid <see cref="ScheduledJobExecutionResponse"/>.
    /// </summary>
    /// <param name="id">Optional. The Id of the execution.</param>
    /// <param name="scheduledJobId">Optional. The Id of the scheduled job whose task was executed.</param>
    /// <param name="taskType">Optional. The type of the task that was executed.</param>
    /// <param name="isCycleRun">Optional. Whether the execution was a cycle run.</param>
    /// <param name="startedOnUtc">Optional. The start time of the execution.</param>
    /// <param name="completedOnUtc">Optional. The completion time of the execution.</param>
    /// <returns>The created <see cref="ScheduledJobExecutionResponse"/>.</returns>
    public ScheduledJobExecutionResponse Create(
        Guid? id = null,
        Guid? scheduledJobId = null,
        ScheduledTaskType? taskType = null,
        bool? isCycleRun = null,
        DateTime? startedOnUtc = null,
        DateTime? completedOnUtc = null)
    {
        return new Faker<ScheduledJobExecutionResponse>()
            .CustomInstantiator(f => new ScheduledJobExecutionResponse(
                id ?? Guid.NewGuid(),
                scheduledJobId ?? Guid.NewGuid(),
                taskType ?? f.PickRandom<ScheduledTaskType>(),
                isCycleRun ?? f.Random.Bool(),
                startedOnUtc ?? DateTime.UtcNow,
                completedOnUtc))
            .Generate();
    }

    /// <summary>
    /// Creates a list of <see cref="ScheduledJobExecutionResponse"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="ScheduledJobExecutionResponse"/> instances.</returns>
    public List<ScheduledJobExecutionResponse> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
