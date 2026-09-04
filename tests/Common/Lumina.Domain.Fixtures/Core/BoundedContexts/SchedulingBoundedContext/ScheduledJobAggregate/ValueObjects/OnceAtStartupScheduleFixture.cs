#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.BoundedContexts.SchedulingBoundedContext.ScheduledJobAggregate.ValueObjects;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Domain.Fixtures.Core.BoundedContexts.SchedulingBoundedContext.ScheduledJobAggregate.ValueObjects;

/// <summary>
/// Fixture class for the <see cref="OnceAtStartupSchedule"/> value object.
/// </summary>
[ExcludeFromCodeCoverage]
public class OnceAtStartupScheduleFixture
{
    /// <summary>
    /// Creates a valid <see cref="OnceAtStartupSchedule"/>.
    /// </summary>
    /// <returns>The created <see cref="OnceAtStartupSchedule"/>.</returns>
    public OnceAtStartupSchedule Create()
    {
        return OnceAtStartupSchedule.Create().Value;
    }

    /// <summary>
    /// Creates a list of <see cref="OnceAtStartupSchedule"/> instances.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="OnceAtStartupSchedule"/> instances.</returns>
    public List<OnceAtStartupSchedule> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
