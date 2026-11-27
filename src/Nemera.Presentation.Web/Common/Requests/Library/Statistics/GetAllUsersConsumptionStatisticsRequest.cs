#region ========================================================================= USING =====================================================================================
using Nemera.Presentation.Web.Common.Enums.MediaConsumption;
using System;
using System.Diagnostics;
#endregion

namespace Nemera.Presentation.Web.Common.Requests.Library.Statistics;

/// <summary>
/// Represents a request for getting the consumption statistics of every user.
/// </summary>
/// <param name="LibraryId">The Id of the media library to restrict the statistics to. Optional.</param>
/// <param name="MediaKind">The kind of media to restrict the statistics to. Optional.</param>
/// <param name="FromUtc">The inclusive start of the date range. Optional.</param>
/// <param name="ToUtc">The inclusive end of the date range. Optional.</param>
[DebuggerDisplay("MediaKind: {MediaKind}; FromUtc: {FromUtc}; ToUtc: {ToUtc}")]
public sealed record GetAllUsersConsumptionStatisticsRequest(
    string? LibraryId,
    MediaKind? MediaKind,
    DateTime? FromUtc,
    DateTime? ToUtc
);
