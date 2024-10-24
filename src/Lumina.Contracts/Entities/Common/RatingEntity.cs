#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Entities.Common;

/// <summary>
/// Represents a rating.
/// </summary>
/// <param name="Value">Gets the rating value.</param>
/// <param name="MaxValue">Gets the maximum possible rating value.</param>
/// <param name="VoteCount">Gets the number of votes that contributed to the rating.</param>
[DebuggerDisplay("{Value}/{MaxValue}")]
public abstract record RatingEntity(
    decimal? Value,
    decimal? MaxValue,
    int? VoteCount
);
