#region ========================================================================= USING =====================================================================================
using Nemera.Domain.Common.Primitives;
using Nemera.Application.Common.CQRS;
using Nemera.Application.Common.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Nemera.Application.Common.DataAccess.Repositories.BookLibrary;
using Nemera.Application.Common.DataAccess.UoW;
using Nemera.Application.Common.Mapping.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Nemera.Contracts.Responses.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nemera.Contracts.Responses.Common;
using Nemera.Application.Common.Infrastructure.Authentication;
using Nemera.Application.Common.Infrastructure.Authorization;
using Nemera.Application.Common.Infrastructure.Authorization.Policies.LibraryOwnership;
using Nemera.Application.Common.Infrastructure.Validation;
using Nemera.Application.Common.Errors;
using Nemera.Application.Common.DTO.Filtering;
using Nemera.Application.Common.DTO.Pagination;
#endregion

namespace Nemera.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Queries.GetBooksLite;

/// <summary>
/// Handler for the query to get the lightweight details of all books.
/// </summary>
public class GetBooksLiteQueryHandler : IQueryHandler<GetBooksLiteQuery, Result<PaginatedResponse<BookLiteResponse>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthorizationService _authorizationService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IValidator<GetBooksLiteQuery> _validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetBooksLiteQueryHandler"/> class.
    /// </summary>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    /// <param name="authorizationService">Injected service for authorization related functionality.</param>
    /// <param name="currentUserService">Injected service to retrieve the current user information.</param>
    /// <param name="validator">Injected validator for application validation rules.</param>
    public GetBooksLiteQueryHandler(IUnitOfWork unitOfWork, IAuthorizationService authorizationService, ICurrentUserService currentUserService, IValidator<GetBooksLiteQuery> validator)
    {
        _unitOfWork = unitOfWork;
        _authorizationService = authorizationService;
        _currentUserService = currentUserService;
        _validator = validator;
    }

    /// <summary>
    /// Handles the query to get the lightweight details of all books.
    /// </summary>
    /// <param name="query">The query to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a collection of <see cref="BookLiteResponse"/>, or an error message.
    /// </returns>
    public async Task<Result<PaginatedResponse<BookLiteResponse>>> HandleAsync(GetBooksLiteQuery query, CancellationToken cancellationToken)
    {
        List<Error> validationResult = _validator.Validate(query);
        if (validationResult.Count > 0)
            return validationResult;

        // An authenticated request must always carry a user identity.
        Guid? currentUserId = _currentUserService.UserId;
        if (currentUserId is null)
            return Errors.Authorization.NotAuthorized;
        Guid userId = currentUserId.Value;

        // The validator guarantees that the library id of the route is a non-empty Guid before this point.
        Guid libraryId = Guid.Parse(query.LibraryId!);

        // Admins can see all libraries; for everyone else, only the libraries they own.
        bool canAccessLibrary = await _authorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(
            userId, new LibraryOwnershipPolicyContext(libraryId), cancellationToken).ConfigureAwait(false);
        if (!canAccessLibrary)
            return Errors.Authorization.NotAuthorized;

        LibraryFilterDto libraryFilter = new()
        {
            LibraryId = libraryId,
            SearchTerm = query.SearchTerm,
            FilterAlphaKey = query.FilterAlphaKey,
            ShouldIgnoreThePrefixForAlphaPicker = query.ShouldIgnoreThePrefixForAlphaPicker
        };

        // Get the lightweight read models of the books of the media library from the book repository, projecting only the fields needed for a grid or a list.
        Result<PaginatedResultDto<BookLiteRow>> getBooksResult = await _unitOfWork.BookRepository.GetAllLiteAsync(
            query.PaginationData,
            query.SortBy,
            query.SortOrder,
            libraryFilter,
            shouldTrackEntities: false,
            cancellationToken: cancellationToken).ConfigureAwait(false);
        return getBooksResult.Match(value => Result.From(value.ToResponses()), errors => errors);
    }
}
