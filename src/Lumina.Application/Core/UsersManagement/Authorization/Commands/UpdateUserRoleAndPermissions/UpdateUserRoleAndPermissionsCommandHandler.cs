#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.DataAccess.Entities.Authorization;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Common.DataAccess.Repositories.Authorization;
using Lumina.Application.Common.DataAccess.Repositories.Users;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Contracts.Responses.Authorization;
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
public class UpdateUserRoleAndPermissionsCommandHandler : ICommandHandler<UpdateUserRoleAndPermissionsCommand, Result<AuthorizationResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthorizationService _authorizationService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IValidator<UpdateUserRoleAndPermissionsCommand> _validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateUserRoleAndPermissionsCommandHandler"/> class.
    /// </summary>
    /// <param name="authorizationService">Injected service for authorization related functionality.</param>
    /// <param name="currentUserService">Injected service to retrieve the current user information.</param>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    /// <param name="validator">Injected validator for application validation rules.</param>
    public UpdateUserRoleAndPermissionsCommandHandler(IAuthorizationService authorizationService, ICurrentUserService currentUserService, IUnitOfWork unitOfWork, IValidator<UpdateUserRoleAndPermissionsCommand> validator)
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
    public async Task<Result<AuthorizationResponse>> HandleAsync(UpdateUserRoleAndPermissionsCommand command, CancellationToken cancellationToken)
    {
        List<Error> validationResult = _validator.Validate(command);
        if (validationResult.Count > 0)
            return validationResult;

        // only admins can update user authorizations
        bool isAdmin = await _authorizationService.IsInRoleAsync(_currentUserService.UserId!.Value, "Admin", cancellationToken).ConfigureAwait(false);
        if (!isAdmin)
            return ApplicationErrors.Authorization.NotAuthorized;

        // get the user to update
        Result<UserEntity?> getUserResult = await _unitOfWork.UserRepository.GetByIdAsync(command.UserId, cancellationToken).ConfigureAwait(false);
        if (getUserResult.IsFailure || getUserResult.Value is null)
            return DomainErrors.Users.UserDoesNotExist;

        Result<RoleEntity?> getRoleResult = default;
        if (command.RoleId is not null)
        {
            // get the new role
            getRoleResult = await _unitOfWork.RoleRepository.GetByIdAsync(command.RoleId!.Value, cancellationToken).ConfigureAwait(false);
            if (getRoleResult.IsFailure || getRoleResult.Value is null)
                return ApplicationErrors.Authorization.RoleNotFound;

            // check if we're changing an admin's role and if this would leave us without admins
            if (getUserResult.Value.UserRole?.Role.RoleName == "Admin" && getRoleResult.Value.RoleName != "Admin")
            {
                // count how many admins we have
                Result<IEnumerable<UserEntity>> getAllUsersResult = await _unitOfWork.UserRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
                if (getAllUsersResult.IsFailure)
                    return getAllUsersResult.Errors;

                int adminCount = getAllUsersResult.Value.Count(user => user.UserRole?.Role.RoleName == "Admin");
                if (adminCount <= 1)
                    return ApplicationErrors.Authorization.CannotRemoveLastAdmin;
            }
        }
        // get the permissions to assign
        Result<IEnumerable<PermissionEntity>> getPermissionsResult =
            await _unitOfWork.PermissionRepository.GetByIdsAsync(command.Permissions, cancellationToken).ConfigureAwait(false);
        if (getPermissionsResult.IsFailure)
            return getPermissionsResult.Errors;

        // update the user
        UserEntity userToUpdate = getUserResult.Value;

        UserRoleEntity? userRole = default!;
        if (command.RoleId is not null)
        {
            userRole = new()
            {
                UserId = userToUpdate.Id,
                RoleId = command.RoleId.Value,
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
            UserPermissions = [.. command.Permissions.Select(permissionId => new UserPermissionEntity
            {
                UserId = userToUpdate.Id,
                PermissionId = permissionId,
                Permission = getPermissionsResult.Value.First(permission => permission.Id == permissionId),
                User = userToUpdate
            })],
            LibraryScans = userToUpdate.LibraryScans
        };

        // save changes and return result
        Result<Updated> updateResult = await _unitOfWork.UserRepository.UpdateAsync(updatedUser, cancellationToken).ConfigureAwait(false);
        if (updateResult.IsFailure)
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
