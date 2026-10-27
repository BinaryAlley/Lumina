#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.Responses.MediaConsumption;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.Responses.MediaConsumption;

/// <summary>
/// Fixture class for the <see cref="TagConsumptionResponse"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class TagConsumptionResponseFixture
{
    private const double MAXIMUM_DURATION_SECONDS = 36_000;

    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="TagConsumptionResponse"/>.
    /// </summary>
    /// <param name="name">Optional. The lowercase name of the tag.</param>
    /// <param name="consumedItemCount">Optional. The number of distinct consumed media items that carry the tag.</param>
    /// <param name="consumedDurationInSeconds">Optional. The consumed duration of the media items that carry the tag, in seconds. Defaults to a bounded random duration.</param>
    /// <returns>The created <see cref="TagConsumptionResponse"/>.</returns>
    public TagConsumptionResponse Create(
        string? name = null,
        int? consumedItemCount = null,
        double? consumedDurationInSeconds = null)
    {
        return new TagConsumptionResponse(
            name ?? _faker.Lorem.Word().ToLowerInvariant(),
            consumedItemCount ?? _faker.Random.Int(0, 1000),
            consumedDurationInSeconds ?? _faker.Random.Double(0, MAXIMUM_DURATION_SECONDS));
    }

    /// <summary>
    /// Creates a list of <see cref="TagConsumptionResponse"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="TagConsumptionResponse"/> instances.</returns>
    public List<TagConsumptionResponse> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
