#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.Requests.MediaConsumption;

/// <summary>
/// Represents a request to report a position in a media consumption session.
/// </summary>
/// <param name="Kind">The kind of the position. Required.</param>
/// <param name="TimeOffsetSeconds">The position within the media item, in seconds, when the position is a time offset. Optional.</param>
/// <param name="Location">The opaque locator within the media item, when the position is not a time offset. Optional.</param>
[DebuggerDisplay("Kind: {Kind}; TimeOffsetSeconds: {TimeOffsetSeconds}")]
public sealed record MediaPositionRequest(
    string? Kind,
    double? TimeOffsetSeconds,
    string? Location
);
