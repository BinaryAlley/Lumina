#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.DataAccess.Repositories.Books;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Mapping.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Contracts.Entities.WrittenContentLibrary.BookLibrary;
using Lumina.Contracts.Responses.WrittenContentLibrary.BookLibrary.Books;
using Mediator;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Core.WrittenContentLibrary.BooksLibrary.Books.Queries.GetBooks;

/// <summary>
/// Handler for the query to get all books.
/// </summary>
public class GetBooksQueryHandler : IRequestHandler<GetBooksQuery, ErrorOr<IEnumerable<BookResponse>>>
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
    /// <param name="request">The request to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="ErrorOr{TValue}"/> containing either a collection of <see cref="BookResponse"/>, or an error message.
    /// </returns>
    public async ValueTask<ErrorOr<IEnumerable<BookResponse>>> Handle(GetBooksQuery request, CancellationToken cancellationToken)
    {
        // get a books repository
        IBookRepository bookRepository = _unitOfWork.GetRepository<IBookRepository>();
        // get all books from the book repository
        ErrorOr<IEnumerable<BookEntity>> getBooksResult = await bookRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
        return getBooksResult.Match(result => ErrorOrFactory.From(getBooksResult.Value.ToResponses()), errors => errors);
    }
}
