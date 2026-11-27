#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.DTO.Favorites;
using Lumina.Application.Common.DTO.Pagination;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Infrastructure.Authorization.Policies.LibraryOwnership;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Mapping.Favorites;
using Lumina.Contracts.Responses.Common;
using Lumina.Contracts.Responses.Favorites;
using Lumina.Domain.Common.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
#endregion

namespace Lumina.Application.Core.Favorites.Queries.GetFavorites;

/// <summary>
/// Handler for the query to get a window of the favorites of the current user.
/// </summary>
public class GetFavoritesQueryHandler : IQueryHandler<GetFavoritesQuery, Result<PaginatedResponse<FavoriteItemResponse>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuthorizationService _authorizationService;
    private readonly IValidator<GetFavoritesQuery> _validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetFavoritesQueryHandler"/> class.
    /// </summary>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    /// <param name="currentUserService">Injected service to retrieve the current user information.</param>
    /// <param name="authorizationService">Injected service for authorization related functionality.</param>
    /// <param name="validator">Injected validator for application validation rules.</param>
    public GetFavoritesQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService, IAuthorizationService authorizationService, IValidator<GetFavoritesQuery> validator)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _authorizationService = authorizationService;
        _validator = validator;
    }

    /// <summary>
    /// Handles the query to get a window of the favorites of the current user.
    /// </summary>
    /// <param name="query">The query to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either the requested page of favorites, or an error message.
    /// </returns>
    public async Task<Result<PaginatedResponse<FavoriteItemResponse>>> HandleAsync(GetFavoritesQuery query, CancellationToken cancellationToken)
    {
        List<Error> validationResult = _validator.Validate(query);
        if (validationResult.Count > 0)
            return validationResult;

        // An authenticated request must always carry a user identity.
        Guid? currentUserId = _currentUserService.UserId;
        if (currentUserId is null)
            return ApplicationErrors.Authorization.NotAuthorized;
        Guid userId = currentUserId.Value;

        // The favorites are scoped to the current user by the repository.
        Result<PaginatedResultDto<FavoriteItemDto>> getFavoritesResult = await _unitOfWork.FavoriteRepository
            .GetAllForUserAsync(userId, query.MediaKind, query.PaginationData, cancellationToken: cancellationToken).ConfigureAwait(false);
        if (getFavoritesResult.IsFailure)
            return getFavoritesResult.Errors;

        // Check the access permissions once for each library that the favorited items might belong to.
        PaginatedResultDto<FavoriteItemDto> page = getFavoritesResult.Value;
        List<Guid> libraryIds = [.. page.Data
            .Select(item => item.Display?.LibraryId ?? Guid.Empty)
            .Where(libraryId => libraryId != Guid.Empty)
            .Distinct()];
        Dictionary<Guid, bool> libraryAccess = [];
        foreach (Guid libraryId in libraryIds)
        {
            bool canAccessLibrary = await _authorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(
                userId, new LibraryOwnershipPolicyContext(libraryId), cancellationToken).ConfigureAwait(false);
            libraryAccess[libraryId] = canAccessLibrary;
        }

        if (libraryAccess.Values.Any(canAccessLibrary => !canAccessLibrary))
        {
            List<FavoriteItemDto> items = [];
            foreach (FavoriteItemDto item in page.Data)
            {
                Guid libraryId = item.Display?.LibraryId ?? Guid.Empty;
                if (libraryId == Guid.Empty || libraryAccess[libraryId])
                    items.Add(item);
                else // Keep the row so its owner can see and remove it, but do not expose display data from a library the user cannot access.
                    items.Add(item with { Display = null });
            }
            page = page with { Data = items };
        }

        return Result.From(page.ToResponses());
    }
}
