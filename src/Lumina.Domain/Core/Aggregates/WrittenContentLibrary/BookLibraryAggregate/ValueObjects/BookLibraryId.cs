#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Domain.Common.Models.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Domain.Core.Aggregates.WrittenContentLibrary.BookLibraryAggregate.ValueObjects;

/// <summary>
/// Value Object for the Id of a book library.
/// </summary>
[DebuggerDisplay("{Value}")]
public sealed class BookLibraryId : EntityId<Guid>
{
    #region ====================================================================== CTOR =====================================================================================
    /// <summary>
    /// Initializes a new instance of the <see cref="BookLibraryId"/> class.
    /// </summary>
    /// <param name="value">The value representing this object.</param>
    private BookLibraryId(Guid value) : base(value)
    {
    }
    #endregion

    #region ===================================================================== METHODS ===================================================================================
    /// <summary>
    /// Creates a new instance of the <see cref="BookLibraryId"/> class.
    /// </summary>
    /// <returns>The created <see cref="BookLibraryId"/> instance.</returns>
    public static ErrorOr<BookLibraryId> CreateUnique()
    {
        // TODO: enforce invariants
        return new BookLibraryId(Guid.NewGuid());
    }

    /// <summary>
    /// Creates a new instance of the <see cref="BookLibraryId"/> class, from a pre-existing <paramref name="value"/>.
    /// </summary>
    /// <param name="value">The value used to create the <see cref="BookLibraryId"/> instance.</param>
    /// <returns>The created <see cref="BookLibraryId"/> instance.</returns>
    public static ErrorOr<BookLibraryId> Create(Guid value)
    {
        // TODO: enforce invariants
        return new BookLibraryId(value);
    }

    /// <inheritdoc/>
    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
    #endregion
}