#region ========================================================================= USING =====================================================================================
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Requests.MediaConsumption;

/// <summary>
/// Represents a request to delete every consumption record of a media item.
/// </summary>
/// <param name="MediaKind">The kind of the consumed media item.</param>
/// <param name="MediaItemId">The Id of the consumed media item.</param>
/// <param name="LibraryId">The Id of the media library that contains the media item.</param>
[DebuggerDisplay("MediaKind: {MediaKind}; MediaItemId: {MediaItemId}; LibraryId: {LibraryId}")]
public sealed record DeleteConsumptionItemHistoryRequest(
    MediaKind MediaKind,
    Guid MediaItemId,
    Guid LibraryId
);
