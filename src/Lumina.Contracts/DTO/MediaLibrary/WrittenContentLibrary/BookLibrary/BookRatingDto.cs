#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.Common;
using Lumina.Domain.Common.Enums.BookLibrary;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary;

/// <summary>
/// Data transfer object for book rating.
/// </summary>
/// <param name="Value">The rating value for the book.</param>
/// <param name="MaxValue">The maximum possible rating value for the book.</param>
/// <param name="Source">The source of the book rating (e.g., a specific website or platform).</param>
/// <param name="VoteCount">The number of votes that contributed to the book rating.</param>
[DebuggerDisplay("{Value}/{MaxValue}")]
public record BookRatingDto(
    decimal? Value,
    decimal? MaxValue,
    BookRatingSource? Source,
    int? VoteCount
) : RatingDto(Value, MaxValue, VoteCount);
