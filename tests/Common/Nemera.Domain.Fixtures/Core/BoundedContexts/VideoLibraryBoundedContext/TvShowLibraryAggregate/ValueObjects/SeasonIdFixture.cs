#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.BoundedContexts.VideoLibraryBoundedContext.TvShowLibraryAggregate.ValueObjects;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Domain.Fixtures.Core.BoundedContexts.VideoLibraryBoundedContext.TvShowLibraryAggregate.ValueObjects;

/// <summary>
/// Fixture class for the <see cref="SeasonId"/> domain value object.
/// </summary>
[ExcludeFromCodeCoverage]
public class SeasonIdFixture
{
    /// <summary>
    /// Creates a random valid <see cref="SeasonId"/>.
    /// </summary>
    /// <param name="value">Optional. The value used to create the <see cref="SeasonId"/>. If not provided, a random value is generated.</param>
    /// <returns>The created <see cref="SeasonId"/>.</returns>
    public SeasonId Create(Guid? value = null)
    {
        return value is null ? SeasonId.CreateUnique() : SeasonId.Create(value.Value);
    }

    /// <summary>
    /// Creates multiple <see cref="SeasonId"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="SeasonId"/> instances.</returns>
    public List<SeasonId> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
