#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.MediaConsumptionRecordAggregate.ValueObjects;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Domain.Fixtures.Core.BoundedContexts.MediaConsumptionBoundedContext.MediaConsumptionRecordAggregate.ValueObjects;

/// <summary>
/// Fixture class for the <see cref="VideoConsumptionRule"/> domain value object.
/// </summary>
[ExcludeFromCodeCoverage]
public class VideoConsumptionRuleFixture
{
    /// <summary>
    /// Creates a valid <see cref="VideoConsumptionRule"/>.
    /// </summary>
    /// <returns>The created <see cref="VideoConsumptionRule"/>.</returns>
    public VideoConsumptionRule Create()
    {
        return new VideoConsumptionRule();
    }

    /// <summary>
    /// Creates multiple <see cref="VideoConsumptionRule"/> instances.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="VideoConsumptionRule"/> instances.</returns>
    public List<VideoConsumptionRule> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
