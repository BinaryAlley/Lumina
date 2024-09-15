#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Models.Core;
using Lumina.Domain.Core.Aggregates.WrittenContentLibrary.BookLibraryAggregate.ValueObjects;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Domain.Core.Aggregates.WrittenContentLibrary.BookLibraryAggregate.Entities;

/// <summary>
/// Entity for a book series.
/// </summary>
[DebuggerDisplay("{Id}: {Title}")]
public sealed class BookSeries : Entity<BookSeriesId>
{
    #region ================================================================== FIELD MEMBERS ================================================================================
    private readonly List<Book> _books;
    #endregion

    #region ==================================================================== PROPERTIES =================================================================================
    /// <summary>
    /// Gets the written content metadata of the book series.
    /// </summary>
    public WrittenContentMetadata Metadata { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the series is complete.
    /// </summary>
    public bool IsComplete { get; private set; }

    /// <summary>
    /// Gets the collection of books in the series.
    /// </summary>
    public IReadOnlyCollection<Book> Books
    {
        get { return _books.AsReadOnly(); }
    }
    #endregion

    #region ====================================================================== CTOR =====================================================================================
    /// <summary>
    /// Initializes a new instance of the <see cref="BookSeries"/> class.
    /// </summary>
    /// <param name="id">The identifier of the book series.</param>
    /// <param name="metadata">The metadata of the book series.</param>
    /// <param name="isComplete">The current status of the book series.</param>
    /// <param name="books">The books of the book series.</param>
    private BookSeries(BookSeriesId id, WrittenContentMetadata metadata, bool isComplete, List<Book> books)
        : base(id)
    {
        Id = id;
        Metadata = metadata;
        IsComplete = isComplete;
        _books = books;
    }
    #endregion

    #region ===================================================================== METHODS ===================================================================================
    /// <summary>
    /// Creates a new instance of the <see cref="BookSeries"/> class.
    /// </summary>
    /// <param name="id">The identifier of the book series.</param>
    /// <param name="metadata">The metadata of the book series.</param>
    /// <param name="isComplete">The current status of the book series.</param>
    /// <param name="books">The books of the book series.</param>
    /// <returns>
    /// An <see cref="ErrorOr{TValue}"/> containing either a successfully created <see cref="BookSeries"/>, or an error message.
    /// </returns>
    public static ErrorOr<BookSeries> Create(
        BookSeriesId id, 
        WrittenContentMetadata metadata, 
        bool isComplete, 
        List<Book> books)
    {
        // TODO: enforce invariants
        return new BookSeries(
            id,
            metadata,
            isComplete,
            books
        );
    }

    /// <summary>
    /// Creates a new instance of the <see cref="BookSeries"/> class.
    /// </summary>
    /// <param name="metadata">The metadata of the book series.</param>
    /// <param name="isComplete">The current status of the book series.</param>
    /// <param name="books">The books of the book series.</param>
    /// <returns>
    /// An <see cref="ErrorOr{TValue}"/> containing either a successfully created <see cref="BookSeries"/>, or an error message.
    /// </returns>
    public static ErrorOr<BookSeries> Create(
        WrittenContentMetadata metadata,
        bool isComplete,
        List<Book> books)
    {
        // TODO: enforce invariants
        return new BookSeries(
            BookSeriesId.CreateUnique(),
            metadata,
            isComplete,
            books
        );
    }
    /// <summary>
    /// Adds a book to the series.
    /// </summary>
    /// <param name="book">The book to be added.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> representing either a successful operation, or an error.</returns>
    public ErrorOr<Created> AddBook(Book book)
    {
        if (_books.Contains(book))
            return Errors.WrittenContent.TheBookIsAlreadyInTheSeries;
        _books.Add(book);
        return Result.Created;
    }

    /// <summary>
    /// Removes a book from the series.
    /// </summary>
    /// <param name="book">The book to be removed.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> representing either a successful operation, or an error.</returns>
    public ErrorOr<Deleted> RemoveBook(Book book)
    {
        if (!_books.Contains(book))
            return Errors.WrittenContent.TheBookIsNotInTheSeries;
        _books.Remove(book);
        return Result.Deleted;
    }
    #endregion
}