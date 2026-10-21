#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Playlists;
using System.Collections.Generic;
#endregion

namespace Lumina.Application.Common.DTO.Playlists;

/// <summary>
/// A stored playlist together with a window of its entries and their display data.
/// </summary>
/// <param name="Playlist">The stored playlist.</param>
/// <param name="Entries">The requested window of entries.</param>
public sealed record PlaylistDetailsDto(
    PlaylistEntity Playlist,
    IReadOnlyList<PlaylistEntryDto> Entries
);
