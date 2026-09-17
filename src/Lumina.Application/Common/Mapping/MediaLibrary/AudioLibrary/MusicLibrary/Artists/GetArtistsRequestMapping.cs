#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artists.Queries.GetArtists;
using Lumina.Contracts.Requests.MediaLibrary.AudioLibrary.MusicLibrary.Artists;
#endregion

namespace Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Artists;

/// <summary>
/// Extension methods for converting <see cref="GetArtistsRequest"/>.
/// </summary>
public static class GetArtistsRequestMapping
{
    /// <summary>
    /// Converts <paramref name="request"/> to <see cref="GetArtistsQuery"/>.
    /// </summary>
    /// <param name="request">The request to be converted.</param>
    /// <returns>The converted query.</returns>
    public static GetArtistsQuery ToQuery(this GetArtistsRequest request)
    {
        return new GetArtistsQuery(
            request.LibraryId,
            request.CurrentPage,
            request.PerPage,
            request.SearchTerm);
    }
}
