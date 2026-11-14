#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Infrastructure.Authorization.Policies.LibraryOwnership;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Mapping.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Contracts.Responses.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Domain.Common.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
#endregion

namespace Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Queries.GetDeletedBooks;

/// <summary>
/// Handler for the query to get the deleted books of a media library.
/// </summary>
public class GetDeletedBooksQueryHandler : IQueryHandler<GetDeletedBooksQuery, Result<IReadOnlyList<DeletedBookResponse>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthorizationService _authorizationService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IValidator<GetDeletedBooksQuery> _validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetDeletedBooksQueryHandler"/> class.
    /// </summary>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    /// <param name="authorizationService">Injected service for authorization related functionality.</param>
    /// <param name="currentUserService">Injected service to retrieve the current user information.</param>
    /// <param name="validator">Injected validator for application validation rules.</param>
    public GetDeletedBooksQueryHandler(IUnitOfWork unitOfWork, IAuthorizationService authorizationService, ICurrentUserService currentUserService, IValidator<GetDeletedBooksQuery> validator)
    {
        _unitOfWork = unitOfWork;
        _authorizationService = authorizationService;
        _currentUserService = currentUserService;
        _validator = validator;
    }

    /// <summary>
    /// Handles the query to get the deleted books of a media library.
    /// </summary>
    /// <param name="query">The query to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the deleted books, or an error.</returns>
    public async Task<Result<IReadOnlyList<DeletedBookResponse>>> HandleAsync(GetDeletedBooksQuery query, CancellationToken cancellationToken)
    {
        List<Error> validationResult = _validator.Validate(query);
        if (validationResult.Count > 0)
            return validationResult;

        // An authenticated request must always carry a user identity.
        Guid? currentUserId = _currentUserService.UserId;
        if (currentUserId is null)
            return ApplicationErrors.Authorization.NotAuthorized;
        Guid userId = currentUserId.Value;

        // The validator guarantees that the library id of the route is a non-empty Guid before this point.
        Guid libraryId = Guid.Parse(query.LibraryId!);

        // Admins can see all libraries; for everyone else, only the libraries they own.
        bool canAccessLibrary = await _authorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(
            userId, new LibraryOwnershipPolicyContext(libraryId), cancellationToken).ConfigureAwait(false);
        if (!canAccessLibrary)
            return ApplicationErrors.Authorization.NotAuthorized;

        Result<IReadOnlyList<BookEntity>> getDeletedBooksResult = await _unitOfWork.BookRepository.GetSoftDeletedByLibraryIdAsync(libraryId, cancellationToken).ConfigureAwait(false);
        if (getDeletedBooksResult.IsFailure)
            return getDeletedBooksResult.Errors;

        return Result.From<IReadOnlyList<DeletedBookResponse>>([.. getDeletedBooksResult.Value.Select(book => book.ToResponse(book.DeletedOnUtc ?? book.CreatedOnUtc))]);
    }
}
