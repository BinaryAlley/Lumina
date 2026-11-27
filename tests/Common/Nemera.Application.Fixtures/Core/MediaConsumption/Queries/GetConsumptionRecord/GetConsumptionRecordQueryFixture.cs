#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Core.MediaConsumption.Queries.GetConsumptionRecord;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Core.MediaConsumption.Queries.GetConsumptionRecord;

/// <summary>
/// Fixture class for the <see cref="GetConsumptionRecordQuery"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetConsumptionRecordQueryFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="GetConsumptionRecordQuery"/>.
    /// </summary>
    /// <param name="consumptionRecordId">Optional. The Id of the consumption record to read, taken from the route. Defaults to a random Id.</param>
    /// <param name="includeConsumptionRecordId">Whether the consumption record Id should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created <see cref="GetConsumptionRecordQuery"/>.</returns>
    public GetConsumptionRecordQuery Create(
        string? consumptionRecordId = null,
        bool includeConsumptionRecordId = true)
    {
        string? resolvedConsumptionRecordId = includeConsumptionRecordId ? consumptionRecordId ?? _faker.Random.Guid().ToString() : null;

        return new GetConsumptionRecordQuery(resolvedConsumptionRecordId);
    }

    /// <summary>
    /// Creates a list of <see cref="GetConsumptionRecordQuery"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="GetConsumptionRecordQuery"/> instances.</returns>
    public List<GetConsumptionRecordQuery> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
