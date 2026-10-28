#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.DTO.Pagination;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Core.MediaConsumption.Queries.GetRecentlyConsumed;

/// <summary>
/// Query for getting the recently consumed media items of a user, from the most recently consumed.
/// </summary>
/// <param name="TargetUserId">The Id of the user whose history is retrieved, or <see langword="null"/> for the current user.</param>
/// <param name="LibraryId">The Id of the media library to restrict the history to, or <see langword="null"/> for every library.</param>
/// <param name="MediaKind">The name of the kind of media to restrict the history to, or <see langword="null"/> for every kind.</param>
/// <param name="FromUtc">The inclusive start of the date range, in UTC, or <see langword="null"/> when the range has no start.</param>
/// <param name="ToUtc">The inclusive end of the date range, in UTC, or <see langword="null"/> when the range has no end.</param>
/// <param name="PaginationData">The object containing the requested pagination data. When <see langword="null"/>, every matching item is returned; when present, its requested page is returned.</param>
[DebuggerDisplay("TargetUserId: {TargetUserId}; MediaKind: {MediaKind}")]
public record GetRecentlyConsumedQuery(
    string? TargetUserId,
    string? LibraryId,
    string? MediaKind,
    string? FromUtc,
    string? ToUtc,
    PaginationDataDto? PaginationData
) : IQuery;
