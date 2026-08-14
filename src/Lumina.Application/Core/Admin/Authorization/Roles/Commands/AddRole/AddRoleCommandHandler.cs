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

namespace Lumina.Application.Core.Admin.Authorization.Roles.Commands.AddRole;

/// <summary>
/// Handler for the command to add an authorization role.
/// </summary>
public class AddRoleCommandHandler : ICommandHandler<AddRoleCommand, Result<RolePermissionsResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthorizationService _authorizationService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IValidator<AddRoleCommand> _validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="AddRoleCommandHandler"/> class.
    /// </summary>
    /// <param name="authorizationService">Injected service for authorization related functionality.</param>
    /// <param name="currentUserService">Injected service to retrieve the current user information.</param>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    /// <param name="validator">Injected validator for application validation rules.</param>
    public AddRoleCommandHandler(IAuthorizationService authorizationService, ICurrentUserService currentUserService, IUnitOfWork unitOfWork, IValidator<AddRoleCommand> validator)
    {
        _unitOfWork = unitOfWork;
        _authorizationService = authorizationService;
        _currentUserService = currentUserService;
        _validator = validator;
    }

    /// <summary>
    /// Handles the command to add an authorization role.
    /// </summary>
    /// <param name="request">The request to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a successfully created <see cref="RoleResponse"/>, or an error message.
    /// </returns>
    public async Task<Result<RolePermissionsResponse>> HandleAsync(AddRoleCommand request, CancellationToken cancellationToken)
    {
        List<Error> validationResult = _validator.Validate(request);
        if (validationResult.Count > 0)
            return validationResult;

        // only admins can create authorization roles
        bool isAdmin = await _authorizationService.IsInRoleAsync(_currentUserService.UserId!.Value, "Admin", cancellationToken).ConfigureAwait(false);
        if (!isAdmin)
            return Errors.Authorization.NotAuthorized;

        IRoleRepository roleRepository = _unitOfWork.GetRepository<IRoleRepository>();

        // create the new role, with its permissions
        RoleEntity newRole = new()
        {
            RoleName = request.RoleName,
            RolePermissions = [.. request.Permissions.Select(permissionId => new RolePermissionEntity()
            {
                PermissionId = permissionId,
                Permission = null!,
                Role = null!,
                RoleId = default
            })]
        };
        // save the new role in the repository
        Result<Created> insertRoleResult = await roleRepository.InsertAsync(newRole, cancellationToken).ConfigureAwait(false);
        if (insertRoleResult.IsFailure)
            return insertRoleResult.Errors;
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        // retrieve the newly saved authorization role from the persistence medium and return it
        Result<RoleEntity?> getRoleResult = await roleRepository.GetByNameAsync(request.RoleName, cancellationToken).ConfigureAwait(false);
        if (getRoleResult.IsFailure)
            return getRoleResult.Errors;
        if (getRoleResult.Value is null)
            return Errors.Persistence.ErrorPersistingAuthorizationRole;
        return getRoleResult.Value.ToRolePermissionsResponse();
    }
}
