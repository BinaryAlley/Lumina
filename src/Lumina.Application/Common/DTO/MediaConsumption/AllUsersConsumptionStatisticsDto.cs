#region ========================================================================= USING =====================================================================================
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Common.DTO.MediaConsumption;

/// <summary>
/// Data transfer object for the consumption statistics of every user, together with their combined totals.
/// </summary>
/// <param name="Total">The combined consumption statistics of every user.</param>
/// <param name="Users">The consumption statistics of each user.</param>
[DebuggerDisplay("UserCount: {Users.Count}; CountedSessionCount: {Total.CountedSessionCount}")]
public sealed record AllUsersConsumptionStatisticsDto(
    ConsumptionTotalsDto Total,
    IReadOnlyList<ConsumptionStatisticsDto> Users
);
