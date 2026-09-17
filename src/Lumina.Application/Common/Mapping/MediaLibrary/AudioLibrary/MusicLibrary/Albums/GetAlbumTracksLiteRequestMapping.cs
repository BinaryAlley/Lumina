#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Albums.Queries.GetAlbumTracksLite;
using Lumina.Contracts.Requests.MediaLibrary.AudioLibrary.MusicLibrary.Albums;
#endregion

namespace Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Albums;

/// <summary>
/// Extension methods for converting <see cref="GetAlbumTracksLiteRequest"/>.
/// </summary>
public static class GetAlbumTracksLiteRequestMapping
{
    /// <summary>
    /// Converts <paramref name="request"/> to <see cref="GetAlbumTracksLiteQuery"/>.
    /// </summary>
    /// <param name="request">The request to be converted.</param>
    /// <returns>The converted query.</returns>
    public static GetAlbumTracksLiteQuery ToQuery(this GetAlbumTracksLiteRequest request)
    {
        return new GetAlbumTracksLiteQuery(
            request.AlbumId,
            request.CurrentPage,
            request.PerPage);
    }
}
