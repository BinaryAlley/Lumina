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
/// Fixture class for the <see cref="IntervalSchedule"/> value object.
/// </summary>
[ExcludeFromCodeCoverage]
public class IntervalScheduleFixture
{
    /// <summary>
    /// Creates a random valid <see cref="IntervalSchedule"/>.
    /// </summary>
    /// <param name="intervalMinutes">Optional. The number of minutes between each execution.</param>
    /// <returns>The created <see cref="IntervalSchedule"/>.</returns>
    public IntervalSchedule Create(int? intervalMinutes = null)
    {
        Faker faker = new();
        return IntervalSchedule.Create(intervalMinutes ?? faker.Random.Int(1, 1440)).Value;
    }

    /// <summary>
    /// Creates a list of <see cref="IntervalSchedule"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="IntervalSchedule"/> instances.</returns>
    public List<IntervalSchedule> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
