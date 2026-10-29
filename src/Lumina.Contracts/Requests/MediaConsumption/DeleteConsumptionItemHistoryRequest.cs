#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Requests.MediaConsumption;

/// <summary>
/// Represents a request to delete every consumption record of a media item.
/// </summary>
/// <param name="MediaKind">The name of the kind of the consumed media item, taken from the route.</param>
/// <param name="MediaItemId">The Id of the consumed media item, taken from the route.</param>
/// <param name="LibraryId">The Id of the media library that contains the media item, taken from the route.</param>
[DebuggerDisplay("MediaKind: {MediaKind}; MediaItemId: {MediaItemId}; LibraryId: {LibraryId}")]
public sealed record DeleteConsumptionItemHistoryRequest(
    string? MediaKind,
    string? MediaItemId,
    string? LibraryId
);
