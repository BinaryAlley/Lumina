#region ========================================================================= USING =====================================================================================
using Lumina.Domain.SharedKernel.Common.Enums.MediaConsumption;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Common.DTO.MediaConsumption;

/// <summary>
/// Data transfer object for a media consumption session.
/// </summary>
/// <param name="Id">The Id of the consumption record.</param>
/// <param name="LibraryId">The Id of the media library that contains the consumed media item.</param>
/// <param name="MediaItemId">The Id of the consumed media item.</param>
/// <param name="MediaKind">The kind of media that was consumed.</param>
/// <param name="Status">The state of the consumption session.</param>
/// <param name="StartedOnUtc">The date and time when the session started.</param>
/// <param name="LastActivityOnUtc">The date and time when the session last received activity.</param>
/// <param name="EndedOnUtc">The date and time when the session completed or stopped, if it has ended.</param>
/// <param name="ConsumedDurationMilliseconds">The active duration consumed by the session, in milliseconds.</param>
/// <param name="LastPosition">The last known position for the media item, if any.</param>
/// <param name="LastProgressFraction">The latest reported progress fraction of the media item.</param>
/// <param name="ConsecutivePagesRead">The number of consecutive pages read in the session.</param>
[DebuggerDisplay("Id: {Id}; MediaKind: {MediaKind}; Status: {Status}")]
public sealed record ConsumptionRecordDto(
    Guid Id,
    Guid LibraryId,
    Guid MediaItemId,
    MediaKind MediaKind,
    ConsumptionStatus Status,
    DateTime StartedOnUtc,
    DateTime LastActivityOnUtc,
    DateTime? EndedOnUtc,
    long ConsumedDurationMilliseconds,
    MediaPositionDto? LastPosition,
    double LastProgressFraction,
    int ConsecutivePagesRead
);
