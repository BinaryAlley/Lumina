#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Requests.MediaConsumption;

/// <summary>
/// Represents a request to report the progress of a media consumption session.
/// </summary>
/// <param name="Position">The last position reported for the media item, if the client knows it. Optional.</param>
/// <param name="ProgressFraction">The fraction of the media item that the client reached, between 0 and 1. Required.</param>
/// <param name="ConsumedDurationInSeconds">The active duration consumed by the session, in seconds. Required.</param>
/// <param name="ConsecutivePagesRead">The number of consecutive pages read in the current session, used by page based media such as books and ignored for other kinds. Defaults to 0. Optional.</param>
[DebuggerDisplay("ProgressFraction: {ProgressFraction}; ConsumedDurationInSeconds: {ConsumedDurationInSeconds}; ConsecutivePagesRead: {ConsecutivePagesRead}")]
public sealed record ReportConsumptionProgressRequest(
    MediaPositionRequest? Position,
    double? ProgressFraction,
    double? ConsumedDurationInSeconds,
    int ConsecutivePagesRead = 0
);
