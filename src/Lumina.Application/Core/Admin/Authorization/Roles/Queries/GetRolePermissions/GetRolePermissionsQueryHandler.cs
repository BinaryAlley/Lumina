#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.DataAccess.Entities.Authorization;
using Lumina.Application.Common.DataAccess.Repositories.Authorization;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Errors;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Mapping.Authorization;
using Lumina.Contracts.Responses.Authorization;
using Mediator;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Core.Admin.Authorization.Roles.Queries.GetRolePermissions;

/// <summary>
/// Handler for the query to retrieve the list of authorization permissions for a role.
/// </summary>
public class GetRolePermissionsQueryHandler : IRequestHandler<GetRolePermissionsQuery, ErrorOr<RolePermissionsResponse>>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuthorizationService _authorizationService;
    private readonly IRoleRepository _roleRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetRolePermissionsQueryHandler"/> class.
    /// </summary>
    /// <param name="authorizationService">Injected service for authorization related functionality.</param>
    /// <param name="currentUserService">Injected service to retrieve the current user information.</param>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    public GetRolePermissionsQueryHandler(IAuthorizationService authorizationService, ICurrentUserService currentUserService, IUnitOfWork unitOfWork)
    {
        _authorizationService = authorizationService;
        _currentUserService = currentUserService;
        _roleRepository = unitOfWork.GetRepository<IRoleRepository>();
    }

    /// <summary>
    /// Handles the query to retrieve the list of authorization permissions for a role.
    /// </summary>
    /// <param name="request">The request to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="ErrorOr{TValue}"/> containing either a collection of <see cref="RoleResponse"/>, or an error message.
    /// </returns>
    public async ValueTask<ErrorOr<RolePermissionsResponse>> Handle(GetRolePermissionsQuery request, CancellationToken cancellationToken)
    {
        // only admins can see the list of authorization roles
        bool isAdmin = await _authorizationService.IsInRoleAsync(_currentUserService.UserId!.Value, "Admin", cancellationToken).ConfigureAwait(false);
        if (!isAdmin)
            return Errors.Authorization.NotAuthorized;
        ErrorOr<RoleEntity?> getRoleResult = await _roleRepository.GetByIdAsync(request.RoleId, cancellationToken).ConfigureAwait(false);
        if (getRoleResult.IsError)
            return getRoleResult.Errors;
        else if (getRoleResult.Value is null)
            return Errors.Authorization.RoleNotFound;
        return getRoleResult.Value.ToRolePermissionsResponse();
    }
}
