#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Contracts.Responses.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Mediator;
using System;
using System.Collections.Generic;
#endregion

namespace Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BooksLibrary.Books.Queries.GetBooks;

/// <summary>
/// Query for getting all the books of a media library.
/// </summary>
/// <param name="LibraryId">The Id of the media library whose books are retrieved.</param>
public record GetBooksQuery(
    Guid LibraryId
) : IRequest<ErrorOr<IEnumerable<BookResponse>>>;
