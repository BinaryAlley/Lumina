#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.DataAccess.Entities.Authorization;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Common.DataAccess.Repositories.Authorization;
using Lumina.Application.Common.DataAccess.Repositories.Users;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Contracts.Responses.Authorization;
using Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
using DomainErrors = Lumina.Domain.Common.Errors.Errors;
#endregion

namespace Lumina.Application.Core.UsersManagement.Authorization.Commands.UpdateUserRoleAndPermissions;

/// <summary>
/// Handler for the command to update an authorization role.
/// </summary>
public class UpdateUserRoleAndPermissionsCommandHandler : IRequestHandler<UpdateUserRoleAndPermissionsCommand, ErrorOr<AuthorizationResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthorizationService _authorizationService;
    private readonly ICurrentUserService _currentUserService;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateUserRoleAndPermissionsCommandHandler"/> class.
    /// </summary>
    /// <param name="authorizationService">Injected service for authorization related functionality.</param>
    /// <param name="currentUserService">Injected service to retrieve the current user information.</param>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    public UpdateUserRoleAndPermissionsCommandHandler(IAuthorizationService authorizationService, ICurrentUserService currentUserService, IUnitOfWork unitOfWork)
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
    /// An <see cref="ErrorOr{TValue}"/> containing either a successfuly updated <see cref="RoleResponse"/>, or an error message.
    /// </returns>
    public async ValueTask<ErrorOr<AuthorizationResponse>> Handle(UpdateUserRoleAndPermissionsCommand request, CancellationToken cancellationToken)
    {
        // only admins can update user authorizations
        bool isAdmin = await _authorizationService.IsInRoleAsync(_currentUserService.UserId!.Value, "Admin", cancellationToken).ConfigureAwait(false);
        if (!isAdmin)
            return ApplicationErrors.Authorization.NotAuthorized;

        // get repositories
        IUserRepository userRepository = _unitOfWork.GetRepository<IUserRepository>();
        IRoleRepository roleRepository = _unitOfWork.GetRepository<IRoleRepository>();
        IPermissionRepository permissionRepository = _unitOfWork.GetRepository<IPermissionRepository>();

        // get the user to update
        ErrorOr<UserEntity?> getUserResult = await userRepository.GetByIdAsync(request.UserId, cancellationToken).ConfigureAwait(false);
        if (getUserResult.IsError || getUserResult.Value is null)
            return DomainErrors.Users.UserDoesNotExist;

        ErrorOr<RoleEntity?> getRoleResult = default;
        if (request.RoleId is not null)
        {
            // get the new role
            getRoleResult = await roleRepository.GetByIdAsync(request.RoleId!.Value, cancellationToken).ConfigureAwait(false);
            if (getRoleResult.IsError || getRoleResult.Value is null)
                return ApplicationErrors.Authorization.RoleNotFound;

            // check if we're changing an admin's role and if this would leave us without admins
            if (getUserResult.Value.UserRole?.Role.RoleName == "Admin" && getRoleResult.Value.RoleName != "Admin")
            {
                // count how many admins we have
                ErrorOr<IEnumerable<UserEntity>> getAllUsersResult = await userRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
                if (getAllUsersResult.IsError)
                    return getAllUsersResult.Errors;

                int adminCount = getAllUsersResult.Value.Count(user => user.UserRole?.Role.RoleName == "Admin");
                if (adminCount <= 1)
                    return ApplicationErrors.Authorization.CannotRemoveLastAdmin;
            }
        }
        // get the permissions to assign
        ErrorOr<IEnumerable<PermissionEntity>> getPermissionsResult =
            await permissionRepository.GetByIdsAsync(request.Permissions, cancellationToken).ConfigureAwait(false);
        if (getPermissionsResult.IsError)
            return getPermissionsResult.Errors;

        // update the user
        UserEntity userToUpdate = getUserResult.Value;

        UserRoleEntity? userRole = default!;
        if (request.RoleId is not null)
        {
            userRole = new()
            {
                UserId = userToUpdate.Id,
                RoleId = request.RoleId.Value,
                Role = getRoleResult.Value!,
                User = userToUpdate
            };
        }
        
        UserEntity updatedUser = new()
        {
            Id = userToUpdate.Id,
            Username = userToUpdate.Username,
            Password = userToUpdate.Password,
            TempPassword = userToUpdate.TempPassword,
            TotpSecret = userToUpdate.TotpSecret,
            TempPasswordCreated = userToUpdate.TempPasswordCreated,
            Libraries = userToUpdate.Libraries,
            UserRole = userRole,
            UserPermissions = request.Permissions.Select(permissionId => new UserPermissionEntity
            {
                UserId = userToUpdate.Id,
                PermissionId = permissionId,
                Permission = getPermissionsResult.Value.First(permission => permission.Id == permissionId),
                User = userToUpdate
            }).ToList()
        };

        // save changes and return result
        ErrorOr<Updated> updateResult = await userRepository.UpdateAsync(updatedUser, cancellationToken).ConfigureAwait(false);
        if (updateResult.IsError)
            return updateResult.Errors;

        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new AuthorizationResponse(
            userToUpdate.Id,
            getRoleResult.Value?.RoleName,
            userToUpdate.UserPermissions
                .Select(up => up.Permission.PermissionName)
                .ToHashSet()
        );
    }
}
