#region ========================================================================= USING =====================================================================================
using AutoFixture;
using Lumina.Domain.Common.Enums.BookLibrary;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate.ValueObjects;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.WrittenContentLibrary.BooksLibrary.Common.Fixtures;

/// <summary>
/// Fixture class for the <see cref="BookRating"/> domain entity.
/// </summary>
[ExcludeFromCodeCoverage]
public class BookRatingFixture
{
    private readonly Fixture _fixture;
    private readonly Random _random = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="BookRatingFixture"/> class.
    /// </summary>
    public BookRatingFixture()
    {
        _fixture = new Fixture();
    }

    /// <summary>
    /// Creates a random valid book rating.
    /// </summary>
    /// <returns>The created book rating.</returns>
    public BookRating CreateBookRating()
    {
        decimal maxValue = _random.Next(5, 10);
        decimal value = _random.Next(1, (int)maxValue);
        Optional<BookRatingSource> source = Optional<BookRatingSource>.Some(_fixture.Create<BookRatingSource>());
        Optional<int> voteCount = Optional<int>.Some(_random.Next(1, 1000));

        return BookRating.Create(value, maxValue, source, voteCount).Value;
    }

    /// <summary>
    /// Creates a random valid book rating with specific parameters.
    /// </summary>
    /// <param name="value">The rating value.</param>
    /// <param name="maxValue">The maximum possible rating value.</param>
    /// <param name="source">The rating source.</param>
    /// <param name="voteCount">The number of votes.</param>
    /// <returns>The created book rating.</returns>
    public BookRating CreateBookRating(
        decimal value,
        decimal maxValue,
        Optional<BookRatingSource> source,
        Optional<int> voteCount)
    {
        return BookRating.Create(value, maxValue, source, voteCount).Value;
    }

    /// <summary>
    /// Creates a random valid book rating without a source.
    /// </summary>
    /// <returns>The created book rating.</returns>
    public BookRating CreateBookRatingWithoutSource()
    {
        decimal maxValue = _random.Next(5, 10);
        decimal value = _random.Next(1, (int)maxValue);
        Optional<int> voteCount = Optional<int>.Some(_random.Next(1, 1000));

        return BookRating.Create(value, maxValue, Optional<BookRatingSource>.None(), voteCount).Value;
    }

    /// <summary>
    /// Creates a random valid book rating without vote count.
    /// </summary>
    /// <returns>The created book rating.</returns>
    public BookRating CreateBookRatingWithoutVoteCount()
    {
        decimal maxValue = _random.Next(5, 10);
        decimal value = _random.Next(1, (int)maxValue);
        Optional<BookRatingSource> source = Optional<BookRatingSource>.Some(_fixture.Create<BookRatingSource>());

        return BookRating.Create(value, maxValue, source, Optional<int>.None()).Value;
    }
}
