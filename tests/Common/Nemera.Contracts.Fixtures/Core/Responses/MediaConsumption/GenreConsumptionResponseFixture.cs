#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.Responses.MediaConsumption;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.Responses.MediaConsumption;

/// <summary>
/// Fixture class for the <see cref="GenreConsumptionResponse"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class GenreConsumptionResponseFixture
{
    private const double MAXIMUM_DURATION_SECONDS = 36_000;

    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="GenreConsumptionResponse"/>.
    /// </summary>
    /// <param name="name">Optional. The lowercase name of the genre. Defaults to a random lowercase word.</param>
    /// <param name="consumedItemCount">Optional. The number of distinct consumed media items that carry the genre. Defaults to a bounded random count.</param>
    /// <param name="consumedDurationInSeconds">Optional. The consumed duration of the media items that carry the genre, in seconds. Defaults to a bounded random duration.</param>
    /// <param name="includeName">Whether the genre name should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeConsumedItemCount">Whether the consumed item count should be included, or forced to zero.</param>
    /// <param name="includeConsumedDurationInSeconds">Whether the consumed duration should be included, or forced to zero.</param>
    /// <returns>The created <see cref="GenreConsumptionResponse"/>.</returns>
    public GenreConsumptionResponse Create(
        string? name = null,
        int? consumedItemCount = null,
        double? consumedDurationInSeconds = null,
        bool includeName = true,
        bool includeConsumedItemCount = true,
        bool includeConsumedDurationInSeconds = true)
    {
        return new GenreConsumptionResponse(
            includeName ? name ?? _faker.Lorem.Word().ToLowerInvariant() : null!,
            includeConsumedItemCount ? consumedItemCount ?? _faker.Random.Int(0, 1000) : 0,
            includeConsumedDurationInSeconds ? consumedDurationInSeconds ?? _faker.Random.Double(0, MAXIMUM_DURATION_SECONDS) : 0);
    }

    /// <summary>
    /// Creates a list of <see cref="GenreConsumptionResponse"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="GenreConsumptionResponse"/> instances.</returns>
    public List<GenreConsumptionResponse> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
