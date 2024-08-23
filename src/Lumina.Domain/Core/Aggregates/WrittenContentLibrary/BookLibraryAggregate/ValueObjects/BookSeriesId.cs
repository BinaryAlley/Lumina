#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Domain.Common.Models.Core;
#endregion

namespace Lumina.Domain.Core.Aggregates.WrittenContentLibrary.BookLibraryAggregate.ValueObjects;

/// <summary>
/// Value Object for the Id of a book series.
/// </summary>
public sealed class BookSeriesId : ValueObject
{
    #region ==================================================================== PROPERTIES =================================================================================
    public Guid Value { get; }
    #endregion

    #region ====================================================================== CTOR =====================================================================================
    /// <summary>
    /// Initializes a new instance of the <see cref="BookSeriesId"/> class.
    /// </summary>
    /// <param name="value">The value representing this object.</param>
    private BookSeriesId(Guid value)
    {
        Value = value;
    }
    #endregion

    #region ===================================================================== METHODS ===================================================================================
    /// <summary>
    /// Creates a new instance of the <see cref="BookSeriesId"/> class.
    /// </summary>
    /// <returns>The created <see cref="BookSeriesId"/> instance.</returns>
    public static ErrorOr<BookSeriesId> CreateUnique()
    {
        // TODO: enforce invariants
        return new BookSeriesId(Guid.NewGuid());
    }

    /// <summary>
    /// Creates a new instance of the <see cref="BookSeriesId"/> class, from a pre-existing <paramref name="value"/>.
    /// </summary>
    /// <param name="value">The value used to create the <see cref="BookSeriesId"/> instance.</param>
    /// <returns>The created <see cref="BookSeriesId"/> instance.</returns>
    public static ErrorOr<BookSeriesId> Create(Guid value)
    {
        // TODO: enforce invariants
        return new BookSeriesId(value);
    }

    /// <inheritdoc/>
    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
    #endregion
}