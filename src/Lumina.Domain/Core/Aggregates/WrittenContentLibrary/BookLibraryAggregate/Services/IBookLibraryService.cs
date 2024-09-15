#region ========================================================================= USING =====================================================================================
using System.Collections.Generic;
#endregion

namespace Lumina.Domain.Core.Aggregates.WrittenContentLibrary.BookLibraryAggregate.Services;

/// <summary>
/// Interface for the service for managing book libraries.
/// </summary>
public interface IBookLibraryService
{
    #region ==================================================================== PROPERTIES =================================================================================
    /// <summary>
    /// Gets the collection of books in the library.
    /// </summary>
    ICollection<Book> Books { get; }
    
    // ICollection<BookSeries> Series { get; }
    #endregion

    // void AddBook(Book book);
    // void RemoveBook(Guid bookId);
    // void AddSeries(BookSeries series);
    // void RemoveSeries(Guid seriesId);
    // Book GetBook(Guid bookId);
    // BookSeries GetSeries(Guid seriesId);
}