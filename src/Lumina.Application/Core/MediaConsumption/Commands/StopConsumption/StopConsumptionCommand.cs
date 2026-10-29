#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Core.MediaConsumption.Commands.StopConsumption;

/// <summary>
/// Command for reporting that a consumption session of the current user stopped before it completed.
/// </summary>
/// <param name="ConsumptionRecordId">The unique identifier of the consumption record, taken from the route.</param>
/// <param name="PositionKind">The name of the kind of the final position reached in the media item, if the client reported one.</param>
/// <param name="PositionTimeOffsetInSeconds">The final position within the media item, in seconds, when the position is a time offset.</param>
/// <param name="PositionLocation">The final opaque locator within the media item, when the position is not a time offset.</param>
/// <param name="ProgressFraction">The final fraction of the media item that the client reached, between 0 and 1.</param>
/// <param name="ConsumedDurationInSeconds">The active duration consumed by the session, in seconds.</param>
/// <param name="ConsecutivePagesRead">The number of consecutive pages read in the session, used by page based media such as books. It must be absent or zero for other kinds. When it is absent, the count of the last progress report is kept.</param>
[DebuggerDisplay("ConsumptionRecordId: {ConsumptionRecordId}; ProgressFraction: {ProgressFraction}; ConsecutivePagesRead: {ConsecutivePagesRead}")]
public record StopConsumptionCommand(
    string? ConsumptionRecordId,
    string? PositionKind,
    double? PositionTimeOffsetInSeconds,
    string? PositionLocation,
    double? ProgressFraction,
    double? ConsumedDurationInSeconds,
    int? ConsecutivePagesRead
) : ICommand;
