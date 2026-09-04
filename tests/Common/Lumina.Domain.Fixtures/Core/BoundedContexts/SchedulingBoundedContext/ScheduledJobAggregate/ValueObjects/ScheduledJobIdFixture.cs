#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.BoundedContexts.SchedulingBoundedContext.ScheduledJobAggregate.ValueObjects;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Domain.Fixtures.Core.BoundedContexts.SchedulingBoundedContext.ScheduledJobAggregate.ValueObjects;

/// <summary>
/// Fixture class for the <see cref="ScheduledJobId"/> value object.
/// </summary>
[ExcludeFromCodeCoverage]
public class ScheduledJobIdFixture
{
    /// <summary>
    /// Creates a random valid <see cref="ScheduledJobId"/>.
    /// </summary>
    /// <param name="value">Optional. The raw value of the scheduled job Id.</param>
    /// <returns>The created <see cref="ScheduledJobId"/>.</returns>
    public ScheduledJobId Create(
        Guid? value = null)
    {
        return ScheduledJobId.Create(value ?? Guid.NewGuid());
    }

    /// <summary>
    /// Creates a list of <see cref="ScheduledJobId"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="ScheduledJobId"/> instances.</returns>
    public List<ScheduledJobId> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
