#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.Requests.Scheduling;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.Requests.Scheduling;

/// <summary>
/// Fixture class for the <see cref="GetScheduledJobHistoryRequest"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetScheduledJobHistoryRequestFixture
{
    /// <summary>
    /// Creates a random valid request to get the history of the executions of the tasks of scheduled jobs.
    /// </summary>
    /// <param name="from">Optional. The inclusive lower bound of the requested interval.</param>
    /// <param name="to">Optional. The inclusive upper bound of the requested interval.</param>
    /// <returns>The created request.</returns>
    public GetScheduledJobHistoryRequest Create(
        DateTime? from = null, 
        DateTime? to = null)
    {
        return new Faker<GetScheduledJobHistoryRequest>()
            .CustomInstantiator(f =>
            {
                DateTime resolvedFrom = from ?? f.Date.Recent(30);
                DateTime resolvedTo = to ?? f.Date.Soon(1, resolvedFrom);
                return new GetScheduledJobHistoryRequest(resolvedFrom, resolvedTo);
            })
            .Generate();
    }

    /// <summary>
    /// Creates a list of <see cref="GetScheduledJobHistoryRequest"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<GetScheduledJobHistoryRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
