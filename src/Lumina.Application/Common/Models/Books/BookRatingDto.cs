#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Enums;
using Lumina.Application.Common.Models.Common;
#endregion

namespace Lumina.Application.Common.Models.Books;

/// <summary>
/// Represents a book rating.
/// </summary>
public record BookRatingDto(
    decimal Value,
    decimal MaxValue,
    BookRatingSource? Source,
    int? VoteCount
) : RatingDto(Value, MaxValue, VoteCount);