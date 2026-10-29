#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Core.Playlists.Commands.RemovePlaylistEntry;

/// <summary>
/// Command for removing an entry from a playlist of the current user.
/// </summary>
/// <param name="PlaylistId">The unique identifier of the playlist, taken from the route.</param>
/// <param name="PlaylistEntryId">The unique identifier of the entry to remove, taken from the route.</param>
[DebuggerDisplay("PlaylistId: {PlaylistId}; PlaylistEntryId: {PlaylistEntryId}")]
public record RemovePlaylistEntryCommand(
    string? PlaylistId,
    string? PlaylistEntryId
) : ICommand;
