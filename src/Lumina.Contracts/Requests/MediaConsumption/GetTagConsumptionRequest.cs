#region ========================================================================= USING =====================================================================================
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Requests.MediaConsumption;

/// <summary>
/// Represents a request for getting the consumption statistics of the tags of a user.
/// </summary>
/// <param name="LibraryId">The Id of the media library to restrict the statistics to. Optional.</param>
/// <param name="MediaKind">The kind of media to restrict the statistics to. Optional.</param>
/// <param name="FromUtc">The inclusive start of the date range. Optional.</param>
/// <param name="ToUtc">The inclusive end of the date range. Optional.</param>
[DebuggerDisplay("MediaKind: {MediaKind}; FromUtc: {FromUtc}; ToUtc: {ToUtc}")]
public sealed record GetTagConsumptionRequest(
    string? LibraryId,
    MediaKind? MediaKind,
    DateTime? FromUtc,
    DateTime? ToUtc
);
