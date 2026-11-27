#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Core.Playlists.Commands.DeletePlaylist;

/// <summary>
/// Command for deleting a playlist of the current user.
/// </summary>
/// <param name="PlaylistId">The unique identifier of the playlist, taken from the route.</param>
[DebuggerDisplay("PlaylistId: {PlaylistId}")]
public record DeletePlaylistCommand(
    string? PlaylistId
) : ICommand;
