#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Models.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Domain.Core.Aggregates.WrittenContentLibrary.BookLibraryAggregate.ValueObjects;

/// <summary>
/// Value Object for the Id of a book.
/// </summary>
[DebuggerDisplay("{Value}")]
public sealed class BookId : EntityId<Guid>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BookId"/> class.
    /// </summary>
    /// <param name="value">The value representing this object.</param>
    private BookId(Guid value) : base(value)
    {
    }

    /// <summary>
    /// Creates a new instance of the <see cref="BookId"/> class.
    /// </summary>
    /// <returns>The created <see cref="BookId"/> instance.</returns>
    public static BookId CreateUnique()
    {
        return new BookId(Guid.NewGuid());
    }

    /// <summary>
    /// Creates a new instance of the <see cref="BookId"/> class, from a pre-existing <paramref name="value"/>.
    /// </summary>
    /// <param name="value">The value used to create the <see cref="BookId"/> instance.</param>
    /// <returns>The created <see cref="BookId"/> instance.</returns>
    public static BookId Create(Guid value)
    {
        return new BookId(value);
    }

    /// <inheritdoc/>
    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}