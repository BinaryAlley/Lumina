#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Core.Scheduling.Queries.GetScheduledJobHistory;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Core.Scheduling.Queries.GetScheduledJobHistory;

/// <summary>
/// Fixture class for the <see cref="GetScheduledJobHistoryQuery"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetScheduledJobHistoryQueryFixture
{
    /// <summary>
    /// Creates a random valid query to get the history of the executions of the tasks of scheduled jobs.
    /// </summary>
    /// <param name="from">Optional. The inclusive lower bound of the requested interval.</param>
    /// <param name="to">Optional. The inclusive upper bound of the requested interval.</param>
    /// <param name="includeFrom">Whether to include the lower bound in the query. When false, the lower bound is left unset.</param>
    /// <param name="includeTo">Whether to include the upper bound in the query. When false, the upper bound is left unset.</param>
    /// <returns>The created query.</returns>
    public GetScheduledJobHistoryQuery Create(
        DateTime? from = null,
        DateTime? to = null,
        bool includeFrom = true,
        bool includeTo = true)
    {
        return new Faker<GetScheduledJobHistoryQuery>()
            .CustomInstantiator(f =>
            {
                DateTime generatedFrom = f.Date.Recent(30);
                DateTime? resolvedFrom = includeFrom ? (from ?? generatedFrom) : null;
                DateTime resolvedToBase = resolvedFrom ?? generatedFrom;
                DateTime? resolvedTo = includeTo ? (to ?? f.Date.Soon(1, resolvedToBase)) : null;
                return new GetScheduledJobHistoryQuery(resolvedFrom, resolvedTo);
            })
            .Generate();
    }

    /// <summary>
    /// Creates a list of <see cref="GetScheduledJobHistoryQuery"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<GetScheduledJobHistoryQuery> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
