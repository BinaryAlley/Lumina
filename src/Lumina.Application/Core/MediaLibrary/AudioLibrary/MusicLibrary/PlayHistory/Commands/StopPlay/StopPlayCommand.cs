#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.PlayHistory.Commands.StopPlay;

/// <summary>
/// Command for reporting that a play of a track by the current user stopped before the track completed.
/// </summary>
/// <param name="PlayRecordId">The Id of the play record, taken from the route.</param>
/// <param name="PositionInSeconds">The last position reported by the player, in seconds.</param>
/// <param name="ListenedDurationInSeconds">The duration the user listened to, in seconds.</param>
[DebuggerDisplay("PlayRecordId: {PlayRecordId}; PositionInSeconds: {PositionInSeconds}")]
public record StopPlayCommand(
    string? PlayRecordId,
    double? PositionInSeconds,
    double? ListenedDurationInSeconds
) : ICommand;
