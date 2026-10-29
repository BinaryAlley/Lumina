#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Playlists;
using Lumina.Application.Common.DTO.Pagination;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Common.DTO.Playlists;

/// <summary>
/// Data transfer object for a stored playlist together with a page of its entries and their display data.
/// </summary>
/// <param name="Playlist">The stored playlist.</param>
/// <param name="Entries">The requested page of entries.</param>
[DebuggerDisplay("PlaylistId: {Playlist.Id}; Name: {Playlist.Name}; Entries: {Entries.Count}")]
public sealed record PlaylistDetailsDto(
    PlaylistEntity Playlist,
    PaginatedResultDto<PlaylistEntryDto> Entries
);
