#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Presentation.Web.Common.DTO.AudioLibrary;
using Lumina.Presentation.Web.Common.Enums.AudioLibrary;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.DTO.AudioLibrary;

/// <summary>
/// Fixture class for the <see cref="AudioRatingDto"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class AudioRatingDtoFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a new <see cref="AudioRatingDto"/> instance with randomized test data.
    /// </summary>
    /// <param name="value">Optional. The numeric value of the rating.</param>
    /// <param name="maxValue">Optional. The maximum possible rating value.</param>
    /// <param name="voteCount">Optional. The number of votes the rating is based on.</param>
    /// <param name="source">Optional. The source of the audio rating.</param>
    /// <param name="includeValue">Whether the value should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeMaxValue">Whether the maximum value should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeVoteCount">Whether the vote count should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeSource">Whether the source should be included, or forced to <see langword="null"/>.</param>
    /// <returns>A configured <see cref="AudioRatingDto"/> instance.</returns>
    public AudioRatingDto Create(
        decimal? value = null,
        decimal? maxValue = null,
        int? voteCount = null,
        AudioRatingSource? source = null,
        bool includeValue = true,
        bool includeMaxValue = true,
        bool includeVoteCount = true,
        bool includeSource = true)
    {
        return new AudioRatingDto
        {
            Value = includeValue ? (value ?? _faker.Random.Decimal(1, 5)) : null,
            MaxValue = includeMaxValue ? (maxValue ?? 5) : null,
            VoteCount = includeVoteCount ? (voteCount ?? _faker.Random.Number(1, 1000)) : null,
            Source = includeSource ? (source ?? _faker.PickRandom<AudioRatingSource>()) : null
        };
    }

    /// <summary>
    /// Creates multiple <see cref="AudioRatingDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="AudioRatingDto"/> instances.</returns>
    public List<AudioRatingDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
