#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Enums.MediaConsumption;
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.DTO.MediaConsumption;

/// <summary>
/// Data transfer object for the consumption statistics of one kind of media.
/// </summary>
/// <param name="MediaKind">The kind of media the statistics belong to.</param>
/// <param name="TotalConsumedDurationInSeconds">The total duration consumed over every recorded session of the media kind, in seconds.</param>
/// <param name="CountedSessionCount">The number of recorded sessions of the media kind that count towards statistics.</param>
/// <param name="CompletedSessionCount">The number of recorded sessions of the media kind that completed.</param>
/// <param name="StoppedSessionCount">The number of recorded sessions of the media kind that stopped before they completed and count towards statistics.</param>
/// <param name="CompletionRate">The fraction of the counted sessions of the media kind that completed, between 0 and 1.</param>
[DebuggerDisplay("MediaKind: {MediaKind}; CountedSessionCount: {CountedSessionCount}")]
public sealed record MediaKindConsumptionSummaryDto(
    MediaKind MediaKind,
    double TotalConsumedDurationInSeconds,
    int CountedSessionCount,
    int CompletedSessionCount,
    int StoppedSessionCount,
    double CompletionRate
);
