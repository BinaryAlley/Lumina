#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Plugins.MusicBrainz.Common.Models.Contracts.Responses;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Plugins.MusicBrainz.Fixtures.Common.Models.Contracts.Responses;

/// <summary>
/// Fixture class for the <see cref="MusicBrainzRatingResponse"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
internal sealed class MusicBrainzRatingResponseFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a new <see cref="MusicBrainzRatingResponse"/> instance with randomized test data.
    /// </summary>
    /// <param name="value">Optional. The average rating value.</param>
    /// <param name="includeValue">Whether the value should be included, or forced to <see langword="null"/>.</param>
    /// <param name="votesCount">Optional. The number of votes that contributed to the rating.</param>
    /// <param name="includeVotesCount">Whether the votes count should be included, or forced to <see langword="null"/>.</param>
    /// <returns>A configured <see cref="MusicBrainzRatingResponse"/> instance.</returns>
    public MusicBrainzRatingResponse Create(
        decimal? value = null,
        bool includeValue = true,
        int? votesCount = null,
        bool includeVotesCount = true)
    {
        return new MusicBrainzRatingResponse
        {
            Value = includeValue ? value ?? _faker.Random.Decimal(1m, 5m) : null,
            VotesCount = includeVotesCount ? votesCount ?? _faker.Random.Int(1, 500) : null
        };
    }

    /// <summary>
    /// Creates multiple <see cref="MusicBrainzRatingResponse"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="MusicBrainzRatingResponse"/> instances.</returns>
    public List<MusicBrainzRatingResponse> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
