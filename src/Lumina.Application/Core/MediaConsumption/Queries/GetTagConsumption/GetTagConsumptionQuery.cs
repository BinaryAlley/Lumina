#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Domain.SharedKernel.Common.Enums.MediaConsumption;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Core.MediaConsumption.Queries.GetTagConsumption;

/// <summary>
/// Query for getting the consumption statistics of the tags and genres of a user.
/// </summary>
/// <param name="TargetUserId">The Id of the user whose statistics are retrieved, or <see langword="null"/> for the current user.</param>
/// <param name="LibraryId">The Id of the media library to restrict the statistics to, or <see langword="null"/> for every library.</param>
/// <param name="MediaKind">The kind of media to restrict the statistics to, or <see langword="null"/> for every kind.</param>
/// <param name="TagKind">Whether to return genres or tags, or <see langword="null"/> for both.</param>
/// <param name="FromUtc">The inclusive start of the date range, or <see langword="null"/> when the range has no start.</param>
/// <param name="ToUtc">The inclusive end of the date range, or <see langword="null"/> when the range has no end.</param>
[DebuggerDisplay("TargetUserId: {TargetUserId}; TagKind: {TagKind}")]
public record GetTagConsumptionQuery(
    string? TargetUserId,
    string? LibraryId,
    MediaKind? MediaKind,
    TagKind? TagKind,
    DateTime? FromUtc,
    DateTime? ToUtc
) : IQuery;
