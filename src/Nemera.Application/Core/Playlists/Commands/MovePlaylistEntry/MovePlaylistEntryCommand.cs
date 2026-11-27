#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Core.Playlists.Commands.MovePlaylistEntry;

/// <summary>
/// Command for moving one entry of a playlist of the current user to a new position.
/// </summary>
/// <param name="PlaylistId">The unique identifier of the playlist, taken from the route.</param>
/// <param name="PlaylistEntryId">The unique identifier of the entry to move, taken from the route.</param>
/// <param name="TargetPosition">The new zero based position of the entry.</param>
[DebuggerDisplay("PlaylistId: {PlaylistId}; PlaylistEntryId: {PlaylistEntryId}; TargetPosition: {TargetPosition}")]
public record MovePlaylistEntryCommand(
    string? PlaylistId,
    string? PlaylistEntryId,
    int? TargetPosition
) : ICommand;
