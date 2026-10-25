#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.DTO.Pagination;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Core.MediaConsumption.Queries.GetMostConsumed;

/// <summary>
/// Query for getting the most consumed media items of a user.
/// </summary>
/// <param name="TargetUserId">The Id of the user whose history is retrieved, or <see langword="null"/> for the current user.</param>
/// <param name="LibraryId">The Id of the media library to restrict the history to, or <see langword="null"/> for every library.</param>
/// <param name="MediaKind">The kind of media to restrict the history to, or <see langword="null"/> for every kind.</param>
/// <param name="FromUtc">The inclusive start of the date range, or <see langword="null"/> when the range has no start.</param>
/// <param name="ToUtc">The inclusive end of the date range, or <see langword="null"/> when the range has no end.</param>
/// <param name="PaginationData">The object containing the requested pagination data. When <see langword="null"/>, every matching item is returned; when present, its requested page is returned.</param>
[DebuggerDisplay("TargetUserId: {TargetUserId}; MediaKind: {MediaKind}")]
public record GetMostConsumedQuery(
    string? TargetUserId,
    string? LibraryId,
    MediaKind? MediaKind,
    DateTime? FromUtc,
    DateTime? ToUtc,
    PaginationDataDto? PaginationData
) : IQuery;
