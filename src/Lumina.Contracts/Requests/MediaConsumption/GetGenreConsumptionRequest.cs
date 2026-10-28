#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Requests.MediaConsumption;

/// <summary>
/// Represents a request for getting the consumption statistics of the genres of a user.
/// </summary>
/// <param name="LibraryId">The Id of the media library to restrict the statistics to. Optional.</param>
/// <param name="MediaKind">The name of the kind of media to restrict the statistics to. Optional.</param>
/// <param name="FromUtc">The inclusive start of the date range, in UTC, as a text value. Optional.</param>
/// <param name="ToUtc">The inclusive end of the date range, in UTC, as a text value. Optional.</param>
[DebuggerDisplay("MediaKind: {MediaKind}; FromUtc: {FromUtc}; ToUtc: {ToUtc}")]
public sealed record GetGenreConsumptionRequest(
    string? LibraryId,
    string? MediaKind,
    string? FromUtc,
    string? ToUtc
);
