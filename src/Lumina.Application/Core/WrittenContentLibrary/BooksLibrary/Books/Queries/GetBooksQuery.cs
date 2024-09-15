#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Domain.Core.Aggregates.WrittenContentLibrary.BookLibraryAggregate;
using Mediator;
using System.Collections.Generic;
#endregion

namespace Lumina.Application.Core.WrittenContentLibrary.BooksLibrary.Books.Queries;

/// <summary>
/// Query for getting all books.
/// </summary>
public record class GetBooksQuery() : IRequest<ErrorOr<IEnumerable<Book>>>;