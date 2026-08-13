#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.DataAccess.Entities.Authorization;
using Lumina.Application.Common.Errors;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Mapping.Authorization;
using Lumina.Contracts.Responses.Authorization;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Core.UsersManagement.Authorization.Queries.GetAuthorization;

/// <summary>
/// Handler for the query to retrieve the authorization roles and permissions of an account.
/// </summary>
public class GetAuthorizationQueryHandler : IQueryHandler<GetAuthorizationQuery, ErrorOr<AuthorizationResponse>>
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
    /// An <see cref="ErrorOr{TValue}"/> containing either a <see cref="AuthorizationResponse"/>, or an error message.
    /// </returns>
    public async Task<ErrorOr<AuthorizationResponse>> HandleAsync(GetAuthorizationQuery query, CancellationToken cancellationToken)
    {
        List<Error> validationResult = _validator.Validate(query);
        if (validationResult.Count > 0)
            return validationResult;

        // first, check if the Id of the user for whom to get the permission list is different from the Id currently making the request
        if (_currentUserService.UserId != query.UserId)
        {
            // if it is, get the role of the current user, and see if they are Admin
            ErrorOr<UserAuthorizationEntity> getCurrentUserPermissionResult = await _authorizationService.GetUserAuthorizationAsync(_currentUserService.UserId!.Value, cancellationToken).ConfigureAwait(false);
            if (getCurrentUserPermissionResult.IsError)
                return getCurrentUserPermissionResult.Errors;
            // if the current user is not an Admin, and the account for whom they request the permissions list is not theirs, deny the request
            if (getCurrentUserPermissionResult.Value.Role != "Admin")
                return Errors.Authorization.NotAuthorized;
        }
        ErrorOr<UserAuthorizationEntity> getUserPermissionResult = await _authorizationService.GetUserAuthorizationAsync(query.UserId!.Value, cancellationToken).ConfigureAwait(false);
        return getUserPermissionResult.Match(value => ErrorOrFactory.From(value.ToResponse()), errors => errors);
    }
}
