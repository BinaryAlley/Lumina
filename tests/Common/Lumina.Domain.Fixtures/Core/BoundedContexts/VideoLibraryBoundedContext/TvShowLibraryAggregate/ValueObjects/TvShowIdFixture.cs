#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.BoundedContexts.VideoLibraryBoundedContext.TvShowLibraryAggregate.ValueObjects;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Domain.Fixtures.Core.BoundedContexts.VideoLibraryBoundedContext.TvShowLibraryAggregate.ValueObjects;

/// <summary>
/// Fixture class for the <see cref="TvShowId"/> domain value object.
/// </summary>
[ExcludeFromCodeCoverage]
public class TvShowIdFixture
{
    /// <summary>
    /// Creates a random valid <see cref="TvShowId"/>.
    /// </summary>
    /// <param name="value">Optional. The value used to create the <see cref="TvShowId"/>. If not provided, a random value is generated.</param>
    /// <returns>The created <see cref="TvShowId"/>.</returns>
    public TvShowId Create(Guid? value = null)
    {
        return value is null ? TvShowId.CreateUnique() : TvShowId.Create(value.Value);
    }

    /// <summary>
    /// Creates multiple <see cref="TvShowId"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="TvShowId"/> instances.</returns>
    public List<TvShowId> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
