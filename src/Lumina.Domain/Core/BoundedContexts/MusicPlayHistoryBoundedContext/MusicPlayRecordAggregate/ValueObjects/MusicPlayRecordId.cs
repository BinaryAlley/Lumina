#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Models.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.MusicPlayHistoryBoundedContext.MusicPlayRecordAggregate.ValueObjects;

/// <summary>
/// Value Object for the Id of a music play record.
/// </summary>
[DebuggerDisplay("{Value}")]
public sealed class MusicPlayRecordId : EntityId<Guid>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MusicPlayRecordId"/> class.
    /// </summary>
    /// <param name="value">The value representing this object.</param>
    private MusicPlayRecordId(Guid value) : base(value)
    {
    }

    /// <summary>
    /// Creates a new instance of the <see cref="MusicPlayRecordId"/> class.
    /// </summary>
    /// <returns>The created <see cref="MusicPlayRecordId"/> instance.</returns>
    public static MusicPlayRecordId CreateUnique()
    {
        return new MusicPlayRecordId(Guid.NewGuid());
    }

    /// <summary>
    /// Creates a new instance of the <see cref="MusicPlayRecordId"/> class, from a pre-existing <paramref name="value"/>.
    /// </summary>
    /// <param name="value">The value used to create the <see cref="MusicPlayRecordId"/> instance.</param>
    /// <returns>The created <see cref="MusicPlayRecordId"/> instance.</returns>
    public static MusicPlayRecordId Create(Guid value)
    {
        return new MusicPlayRecordId(value);
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
