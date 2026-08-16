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
using Lumina.Contracts.Responses.Common;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Errors;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DTO.Pagination;
#endregion

namespace Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BooksLibrary.Books.Queries.GetBooks;

/// <summary>
/// Handler for the query to get all books.
/// </summary>
public class GetBooksQueryHandler : IQueryHandler<GetBooksQuery, Result<PaginatedResponse<BookResponse>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthorizationService _authorizationService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IValidator<GetBooksQuery> _validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetBooksQueryHandler"/> class.
    /// </summary>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    /// <param name="authorizationService">Injected service for authorization related functionality.</param>
    /// <param name="currentUserService">Injected service to retrieve the current user information.</param>
    /// <param name="validator">Injected validator for application validation rules.</param>
    public GetBooksQueryHandler(IUnitOfWork unitOfWork, IAuthorizationService authorizationService, ICurrentUserService currentUserService, IValidator<GetBooksQuery> validator)
    {
        _unitOfWork = unitOfWork;
        _authorizationService = authorizationService;
        _currentUserService = currentUserService;
        _validator = validator;
    }

    /// <summary>
    /// Handles the query to get all books.
    /// </summary>
    /// <param name="query">The query to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a collection of <see cref="BookResponse"/>, or an error message.
    /// </returns>
    public async Task<Result<PaginatedResponse<BookResponse>>> HandleAsync(GetBooksQuery query, CancellationToken cancellationToken)
    {
        List<Error> validationResult = _validator.Validate(query);
        if (validationResult.Count > 0)
            return validationResult;

        // check if the user has the rights to access the library they are requesting (admins can see all libraries)
        bool isAdmin = await _authorizationService.IsInRoleAsync(_currentUserService.UserId!.Value, "Admin", cancellationToken).ConfigureAwait(false);
        if (!isAdmin) // if its not an admin, then it must own the library
        {
            Result<LibraryEntity?> getLibraryResult = await _unitOfWork.LibraryRepository.GetByIdAsync(query.Filter.LibraryId, cancellationToken).ConfigureAwait(false);
            if (getLibraryResult.IsFailure || getLibraryResult.Value?.UserId != _currentUserService.UserId)
                return Errors.Authorization.NotAuthorized;
        }

        // get all books of the media library from the book repository
        Result<PaginatedResultDto<BookEntity>> getBooksResult = await _unitOfWork.BookRepository.GetPaginatedAsync(
            query.PaginationData,
            query.SortBy,
            query.SortOrder,
            query.Filter,
            cancellationToken).ConfigureAwait(false);
        return getBooksResult.Match(value => Result.From(value.ToResponses()), errors => errors);
    }
}
