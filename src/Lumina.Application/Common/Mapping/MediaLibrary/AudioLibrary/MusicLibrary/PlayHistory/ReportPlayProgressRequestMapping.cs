#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.PlayHistory.Commands.ReportPlayProgress;
using Lumina.Contracts.Requests.MediaLibrary.AudioLibrary.MusicLibrary.PlayHistory;
#endregion

namespace Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.PlayHistory;

/// <summary>
/// Extension methods for converting <see cref="ReportPlayProgressRequest"/>.
/// </summary>
public static class ReportPlayProgressRequestMapping
{
    /// <summary>
    /// Converts <paramref name="request"/> to <see cref="ReportPlayProgressCommand"/>.
    /// </summary>
    /// <param name="request">The request to be converted.</param>
    /// <param name="playRecordId">The Id of the play record, taken from the route.</param>
    /// <returns>The converted command.</returns>
    public static ReportPlayProgressCommand ToCommand(this ReportPlayProgressRequest request, string? playRecordId)
    {
        return new ReportPlayProgressCommand(
            playRecordId,
            request.PositionInSeconds,
            request.ListenedDurationInSeconds
        );
    }
}
