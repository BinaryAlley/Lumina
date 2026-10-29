#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.BoundedContexts.PlaylistBoundedContext.PlaylistAggregate.ValueObjects;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Domain.Fixtures.Core.BoundedContexts.PlaylistBoundedContext.PlaylistAggregate.ValueObjects;

/// <summary>
/// Fixture class for the <see cref="PlaylistId"/> value object.
/// </summary>
[ExcludeFromCodeCoverage]
public class PlaylistIdFixture
{
    /// <summary>
    /// Creates a random valid <see cref="PlaylistId"/>.
    /// </summary>
    /// <param name="value">Optional. The raw value of the playlist Id.</param>
    /// <returns>The created <see cref="PlaylistId"/>.</returns>
    public PlaylistId Create(
        Guid? value = null)
    {
        return PlaylistId.Create(value ?? Guid.NewGuid());
    }

    /// <summary>
    /// Creates a list of <see cref="PlaylistId"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="PlaylistId"/> instances.</returns>
    public List<PlaylistId> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
