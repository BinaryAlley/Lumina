#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.Playlists.Commands.UpdatePlaylist;
using Lumina.Contracts.Requests.Playlists;
#endregion

namespace Lumina.Application.Common.Mapping.Playlists;

/// <summary>
/// Extension methods for converting <see cref="UpdatePlaylistRequest"/>.
/// </summary>
public static class UpdatePlaylistRequestMapping
{
    /// <summary>
    /// Converts <paramref name="request"/> to <see cref="UpdatePlaylistCommand"/>.
    /// </summary>
    /// <param name="request">The request to be converted.</param>
    /// <param name="playlistId">The Id of the playlist, taken from the route.</param>
    /// <returns>The converted command.</returns>
    public static UpdatePlaylistCommand ToCommand(this UpdatePlaylistRequest request, string? playlistId)
    {
        return new UpdatePlaylistCommand(playlistId, request.Name, request.Description);
    }
}
