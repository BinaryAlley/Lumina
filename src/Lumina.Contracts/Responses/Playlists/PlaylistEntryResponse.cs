#region ========================================================================= USING =====================================================================================
using Lumina.Domain.SharedKernel.Common.Enums.Playlists;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Responses.Playlists;

/// <summary>
/// Represents a response for an ordered entry of a playlist.
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
[DebuggerDisplay("MediaItemId: {MediaItemId}; MediaItemType: {MediaItemType}; Position: {Position}")]
public sealed record PlaylistEntryResponse(
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
