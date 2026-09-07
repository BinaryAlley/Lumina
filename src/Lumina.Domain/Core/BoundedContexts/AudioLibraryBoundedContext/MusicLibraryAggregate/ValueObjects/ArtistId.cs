#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Models.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;

/// <summary>
/// Value Object for the Id of an artist.
/// </summary>
[DebuggerDisplay("{Value}")]
public sealed class ArtistId : EntityId<Guid>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ArtistId"/> class.
    /// </summary>
    /// <param name="value">The value representing this object.</param>
    private ArtistId(Guid value) : base(value)
    {
    }

    /// <summary>
    /// Creates a new instance of the <see cref="ArtistId"/> class.
    /// </summary>
    /// <returns>The created <see cref="ArtistId"/> instance.</returns>
    public static ArtistId CreateUnique()
    {
        return new ArtistId(Guid.NewGuid());
    }

    /// <summary>
    /// Creates a new instance of the <see cref="ArtistId"/> class, from a pre-existing <paramref name="value"/>.
    /// </summary>
    /// <param name="value">The value used to create the <see cref="ArtistId"/> instance.</param>
    /// <returns>The created <see cref="ArtistId"/> instance.</returns>
    public static ArtistId Create(Guid value)
    {
        return new ArtistId(value);
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
