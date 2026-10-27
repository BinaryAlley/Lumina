#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Common.DTO.MediaConsumption;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Common.DTO.MediaConsumption;

/// <summary>
/// Fixture class for the <see cref="TagConsumptionDto"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class TagConsumptionDtoFixture
{
    private const int MAXIMUM_DURATION_MILLISECONDS = 36_000_000;

    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="TagConsumptionDto"/>.
    /// </summary>
    /// <param name="name">Optional. The lowercase name of the tag.</param>
    /// <param name="consumedItemCount">Optional. The number of distinct consumed media items that carry the tag.</param>
    /// <param name="consumedDurationMilliseconds">Optional. The consumed duration of the media items that carry the tag, in milliseconds. Defaults to a bounded random duration.</param>
    /// <returns>The created <see cref="TagConsumptionDto"/>.</returns>
    public TagConsumptionDto Create(
        string? name = null,
        int? consumedItemCount = null,
        long? consumedDurationMilliseconds = null)
    {
        return new TagConsumptionDto(
            name ?? _faker.Lorem.Word().ToLowerInvariant(),
            consumedItemCount ?? _faker.Random.Int(0, 1000),
            consumedDurationMilliseconds ?? _faker.Random.Long(0, MAXIMUM_DURATION_MILLISECONDS));
    }

    /// <summary>
    /// Creates a list of <see cref="TagConsumptionDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="TagConsumptionDto"/> instances.</returns>
    public List<TagConsumptionDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
