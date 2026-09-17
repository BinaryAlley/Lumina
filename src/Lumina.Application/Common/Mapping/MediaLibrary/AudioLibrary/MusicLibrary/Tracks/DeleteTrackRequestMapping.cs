#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Commands.DeleteTrack;
using Lumina.Contracts.Requests.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;
#endregion

namespace Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;

/// <summary>
/// Extension methods for converting <see cref="DeleteTrackRequest"/>.
/// </summary>
public static class DeleteTrackRequestMapping
{
    /// <summary>
    /// Converts <paramref name="request"/> to <see cref="DeleteTrackCommand"/>.
    /// </summary>
    /// <param name="request">The request to be converted.</param>
    /// <returns>The converted command.</returns>
    public static DeleteTrackCommand ToCommand(this DeleteTrackRequest request)
    {
        return new DeleteTrackCommand(
            request.TrackId);
    }
}
