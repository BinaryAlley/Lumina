#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.Playlists.Commands.AddPlaylistScopeEntries;
using Lumina.Contracts.Requests.Playlists;
#endregion

namespace Lumina.Application.Common.Mapping.Playlists;

/// <summary>
/// Extension methods for converting <see cref="AddPlaylistScopeEntriesRequest"/>.
/// </summary>
public static class AddPlaylistScopeEntriesRequestMapping
{
    /// <summary>
    /// Converts <paramref name="request"/> to <see cref="AddPlaylistScopeEntriesCommand"/>.
    /// </summary>
    /// <param name="request">The request to be converted.</param>
    /// <param name="playlistId">The Id of the playlist, taken from the route.</param>
    /// <returns>The converted command.</returns>
    public static AddPlaylistScopeEntriesCommand ToCommand(this AddPlaylistScopeEntriesRequest request, string? playlistId)
    {
        return new AddPlaylistScopeEntriesCommand(playlistId, request.LibraryId, request.ArtistId, request.AlbumId);
    }
}
