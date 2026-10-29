#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.Playlists.Commands.MovePlaylistEntry;
using Lumina.Contracts.Requests.Playlists;
#endregion

namespace Lumina.Application.Common.Mapping.Playlists;

/// <summary>
/// Extension methods for converting <see cref="MovePlaylistEntryRequest"/>.
/// </summary>
public static class MovePlaylistEntryRequestMapping
{
    /// <summary>
    /// Converts <paramref name="request"/> to <see cref="MovePlaylistEntryCommand"/>.
    /// </summary>
    /// <param name="request">The request to be converted.</param>
    /// <param name="playlistId">The Id of the playlist, taken from the route.</param>
    /// <param name="playlistEntryId">The Id of the entry to move, taken from the route.</param>
    /// <returns>The converted command.</returns>
    public static MovePlaylistEntryCommand ToCommand(this MovePlaylistEntryRequest request, string? playlistId, string? playlistEntryId)
    {
        return new MovePlaylistEntryCommand(playlistId, playlistEntryId, request.TargetPosition);
    }
}
