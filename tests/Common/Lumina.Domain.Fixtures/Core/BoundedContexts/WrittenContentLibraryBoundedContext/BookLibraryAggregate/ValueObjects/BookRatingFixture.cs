#region ========================================================================= USING =====================================================================================
using AutoFixture;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.BookLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.Fixtures.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate.ValueObjects;

/// <summary>
/// Fixture class for the <see cref="BookRating"/> domain value object.
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
    /// Creates a random valid <see cref="BookRating"/>.
    /// </summary>
    /// <param name="value">Optional. The rating value.</param>
    /// <param name="maxValue">Optional. The maximum possible rating value.</param>
    /// <param name="source">Optional. The rating source. If not provided, a random source is selected.</param>
    /// <param name="voteCount">Optional. The number of votes. If not provided, a random vote count is selected.</param>
    /// <returns>The created <see cref="BookRating"/>.</returns>
    public BookRating Create(
        decimal? value = null,
        decimal? maxValue = null,
        Optional<BookRatingSource>? source = null,
        Optional<int>? voteCount = null)
    {
        maxValue ??= _random.Next(5, 10);
        value ??= _random.Next(1, (int)maxValue.Value);
        source ??= Optional<BookRatingSource>.Some(_fixture.Create<BookRatingSource>());
        voteCount ??= Optional<int>.Some(_random.Next(1, 1000));

        return BookRating.Create(value.Value, maxValue.Value, source.Value, voteCount.Value).Value;
    }

    /// <summary>
    /// Creates multiple <see cref="BookRating"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="BookRating"/> instances.</returns>
    public List<BookRating> CreateMany(int count = 3)
    {
        List<BookRating> result = [];
        for (int i = 0; i < count; i++)
            result.Add(Create());
        return result;
    }
}
