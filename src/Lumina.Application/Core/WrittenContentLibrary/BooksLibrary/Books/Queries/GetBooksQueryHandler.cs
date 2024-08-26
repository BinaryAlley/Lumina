#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.DataAccess.Repositories.Books;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Domain.Core.Aggregates.WrittenContentLibrary.BookLibraryAggregate;
using MapsterMapper;
using Mediator;
#endregion

namespace Lumina.Application.Core.WrittenContentLibrary.BooksLibrary.Books.Queries;

/// <summary>
/// Handler for the query to get all books.
/// </summary>
public class GetBooksQueryHandler : IRequestHandler<GetBooksQuery, ErrorOr<IEnumerable<Book>>>
{
    #region ================================================================== FIELD MEMBERS ================================================================================
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    #endregion

    #region ====================================================================== CTOR =====================================================================================
    /// <summary>
    /// Initializes a new instance of the <see cref="GetBooksQueryHandler"/> class.
    /// </summary>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    /// <param name="mapper">Injected service for mapping objects.</param>
    public GetBooksQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }
    #endregion

    #region ===================================================================== METHODS ===================================================================================
    /// <summary>
    /// Handles the query to get all books.
    /// </summary>
    /// <param name="request">The request to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="ErrorOr{T}"/> containing either a collection of <see cref="Book"/>, or an error message.
    /// </returns>
    public async ValueTask<ErrorOr<IEnumerable<Book>>> Handle(GetBooksQuery request, CancellationToken cancellationToken)
    {
        var bookRepository = _unitOfWork.GetRepository<IBookRepository>();
        var getBooksResult = await bookRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
        if (getBooksResult.IsError)
            return getBooksResult.Errors;
        return ErrorOrFactory.From(getBooksResult.Value.Select(book => _mapper.Map<Book>(book)));
    }
    #endregion
}
