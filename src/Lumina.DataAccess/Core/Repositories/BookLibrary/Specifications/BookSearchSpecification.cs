#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Common.Specifications;
using System;
using System.Linq.Expressions;
#endregion

namespace Lumina.DataAccess.Core.Repositories.BookLibrary.Specifications;

/// <summary>
/// Represents a filter specification for filtering or searching book entities based on a search term.
/// </summary>
internal sealed class BookSearchSpecification : FilterSpecification<BookEntity>
{
    private readonly string _searchTerm;

    /// <summary>
    /// Initializes a new instance of the <see cref="BookSearchSpecification"/> class.
    /// </summary>
    /// <param name="searchTerm">The term to use when filtering or searching for books.</param>
    public BookSearchSpecification(string searchTerm)
    {
        _searchTerm = searchTerm;
    }

    /// <summary>
    /// Creates a LINQ expression that represents the predicate defined by the current specification.
    /// </summary>
    /// <returns>An expression tree that can be used to evaluate whether a <see cref="BookEntity"/> satisfies the specification criteria.</returns>
    public override Expression<Func<BookEntity, bool>> ToExpression()
    {
        // The term is normalized with the invariant culture, so that the case folding never depends on the current culture. The columns are
        // folded by the storage medium, which keeps the search case-insensitive.
        string normalizedSearchTerm = _searchTerm.ToLowerInvariant();
#pragma warning disable IDE0079 // Remove unnecessary suppression.
#pragma warning disable CA1862 // Use the 'StringComparison' method overloads to perform case-insensitive string comparisons. This expression is translated to SQL; the StringComparison overloads are client-side only and are not translated, so they would fail at query time.
        return book => book.Title.ToLower().Contains(normalizedSearchTerm)
            || (book.OriginalTitle != null && book.OriginalTitle.ToLower().Contains(normalizedSearchTerm));
#pragma warning restore CA1862 // Use the 'StringComparison' method overloads to perform case-insensitive string comparisons.
#pragma warning restore IDE0079 // Remove unnecessary suppression.
    }
}
