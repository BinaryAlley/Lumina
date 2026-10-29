#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Presentation.Web.Common.DTO.MediaConsumption;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.DTO.MediaConsumption;

/// <summary>
/// Fixture class for the <see cref="TagConsumptionDto"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class TagConsumptionDtoFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a new <see cref="TagConsumptionDto"/> instance with randomized test data.
    /// </summary>
    /// <param name="name">Optional lowercase name of the tag.</param>
    /// <param name="consumedItemCount">Optional number of distinct consumed media items that carry the tag.</param>
    /// <param name="consumedDurationInSeconds">Optional consumed duration, in seconds.</param>
    /// <returns>A configured <see cref="TagConsumptionDto"/> instance.</returns>
    public TagConsumptionDto Create(
        string? name = null,
        int? consumedItemCount = null,
        double? consumedDurationInSeconds = null)
    {
        return new TagConsumptionDto(
            Name: name ?? _faker.Lorem.Word().ToLowerInvariant(),
            ConsumedItemCount: consumedItemCount ?? _faker.Random.Int(0, 1000),
            ConsumedDurationInSeconds: consumedDurationInSeconds ?? _faker.Random.Double(0, 100000)
        );
    }

    /// <summary>
    /// Creates multiple <see cref="TagConsumptionDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="TagConsumptionDto"/> instances.</returns>
    public List<TagConsumptionDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
