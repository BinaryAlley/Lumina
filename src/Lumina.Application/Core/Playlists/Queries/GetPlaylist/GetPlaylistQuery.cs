#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.DTO.Pagination;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Core.Playlists.Queries.GetPlaylist;

/// <summary>
/// Query for getting a playlist of the current user, together with a page of its ordered entries.
/// </summary>
/// <param name="PlaylistId">The unique identifier of the playlist, taken from the route.</param>
/// <param name="PaginationData">The object containing the requested pagination data.</param>
[DebuggerDisplay("PlaylistId: {PlaylistId}")]
public record GetPlaylistQuery(
    string? PlaylistId,
    PaginationDataDto? PaginationData
) : IQuery;
