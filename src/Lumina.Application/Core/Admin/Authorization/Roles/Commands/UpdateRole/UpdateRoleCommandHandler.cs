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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Core.Admin.Authorization.Roles.Commands.UpdateRole;

/// <summary>
/// Handler for the command to update an authorization role.
/// </summary>
public class UpdateRoleCommandHandler : IRequestHandler<UpdateRoleCommand, ErrorOr<RolePermissionsResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthorizationService _authorizationService;
    private readonly ICurrentUserService _currentUserService;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateRoleCommandHandler"/> class.
    /// </summary>
    /// <param name="authorizationService">Injected service for authorization related functionality.</param>
    /// <param name="currentUserService">Injected service to retrieve the current user information.</param>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    public UpdateRoleCommandHandler(IAuthorizationService authorizationService, ICurrentUserService currentUserService, IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        _authorizationService = authorizationService;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Handles the command to update an authorization role.
    /// </summary>
    /// <param name="request">The request to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="ErrorOr{TValue}"/> containing either a successfully updated <see cref="RoleResponse"/>, or an error message.
    /// </returns>
    public async ValueTask<ErrorOr<RolePermissionsResponse>> Handle(UpdateRoleCommand request, CancellationToken cancellationToken)
    {
        // only admins can update authorization roles
        bool isAdmin = await _authorizationService.IsInRoleAsync(_currentUserService.UserId!.Value, "Admin", cancellationToken).ConfigureAwait(false);
        if (!isAdmin)
            return Errors.Authorization.NotAuthorized;

        IRoleRepository roleRepository = _unitOfWork.GetRepository<IRoleRepository>();

        // update the role and its permissions
        RoleEntity newRole = new()
        {
            Id = request.RoleId,
            RoleName = request.RoleName,
            RolePermissions = request.Permissions.Select(permissionId => new RolePermissionEntity()
            {
                PermissionId = permissionId,
                Permission = null!,
                Role = null!,
                RoleId = default
            }).ToList()
        };
        // save the updated role in the repository
        ErrorOr<Updated> updateRoleResult = await roleRepository.UpdateAsync(newRole, cancellationToken).ConfigureAwait(false);
        if (updateRoleResult.IsError)
            return updateRoleResult.Errors;
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        // retrieve the updated authorization role from the persistence medium and return it
        ErrorOr<RoleEntity?> getRoleResult = await roleRepository.GetByIdAsync(request.RoleId, cancellationToken).ConfigureAwait(false);
        if (getRoleResult.IsError)
            return getRoleResult.Errors;
        if (getRoleResult.Value is null)
            return Errors.Persistence.ErrorPersistingAuthorizationRole;
        return getRoleResult.Value.ToRolePermissionsResponse();
    }
}
