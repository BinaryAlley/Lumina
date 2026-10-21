#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Core.Playlists.Queries.GetPlaylist;

/// <summary>
/// Query for getting a playlist of the current user, together with a window of its ordered entries.
/// </summary>
/// <param name="PlaylistId">The unique identifier of the playlist, taken from the route.</param>
/// <param name="Offset">The zero based position of the first entry to return.</param>
/// <param name="Limit">The maximum number of entries to return in this window.</param>
[DebuggerDisplay("PlaylistId: {PlaylistId}; Offset: {Offset}; Limit: {Limit}")]
public record GetPlaylistQuery(
    string? PlaylistId,
    int Offset = 0,
    int Limit = PlaylistWindow.DEFAULT_ENTRY_WINDOW_SIZE
) : IQuery;
