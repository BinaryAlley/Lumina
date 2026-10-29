#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Responses.Playlists;

/// <summary>
/// Represents a response for the entry that was appended to a playlist, together with its playlist and the new activity time of the playlist.
/// </summary>
/// <param name="PlaylistId">The Id of the playlist the entry was added to.</param>
/// <param name="Entry">The added entry, with the display details of the media item it references.</param>
/// <param name="UpdatedOnUtc">The date and time when the playlist was last updated, if applicable.</param>
[DebuggerDisplay("PlaylistId: {PlaylistId}; Entry: {Entry.Id}")]
public sealed record PlaylistEntryAddedResponse(
    Guid PlaylistId,
    PlaylistEntryResponse Entry,
    DateTime? UpdatedOnUtc
);
