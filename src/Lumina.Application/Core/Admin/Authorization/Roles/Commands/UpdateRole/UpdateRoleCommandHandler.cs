#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.DataAccess.Entities.Authorization;
using Lumina.Application.Common.DataAccess.Repositories.Authorization;
using Lumina.Application.Common.DataAccess.UoW;
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

namespace Lumina.Application.Core.Admin.Authorization.Roles.Commands.UpdateRole;

/// <summary>
/// Handler for the command to update an authorization role.
/// </summary>
public class UpdateRoleCommandHandler : ICommandHandler<UpdateRoleCommand, Result<RolePermissionsResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthorizationService _authorizationService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IValidator<UpdateRoleCommand> _validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateRoleCommandHandler"/> class.
    /// </summary>
    /// <param name="authorizationService">Injected service for authorization related functionality.</param>
    /// <param name="currentUserService">Injected service to retrieve the current user information.</param>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    /// <param name="validator">Injected validator for application validation rules.</param>
    public UpdateRoleCommandHandler(IAuthorizationService authorizationService, ICurrentUserService currentUserService, IUnitOfWork unitOfWork, IValidator<UpdateRoleCommand> validator)
    {
        _unitOfWork = unitOfWork;
        _authorizationService = authorizationService;
        _currentUserService = currentUserService;
        _validator = validator;
    }

    /// <summary>
    /// Handles the command to update an authorization role.
    /// </summary>
    /// <param name="command">The request to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a successfully updated <see cref="RoleResponse"/>, or an error message.
    /// </returns>
    public async Task<Result<RolePermissionsResponse>> HandleAsync(UpdateRoleCommand command, CancellationToken cancellationToken)
    {
        List<Error> validationResult = _validator.Validate(command);
        if (validationResult.Count > 0)
            return validationResult;

        // only admins can update authorization roles
        bool isAdmin = await _authorizationService.IsInRoleAsync(_currentUserService.UserId!.Value, "Admin", cancellationToken).ConfigureAwait(false);
        if (!isAdmin)
            return Errors.Authorization.NotAuthorized;

        IRoleRepository roleRepository = _unitOfWork.GetRepository<IRoleRepository>();

        // update the role and its permissions
        RoleEntity newRole = new()
        {
            Id = command.RoleId,
            RoleName = command.RoleName,
            RolePermissions = command.Permissions.Select(permissionId => new RolePermissionEntity()
            {
                PermissionId = permissionId,
                Permission = null!,
                Role = null!,
                RoleId = default
            }).ToList()
        };
        // save the updated role in the repository
        Result<Updated> updateRoleResult = await roleRepository.UpdateAsync(newRole, cancellationToken).ConfigureAwait(false);
        if (updateRoleResult.IsFailure)
            return updateRoleResult.Errors;
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        // retrieve the updated authorization role from the persistence medium and return it
        Result<RoleEntity?> getRoleResult = await roleRepository.GetByIdAsync(command.RoleId, cancellationToken).ConfigureAwait(false);
        if (getRoleResult.IsFailure)
            return getRoleResult.Errors;
        if (getRoleResult.Value is null)
            return Errors.Persistence.ErrorPersistingAuthorizationRole;
        return getRoleResult.Value.ToRolePermissionsResponse();
    }
}
