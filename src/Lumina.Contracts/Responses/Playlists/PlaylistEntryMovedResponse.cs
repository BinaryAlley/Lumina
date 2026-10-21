#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Responses.Playlists;

/// <summary>
/// Represents a response for the result of moving one entry of a playlist to a new position.
/// </summary>
/// <param name="PlaylistId">The Id of the playlist whose entry was moved.</param>
/// <param name="EntryId">The Id of the moved entry.</param>
/// <param name="Position">The new zero based position of the entry.</param>
/// <param name="UpdatedOnUtc">The date and time when the playlist was last updated, if applicable.</param>
[DebuggerDisplay("PlaylistId: {PlaylistId}; EntryId: {EntryId}; Position: {Position}")]
public sealed record PlaylistEntryMovedResponse(
    Guid PlaylistId,
    Guid EntryId,
    int Position,
    DateTime? UpdatedOnUtc
);
