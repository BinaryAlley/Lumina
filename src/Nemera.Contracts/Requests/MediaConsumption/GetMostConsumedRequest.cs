#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Requests.MediaConsumption;

/// <summary>
/// Represents a request for getting a window of the most consumed media items of a user.
/// </summary>
/// <param name="LibraryId">The Id of the media library to restrict the history to. Optional.</param>
/// <param name="MediaKind">The name of the kind of media to restrict the history to. Optional.</param>
/// <param name="CurrentPage">The one based page of media items to return. Optional.</param>
/// <param name="PerPage">The number of media items per page. Optional.</param>
/// <param name="FromUtc">The inclusive start of the date range, in UTC, as a text value. Optional.</param>
/// <param name="ToUtc">The inclusive end of the date range, in UTC, as a text value. Optional.</param>
[DebuggerDisplay("MediaKind: {MediaKind}; CurrentPage: {CurrentPage}; PerPage: {PerPage}")]
public sealed record GetMostConsumedRequest(
    string? LibraryId,
    string? MediaKind,
    int? CurrentPage,
    int? PerPage,
    string? FromUtc,
    string? ToUtc
);
