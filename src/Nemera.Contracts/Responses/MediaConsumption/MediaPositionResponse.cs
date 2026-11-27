#region ========================================================================= USING =====================================================================================
using Lumina.Domain.SharedKernel.Common.Enums.MediaConsumption;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Responses.MediaConsumption;

/// <summary>
/// Represents a position reached in a media item during a consumption session.
/// </summary>
/// <param name="Kind">The kind of representation used by the position.</param>
/// <param name="TimeOffsetSeconds">The position within the media item, in seconds, when the position is a time offset.</param>
/// <param name="Location">The opaque locator within the media item, when the position is not a time offset.</param>
[DebuggerDisplay("Kind: {Kind}; TimeOffsetSeconds: {TimeOffsetSeconds}")]
public sealed record MediaPositionResponse(
    MediaPositionKind Kind,
    double? TimeOffsetSeconds,
    string? Location
);
