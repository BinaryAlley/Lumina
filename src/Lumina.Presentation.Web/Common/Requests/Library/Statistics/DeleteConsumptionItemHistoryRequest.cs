#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Enums.MediaConsumption;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.Requests.Library.Statistics;

/// <summary>
/// Represents a request to delete every consumption record of a media item.
/// </summary>
/// <param name="UserId">The Id of the user whose history is deleted. Optional; when omitted, the current user is used.</param>
/// <param name="MediaKind">The kind of the consumed media item.</param>
/// <param name="MediaItemId">The Id of the consumed media item.</param>
/// <param name="LibraryId">The Id of the media library that contains the media item.</param>
[DebuggerDisplay("UserId: {UserId}; MediaKind: {MediaKind}; MediaItemId: {MediaItemId}")]
public sealed record DeleteConsumptionItemHistoryRequest(
    string? UserId,
    MediaKind MediaKind,
    Guid MediaItemId,
    Guid LibraryId
);
