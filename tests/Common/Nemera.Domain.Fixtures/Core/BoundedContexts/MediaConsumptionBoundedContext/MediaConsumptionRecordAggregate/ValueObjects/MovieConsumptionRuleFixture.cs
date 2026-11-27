#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.MediaConsumptionRecordAggregate.ValueObjects;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Domain.Fixtures.Core.BoundedContexts.MediaConsumptionBoundedContext.MediaConsumptionRecordAggregate.ValueObjects;

/// <summary>
/// Fixture class for the <see cref="MovieConsumptionRule"/> domain value object.
/// </summary>
[ExcludeFromCodeCoverage]
public class MovieConsumptionRuleFixture
{
    /// <summary>
    /// Creates a valid <see cref="MovieConsumptionRule"/>.
    /// </summary>
    /// <returns>The created <see cref="MovieConsumptionRule"/>.</returns>
    public MovieConsumptionRule Create()
    {
        return new MovieConsumptionRule();
    }

    /// <summary>
    /// Creates multiple <see cref="MovieConsumptionRule"/> instances.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="MovieConsumptionRule"/> instances.</returns>
    public List<MovieConsumptionRule> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
