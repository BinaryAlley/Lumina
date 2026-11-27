#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.Common.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Domain.Fixtures.Core.BoundedContexts.AudioLibraryBoundedContext.Common.ValueObjects;

/// <summary>
/// Fixture class for the <see cref="AudioRating"/> domain value object.
/// </summary>
[ExcludeFromCodeCoverage]
public class AudioRatingFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="AudioRating"/>.
    /// </summary>
    /// <param name="value">Optional. The numeric value of the rating.</param>
    /// <param name="maxValue">Optional. The maximum possible rating value.</param>
    /// <param name="source">Optional. The source of the rating.</param>
    /// <param name="voteCount">Optional. The number of votes or reviews.</param>
    /// <returns>The created <see cref="AudioRating"/>.</returns>
    public AudioRating Create(
        decimal? value = null,
        decimal? maxValue = null,
        Optional<AudioRatingSource>? source = null,
        Optional<int>? voteCount = null)
    {
        decimal resolvedMaxValue = maxValue ?? 5m;
        Result<AudioRating> ratingResult = AudioRating.Create(
            value ?? _faker.Random.Decimal(0m, resolvedMaxValue),
            resolvedMaxValue,
            source ?? Optional<AudioRatingSource>.Some(_faker.PickRandom<AudioRatingSource>()),
            voteCount ?? Optional<int>.Some(_faker.Random.Int(0, 10000)));

        if (ratingResult.IsFailure)
            throw new InvalidOperationException("Failed to create AudioRating: " + string.Join(", ", ratingResult.Errors));
        return ratingResult.Value;
    }

    /// <summary>
    /// Creates multiple <see cref="AudioRating"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="AudioRating"/> instances.</returns>
    public List<AudioRating> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
