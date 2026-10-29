#region ========================================================================= USING =====================================================================================
using System;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Common.DTO.MediaConsumption;

/// <summary>
/// Data transfer object for the consumption statistics of a user.
/// </summary>
/// <param name="UserId">The Id of the user the statistics belong to.</param>
/// <param name="TotalConsumedDurationMilliseconds">The total duration consumed over every recorded session, in milliseconds.</param>
/// <param name="CountedSessionCount">The number of recorded sessions that count towards statistics.</param>
/// <param name="CompletedSessionCount">The number of recorded sessions that completed.</param>
/// <param name="StoppedSessionCount">The number of recorded sessions that stopped before they completed and count towards statistics.</param>
/// <param name="CompletionRate">The fraction of the counted sessions that completed, between 0 and 1, or 0 when there is no counted session.</param>
/// <param name="MediaKindSummaries">The consumption statistics broken down by media kind.</param>
[DebuggerDisplay("UserId: {UserId}; CountedSessionCount: {CountedSessionCount}")]
public sealed record ConsumptionStatisticsDto(
    Guid UserId,
    long TotalConsumedDurationMilliseconds,
    int CountedSessionCount,
    int CompletedSessionCount,
    int StoppedSessionCount,
    double CompletionRate,
    IReadOnlyList<MediaKindConsumptionStatisticsDto> MediaKindSummaries
);
