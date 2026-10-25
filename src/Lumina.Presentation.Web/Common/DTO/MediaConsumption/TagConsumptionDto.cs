#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.DTO.MediaConsumption;

/// <summary>
/// Data transfer object for the consumption statistics of a tag, grouped across media kinds.
/// </summary>
/// <param name="Name">The lowercase name of the tag.</param>
/// <param name="ConsumedItemCount">The number of distinct media items that carry the tag and were consumed.</param>
/// <param name="ConsumedDurationInSeconds">The active duration consumed in the media items that carry the tag, in seconds.</param>
[DebuggerDisplay("Name: {Name}; ConsumedItemCount: {ConsumedItemCount}")]
public sealed record TagConsumptionDto(
    string Name,
    int ConsumedItemCount,
    double ConsumedDurationInSeconds
);
