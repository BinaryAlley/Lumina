#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Contracts.Responses.Favorites;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.Favorites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
#endregion

namespace Lumina.Application.Core.Favorites.Queries.GetFavoriteStatus;

/// <summary>
/// Handler for the query to find out which media items of one kind are favorites of the current user.
/// </summary>
public class GetFavoriteStatusQueryHandler : IQueryHandler<GetFavoriteStatusQuery, Result<FavoriteStatusResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IValidator<GetFavoriteStatusQuery> _validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetFavoriteStatusQueryHandler"/> class.
    /// </summary>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    /// <param name="currentUserService">Injected service to retrieve the current user information.</param>
    /// <param name="validator">Injected validator for application validation rules.</param>
    public GetFavoriteStatusQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService, IValidator<GetFavoriteStatusQuery> validator)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _validator = validator;
    }

    /// <summary>
    /// Handles the query to find out which media items of one kind are favorites of the current user.
    /// </summary>
    /// <param name="query">The query to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either the requested <see cref="FavoriteStatusResponse"/>, or an error message.
    /// </returns>
    public async Task<Result<FavoriteStatusResponse>> HandleAsync(GetFavoriteStatusQuery query, CancellationToken cancellationToken)
    {
        List<Error> validationResult = _validator.Validate(query);
        if (validationResult.Count > 0)
            return validationResult;

        // An authenticated request must always carry a user identity.
        Guid? currentUserId = _currentUserService.UserId;
        if (currentUserId is null)
            return ApplicationErrors.Authorization.NotAuthorized;
        Guid userId = currentUserId.Value;

        // The validator guarantees that the media kind is a defined value and the identifiers are a non-empty collection of non-empty Guids before this point.
        FavoriteMediaKind mediaKind = query.MediaKind!.Value;
        List<Guid> mediaItemIds = [.. query.MediaItemIds!.Distinct()];

        // Only the favorites of the current user are checked, so no library access is evaluated: the answer reveals nothing about other users.
        Result<IReadOnlyList<Guid>> getFavoritedIdsResult = await _unitOfWork.FavoriteRepository
            .GetFavoritedMediaItemIdsForUserAsync(userId, mediaKind, mediaItemIds, cancellationToken).ConfigureAwait(false);
        if (getFavoritedIdsResult.IsFailure)
            return getFavoritedIdsResult.Errors;

        return new FavoriteStatusResponse(getFavoritedIdsResult.Value);
    }
}
