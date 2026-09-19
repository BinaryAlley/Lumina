#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Commands.AddTrack;
using Lumina.Contracts.Requests.MediaLibrary.AudioLibrary.MusicLibrary.Artists;
using System.Collections.Generic;
using System.Linq;
#endregion

namespace Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Artists;

/// <summary>
/// Extension methods for converting <see cref="UpdateArtistTrackRequest"/>.
/// </summary>
public static class UpdateArtistTrackRequestMapping
{
    /// <summary>
    /// Converts <paramref name="request"/> to <see cref="AddTrackCommand"/>.
    /// </summary>
    /// <param name="request">The request to be converted.</param>
    /// <param name="libraryId">The Id of the library the track belongs to, taken from the route.</param>
    /// <param name="artistId">The Id of the artist the track belongs to, taken from the route.</param>
    /// <param name="albumId">The Id of the album the track belongs to, taken from the route.</param>
    /// <returns>The converted command.</returns>
    public static AddTrackCommand ToCommand(this UpdateArtistTrackRequest request, string? libraryId, string? artistId, string? albumId)
    {
        return new AddTrackCommand(
            libraryId,
            artistId,
            albumId,
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
            request.Moods,
            request.Isrcs,
            request.Contributors,
            request.Ratings,
            request.TrackId);
    }

    /// <summary>
    /// Converts <paramref name="requests"/> to a collection of <see cref="AddTrackCommand"/>.
    /// </summary>
    /// <param name="requests">The requests to be converted.</param>
    /// <param name="libraryId">The Id of the library the tracks belong to, taken from the route.</param>
    /// <param name="artistId">The Id of the artist the tracks belong to, taken from the route.</param>
    /// <param name="albumId">The Id of the album the tracks belong to, taken from the route.</param>
    /// <returns>The converted commands.</returns>
    public static IEnumerable<AddTrackCommand> ToCommands(this IEnumerable<UpdateArtistTrackRequest> requests, string? libraryId, string? artistId, string? albumId)
    {
        return requests.Select(request => request.ToCommand(libraryId, artistId, albumId));
    }
}
