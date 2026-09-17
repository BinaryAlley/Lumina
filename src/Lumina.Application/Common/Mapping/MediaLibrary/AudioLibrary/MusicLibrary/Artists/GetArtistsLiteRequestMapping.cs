#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artists.Queries.GetArtistsLite;
using Lumina.Contracts.Requests.MediaLibrary.AudioLibrary.MusicLibrary.Artists;
#endregion

namespace Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Artists;

/// <summary>
/// Extension methods for converting <see cref="GetArtistsLiteRequest"/>.
/// </summary>
public static class GetArtistsLiteRequestMapping
{
    /// <summary>
    /// Converts <paramref name="request"/> to <see cref="GetArtistsLiteQuery"/>.
    /// </summary>
    /// <param name="request">The request to be converted.</param>
    /// <returns>The converted query.</returns>
    public static GetArtistsLiteQuery ToQuery(this GetArtistsLiteRequest request)
    {
        return new GetArtistsLiteQuery(
            request.LibraryId,
            request.CurrentPage,
            request.PerPage,
            request.SearchTerm);
    }
}
