#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Presentation.Web.Common.DTO.MediaConsumption;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.DTO.MediaConsumption;

/// <summary>
/// Fixture class for the <see cref="GenreConsumptionDto"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class GenreConsumptionDtoFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a new <see cref="GenreConsumptionDto"/> instance with randomized test data.
    /// </summary>
    /// <param name="name">Optional lowercase name of the genre.</param>
    /// <param name="consumedItemCount">Optional number of distinct consumed media items that carry the genre.</param>
    /// <param name="consumedDurationInSeconds">Optional consumed duration, in seconds.</param>
    /// <returns>A configured <see cref="GenreConsumptionDto"/> instance.</returns>
    public GenreConsumptionDto Create(
        string? name = null,
        int? consumedItemCount = null,
        double? consumedDurationInSeconds = null)
    {
        return new GenreConsumptionDto(
            Name: name ?? _faker.Lorem.Word().ToLowerInvariant(),
            ConsumedItemCount: consumedItemCount ?? _faker.Random.Int(0, 1000),
            ConsumedDurationInSeconds: consumedDurationInSeconds ?? _faker.Random.Double(0, 100000)
        );
    }

    /// <summary>
    /// Creates multiple <see cref="GenreConsumptionDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="GenreConsumptionDto"/> instances.</returns>
    public List<GenreConsumptionDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
