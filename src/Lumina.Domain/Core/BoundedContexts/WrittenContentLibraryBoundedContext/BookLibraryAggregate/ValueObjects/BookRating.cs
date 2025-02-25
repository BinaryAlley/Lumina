#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Domain.Common.Enums.BookLibrary;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Common.ValueObjects.Metadata;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate.ValueObjects;

/// <summary>
/// Value Object for the rating of a media element.
/// </summary>
[DebuggerDisplay("{Value}/{MaxValue}")]
public class BookRating : Rating
{
    /// <summary>
    /// Gets the optional source of the rating (e.g., "Goodreads", "Amazon").
    /// </summary>
    public Optional<BookRatingSource> Source { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Rating"/> class.
    /// </summary>
    /// <param name="value">The numeric value of the rating.</param>
    /// <param name="maxValue">The maximum possible rating value.</param>
    /// <param name="source">The optional source of the rating.</param>
    /// <param name="voteCount">The optional number of votes or reviews.</param>
    private BookRating(decimal value, decimal maxValue, Optional<BookRatingSource> source, Optional<int> voteCount)
        : base(value, maxValue, voteCount)
    {
        Source = source;
    }

    /// <summary>
    /// Creates a new instance of the <see cref="Rating"/> class.
    /// </summary>
    /// <param name="value">The numeric value of the rating.</param>
    /// <param name="maxValue">The maximum possible rating value.</param>
    /// <param name="source">The optional source of the rating.</param>
    /// <param name="voteCount">The optional number of votes or reviews.</param>
    /// <returns>
    /// An <see cref="ErrorOr{TValue}"/> containing either a successfuly created <see cref="Rating"/>, or an error message.
    /// </returns>
    public static ErrorOr<BookRating> Create(decimal value, decimal maxValue, Optional<BookRatingSource> source, Optional<int> voteCount)
    {
        if (maxValue < 0 || value < 0)
            return Errors.Metadata.RatingValueMustBePositive;
        if (value > maxValue)
            return Errors.Metadata.RatingValueCannotBeGreaterThanMaxValue;
        return new BookRating(value, maxValue, source, voteCount);
    }

    /// <inheritdoc/>
    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
        yield return MaxValue;
        yield return Source;
        yield return VoteCount;
    }

    /// <summary>
    /// Customized ToString() method.
    /// </summary>
    /// <returns>Custom string value showing relevant data for current class.</returns>
    public override string ToString()
    {
        string baseString = $"{Value}/{MaxValue}";
        if (Source.HasValue)
            baseString += $" ({Source.Value})";
        if (VoteCount.HasValue)
            baseString += $" [{VoteCount.Value} votes]";
        return baseString;
    }
}
