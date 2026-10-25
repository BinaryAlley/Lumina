#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DTO.Pagination;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.PlayHistory.Queries.GetRecentlyPlayed;
using Lumina.Contracts.Requests.MediaLibrary.AudioLibrary.MusicLibrary.PlayHistory;
#endregion

namespace Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.PlayHistory;

/// <summary>
/// Extension methods for converting <see cref="GetRecentlyPlayedRequest"/>.
/// </summary>
public static class GetRecentlyPlayedRequestMapping
{
    /// <summary>
    /// Converts <paramref name="request"/> to <see cref="GetRecentlyPlayedQuery"/>.
    /// </summary>
    /// <param name="request">The request to be converted.</param>
    /// <returns>The converted query.</returns>
    public static GetRecentlyPlayedQuery ToQuery(this GetRecentlyPlayedRequest request)
    {
        PaginationDataDto? paginationData = null;
        if (request.CurrentPage is not null || request.PerPage is not null)
            paginationData = new PaginationDataDto
            {
                CurrentPage = request.CurrentPage ?? 1,
                PerPage = request.PerPage ?? 50
            };

        return new GetRecentlyPlayedQuery(paginationData);
    }
}
