#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Models.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Domain.Core.Aggregates.WrittenContentLibrary.BookLibraryAggregate.ValueObjects;

/// <summary>
/// Value Object for the Id of a book series.
/// </summary>
[DebuggerDisplay("{Value}")]
public sealed class BookSeriesId : ValueObject
{
    public Guid Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="BookSeriesId"/> class.
    /// </summary>
    /// <param name="value">The value representing this object.</param>
    private BookSeriesId(Guid value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a new instance of the <see cref="BookSeriesId"/> class.
    /// </summary>
    /// <returns>The created <see cref="BookSeriesId"/> instance.</returns>
    public static BookSeriesId CreateUnique()
    {
        return new BookSeriesId(Guid.NewGuid());
    }

    /// <summary>
    /// Creates a new instance of the <see cref="BookSeriesId"/> class, from a pre-existing <paramref name="value"/>.
    /// </summary>
    /// <param name="value">The value used to create the <see cref="BookSeriesId"/> instance.</param>
    /// <returns>The created <see cref="BookSeriesId"/> instance.</returns>
    public static BookSeriesId Create(Guid value)
    {
        return new BookSeriesId(value);
    }

    /// <inheritdoc/>
    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}