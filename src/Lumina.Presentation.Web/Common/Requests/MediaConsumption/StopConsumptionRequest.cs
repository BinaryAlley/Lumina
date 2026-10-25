#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.Requests.MediaConsumption;

/// <summary>
/// Represents a request to report that a media consumption session stopped.
/// </summary>
/// <param name="Position">The final position reported for the media item, if the client knows it. Optional.</param>
/// <param name="ProgressFraction">The fraction of the media item that the client reached, between 0 and 1. Required.</param>
/// <param name="ConsumedDurationInSeconds">The active duration consumed by the session, in seconds. Required.</param>
[DebuggerDisplay("ProgressFraction: {ProgressFraction}; ConsumedDurationInSeconds: {ConsumedDurationInSeconds}")]
public sealed record StopConsumptionRequest(
    MediaPositionRequest? Position,
    double? ProgressFraction,
    double? ConsumedDurationInSeconds
);
