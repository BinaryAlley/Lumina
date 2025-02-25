#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Common;
using Lumina.Domain.Common.Enums.BookLibrary;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;

/// <summary>
/// Repository entity for a book rating.
/// </summary>
/// <param name="Value">The rating value for the book.</param>
/// <param name="MaxValue">The maximum possible rating value for the book.</param>
/// <param name="Source">The source of the book rating (e.g., a specific website or platform).</param>
/// <param name="VoteCount">The number of votes that contributed to the book rating.</param>
[DebuggerDisplay("{Value}/{MaxValue}")]
public record BookRatingEntity(
    decimal? Value,
    decimal? MaxValue,
    BookRatingSource? Source,
    int? VoteCount
) : RatingEntity(Value, MaxValue, VoteCount);
