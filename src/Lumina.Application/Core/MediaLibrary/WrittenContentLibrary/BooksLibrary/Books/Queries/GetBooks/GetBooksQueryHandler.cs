#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Common.DataAccess.Repositories.Books;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Mapping.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Contracts.Responses.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BooksLibrary.Books.Queries.GetBooks;

/// <summary>
/// Handler for the query to get all books.
/// </summary>
public class GetBooksQueryHandler : IQueryHandler<GetBooksQuery, Result<IEnumerable<BookResponse>>>
{
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetBooksQueryHandler"/> class.
    /// </summary>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    public GetBooksQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Handles the query to get all books.
    /// </summary>
    /// <param name="query">The query to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a collection of <see cref="BookResponse"/>, or an error message.
    /// </returns>
    public async Task<Result<IEnumerable<BookResponse>>> HandleAsync(GetBooksQuery query, CancellationToken cancellationToken)
    {
        // get a books repository
        IBookRepository bookRepository = _unitOfWork.GetRepository<IBookRepository>();
        // get all books of the media library from the book repository
        Result<IEnumerable<BookEntity>> getBooksResult = await bookRepository.GetByLibraryIdAsync(query.LibraryId, cancellationToken).ConfigureAwait(false);
        return getBooksResult.Match(result => Result.From(getBooksResult.Value.ToResponses()), errors => errors);
    }
}
