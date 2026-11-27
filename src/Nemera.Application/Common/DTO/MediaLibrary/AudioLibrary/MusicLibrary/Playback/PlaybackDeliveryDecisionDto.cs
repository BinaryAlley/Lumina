#region ========================================================================= USING =====================================================================================
using Lumina.Domain.SharedKernel.Common.Enums.Playback;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Common.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Playback;

/// <summary>
/// Data transfer object for the decision of how the audio of a track is delivered to a player: as the original file, as the same codec repackaged, or re-encoded.
/// </summary>
/// <param name="Mode">The way the audio is delivered.</param>
/// <param name="Container">The container of the delivered audio, which is the stored container of the track for a direct play.</param>
/// <param name="Codec">The codec of the delivered audio, which is the stored codec of the track for a direct play.</param>
/// <param name="Shape">The shape the delivered audio is addressed and seeked by.</param>
/// <param name="SegmentDurationSeconds">The duration of each segment of the delivered audio, in seconds, or zero when the shape is not time addressed.</param>
[DebuggerDisplay("Mode: {Mode} Container: {Container} Codec: {Codec} Shape: {Shape}")]
public sealed record PlaybackDeliveryDecisionDto(
    PlaybackDeliveryMode Mode,
    string Container,
    string Codec,
    PlaybackDeliveryShape Shape,
    int SegmentDurationSeconds
);
