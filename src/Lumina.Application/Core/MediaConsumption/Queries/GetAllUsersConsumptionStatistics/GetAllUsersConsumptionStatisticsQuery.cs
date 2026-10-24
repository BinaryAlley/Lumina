#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Core.MediaConsumption.Queries.GetAllUsersConsumptionStatistics;

/// <summary>
/// Query for getting the consumption statistics of every user.
/// </summary>
/// <param name="LibraryId">The Id of the media library to restrict the statistics to, or <see langword="null"/> for every library.</param>
/// <param name="MediaKind">The kind of media to restrict the statistics to, or <see langword="null"/> for every kind.</param>
/// <param name="FromUtc">The inclusive start of the date range, or <see langword="null"/> when the range has no start.</param>
/// <param name="ToUtc">The inclusive end of the date range, or <see langword="null"/> when the range has no end.</param>
[DebuggerDisplay("MediaKind: {MediaKind}")]
public record GetAllUsersConsumptionStatisticsQuery(
    string? LibraryId,
    MediaKind? MediaKind,
    DateTime? FromUtc,
    DateTime? ToUtc
) : IQuery;
