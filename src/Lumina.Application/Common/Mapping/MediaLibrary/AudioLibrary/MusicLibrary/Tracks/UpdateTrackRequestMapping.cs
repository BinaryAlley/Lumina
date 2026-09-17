#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Commands.UpdateTrack;
using Lumina.Contracts.Requests.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;
#endregion

namespace Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;

/// <summary>
/// Extension methods for converting <see cref="UpdateTrackRequest"/>.
/// </summary>
public static class UpdateTrackRequestMapping
{
    /// <summary>
    /// Converts <paramref name="request"/> to <see cref="UpdateTrackCommand"/>.
    /// </summary>
    /// <param name="request">The request to be converted.</param>
    /// <param name="libraryId">The Id of the library the track belongs to, taken from the route.</param>
    /// <param name="artistId">The Id of the artist the track belongs to, taken from the route.</param>
    /// <param name="albumId">The Id of the album the track belongs to, taken from the route.</param>
    /// <param name="trackId">The Id of the track to update, taken from the route.</param>
    /// <returns>The converted command.</returns>
    public static UpdateTrackCommand ToCommand(this UpdateTrackRequest request, string? libraryId, string? artistId, string? albumId, string? trackId)
    {
        return new UpdateTrackCommand(
            libraryId,
            artistId,
            albumId,
            trackId,
            request.Path,
            request.Metadata,
            request.TrackNumber,
            request.DiscNumber,
            request.Script,
            request.Key,
            request.Bpm,
            request.Work,
            request.MusicBrainzRecordingId,
            request.MusicBrainzTrackId,
            request.MusicBrainzWorkId,
            request.Ratings,
            request.Moods,
            request.Isrcs);
    }
}
