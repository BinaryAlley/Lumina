#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using System;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Core.Playlists.Commands.AddPlaylistEntries;

/// <summary>
/// Command for adding several tracks to a playlist of the current user, in order.
/// </summary>
/// <param name="PlaylistId">The unique identifier of the playlist, taken from the route.</param>
/// <param name="TrackIds">The unique identifiers of the tracks to add, in the order they must take.</param>
[DebuggerDisplay("PlaylistId: {PlaylistId}; TrackCount: {TrackIds.Count}")]
public record AddPlaylistEntriesCommand(
    string? PlaylistId,
    List<Guid>? TrackIds
) : ICommand;
