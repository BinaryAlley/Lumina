#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Common.Specifications;
using System;
using System.Linq.Expressions;
#endregion

namespace Lumina.DataAccess.Core.Repositories.Books.Specifications;

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
        return book => book.Title.Contains(_searchTerm) || (book.OriginalTitle != null && book.OriginalTitle.Contains(_searchTerm));
    }
}
