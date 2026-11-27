#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.MediaConsumptionRecordAggregate.ValueObjects;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Domain.Fixtures.Core.BoundedContexts.MediaConsumptionBoundedContext.MediaConsumptionRecordAggregate.ValueObjects;

/// <summary>
/// Fixture class for the <see cref="PodcastEpisodeConsumptionRule"/> domain value object.
/// </summary>
[ExcludeFromCodeCoverage]
public class PodcastEpisodeConsumptionRuleFixture
{
    /// <summary>
    /// Creates a valid <see cref="PodcastEpisodeConsumptionRule"/>.
    /// </summary>
    /// <returns>The created <see cref="PodcastEpisodeConsumptionRule"/>.</returns>
    public PodcastEpisodeConsumptionRule Create()
    {
        return new PodcastEpisodeConsumptionRule();
    }

    /// <summary>
    /// Creates multiple <see cref="PodcastEpisodeConsumptionRule"/> instances.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="PodcastEpisodeConsumptionRule"/> instances.</returns>
    public List<PodcastEpisodeConsumptionRule> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
