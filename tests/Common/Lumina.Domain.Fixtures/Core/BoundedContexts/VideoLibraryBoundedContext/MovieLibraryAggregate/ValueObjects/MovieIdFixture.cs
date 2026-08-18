#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.BoundedContexts.VideoLibraryBoundedContext.MovieLibraryAggregate.ValueObjects;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Domain.Fixtures.Core.BoundedContexts.VideoLibraryBoundedContext.MovieLibraryAggregate.ValueObjects;

/// <summary>
/// Fixture class for the <see cref="MovieId"/> domain value object.
/// </summary>
[ExcludeFromCodeCoverage]
public class MovieIdFixture
{
    /// <summary>
    /// Creates a random valid <see cref="MovieId"/>.
    /// </summary>
    /// <param name="value">Optional. The value used to create the <see cref="MovieId"/>. If not provided, a random value is generated.</param>
    /// <returns>The created <see cref="MovieId"/>.</returns>
    public MovieId Create(Guid? value = null)
    {
        return value is null ? MovieId.CreateUnique() : MovieId.Create(value.Value);
    }

    /// <summary>
    /// Creates multiple <see cref="MovieId"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="MovieId"/> instances.</returns>
    public List<MovieId> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
