#region ========================================================================= USING =====================================================================================
using Lumina.Domain.SharedKernel.Common.Enums.MediaConsumption;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Responses.MediaConsumption;

/// <summary>
/// Represents the consumption statistics of a tag or genre, grouped across media kinds.
/// </summary>
/// <param name="Name">The lowercase name of the tag or genre.</param>
/// <param name="TagKind">Whether the entry is a genre or a tag.</param>
/// <param name="ConsumedItemCount">The number of distinct media items that carry the term and were consumed.</param>
/// <param name="ConsumedDurationInSeconds">The active duration consumed in the media items that carry the term, in seconds.</param>
[DebuggerDisplay("TagKind: {TagKind}; Name: {Name}; ConsumedItemCount: {ConsumedItemCount}")]
public sealed record TagConsumptionResponse(
    string Name,
    TagKind TagKind,
    int ConsumedItemCount,
    double ConsumedDurationInSeconds
);
