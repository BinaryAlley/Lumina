#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Core.MediaConsumption.Commands.ReportConsumptionProgress;

/// <summary>
/// Command for reporting the progress of a consumption session of the current user.
/// </summary>
/// <param name="ConsumptionRecordId">The unique identifier of the consumption record, taken from the route.</param>
/// <param name="PositionKind">The name of the kind of the position reached in the media item, if the client reported one.</param>
/// <param name="PositionTimeOffsetInSeconds">The position within the media item, in seconds, when the position is a time offset.</param>
/// <param name="PositionLocation">The opaque locator within the media item, when the position is not a time offset.</param>
/// <param name="ProgressFraction">The fraction of the media item that the client reached, between 0 and 1.</param>
/// <param name="ConsumedDurationInSeconds">The active duration consumed by the session, in seconds.</param>
/// <param name="ConsecutivePagesRead">The number of consecutive pages read in the current session, used by page based media such as books. It must be zero for other kinds. Defaults to 0. Optional.</param>
[DebuggerDisplay("ConsumptionRecordId: {ConsumptionRecordId}; ProgressFraction: {ProgressFraction}")]
public record ReportConsumptionProgressCommand(
    string? ConsumptionRecordId,
    string? PositionKind,
    double? PositionTimeOffsetInSeconds,
    string? PositionLocation,
    double? ProgressFraction,
    double? ConsumedDurationInSeconds,
    int ConsecutivePagesRead
) : ICommand;
