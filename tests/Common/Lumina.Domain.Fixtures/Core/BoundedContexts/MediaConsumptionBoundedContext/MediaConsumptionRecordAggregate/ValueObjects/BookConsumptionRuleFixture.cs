#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.MediaConsumptionRecordAggregate.ValueObjects;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Domain.Fixtures.Core.BoundedContexts.MediaConsumptionBoundedContext.MediaConsumptionRecordAggregate.ValueObjects;

/// <summary>
/// Fixture class for the <see cref="BookConsumptionRule"/> domain value object.
/// </summary>
[ExcludeFromCodeCoverage]
public class BookConsumptionRuleFixture
{
    /// <summary>
    /// Creates a valid <see cref="BookConsumptionRule"/>.
    /// </summary>
    /// <returns>The created <see cref="BookConsumptionRule"/>.</returns>
    public BookConsumptionRule Create()
    {
        return new BookConsumptionRule();
    }

    /// <summary>
    /// Creates multiple <see cref="BookConsumptionRule"/> instances.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="BookConsumptionRule"/> instances.</returns>
    public List<BookConsumptionRule> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
