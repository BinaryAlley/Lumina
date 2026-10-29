#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.Playlists.Commands.AddPlaylistEntry;
using Lumina.Contracts.Requests.Playlists;
#endregion

namespace Lumina.Application.Common.Mapping.Playlists;

/// <summary>
/// Extension methods for converting <see cref="AddPlaylistEntryRequest"/>.
/// </summary>
public static class AddPlaylistEntryRequestMapping
{
    /// <summary>
    /// Converts <paramref name="request"/> to <see cref="AddPlaylistEntryCommand"/>.
    /// </summary>
    /// <param name="request">The request to be converted.</param>
    /// <param name="playlistId">The Id of the playlist, taken from the route.</param>
    /// <returns>The converted command.</returns>
    public static AddPlaylistEntryCommand ToCommand(this AddPlaylistEntryRequest request, string? playlistId)
    {
        return new AddPlaylistEntryCommand(playlistId, request.TrackId);
    }
}
