#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Requests.Scheduling;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.Requests.Scheduling;

/// <summary>
/// Fixture class for generating <see cref="StopScheduledJobRequest"/> test data.
/// </summary>
[ExcludeFromCodeCoverage]
public class StopScheduledJobRequestFixture
{
    /// <summary>
    /// Creates a new <see cref="StopScheduledJobRequest"/> instance with randomized test data.
    /// </summary>
    /// <param name="scheduledJobId">Optional unique identifier of the scheduled job whose execution cycle is stopped.</param>
    /// <returns>A configured <see cref="StopScheduledJobRequest"/> instance.</returns>
    public StopScheduledJobRequest Create(
        Guid? scheduledJobId = null)
    {
        return new StopScheduledJobRequest(
            ScheduledJobId: scheduledJobId ?? Guid.NewGuid()
        );
    }

    /// <summary>
    /// Creates multiple <see cref="StopScheduledJobRequest"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="StopScheduledJobRequest"/> instances.</returns>
    public List<StopScheduledJobRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
