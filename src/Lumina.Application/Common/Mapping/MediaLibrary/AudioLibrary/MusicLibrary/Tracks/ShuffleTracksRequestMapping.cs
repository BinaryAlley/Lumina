#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Queries.ShuffleTracks;
using Lumina.Contracts.Requests.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;
#endregion

namespace Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;

/// <summary>
/// Extension methods for converting <see cref="ShuffleTracksRequest"/>.
/// </summary>
public static class ShuffleTracksRequestMapping
{
    /// <summary>
    /// Converts <paramref name="request"/> to <see cref="ShuffleTracksQuery"/>.
    /// </summary>
    /// <param name="request">The request to be converted.</param>
    /// <param name="libraryId">The Id of the library the tracks are sampled from, taken from the route.</param>
    /// <returns>The converted query.</returns>
    public static ShuffleTracksQuery ToQuery(this ShuffleTracksRequest request, string? libraryId)
    {
        return new ShuffleTracksQuery(
            libraryId,
            request.Count,
            request.ArtistId,
            request.AlbumId,
            request.ExcludedTrackIds);
    }
}
