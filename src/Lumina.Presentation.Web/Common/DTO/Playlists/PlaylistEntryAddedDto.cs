#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.DTO.Playlists;

/// <summary>
/// Data transfer object for an entry appended to a playlist.
/// </summary>
/// <param name="PlaylistId">The Id of the playlist the entry was added to.</param>
/// <param name="Entry">The added entry.</param>
/// <param name="UpdatedOnUtc">The date and time when the playlist was last updated, if applicable.</param>
[DebuggerDisplay("PlaylistId: {PlaylistId}; Entry: {Entry.Id}")]
public sealed record PlaylistEntryAddedDto(
    Guid PlaylistId,
    PlaylistEntryDto Entry,
    DateTime? UpdatedOnUtc
);
