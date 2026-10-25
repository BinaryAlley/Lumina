#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DTO.MediaLibrary.AudioLibrary.MusicLibrary.PlayHistory;
using Lumina.Application.Common.Utilities;
using Lumina.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.PlayHistory;
#endregion

namespace Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.PlayHistory;

/// <summary>
/// Extension methods for converting <see cref="MusicPlayStatisticsDto"/>.
/// </summary>
public static class MusicPlayStatisticsDtoMapping
{
    /// <summary>
    /// Converts <paramref name="data"/> to <see cref="MusicPlayStatisticsResponse"/>.
    /// </summary>
    /// <param name="data">The listening statistics data to be converted.</param>
    /// <returns>The converted response.</returns>
    public static MusicPlayStatisticsResponse ToResponse(this MusicPlayStatisticsDto data)
    {
        return new MusicPlayStatisticsResponse(
            data.TotalListeningDurationMilliseconds.ToClampedTimeSpan().TotalSeconds,
            data.TotalPlayCount,
            data.CompletedPlayCount,
            data.StoppedPlayCount,
            data.CompletionRate
        );
    }
}
