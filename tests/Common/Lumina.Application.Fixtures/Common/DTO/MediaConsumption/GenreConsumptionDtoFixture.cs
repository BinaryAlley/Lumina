#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Common.DTO.MediaConsumption;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Common.DTO.MediaConsumption;

/// <summary>
/// Fixture class for the <see cref="GenreConsumptionDto"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class GenreConsumptionDtoFixture
{
    private const int MAXIMUM_DURATION_MILLISECONDS = 36_000_000;

    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="GenreConsumptionDto"/>.
    /// </summary>
    /// <param name="name">Optional. The lowercase name of the genre. Defaults to a random lowercase word.</param>
    /// <param name="consumedItemCount">Optional. The number of distinct consumed media items that carry the genre. Defaults to a bounded random count.</param>
    /// <param name="consumedDurationMilliseconds">Optional. The consumed duration of the media items that carry the genre, in milliseconds. Defaults to a bounded random duration.</param>
    /// <param name="includeName">Whether the genre name should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeConsumedItemCount">Whether the consumed item count should be included, or forced to zero.</param>
    /// <param name="includeConsumedDurationMilliseconds">Whether the consumed duration should be included, or forced to zero.</param>
    /// <returns>The created <see cref="GenreConsumptionDto"/>.</returns>
    public GenreConsumptionDto Create(
        string? name = null,
        int? consumedItemCount = null,
        long? consumedDurationMilliseconds = null,
        bool includeName = true,
        bool includeConsumedItemCount = true,
        bool includeConsumedDurationMilliseconds = true)
    {
        return new GenreConsumptionDto(
            includeName ? name ?? _faker.Lorem.Word().ToLowerInvariant() : null!,
            includeConsumedItemCount ? consumedItemCount ?? _faker.Random.Int(0, 1000) : 0,
            includeConsumedDurationMilliseconds ? consumedDurationMilliseconds ?? _faker.Random.Long(0, MAXIMUM_DURATION_MILLISECONDS) : 0);
    }

    /// <summary>
    /// Creates a list of <see cref="GenreConsumptionDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="GenreConsumptionDto"/> instances.</returns>
    public List<GenreConsumptionDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
