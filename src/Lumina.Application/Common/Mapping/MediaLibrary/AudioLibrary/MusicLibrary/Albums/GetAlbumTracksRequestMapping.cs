#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Albums.Queries.GetAlbumTracks;
using Lumina.Contracts.Requests.MediaLibrary.AudioLibrary.MusicLibrary.Albums;
#endregion

namespace Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Albums;

/// <summary>
/// Extension methods for converting <see cref="GetAlbumTracksRequest"/>.
/// </summary>
public static class GetAlbumTracksRequestMapping
{
    /// <summary>
    /// Converts <paramref name="request"/> to <see cref="GetAlbumTracksQuery"/>.
    /// </summary>
    /// <param name="request">The request to be converted.</param>
    /// <returns>The converted query.</returns>
    public static GetAlbumTracksQuery ToQuery(this GetAlbumTracksRequest request)
    {
        return new GetAlbumTracksQuery(
            request.AlbumId);
    }
}
