#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.DTO.AudioLibrary.MusicLibrary.PlayHistory;

/// <summary>
/// Data transfer object for the listening statistics of the current user.
/// </summary>
/// <param name="TotalListeningDurationInSeconds">The total duration the user listened to, over every recorded play, in seconds.</param>
/// <param name="TotalPlayCount">The number of recorded plays that count as plays. An in-progress play counts once it reaches the threshold, so this total can exceed the completed and stopped plays together.</param>
/// <param name="CompletedPlayCount">The number of recorded plays that completed.</param>
/// <param name="StoppedPlayCount">The number of recorded plays that stopped before the track completed and count as plays.</param>
/// <param name="CompletionRate">The fraction of the plays that count as plays that completed, between 0 and 1, or 0 when there is no counted play.</param>
[DebuggerDisplay("TotalPlayCount: {TotalPlayCount}; CompletedPlayCount: {CompletedPlayCount}")]
public sealed record MusicPlayStatisticsDto(
    double TotalListeningDurationInSeconds,
    int TotalPlayCount,
    int CompletedPlayCount,
    int StoppedPlayCount,
    double CompletionRate
);
