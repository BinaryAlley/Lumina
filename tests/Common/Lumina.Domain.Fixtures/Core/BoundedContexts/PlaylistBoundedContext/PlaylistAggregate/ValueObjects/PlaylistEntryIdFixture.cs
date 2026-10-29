#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.BoundedContexts.PlaylistBoundedContext.PlaylistAggregate.ValueObjects;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Domain.Fixtures.Core.BoundedContexts.PlaylistBoundedContext.PlaylistAggregate.ValueObjects;

/// <summary>
/// Fixture class for the <see cref="PlaylistEntryId"/> value object.
/// </summary>
[ExcludeFromCodeCoverage]
public class PlaylistEntryIdFixture
{
    /// <summary>
    /// Creates a random valid <see cref="PlaylistEntryId"/>.
    /// </summary>
    /// <param name="value">Optional. The raw value of the playlist entry Id.</param>
    /// <returns>The created <see cref="PlaylistEntryId"/>.</returns>
    public PlaylistEntryId Create(
        Guid? value = null)
    {
        return PlaylistEntryId.Create(value ?? Guid.NewGuid());
    }

    /// <summary>
    /// Creates a list of <see cref="PlaylistEntryId"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="PlaylistEntryId"/> instances.</returns>
    public List<PlaylistEntryId> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
