#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Core.Playlists.Commands.UpdatePlaylist;

/// <summary>
/// Command for updating the details of a playlist of the current user.
/// </summary>
/// <param name="PlaylistId">The unique identifier of the playlist, taken from the route.</param>
/// <param name="Name">The name of the playlist.</param>
/// <param name="Description">The optional description of the playlist.</param>
[DebuggerDisplay("PlaylistId: {PlaylistId}")]
public record UpdatePlaylistCommand(
    string? PlaylistId,
    string? Name,
    string? Description
) : ICommand;
