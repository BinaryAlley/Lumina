#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.DataAccess.Entities.MediaContributors;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Infrastructure.Authorization.Policies.LibraryOwnership;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Mapping.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Contracts.DTO.MediaContributors;
using Lumina.Contracts.Responses.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Domain.Common.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
using DomainErrors = Lumina.Domain.Common.Errors.Errors;
#endregion

namespace Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Queries.GetBook;

/// <summary>
/// Handler for the query to get a book by its Id.
/// </summary>
public class GetBookQueryHandler : IQueryHandler<GetBookQuery, Result<BookResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthorizationService _authorizationService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IValidator<GetBookQuery> _validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetBookQueryHandler"/> class.
    /// </summary>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    /// <param name="authorizationService">Injected service for authorization related functionality.</param>
    /// <param name="currentUserService">Injected service to retrieve the current user information.</param>
    /// <param name="validator">Injected validator for application validation rules.</param>
    public GetBookQueryHandler(IUnitOfWork unitOfWork, IAuthorizationService authorizationService, ICurrentUserService currentUserService, IValidator<GetBookQuery> validator)
    {
        _unitOfWork = unitOfWork;
        _authorizationService = authorizationService;
        _currentUserService = currentUserService;
        _validator = validator;
    }

    /// <summary>
    /// Handles the query to get a book by its Id.
    /// </summary>
    /// <param name="query">The query to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either the requested <see cref="BookResponse"/>, or an error message.
    /// </returns>
    public async Task<Result<BookResponse>> HandleAsync(GetBookQuery query, CancellationToken cancellationToken)
    {
        List<Error> validationResult = _validator.Validate(query);
        if (validationResult.Count > 0)
            return validationResult;

        // An authenticated request must always carry a user identity.
        Guid? currentUserId = _currentUserService.UserId;
        if (currentUserId is null)
            return ApplicationErrors.Authorization.NotAuthorized;
        Guid userId = currentUserId.Value;

        // Get the book with the specified id from the repository.
        Result<BookEntity?> getBookResult = await _unitOfWork.BookRepository.GetByIdAsync(query.Id, cancellationToken).ConfigureAwait(false);
        if (getBookResult.IsFailure)
            return getBookResult.Errors;
        if (getBookResult.Value is null)
            return DomainErrors.WrittenContent.BookNotFound;

        // Admins can see the books of all libraries; for everyone else, only the books of the libraries they own.
        bool canAccessLibrary = await _authorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(
            userId, new LibraryOwnershipPolicyContext(getBookResult.Value.LibraryId), cancellationToken).ConfigureAwait(false);
        if (!canAccessLibrary)
            return ApplicationErrors.Authorization.NotAuthorized;

        BookResponse response = getBookResult.Value.ToResponse();

        // The response carries the resolved contributor names, so that the clients can display them without querying the contributors themselves.
        List<MediaContributorEntity> contributors = await GetContributorsAsync(getBookResult.Value, cancellationToken).ConfigureAwait(false);
        if (contributors.Count == 0)
            return response;

        Dictionary<Guid, MediaContributorEntity> contributorsById = contributors.ToDictionary(contributor => contributor.Id);
        List<MediaContributorDto> contributorResponses = [];
        foreach (BookContributorEntity participation in getBookResult.Value.BookContributors)
        {
            if (!contributorsById.TryGetValue(participation.MediaContributorId, out MediaContributorEntity? contributor))
                continue;
            contributorResponses.Add(new MediaContributorDto(
                new MediaContributorNameDto(contributor.DisplayName, contributor.LegalName),
                new MediaContributorRoleDto(participation.RoleName, participation.RoleCategory)
            ));
        }

        return response with { Contributors = contributorResponses };
    }

    /// <summary>
    /// Gets the media contributors linked to the book of <paramref name="book"/>, by their unique identifiers.
    /// </summary>
    /// <param name="book">The book whose media contributors are retrieved.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the media contributors of the book, or an error.</returns>
    private async Task<List<MediaContributorEntity>> GetContributorsAsync(BookEntity book, CancellationToken cancellationToken)
    {
        IReadOnlyCollection<Guid> contributorIds = [.. book.BookContributors.Select(participation => participation.MediaContributorId).Distinct()];
        if (contributorIds.Count == 0)
            return [];

        Result<IReadOnlyList<MediaContributorEntity>> getContributorsResult = await _unitOfWork.MediaContributorRepository.GetByIdsAsync(contributorIds, cancellationToken).ConfigureAwait(false);
        if (getContributorsResult.IsFailure)
            return [];
        return [.. getContributorsResult.Value];
    }
}
