#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.MediaConsumptionRecordAggregate.ValueObjects;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Domain.Fixtures.Core.BoundedContexts.MediaConsumptionBoundedContext.MediaConsumptionRecordAggregate.ValueObjects;

/// <summary>
/// Fixture class for the <see cref="MusicTrackConsumptionRule"/> domain value object.
/// </summary>
[ExcludeFromCodeCoverage]
public class MusicTrackConsumptionRuleFixture
{
    /// <summary>
    /// Creates a valid <see cref="MusicTrackConsumptionRule"/>.
    /// </summary>
    /// <returns>The created <see cref="MusicTrackConsumptionRule"/>.</returns>
    public MusicTrackConsumptionRule Create()
    {
        return new MusicTrackConsumptionRule();
    }

    /// <summary>
    /// Creates multiple <see cref="MusicTrackConsumptionRule"/> instances.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="MusicTrackConsumptionRule"/> instances.</returns>
    public List<MusicTrackConsumptionRule> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
