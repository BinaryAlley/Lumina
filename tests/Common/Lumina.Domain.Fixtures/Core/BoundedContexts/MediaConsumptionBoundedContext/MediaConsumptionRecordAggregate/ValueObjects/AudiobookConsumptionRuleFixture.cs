#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.MediaConsumptionRecordAggregate.ValueObjects;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Domain.Fixtures.Core.BoundedContexts.MediaConsumptionBoundedContext.MediaConsumptionRecordAggregate.ValueObjects;

/// <summary>
/// Fixture class for the <see cref="AudiobookConsumptionRule"/> domain value object.
/// </summary>
[ExcludeFromCodeCoverage]
public class AudiobookConsumptionRuleFixture
{
    /// <summary>
    /// Creates a valid <see cref="AudiobookConsumptionRule"/>.
    /// </summary>
    /// <returns>The created <see cref="AudiobookConsumptionRule"/>.</returns>
    public AudiobookConsumptionRule Create()
    {
        return new AudiobookConsumptionRule();
    }

    /// <summary>
    /// Creates multiple <see cref="AudiobookConsumptionRule"/> instances.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="AudiobookConsumptionRule"/> instances.</returns>
    public List<AudiobookConsumptionRule> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
