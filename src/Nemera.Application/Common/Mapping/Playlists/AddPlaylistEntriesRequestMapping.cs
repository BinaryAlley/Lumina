#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.Playlists.Commands.AddPlaylistEntries;
using Lumina.Contracts.Requests.Playlists;
#endregion

namespace Lumina.Application.Common.Mapping.Playlists;

/// <summary>
/// Extension methods for converting <see cref="AddPlaylistEntriesRequest"/>.
/// </summary>
public static class AddPlaylistEntriesRequestMapping
{
    /// <summary>
    /// Converts <paramref name="request"/> to <see cref="AddPlaylistEntriesCommand"/>.
    /// </summary>
    /// <param name="request">The request to be converted.</param>
    /// <param name="playlistId">The Id of the playlist, taken from the route.</param>
    /// <returns>The converted command.</returns>
    public static AddPlaylistEntriesCommand ToCommand(this AddPlaylistEntriesRequest request, string? playlistId)
    {
        return new AddPlaylistEntriesCommand(playlistId, request.TrackIds);
    }
}
