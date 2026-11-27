#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.MediaConsumptionRecordAggregate.ValueObjects;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Domain.Fixtures.Core.BoundedContexts.MediaConsumptionBoundedContext.MediaConsumptionRecordAggregate.ValueObjects;

/// <summary>
/// Fixture class for the <see cref="MediaConsumptionRecordId"/> value object.
/// </summary>
[ExcludeFromCodeCoverage]
public class MediaConsumptionRecordIdFixture
{
    /// <summary>
    /// Creates a random valid <see cref="MediaConsumptionRecordId"/>.
    /// </summary>
    /// <param name="value">Optional. The raw value of the consumption record Id.</param>
    /// <returns>The created <see cref="MediaConsumptionRecordId"/>.</returns>
    public MediaConsumptionRecordId Create(
        Guid? value = null)
    {
        return MediaConsumptionRecordId.Create(value ?? Guid.NewGuid());
    }

    /// <summary>
    /// Creates a list of <see cref="MediaConsumptionRecordId"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="MediaConsumptionRecordId"/> instances.</returns>
    public List<MediaConsumptionRecordId> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
