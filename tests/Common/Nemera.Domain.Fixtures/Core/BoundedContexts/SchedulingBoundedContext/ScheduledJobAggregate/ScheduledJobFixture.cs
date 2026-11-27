#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.SchedulingBoundedContext.ExternalIdentifiers.UserManagementBoundedContext.UserAggregate;
using Lumina.Domain.Core.BoundedContexts.SchedulingBoundedContext.ScheduledJobAggregate;
using Lumina.Domain.Core.BoundedContexts.SchedulingBoundedContext.ScheduledJobAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.SchedulingBoundedContext.ScheduledJobAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.Scheduling;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Domain.Fixtures.Core.BoundedContexts.SchedulingBoundedContext.ScheduledJobAggregate;

/// <summary>
/// Fixture class for the <see cref="ScheduledJob"/> domain aggregate.
/// </summary>
[ExcludeFromCodeCoverage]
public class ScheduledJobFixture
{
    private readonly IntervalScheduleFixture _intervalScheduleFixture = new();

    /// <summary>
    /// Creates a random valid <see cref="ScheduledJob"/> domain aggregate.
    /// </summary>
    /// <param name="id">Optional. The Id of the scheduled job. When provided, the aggregate is created with pre-existing data.</param>
    /// <param name="name">Optional. The name of the scheduled job.</param>
    /// <param name="taskType">Optional. The type of the task executed by the scheduled job.</param>
    /// <param name="schedule">Optional. The schedule of the scheduled job. Defaults to an interval schedule of 60 minutes.</param>
    /// <param name="ownerUserId">Optional. The Id of the user that owns the scheduled job.</param>
    /// <param name="status">Optional. The status of the scheduled job. Only applied when <paramref name="id"/> is provided.</param>
    /// <param name="lastStartedOnUtc">Optional. The last start time of the scheduled job. Only applied when <paramref name="id"/> is provided.</param>
    /// <param name="lastCompletedOnUtc">Optional. The last completion time of the scheduled job. Only applied when <paramref name="id"/> is provided.</param>
    /// <returns>The created <see cref="ScheduledJob"/>.</returns>
    public ScheduledJob Create(
        Guid? id = null,
        string? name = null,
        ScheduledTaskType? taskType = null,
        Schedule? schedule = null,
        Guid? ownerUserId = null,
        ScheduledJobStatus? status = null,
        DateTime? lastStartedOnUtc = null,
        DateTime? lastCompletedOnUtc = null)
    {
        Faker faker = new();
        ScheduledJobStatus resolvedStatus = status ?? ScheduledJobStatus.Added;
        string resolvedName = name ?? faker.Commerce.ProductName();
        ScheduledTaskType resolvedTaskType = taskType ?? faker.PickRandom<ScheduledTaskType>();
        UserId resolvedOwnerUserId = UserId.Create(ownerUserId ?? Guid.NewGuid());
        Schedule resolvedSchedule = schedule ?? _intervalScheduleFixture.Create(faker.Random.Int(5, 1440));

        // When no pre-existing state is requested, create a fresh scheduled job with the default Added status.
        if (id is null && status is null)
        {
            Result<ScheduledJob> createResult = ScheduledJob.Create(resolvedName, resolvedTaskType, resolvedSchedule, resolvedOwnerUserId);
            return createResult.Value;
        }

        // When a pre-existing Id or an explicit status is requested, create the scheduled job with that pre-existing state.
        Result<ScheduledJob> createWithIdResult = ScheduledJob.Create(
            ScheduledJobId.Create(id ?? Guid.NewGuid()),
            resolvedName,
            resolvedTaskType,
            resolvedSchedule,
            resolvedOwnerUserId,
            resolvedStatus,
            Optional<DateTime>.FromNullable(lastStartedOnUtc),
            Optional<DateTime>.FromNullable(lastCompletedOnUtc));
        return createWithIdResult.Value;
    }

    /// <summary>
    /// Creates multiple <see cref="ScheduledJob"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="ScheduledJob"/> instances.</returns>
    public List<ScheduledJob> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
