#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Application.Common.DTO.MediaConsumption;

/// <summary>
/// Data transfer object for the consumption statistics of a genre, grouped across media kinds.
/// </summary>
/// <param name="Name">The lowercase name of the genre.</param>
/// <param name="ConsumedItemCount">The number of distinct media items that carry the genre and were consumed.</param>
/// <param name="ConsumedDurationMilliseconds">The active duration consumed in the media items that carry the genre, in milliseconds.</param>
[DebuggerDisplay("Name: {Name}; ConsumedItemCount: {ConsumedItemCount}")]
public sealed record GenreConsumptionDto(
    string Name,
    int ConsumedItemCount,
    long ConsumedDurationMilliseconds
);
