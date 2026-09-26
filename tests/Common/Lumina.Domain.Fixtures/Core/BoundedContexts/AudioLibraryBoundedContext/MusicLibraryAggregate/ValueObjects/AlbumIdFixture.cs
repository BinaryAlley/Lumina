#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Domain.Fixtures.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;

/// <summary>
/// Fixture class for the <see cref="AlbumId"/> domain value object.
/// </summary>
[ExcludeFromCodeCoverage]
public class AlbumIdFixture
{
    /// <summary>
    /// Creates a random valid <see cref="AlbumId"/>.
    /// </summary>
    /// <param name="value">Optional. The value used to create the <see cref="AlbumId"/>. If not provided, a random value is generated.</param>
    /// <returns>The created <see cref="AlbumId"/>.</returns>
    public AlbumId Create(Guid? value = null)
    {
        return value is null ? AlbumId.CreateUnique() : AlbumId.Create(value.Value);
    }

    /// <summary>
    /// Creates multiple <see cref="AlbumId"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="AlbumId"/> instances.</returns>
    public List<AlbumId> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
