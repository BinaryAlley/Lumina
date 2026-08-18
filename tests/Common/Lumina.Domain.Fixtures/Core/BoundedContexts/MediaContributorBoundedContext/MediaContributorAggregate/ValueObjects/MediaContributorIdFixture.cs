#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.BoundedContexts.MediaContributorBoundedContext.MediaContributorAggregate.ValueObjects;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Domain.Fixtures.Core.BoundedContexts.MediaContributorBoundedContext.MediaContributorAggregate.ValueObjects;

/// <summary>
/// Fixture class for the <see cref="MediaContributorId"/> domain value object.
/// </summary>
[ExcludeFromCodeCoverage]
public class MediaContributorIdFixture
{
    /// <summary>
    /// Creates a random valid <see cref="MediaContributorId"/>.
    /// </summary>
    /// <param name="value">Optional. The value used to create the <see cref="MediaContributorId"/>. If not provided, a random value is generated.</param>
    /// <returns>The created <see cref="MediaContributorId"/>.</returns>
    public MediaContributorId Create(Guid? value = null)
    {
        return value is null ? MediaContributorId.CreateUnique() : MediaContributorId.Create(value.Value);
    }

    /// <summary>
    /// Creates multiple <see cref="MediaContributorId"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="MediaContributorId"/> instances.</returns>
    public List<MediaContributorId> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
