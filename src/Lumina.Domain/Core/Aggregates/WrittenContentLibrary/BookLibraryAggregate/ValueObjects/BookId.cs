#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Models.Core;
#endregion

namespace Lumina.Domain.Core.Aggregates.WrittenContentLibrary.BookLibraryAggregate.ValueObjects;

/// <summary>
/// Value Object for the Id of a book.
/// </summary>
public sealed class BookId : ValueObject
{
    #region ==================================================================== PROPERTIES =================================================================================
    public Guid Value { get; }
    #endregion

    #region ====================================================================== CTOR =====================================================================================
    /// <summary>
    /// Initializes a new instance of the <see cref="BookId"/> class.
    /// </summary>
    /// <param name="value">The value representing this object.</param>
    private BookId(Guid value)
    {
        Value = value;
    }
    #endregion

    #region ===================================================================== METHODS ===================================================================================
    /// <summary>
    /// Creates a new instance of the <see cref="BookId"/> class.
    /// </summary>
    /// <returns>The created <see cref="BookId"/> instance.</returns>
    public static BookId CreateUnique()
    {
        // TODO: enforce invariants
        return new BookId(Guid.NewGuid());
    }

    /// <summary>
    /// Creates a new instance of the <see cref="BookId"/> class, from a pre-existing <paramref name="value"/>.
    /// </summary>
    /// <param name="value">The value used to create the <see cref="BookId"/> instance.</param>
    /// <returns>The created <see cref="BookId"/> instance.</returns>
    public static BookId Create(Guid value)
    {
        // TODO: enforce invariants
        return new BookId(value);
    }

    /// <inheritdoc/>
    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
    #endregion
}