#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artists.Queries.GetArtistAlbumsLite;
using Lumina.Contracts.Requests.MediaLibrary.AudioLibrary.MusicLibrary.Artists;
#endregion

namespace Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Artists;

/// <summary>
/// Extension methods for converting <see cref="GetArtistAlbumsLiteRequest"/>.
/// </summary>
public static class GetArtistAlbumsLiteRequestMapping
{
    /// <summary>
    /// Converts <paramref name="request"/> to <see cref="GetArtistAlbumsLiteQuery"/>.
    /// </summary>
    /// <param name="request">The request to be converted.</param>
    /// <returns>The converted query.</returns>
    public static GetArtistAlbumsLiteQuery ToQuery(this GetArtistAlbumsLiteRequest request)
    {
        return new GetArtistAlbumsLiteQuery(
            request.ArtistId,
            request.CurrentPage,
            request.PerPage);
    }
}
