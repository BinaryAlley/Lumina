#region ========================================================================= USING =====================================================================================
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Requests.MediaConsumption;

/// <summary>
/// Represents a request for getting a window of the recently consumed media items of a user.
/// </summary>
/// <param name="LibraryId">The Id of the media library to restrict the history to. Optional.</param>
/// <param name="MediaKind">The kind of media to restrict the history to. Optional.</param>
/// <param name="CurrentPage">The one based page of media items to return. Optional.</param>
/// <param name="PerPage">The number of media items per page. Optional.</param>
/// <param name="FromUtc">The inclusive start of the date range. Optional.</param>
/// <param name="ToUtc">The inclusive end of the date range. Optional.</param>
[DebuggerDisplay("MediaKind: {MediaKind}; CurrentPage: {CurrentPage}; PerPage: {PerPage}")]
public sealed record GetRecentlyConsumedRequest(
    string? LibraryId,
    MediaKind? MediaKind,
    int? CurrentPage,
    int? PerPage,
    DateTime? FromUtc,
    DateTime? ToUtc
);
