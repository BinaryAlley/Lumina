#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Playlists;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Common.DTO.Playlists;

/// <summary>
/// Data transfer object for a stored playlist entry together with the display data of the media item it references and its dense display position.
/// </summary>
/// <param name="Entry">The stored entry.</param>
/// <param name="Display">The display data of the referenced media item, or <see langword="null"/> when it no longer exists.</param>
/// <param name="DisplayPosition">The dense zero based position shown to the client.</param>
[DebuggerDisplay("EntryId: {Entry.Id}; MediaItemId: {Entry.MediaItemId}; Position: {DisplayPosition}")]
public sealed record PlaylistEntryDto(
    PlaylistEntryEntity Entry,
    PlaylistEntryDisplayDto? Display,
    int DisplayPosition
);
