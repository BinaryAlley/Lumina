#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Models.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.PlaylistBoundedContext.ExternalIdentifiers.MediaLibraryBoundedContext.MediaItemAggregate;

/// <summary>
/// Value Object for the Id of a media item, duplicated from the media library bounded contexts to keep the cross bounded context references decoupled.
/// </summary>
/// <remarks>
/// The kind of media item is carried by the playlist entry and by the playlist type, so this identifier stays generic across music tracks and video files.
/// </remarks>
[DebuggerDisplay("{Value}")]
public sealed class MediaItemId : EntityId<Guid>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MediaItemId"/> class.
    /// </summary>
    /// <param name="value">The value representing this object.</param>
    private MediaItemId(Guid value) : base(value)
    {
    }

    /// <summary>
    /// Creates a new instance of the <see cref="MediaItemId"/> class.
    /// </summary>
    /// <returns>The created <see cref="MediaItemId"/> instance.</returns>
    public static MediaItemId CreateUnique()
    {
        return new MediaItemId(Guid.NewGuid());
    }

    /// <summary>
    /// Creates a new instance of the <see cref="MediaItemId"/> class, from a pre-existing <paramref name="value"/>.
    /// </summary>
    /// <param name="value">The value used to create the <see cref="MediaItemId"/> instance.</param>
    /// <returns>The created <see cref="MediaItemId"/> instance.</returns>
    public static MediaItemId Create(Guid value)
    {
        return new MediaItemId(value);
    }

    /// <summary>
    /// Gets the list of items that define equality of the object.
    /// </summary>
    /// <returns>A list of items defining the equality.</returns>
    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
