#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Presentation.Web.Common.Requests.Library.Statistics;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.Requests.Library.Statistics;

/// <summary>
/// Fixture class for the <see cref="ClearConsumptionHistoryRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class ClearConsumptionHistoryRequestFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a new <see cref="ClearConsumptionHistoryRequest"/> instance with randomized test data.
    /// </summary>
    /// <param name="userId">Optional Id of the user whose history is deleted.</param>
    /// <param name="includeUserId">Whether the user Id should be included, or forced to <see langword="null"/>.</param>
    /// <returns>A configured <see cref="ClearConsumptionHistoryRequest"/> instance.</returns>
    public ClearConsumptionHistoryRequest Create(
        string? userId = null,
        bool includeUserId = true)
    {
        return new ClearConsumptionHistoryRequest(
            UserId: includeUserId ? (userId ?? _faker.Random.Guid().ToString()) : null
        );
    }

    /// <summary>
    /// Creates multiple <see cref="ClearConsumptionHistoryRequest"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="ClearConsumptionHistoryRequest"/> instances.</returns>
    public List<ClearConsumptionHistoryRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
