#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.BoundedContexts.PlaylistBoundedContext.ExternalIdentifiers.MediaLibraryBoundedContext.MediaItemAggregate;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Domain.Fixtures.Core.BoundedContexts.PlaylistBoundedContext.ExternalIdentifiers.MediaLibraryBoundedContext.MediaItemAggregate;

/// <summary>
/// Fixture class for the <see cref="MediaItemId"/> value object.
/// </summary>
[ExcludeFromCodeCoverage]
public class MediaItemIdFixture
{
    /// <summary>
    /// Creates a random valid <see cref="MediaItemId"/>.
    /// </summary>
    /// <param name="value">Optional. The raw value of the media item Id.</param>
    /// <returns>The created <see cref="MediaItemId"/>.</returns>
    public MediaItemId Create(
        Guid? value = null)
    {
        return MediaItemId.Create(value ?? Guid.NewGuid());
    }

    /// <summary>
    /// Creates a list of <see cref="MediaItemId"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="MediaItemId"/> instances.</returns>
    public List<MediaItemId> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
