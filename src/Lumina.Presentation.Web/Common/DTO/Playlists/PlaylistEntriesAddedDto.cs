#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.DTO.Playlists;

/// <summary>
/// Data transfer object for the result of appending several tracks to a playlist.
/// </summary>
/// <param name="PlaylistId">The Id of the playlist the entries were added to.</param>
/// <param name="AddedCount">The number of entries that were appended, including repeated tracks.</param>
/// <param name="UpdatedOnUtc">The date and time when the playlist was last updated, if applicable.</param>
[DebuggerDisplay("PlaylistId: {PlaylistId}; AddedCount: {AddedCount}")]
public sealed record PlaylistEntriesAddedDto(
    Guid PlaylistId,
    int AddedCount,
    DateTime? UpdatedOnUtc
);
