#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.MediaConsumptionRecordAggregate.ValueObjects;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Domain.Fixtures.Core.BoundedContexts.MediaConsumptionBoundedContext.MediaConsumptionRecordAggregate.ValueObjects;

/// <summary>
/// Fixture class for the <see cref="TvEpisodeConsumptionRule"/> domain value object.
/// </summary>
[ExcludeFromCodeCoverage]
public class TvEpisodeConsumptionRuleFixture
{
    /// <summary>
    /// Creates a valid <see cref="TvEpisodeConsumptionRule"/>.
    /// </summary>
    /// <returns>The created <see cref="TvEpisodeConsumptionRule"/>.</returns>
    public TvEpisodeConsumptionRule Create()
    {
        return new TvEpisodeConsumptionRule();
    }

    /// <summary>
    /// Creates multiple <see cref="TvEpisodeConsumptionRule"/> instances.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="TvEpisodeConsumptionRule"/> instances.</returns>
    public List<TvEpisodeConsumptionRule> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
