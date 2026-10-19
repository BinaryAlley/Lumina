#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Requests.MediaLibrary.AudioLibrary.MusicLibrary.PlayHistory;

/// <summary>
/// Represents a request to report that a play of a track by the current user stopped before the track completed.
/// </summary>
/// <param name="PositionInSeconds">The last position reported by the player, in seconds. Required.</param>
/// <param name="ListenedDurationInSeconds">The duration the user listened to, in seconds. Required.</param>
[DebuggerDisplay("PositionInSeconds: {PositionInSeconds}; ListenedDurationInSeconds: {ListenedDurationInSeconds}")]
public sealed record StopPlayRequest(
    double? PositionInSeconds,
    double? ListenedDurationInSeconds
);
