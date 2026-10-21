#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Responses.Playlists;

/// <summary>
/// Represents a response for the result of removing an entry from a playlist.
/// </summary>
/// <param name="PlaylistId">The Id of the playlist the entry was removed from.</param>
/// <param name="RemovedEntryId">The Id of the removed entry.</param>
/// <param name="RepositionedCount">Always zero, because sparse order keys need no remaining entries to be repositioned.</param>
/// <param name="UpdatedOnUtc">The date and time when the playlist was last updated, if applicable.</param>
[DebuggerDisplay("PlaylistId: {PlaylistId}; RemovedEntryId: {RemovedEntryId}; RepositionedCount: {RepositionedCount}")]
public sealed record PlaylistEntryRemovedResponse(
    Guid PlaylistId,
    Guid RemovedEntryId,
    int RepositionedCount,
    DateTime? UpdatedOnUtc
);
