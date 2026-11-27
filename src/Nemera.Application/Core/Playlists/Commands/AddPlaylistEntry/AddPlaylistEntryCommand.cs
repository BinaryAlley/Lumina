#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Core.Playlists.Commands.AddPlaylistEntry;

/// <summary>
/// Command for adding a track to a playlist of the current user.
/// </summary>
/// <param name="PlaylistId">The unique identifier of the playlist, taken from the route.</param>
/// <param name="TrackId">The unique identifier of the track to add.</param>
[DebuggerDisplay("PlaylistId: {PlaylistId}; TrackId: {TrackId}")]
public record AddPlaylistEntryCommand(
    string? PlaylistId,
    Guid? TrackId
) : ICommand;
