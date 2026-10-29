#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Enums.MediaConsumption;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.Requests.Library.Statistics;

/// <summary>
/// Represents a request for getting the consumption statistics of a user.
/// </summary>
/// <param name="UserId">The Id of the user whose statistics are retrieved. Optional; when omitted, the current user is used.</param>
/// <param name="LibraryId">The Id of the media library to restrict the statistics to. Optional.</param>
/// <param name="MediaKind">The kind of media to restrict the statistics to. Optional.</param>
/// <param name="FromUtc">The inclusive start of the date range. Optional.</param>
/// <param name="ToUtc">The inclusive end of the date range. Optional.</param>
[DebuggerDisplay("UserId: {UserId}; MediaKind: {MediaKind}")]
public sealed record GetConsumptionStatisticsRequest(
    string? UserId,
    string? LibraryId,
    MediaKind? MediaKind,
    DateTime? FromUtc,
    DateTime? ToUtc
);
