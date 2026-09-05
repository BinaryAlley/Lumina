#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Requests.Scheduling;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.Requests.Scheduling;

/// <summary>
/// Fixture class for generating <see cref="StartScheduledJobRequest"/> test data.
/// </summary>
[ExcludeFromCodeCoverage]
public class StartScheduledJobRequestFixture
{
    /// <summary>
    /// Creates a new <see cref="StartScheduledJobRequest"/> instance with randomized test data.
    /// </summary>
    /// <param name="scheduledJobId">Optional unique identifier of the scheduled job whose execution cycle is started.</param>
    /// <returns>A configured <see cref="StartScheduledJobRequest"/> instance.</returns>
    public StartScheduledJobRequest Create(
        Guid? scheduledJobId = null)
    {
        return new StartScheduledJobRequest(
            ScheduledJobId: scheduledJobId ?? Guid.NewGuid()
        );
    }

    /// <summary>
    /// Creates multiple <see cref="StartScheduledJobRequest"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="StartScheduledJobRequest"/> instances.</returns>
    public List<StartScheduledJobRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
