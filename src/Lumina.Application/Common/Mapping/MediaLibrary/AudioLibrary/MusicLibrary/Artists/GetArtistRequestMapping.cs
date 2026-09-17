#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artists.Queries.GetArtist;
using Lumina.Contracts.Requests.MediaLibrary.AudioLibrary.MusicLibrary.Artists;
#endregion

namespace Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Artists;

/// <summary>
/// Extension methods for converting <see cref="GetArtistRequest"/>.
/// </summary>
public static class GetArtistRequestMapping
{
    /// <summary>
    /// Converts <paramref name="request"/> to <see cref="GetArtistQuery"/>.
    /// </summary>
    /// <param name="request">The request to be converted.</param>
    /// <returns>The converted query.</returns>
    public static GetArtistQuery ToQuery(this GetArtistRequest request)
    {
        return new GetArtistQuery(
            request.ArtistId);
    }
}
