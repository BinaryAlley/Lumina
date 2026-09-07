#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Models.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;

/// <summary>
/// Value Object for the Id of an album.
/// </summary>
[DebuggerDisplay("{Value}")]
public sealed class AlbumId : EntityId<Guid>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AlbumId"/> class.
    /// </summary>
    /// <param name="value">The value representing this object.</param>
    private AlbumId(Guid value) : base(value)
    {
    }

    /// <summary>
    /// Creates a new instance of the <see cref="AlbumId"/> class.
    /// </summary>
    /// <returns>The created <see cref="AlbumId"/> instance.</returns>
    public static AlbumId CreateUnique()
    {
        return new AlbumId(Guid.NewGuid());
    }

    /// <summary>
    /// Creates a new instance of the <see cref="AlbumId"/> class, from a pre-existing <paramref name="value"/>.
    /// </summary>
    /// <param name="value">The value used to create the <see cref="AlbumId"/> instance.</param>
    /// <returns>The created <see cref="AlbumId"/> instance.</returns>
    public static AlbumId Create(Guid value)
    {
        return new AlbumId(value);
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
