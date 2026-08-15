#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.BookLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;

/// <summary>
/// Fixture class for the <see cref="BookRatingEntity"/>.
/// </summary>
[ExcludeFromCodeCoverage]
public class BookRatingEntityFixture
{
    private readonly Random _random = new();
    private readonly Faker _faker;

    /// <summary>
    /// Initializes a new instance of the <see cref="BookRatingEntityFixture"/> class.
    /// </summary>
    public BookRatingEntityFixture()
    {
        _faker = new Faker();
    }

    /// <summary>
    /// Creates a <see cref="BookRatingEntity"/>.
    /// </summary>
    /// <param name="value">Optional. The rating value.</param>
    /// <param name="maxValue">Optional. The maximum possible rating value.</param>
    /// <param name="source">Optional. The rating source.</param>
    /// <param name="voteCount">Optional. The number of votes.</param>
    /// <param name="includeValues">Whether the rating properties should be populated. Set to <see langword="false"/> to create an invalid entity.</param>
    /// <returns>The created book rating entity.</returns>
    public BookRatingEntity Create(
        decimal? value = null,
        decimal? maxValue = null,
        BookRatingSource? source = null,
        int? voteCount = null,
        bool includeValues = true)
    {
        return new BookRatingEntity(
            includeValues ? (value ?? _random.Next(1, 5)) : null,
            includeValues ? (maxValue ?? 5) : null,
            includeValues ? (source ?? _faker.PickRandom<BookRatingSource>()) : null,
            includeValues ? (voteCount ?? _random.Next(1, 1000)) : null
        );
    }

    /// <summary>
    /// Creates a list of <see cref="BookRatingEntity"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="BookRatingEntity"/> instances.</returns>
    public List<BookRatingEntity> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
