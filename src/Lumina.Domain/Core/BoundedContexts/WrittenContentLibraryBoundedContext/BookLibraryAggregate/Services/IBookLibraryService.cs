#region ========================================================================= USING =====================================================================================
using System.Collections.Generic;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate.Services;

/// <summary>
/// Interface for the service for managing book libraries.
/// </summary>
public interface IBookLibraryService
{
    /// <summary>
    /// Gets the collection of books in the library.
    /// </summary>
    ICollection<Book> Books { get; }

    // ICollection<BookSeries> Series { get; }

    // void AddBook(Book book);
    // void RemoveBook(Guid bookId);
    // void AddSeries(BookSeries series);
    // void RemoveSeries(Guid seriesId);
    // Book GetBook(Guid bookId);
    // BookSeries GetSeries(Guid seriesId);
}