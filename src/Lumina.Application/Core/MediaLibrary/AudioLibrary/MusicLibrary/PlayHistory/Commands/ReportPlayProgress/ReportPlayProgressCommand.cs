#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.PlayHistory.Commands.ReportPlayProgress;

/// <summary>
/// Command for reporting the progress of a play of a track by the current user.
/// </summary>
/// <param name="PlayRecordId">The Id of the play record, taken from the route.</param>
/// <param name="PositionInSeconds">The last position reported by the player, in seconds.</param>
/// <param name="ListenedDurationInSeconds">The duration the user has listened to, in seconds.</param>
[DebuggerDisplay("PlayRecordId: {PlayRecordId}; PositionInSeconds: {PositionInSeconds}")]
public record ReportPlayProgressCommand(
    string? PlayRecordId,
    double? PositionInSeconds,
    double? ListenedDurationInSeconds
) : ICommand;
