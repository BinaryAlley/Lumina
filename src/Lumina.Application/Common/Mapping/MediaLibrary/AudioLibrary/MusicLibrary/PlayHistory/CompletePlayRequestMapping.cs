#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.PlayHistory.Commands.CompletePlay;
using Lumina.Contracts.Requests.MediaLibrary.AudioLibrary.MusicLibrary.PlayHistory;
#endregion

namespace Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.PlayHistory;

/// <summary>
/// Extension methods for converting <see cref="CompletePlayRequest"/>.
/// </summary>
public static class CompletePlayRequestMapping
{
    /// <summary>
    /// Converts <paramref name="request"/> to <see cref="CompletePlayCommand"/>.
    /// </summary>
    /// <param name="request">The request to be converted.</param>
    /// <param name="playRecordId">The Id of the play record, taken from the route.</param>
    /// <returns>The converted command.</returns>
    public static CompletePlayCommand ToCommand(this CompletePlayRequest request, string? playRecordId)
    {
        return new CompletePlayCommand(
            playRecordId,
            request.PositionInSeconds,
            request.ListenedDurationInSeconds
        );
    }
}
