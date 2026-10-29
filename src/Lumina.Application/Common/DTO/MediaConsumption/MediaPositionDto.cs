#region ========================================================================= USING =====================================================================================
using Lumina.Domain.SharedKernel.Common.Enums.MediaConsumption;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Common.DTO.MediaConsumption;

/// <summary>
/// Data transfer object for a position reached in a media item during a consumption session.
/// </summary>
/// <param name="Kind">The kind of representation used by the position.</param>
/// <param name="TimeOffsetMilliseconds">The position within the media item, in milliseconds, when the position is a time offset.</param>
/// <param name="Location">The opaque locator within the media item, when the position is not a time offset.</param>
[DebuggerDisplay("Kind: {Kind}; TimeOffsetMilliseconds: {TimeOffsetMilliseconds}")]
public sealed record MediaPositionDto(
    MediaPositionKind Kind,
    long? TimeOffsetMilliseconds,
    string? Location
);
