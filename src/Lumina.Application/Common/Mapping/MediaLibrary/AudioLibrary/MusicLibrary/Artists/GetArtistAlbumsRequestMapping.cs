#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artists.Queries.GetArtistAlbums;
using Lumina.Contracts.Requests.MediaLibrary.AudioLibrary.MusicLibrary.Artists;
#endregion

namespace Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Artists;

/// <summary>
/// Extension methods for converting <see cref="GetArtistAlbumsRequest"/>.
/// </summary>
public static class GetArtistAlbumsRequestMapping
{
    /// <summary>
    /// Converts <paramref name="request"/> to <see cref="GetArtistAlbumsQuery"/>.
    /// </summary>
    /// <param name="request">The request to be converted.</param>
    /// <returns>The converted query.</returns>
    public static GetArtistAlbumsQuery ToQuery(this GetArtistAlbumsRequest request)
    {
        return new GetArtistAlbumsQuery(
            request.ArtistId);
    }
}
