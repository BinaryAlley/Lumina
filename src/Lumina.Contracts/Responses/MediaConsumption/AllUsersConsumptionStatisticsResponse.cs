#region ========================================================================= USING =====================================================================================
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Responses.MediaConsumption;

/// <summary>
/// Represents the consumption statistics of every user, together with their combined totals.
/// </summary>
/// <param name="Total">The combined consumption statistics of every user.</param>
/// <param name="Users">The consumption statistics of each user.</param>
[DebuggerDisplay("UserCount: {Users.Count}; CountedSessionCount: {Total.CountedSessionCount}")]
public sealed record AllUsersConsumptionStatisticsResponse(
    ConsumptionTotalsResponse Total,
    IReadOnlyList<ConsumptionSummaryResponse> Users
);
