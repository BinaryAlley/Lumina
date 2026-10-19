#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.PlayHistory.Commands.StopPlay;
using Lumina.Contracts.Requests.MediaLibrary.AudioLibrary.MusicLibrary.PlayHistory;
#endregion

namespace Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.PlayHistory;

/// <summary>
/// Extension methods for converting <see cref="StopPlayRequest"/>.
/// </summary>
public static class StopPlayRequestMapping
{
    /// <summary>
    /// Converts <paramref name="request"/> to <see cref="StopPlayCommand"/>.
    /// </summary>
    /// <param name="request">The request to be converted.</param>
    /// <param name="playRecordId">The Id of the play record, taken from the route.</param>
    /// <returns>The converted command.</returns>
    public static StopPlayCommand ToCommand(this StopPlayRequest request, string? playRecordId)
    {
        return new StopPlayCommand(
            playRecordId,
            request.PositionInSeconds,
            request.ListenedDurationInSeconds
        );
    }
}
