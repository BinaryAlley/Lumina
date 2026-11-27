#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Enums.MediaConsumption;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.Requests.Library.Statistics;

/// <summary>
/// Represents a request for getting a window of the recently consumed media items of a user.
/// </summary>
/// <param name="UserId">The Id of the user whose history is retrieved. Optional; when omitted, the current user is used.</param>
/// <param name="LibraryId">The Id of the media library to restrict the history to. Optional.</param>
/// <param name="MediaKind">The kind of media to restrict the history to. Optional.</param>
/// <param name="FromUtc">The inclusive start of the date range. Optional.</param>
/// <param name="ToUtc">The inclusive end of the date range. Optional.</param>
/// <param name="CurrentPage">The one based page of media items to return. Optional.</param>
/// <param name="PerPage">The number of media items per page. Optional.</param>
[DebuggerDisplay("UserId: {UserId}; MediaKind: {MediaKind}; CurrentPage: {CurrentPage}")]
public sealed record GetRecentlyConsumedRequest(
    string? UserId,
    string? LibraryId,
    MediaKind? MediaKind,
    DateTime? FromUtc,
    DateTime? ToUtc,
    int? CurrentPage,
    int? PerPage
);
