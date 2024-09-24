#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.DataAccess.Repositories.Books;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Contracts.Models.WrittenContentLibrary.BookLibrary;
using Lumina.Domain.Core.Aggregates.WrittenContentLibrary.BookLibraryAggregate;
using Mapster;
using Mediator;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Core.WrittenContentLibrary.BooksLibrary.Books.Queries.GetBooks;

/// <summary>
/// Handler for the query to get all books.
/// </summary>
public class GetBooksQueryHandler : IRequestHandler<GetBooksQuery, ErrorOr<IEnumerable<Book>>>
{
    #region ================================================================== FIELD MEMBERS ================================================================================
    private readonly IUnitOfWork _unitOfWork;
    #endregion

    #region ====================================================================== CTOR =====================================================================================
    /// <summary>
    /// Initializes a new instance of the <see cref="GetBooksQueryHandler"/> class.
    /// </summary>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    public GetBooksQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
    #endregion

    #region ===================================================================== METHODS ===================================================================================
    /// <summary>
    /// Handles the query to get all books.
    /// </summary>
    /// <param name="request">The request to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="ErrorOr{TValue}"/> containing either a collection of <see cref="Book"/>, or an error message.
    /// </returns>
    public async ValueTask<ErrorOr<IEnumerable<Book>>> Handle(GetBooksQuery request, CancellationToken cancellationToken)
    {
        IBookRepository bookRepository = _unitOfWork.GetRepository<IBookRepository>();
        ErrorOr<IEnumerable<BookModel>> getBooksResult = await bookRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
        return getBooksResult.Match(values => ErrorOrFactory.From(values.Adapt<IEnumerable<Book>>()), errors => errors);
    }
    #endregion
}
