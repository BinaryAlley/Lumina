#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.DTO.Common;

/// <summary>
/// Data transfer object for a rating.
/// </summary>
/// <param name="Value">The rating value.</param>
/// <param name="MaxValue">The maximum possible rating value.</param>
/// <param name="VoteCount">The number of votes that contributed to the rating.</param>
[DebuggerDisplay("{Value}/{MaxValue}")]
public abstract record RatingDto(
    decimal? Value,
    decimal? MaxValue,
    int? VoteCount
);
