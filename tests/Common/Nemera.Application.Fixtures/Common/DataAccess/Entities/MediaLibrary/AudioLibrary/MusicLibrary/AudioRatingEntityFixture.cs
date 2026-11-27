#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;

/// <summary>
/// Fixture class for the <see cref="AudioRatingEntity"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class AudioRatingEntityFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="AudioRatingEntity"/>.
    /// </summary>
    /// <param name="value">Optional. The rating value of the audio media element.</param>
    /// <param name="maxValue">Optional. The maximum possible rating value of the audio media element.</param>
    /// <param name="source">Optional. The source of the audio rating.</param>
    /// <param name="voteCount">Optional. The number of votes that contributed to the audio rating.</param>
    /// <param name="includeValue">Whether the value should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeMaxValue">Whether the maximum value should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeSource">Whether the source should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeVoteCount">Whether the vote count should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created <see cref="AudioRatingEntity"/>.</returns>
    public AudioRatingEntity Create(
        decimal? value = null,
        decimal? maxValue = null,
        AudioRatingSource? source = null,
        int? voteCount = null,
        bool includeValue = true,
        bool includeMaxValue = true,
        bool includeSource = true,
        bool includeVoteCount = true)
    {
        return new AudioRatingEntity(
            includeValue ? (value ?? _faker.Random.Decimal(1, 5)) : null,
            includeMaxValue ? (maxValue ?? 5) : null,
            includeSource ? (source ?? _faker.PickRandom<AudioRatingSource>()) : null,
            includeVoteCount ? (voteCount ?? _faker.Random.Number(1, 1000)) : null
        );
    }

    /// <summary>
    /// Creates a list of <see cref="AudioRatingEntity"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<AudioRatingEntity> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
