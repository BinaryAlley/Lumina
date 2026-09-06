#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Presentation.Web.Common.DTO.WrittenContentLibrary.BookLibrary;
using Lumina.Presentation.Web.Common.Enums.BookLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.DTO.WrittenContentLibrary.BookLibrary;

/// <summary>
/// Fixture class for generating <see cref="BookRatingDto"/> test data.
/// </summary>
[ExcludeFromCodeCoverage]
public class BookRatingDtoFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a new <see cref="BookRatingDto"/> instance with randomized test data.
    /// </summary>
    /// <param name="value">Optional. The rating value.</param>
    /// <param name="maxValue">Optional. The maximum possible rating value.</param>
    /// <param name="voteCount">Optional. The number of votes the rating is based on.</param>
    /// <param name="source">Optional. The source of the rating.</param>
    /// <returns>A configured <see cref="BookRatingDto"/> instance.</returns>
    public BookRatingDto Create(
        decimal? value = null,
        decimal? maxValue = null,
        int? voteCount = null,
        BookRatingSource? source = null)
    {
        return new BookRatingDto
        {
            Value = value ?? _faker.Random.Decimal(1, 5),
            MaxValue = maxValue ?? 5,
            VoteCount = voteCount ?? _faker.Random.Number(1, 1000),
            Source = source ?? _faker.PickRandom<BookRatingSource>()
        };
    }

    /// <summary>
    /// Creates multiple <see cref="BookRatingDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="BookRatingDto"/> instances.</returns>
    public List<BookRatingDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
