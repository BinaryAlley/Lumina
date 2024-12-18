#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.DataAccess.Entities.Authorization;
using Lumina.Application.Common.Errors;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Mapping.Authorization;
using Lumina.Contracts.Responses.Authorization;
using Mediator;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Core.UsersManagement.Authorization.Queries.GetAuthorization;

/// <summary>
/// Handler for the query to retrieve the authorization roles and permissions of an account.
/// </summary>
public class GetAuthorizationQueryHandler : IRequestHandler<GetAuthorizationQuery, ErrorOr<AuthorizationResponse>>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuthorizationService _authorizationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetAuthorizationQueryHandler"/> class.
    /// </summary>
    /// <param name="authorizationService">Injected service for authorization related functionality.</param>
    /// <param name="currentUserService">Injected service to retrieve the current user information.</param>
    public GetAuthorizationQueryHandler(IAuthorizationService authorizationService, ICurrentUserService currentUserService)
    {
        _authorizationService = authorizationService;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Handles the query to retrieve the authorization roles and permissions of an account.
    /// </summary>
    /// <param name="request">The request to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="ErrorOr{TValue}"/> containing either a <see cref="AuthorizationResponse"/>, or an error message.
    /// </returns>
    public async ValueTask<ErrorOr<AuthorizationResponse>> Handle(GetAuthorizationQuery request, CancellationToken cancellationToken)
    {
        // first, check if the Id of the user for whom to get the permission list is different from the Id currently making the request
        if (_currentUserService.UserId != request.UserId)
        {
            // if it is, get the role of the current user, and see if they are Admin
            ErrorOr<UserAuthorizationEntity> getCurrentUserPermissionResult = await _authorizationService.GetUserAuthorizationAsync(_currentUserService.UserId!.Value, cancellationToken).ConfigureAwait(false);
            if (getCurrentUserPermissionResult.IsError)
                return getCurrentUserPermissionResult.Errors;
            // if the current user is not an Admin, and the account for whom they request the permissions list is not theirs, deny the request
            if (!getCurrentUserPermissionResult.Value.Roles.Any(role => role == "Admin"))
                return Errors.Authorization.NotAuthorized;
        }
        ErrorOr<UserAuthorizationEntity> getUserPermissionResult = await _authorizationService.GetUserAuthorizationAsync(request.UserId!.Value, cancellationToken).ConfigureAwait(false);
        return getUserPermissionResult.Match(value => ErrorOrFactory.From(value.ToResponse()), errors => errors);
    }
}
