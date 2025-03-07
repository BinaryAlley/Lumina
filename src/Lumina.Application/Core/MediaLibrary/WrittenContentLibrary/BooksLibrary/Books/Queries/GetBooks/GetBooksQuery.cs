#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Contracts.Responses.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Mediator;
using System.Collections.Generic;
#endregion

namespace Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BooksLibrary.Books.Queries.GetBooks;

/// <summary>
/// Query for getting all books.
/// </summary>
public record class GetBooksQuery() : IRequest<ErrorOr<IEnumerable<BookResponse>>>;
