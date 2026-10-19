#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Requests.MediaLibrary.AudioLibrary.MusicLibrary.PlayHistory;

/// <summary>
/// Represents a request to report that a play of a track by the current user reached its end.
/// </summary>
/// <param name="PositionInSeconds">The last position reported by the player, in seconds. Required.</param>
/// <param name="ListenedDurationInSeconds">The duration the user listened to, in seconds. Required.</param>
[DebuggerDisplay("PositionInSeconds: {PositionInSeconds}; ListenedDurationInSeconds: {ListenedDurationInSeconds}")]
public sealed record CompletePlayRequest(
    double? PositionInSeconds,
    double? ListenedDurationInSeconds
);
