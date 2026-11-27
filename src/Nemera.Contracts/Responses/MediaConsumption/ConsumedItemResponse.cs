#region ========================================================================= USING =====================================================================================
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Responses.MediaConsumption;

/// <summary>
/// Represents a media item of the consumption history, together with its aggregated consumption counts and display details.
/// </summary>
/// <param name="MediaKind">The kind of the consumed media item.</param>
/// <param name="MediaItemId">The Id of the consumed media item.</param>
/// <param name="LibraryId">The Id of the media library that contains the media item.</param>
/// <param name="IsAvailable">Whether the media item still exists and can be accessed by the user. When <see langword="false"/>, the title, collection title, creator name, and cover path are blank, while the identity and aggregation fields remain.</param>
/// <param name="Title">The title of the media item, or an empty string when the item is not available.</param>
/// <param name="CollectionTitle">The title of the collection that contains the media item, or an empty string when there is none.</param>
/// <param name="CreatorName">The name of the creator of the media item, or an empty string when there is none.</param>
/// <param name="CoverPath">The path of the cover art of the media item, if available.</param>
/// <param name="LastConsumedOnUtc">The date and time when the media item was last consumed.</param>
/// <param name="CountedSessionCount">The number of consumption sessions of the media item that count towards statistics.</param>
/// <param name="CompletedSessionCount">The number of consumption sessions of the media item that completed.</param>
/// <param name="ConsumedDurationInSeconds">The active duration consumed in the media item, over all of its sessions, in seconds.</param>
[DebuggerDisplay("MediaKind: {MediaKind}; MediaItemId: {MediaItemId}; CountedSessionCount: {CountedSessionCount}")]
public sealed record ConsumedItemResponse(
    MediaKind MediaKind,
    Guid MediaItemId,
    Guid LibraryId,
    bool IsAvailable,
    string Title,
    string CollectionTitle,
    string CreatorName,
    string? CoverPath,
    DateTime LastConsumedOnUtc,
    int CountedSessionCount,
    int CompletedSessionCount,
    double ConsumedDurationInSeconds
);
