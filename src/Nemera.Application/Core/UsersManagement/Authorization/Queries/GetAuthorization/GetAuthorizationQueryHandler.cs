#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.CQRS;
using Nemera.Application.Common.DataAccess.Entities.Authorization;
using Nemera.Application.Common.Errors;
using Nemera.Application.Common.Infrastructure.Authentication;
using Nemera.Application.Common.Infrastructure.Authorization;
using Nemera.Application.Common.Infrastructure.Validation;
using Nemera.Application.Common.Mapping.Authorization;
using Nemera.Contracts.Responses.Authorization;
using Nemera.Domain.Common.Primitives;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Nemera.Application.Core.UsersManagement.Authorization.Queries.GetAuthorization;

/// <summary>
/// Handler for the query to retrieve the authorization roles and permissions of an account.
/// </summary>
public class GetAuthorizationQueryHandler : IQueryHandler<GetAuthorizationQuery, Result<AuthorizationResponse>>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuthorizationService _authorizationService;
    private readonly IValidator<GetAuthorizationQuery> _validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetAuthorizationQueryHandler"/> class.
    /// </summary>
    /// <param name="authorizationService">Injected service for authorization related functionality.</param>
    /// <param name="currentUserService">Injected service to retrieve the current user information.</param>
    /// <param name="validator">Injected validator for application validation rules.</param>
    public GetAuthorizationQueryHandler(IAuthorizationService authorizationService, ICurrentUserService currentUserService, IValidator<GetAuthorizationQuery> validator)
    {
        _authorizationService = authorizationService;
        _currentUserService = currentUserService;
        _validator = validator;
    }

    /// <summary>
    /// Handles the query to retrieve the authorization roles and permissions of an account.
    /// </summary>
    /// <param name="query">The request to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a <see cref="AuthorizationResponse"/>, or an error message.
    /// </returns>
    public async Task<Result<AuthorizationResponse>> HandleAsync(GetAuthorizationQuery query, CancellationToken cancellationToken)
    {
        List<Error> validationResult = _validator.Validate(query);
        if (validationResult.Count > 0)
            return validationResult;

        // An authenticated request must always carry a user identity.
        Guid? currentUserId = _currentUserService.UserId;
        if (currentUserId is null)
            return Errors.Authorization.NotAuthorized;
        Guid userId = currentUserId.Value;

        // First, check if the Id of the user for whom to get the permission list is different from the Id currently making the request.
        if (userId != query.UserId)
        {
            // If it is, get the role of the current user, and see if they are Admin.
            Result<UserAuthorizationEntity> getCurrentUserPermissionResult = await _authorizationService.GetUserAuthorizationAsync(userId, cancellationToken).ConfigureAwait(false);
            if (getCurrentUserPermissionResult.IsFailure)
                return getCurrentUserPermissionResult.Errors;
            // If the current user is not an Admin, and the account for whom they request the permissions list is not theirs, deny the request.
            if (getCurrentUserPermissionResult.Value.Role != AuthorizationRoles.ADMIN)
                return Errors.Authorization.NotAuthorized;
        }
        Result<UserAuthorizationEntity> getUserPermissionResult = await _authorizationService.GetUserAuthorizationAsync(query.UserId!.Value, cancellationToken).ConfigureAwait(false);
        return getUserPermissionResult.Match(value => Result.From(value.ToResponse()), errors => errors);
    }
}
