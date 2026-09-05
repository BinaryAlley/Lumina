#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Requests.Scheduling;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.Requests.Scheduling;

/// <summary>
/// Fixture class for generating <see cref="RemoveScheduledJobRequest"/> test data.
/// </summary>
[ExcludeFromCodeCoverage]
public class RemoveScheduledJobRequestFixture
{
    /// <summary>
    /// Creates a new <see cref="RemoveScheduledJobRequest"/> instance with randomized test data.
    /// </summary>
    /// <param name="scheduledJobId">Optional unique identifier of the scheduled job to remove.</param>
    /// <returns>A configured <see cref="RemoveScheduledJobRequest"/> instance.</returns>
    public RemoveScheduledJobRequest Create(
        Guid? scheduledJobId = null)
    {
        return new RemoveScheduledJobRequest(
            ScheduledJobId: scheduledJobId ?? Guid.NewGuid()
        );
    }

    /// <summary>
    /// Creates multiple <see cref="RemoveScheduledJobRequest"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="RemoveScheduledJobRequest"/> instances.</returns>
    public List<RemoveScheduledJobRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
