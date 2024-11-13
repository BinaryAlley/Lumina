#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Models.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;

/// <summary>
/// Value Object for the Id of a media library.
/// </summary>
[DebuggerDisplay("{Value}")]
public sealed class LibraryId : EntityId<Guid>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LibraryId"/> class.
    /// </summary>
    /// <param name="value">The value representing this object.</param>
    private LibraryId(Guid value) : base(value)
    {
    }

    /// <summary>
    /// Creates a new instance of the <see cref="LibraryId"/> class.
    /// </summary>
    /// <returns>The created <see cref="LibraryId"/> instance.</returns>
    public static LibraryId CreateUnique()
    {
        return new LibraryId(Guid.NewGuid());
    }

    /// <summary>
    /// Creates a new instance of the <see cref="LibraryId"/> class, from a pre-existing <paramref name="value"/>.
    /// </summary>
    /// <param name="value">The value used to create the <see cref="LibraryId"/> instance.</param>
    /// <returns>The created <see cref="LibraryId"/> instance.</returns>
    public static LibraryId Create(Guid value)
    {
        return new LibraryId(value);
    }

    /// <inheritdoc/>
    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
