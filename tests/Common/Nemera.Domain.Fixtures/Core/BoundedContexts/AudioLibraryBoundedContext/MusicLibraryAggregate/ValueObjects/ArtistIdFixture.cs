#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Domain.Fixtures.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;

/// <summary>
/// Fixture class for the <see cref="ArtistId"/> domain value object.
/// </summary>
[ExcludeFromCodeCoverage]
public class ArtistIdFixture
{
    /// <summary>
    /// Creates a random valid <see cref="ArtistId"/>.
    /// </summary>
    /// <param name="value">Optional. The value used to create the <see cref="ArtistId"/>. If not provided, a random value is generated.</param>
    /// <returns>The created <see cref="ArtistId"/>.</returns>
    public ArtistId Create(
        Guid? value = null)
    {
        return value is null ? ArtistId.CreateUnique() : ArtistId.Create(value.Value);
    }

    /// <summary>
    /// Creates multiple <see cref="ArtistId"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="ArtistId"/> instances.</returns>
    public List<ArtistId> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
