#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Requests.MediaConsumption;

/// <summary>
/// Represents a request to report that a media consumption session stopped.
/// </summary>
/// <param name="Position">The final position reported for the media item, if the client knows it. Optional.</param>
/// <param name="ProgressFraction">The fraction of the media item that the client reached, between 0 and 1. Required.</param>
/// <param name="ConsumedDurationInSeconds">The active duration consumed by the session, in seconds. Required.</param>
/// <param name="ConsecutivePagesRead">The number of consecutive pages read in the session, used by page based media such as books.
/// It must be omitted or zero for other kinds; a positive value is rejected with a forbidden error. When omitted, the count stored by the last progress report is kept. Optional.</param>
[DebuggerDisplay("ProgressFraction: {ProgressFraction}; ConsumedDurationInSeconds: {ConsumedDurationInSeconds}; ConsecutivePagesRead: {ConsecutivePagesRead}")]
public sealed record StopConsumptionRequest(
    MediaPositionRequest? Position,
    double? ProgressFraction,
    double? ConsumedDurationInSeconds,
    int? ConsecutivePagesRead = null
);
