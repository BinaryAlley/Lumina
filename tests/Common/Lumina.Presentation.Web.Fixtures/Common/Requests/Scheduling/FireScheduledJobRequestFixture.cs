#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Requests.Scheduling;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.Requests.Scheduling;

/// <summary>
/// Fixture class for generating <see cref="FireScheduledJobRequest"/> test data.
/// </summary>
[ExcludeFromCodeCoverage]
public class FireScheduledJobRequestFixture
{
    /// <summary>
    /// Creates a new <see cref="FireScheduledJobRequest"/> instance with randomized test data.
    /// </summary>
    /// <param name="scheduledJobId">Optional unique identifier of the scheduled job whose task is fired.</param>
    /// <returns>A configured <see cref="FireScheduledJobRequest"/> instance.</returns>
    public FireScheduledJobRequest Create(
        Guid? scheduledJobId = null)
    {
        return new FireScheduledJobRequest(
            ScheduledJobId: scheduledJobId ?? Guid.NewGuid()
        );
    }

    /// <summary>
    /// Creates multiple <see cref="FireScheduledJobRequest"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="FireScheduledJobRequest"/> instances.</returns>
    public List<FireScheduledJobRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
