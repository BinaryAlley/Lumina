#region ========================================================================= USING =====================================================================================
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.DTO.MediaConsumption;

/// <summary>
/// Data transfer object for the combined consumption statistics of every user.
/// </summary>
/// <param name="TotalConsumedDurationInSeconds">The total duration consumed over every recorded session of every user, in seconds.</param>
/// <param name="CountedSessionCount">The number of recorded sessions of every user that count towards statistics.</param>
/// <param name="CompletedSessionCount">The number of recorded sessions of every user that completed.</param>
/// <param name="StoppedSessionCount">The number of recorded sessions of every user that stopped before they completed and count towards statistics.</param>
/// <param name="CompletionRate">The fraction of the counted sessions of every user that completed, between 0 and 1, or 0 when there is no counted session.</param>
/// <param name="MediaKindSummaries">The combined consumption statistics broken down by media kind.</param>
[DebuggerDisplay("CountedSessionCount: {CountedSessionCount}; CompletedSessionCount: {CompletedSessionCount}")]
public sealed record ConsumptionTotalsDto(
    double TotalConsumedDurationInSeconds,
    int CountedSessionCount,
    int CompletedSessionCount,
    int StoppedSessionCount,
    double CompletionRate,
    IReadOnlyList<MediaKindConsumptionSummaryDto> MediaKindSummaries
);
