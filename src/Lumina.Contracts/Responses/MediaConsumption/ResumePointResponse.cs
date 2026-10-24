#region ========================================================================= USING =====================================================================================
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Responses.MediaConsumption;

/// <summary>
/// Represents the point at which a user can resume consuming a media item.
/// </summary>
/// <param name="Id">The Id of the resume point.</param>
/// <param name="LibraryId">The Id of the media library that contains the media item.</param>
/// <param name="MediaItemId">The Id of the media item to resume.</param>
/// <param name="MediaKind">The kind of media that is resumed.</param>
/// <param name="LastPosition">The position at which the media item can be resumed.</param>
/// <param name="LastProgressFraction">The progress fraction at which the media item can be resumed.</param>
/// <param name="LastConsumedOnUtc">The date and time when the media item was last consumed.</param>
[DebuggerDisplay("Id: {Id}; MediaKind: {MediaKind}; MediaItemId: {MediaItemId}")]
public sealed record ResumePointResponse(
    Guid Id,
    Guid LibraryId,
    Guid MediaItemId,
    MediaKind MediaKind,
    MediaPositionResponse LastPosition,
    double LastProgressFraction,
    DateTime LastConsumedOnUtc
);
