#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Enums.Playlists;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.DTO.Playlists;

/// <summary>
/// Data transfer object for an ordered playlist entry together with the display details of the media item it references.
/// </summary>
/// <param name="Id">The Id of the entry.</param>
/// <param name="MediaItemId">The Id of the media item referenced by the entry.</param>
/// <param name="MediaItemType">The kind of media item referenced by the entry.</param>
/// <param name="Position">The zero based position of the entry in the playlist.</param>
/// <param name="LibraryId">The Id of the media library that contains the media item, or an empty Guid when the item no longer exists.</param>
/// <param name="CollectionId">The Id of the collection that contains the media item, or an empty Guid when the item no longer exists.</param>
/// <param name="CreatorId">The Id of the creator of the media item, or an empty Guid when the item no longer exists.</param>
/// <param name="MediaItemTitle">The title of the media item, or an empty string when the item no longer exists.</param>
/// <param name="CollectionTitle">The title of the collection that contains the media item, or an empty string when the item no longer exists.</param>
/// <param name="CreatorName">The name of the creator of the media item, or an empty string when the item no longer exists.</param>
/// <param name="DurationInSeconds">The duration of the media item in seconds, or zero when the item no longer exists.</param>
/// <param name="CoverPath">The path of the cover art of the collection, if available.</param>
[DebuggerDisplay("Id: {Id}; MediaItemId: {MediaItemId}; Position: {Position}")]
public sealed record PlaylistEntryDto(
    Guid Id,
    Guid MediaItemId,
    PlaylistMediaItemType MediaItemType,
    int Position,
    Guid LibraryId,
    Guid CollectionId,
    Guid CreatorId,
    string MediaItemTitle,
    string CollectionTitle,
    string CreatorName,
    int DurationInSeconds,
    string? CoverPath
);
