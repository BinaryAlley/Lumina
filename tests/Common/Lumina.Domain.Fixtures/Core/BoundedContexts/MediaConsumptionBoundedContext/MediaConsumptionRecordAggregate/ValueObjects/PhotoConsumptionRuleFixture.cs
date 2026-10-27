#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.MediaConsumptionRecordAggregate.ValueObjects;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Domain.Fixtures.Core.BoundedContexts.MediaConsumptionBoundedContext.MediaConsumptionRecordAggregate.ValueObjects;

/// <summary>
/// Fixture class for the <see cref="PhotoConsumptionRule"/> domain value object.
/// </summary>
[ExcludeFromCodeCoverage]
public class PhotoConsumptionRuleFixture
{
    /// <summary>
    /// Creates a valid <see cref="PhotoConsumptionRule"/>.
    /// </summary>
    /// <returns>The created <see cref="PhotoConsumptionRule"/>.</returns>
    public PhotoConsumptionRule Create()
    {
        return new PhotoConsumptionRule();
    }

    /// <summary>
    /// Creates multiple <see cref="PhotoConsumptionRule"/> instances.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="PhotoConsumptionRule"/> instances.</returns>
    public List<PhotoConsumptionRule> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
