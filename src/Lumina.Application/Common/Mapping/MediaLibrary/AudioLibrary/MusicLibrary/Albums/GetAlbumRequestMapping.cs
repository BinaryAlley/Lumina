#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Albums.Queries.GetAlbum;
using Lumina.Contracts.Requests.MediaLibrary.AudioLibrary.MusicLibrary.Albums;
#endregion

namespace Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Albums;

/// <summary>
/// Extension methods for converting <see cref="GetAlbumRequest"/>.
/// </summary>
public static class GetAlbumRequestMapping
{
    /// <summary>
    /// Converts <paramref name="request"/> to <see cref="GetAlbumQuery"/>.
    /// </summary>
    /// <param name="request">The request to be converted.</param>
    /// <returns>The converted query.</returns>
    public static GetAlbumQuery ToQuery(this GetAlbumRequest request)
    {
        return new GetAlbumQuery(
            request.AlbumId);
    }
}
