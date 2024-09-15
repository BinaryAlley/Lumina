#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Enums.BookLibrary;
using Lumina.Contracts.Models.Common;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Models.WrittenContentLibrary.BookLibrary;

/// <summary>
/// Represents a book rating.
/// </summary>
/// <param name="Value">Gets the rating value for the book.</param>
/// <param name="MaxValue">Gets the maximum possible rating value for the book.</param>
/// <param name="Source">Gets the source of the book rating (e.g., a specific website or platform).</param>
/// <param name="VoteCount">Gets the number of votes that contributed to the book rating.</param>
[DebuggerDisplay("{Value}/{MaxValue}")]
public record BookRatingModel(
    decimal? Value,
    decimal? MaxValue,
    BookRatingSource? Source,
    int? VoteCount
) : RatingModel(Value, MaxValue, VoteCount);