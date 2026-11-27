#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Domain.Fixtures.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;

/// <summary>
/// Fixture class for the <see cref="TrackId"/> domain value object.
/// </summary>
[ExcludeFromCodeCoverage]
public class TrackIdFixture
{
    /// <summary>
    /// Creates a random valid <see cref="TrackId"/>.
    /// </summary>
    /// <param name="value">Optional. The value used to create the <see cref="TrackId"/>. If not provided, a random value is generated.</param>
    /// <returns>The created <see cref="TrackId"/>.</returns>
    public TrackId Create(
        Guid? value = null)
    {
        return value is null ? TrackId.CreateUnique() : TrackId.Create(value.Value);
    }

    /// <summary>
    /// Creates multiple <see cref="TrackId"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="TrackId"/> instances.</returns>
    public List<TrackId> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
