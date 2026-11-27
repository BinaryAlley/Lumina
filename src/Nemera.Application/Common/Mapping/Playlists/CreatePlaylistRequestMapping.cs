#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.Playlists.Commands.CreatePlaylist;
using Lumina.Contracts.Requests.Playlists;
#endregion

namespace Lumina.Application.Common.Mapping.Playlists;

/// <summary>
/// Extension methods for converting <see cref="CreatePlaylistRequest"/>.
/// </summary>
public static class CreatePlaylistRequestMapping
{
    /// <summary>
    /// Converts <paramref name="request"/> to <see cref="CreatePlaylistCommand"/>.
    /// </summary>
    /// <param name="request">The request to be converted.</param>
    /// <returns>The converted command.</returns>
    public static CreatePlaylistCommand ToCommand(this CreatePlaylistRequest request)
    {
        return new CreatePlaylistCommand(request.Name, request.Description, request.Type);
    }
}
