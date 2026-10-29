#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DTO.Pagination;
using Lumina.Application.Core.Playlists.Queries.GetPlaylist;
using Lumina.Contracts.Requests.Playlists;
#endregion

namespace Lumina.Application.Common.Mapping.Playlists;

/// <summary>
/// Extension methods for converting <see cref="GetPlaylistRequest"/>.
/// </summary>
public static class GetPlaylistRequestMapping
{
    /// <summary>
    /// Converts <paramref name="request"/> to <see cref="GetPlaylistQuery"/>.
    /// </summary>
    /// <param name="request">The request to be converted.</param>
    /// <param name="playlistId">The Id of the playlist whose entries are retrieved, taken from the route.</param>
    /// <returns>The converted query.</returns>
    public static GetPlaylistQuery ToQuery(this GetPlaylistRequest request, string? playlistId)
    {
        PaginationDataDto? paginationData = null;
        if (request.CurrentPage is not null || request.PerPage is not null)
            paginationData = new PaginationDataDto
            {
                CurrentPage = request.CurrentPage ?? 1,
                PerPage = request.PerPage ?? 200
            };

        return new GetPlaylistQuery(
            playlistId,
            paginationData);
    }
}
