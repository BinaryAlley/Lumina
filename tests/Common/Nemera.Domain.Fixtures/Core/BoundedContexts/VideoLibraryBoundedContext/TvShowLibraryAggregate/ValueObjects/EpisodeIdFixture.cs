#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.BoundedContexts.VideoLibraryBoundedContext.TvShowLibraryAggregate.ValueObjects;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Domain.Fixtures.Core.BoundedContexts.VideoLibraryBoundedContext.TvShowLibraryAggregate.ValueObjects;

/// <summary>
/// Fixture class for the <see cref="EpisodeId"/> domain value object.
/// </summary>
[ExcludeFromCodeCoverage]
public class EpisodeIdFixture
{
    /// <summary>
    /// Creates a random valid <see cref="EpisodeId"/>.
    /// </summary>
    /// <param name="value">Optional. The value used to create the <see cref="EpisodeId"/>. If not provided, a random value is generated.</param>
    /// <returns>The created <see cref="EpisodeId"/>.</returns>
    public EpisodeId Create(Guid? value = null)
    {
        return value is null ? EpisodeId.CreateUnique() : EpisodeId.Create(value.Value);
    }

    /// <summary>
    /// Creates multiple <see cref="EpisodeId"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="EpisodeId"/> instances.</returns>
    public List<EpisodeId> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
