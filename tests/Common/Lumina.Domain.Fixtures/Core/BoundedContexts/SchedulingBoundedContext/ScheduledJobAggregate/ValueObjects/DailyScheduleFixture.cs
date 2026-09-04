#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Domain.Core.BoundedContexts.SchedulingBoundedContext.ScheduledJobAggregate.ValueObjects;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Domain.Fixtures.Core.BoundedContexts.SchedulingBoundedContext.ScheduledJobAggregate.ValueObjects;

/// <summary>
/// Fixture class for the <see cref="DailySchedule"/> value object.
/// </summary>
[ExcludeFromCodeCoverage]
public class DailyScheduleFixture
{
    /// <summary>
    /// Creates a random valid <see cref="DailySchedule"/>.
    /// </summary>
    /// <param name="hour">Optional. The hour of the day at which the task runs.</param>
    /// <param name="minute">Optional. The minute of the hour at which the task runs.</param>
    /// <returns>The created <see cref="DailySchedule"/>.</returns>
    public DailySchedule Create(int? hour = null, int? minute = null)
    {
        Faker faker = new();
        return DailySchedule.Create(
            hour ?? faker.Random.Int(0, 23),
            minute ?? faker.Random.Int(0, 59)).Value;
    }

    /// <summary>
    /// Creates a list of <see cref="DailySchedule"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="DailySchedule"/> instances.</returns>
    public List<DailySchedule> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
