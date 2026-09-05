#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Requests.Scheduling;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.Requests.Scheduling;

/// <summary>
/// Fixture class for generating <see cref="GetScheduledJobHistoryRequest"/> test data.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetScheduledJobHistoryRequestFixture
{
    /// <summary>
    /// Creates a new <see cref="GetScheduledJobHistoryRequest"/> instance.
    /// </summary>
    /// <param name="from">Optional inclusive lower bound of the interval for which the history is requested.</param>
    /// <param name="to">Optional inclusive upper bound of the interval for which the history is requested.</param>
    /// <returns>A configured <see cref="GetScheduledJobHistoryRequest"/> instance.</returns>
    public GetScheduledJobHistoryRequest Create(
        DateTime? from = null,
        DateTime? to = null)
    {
        return new GetScheduledJobHistoryRequest(
            From: from,
            To: to
        );
    }

    /// <summary>
    /// Creates multiple <see cref="GetScheduledJobHistoryRequest"/> instances.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="GetScheduledJobHistoryRequest"/> instances.</returns>
    public List<GetScheduledJobHistoryRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
