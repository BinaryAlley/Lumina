#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.CQRS;
using Nemera.Application.Common.DataAccess.Entities.Authorization;
using Nemera.Application.Common.DataAccess.UoW;
using Nemera.Application.Common.Errors;
using Nemera.Application.Common.Infrastructure.Authentication;
using Nemera.Application.Common.Infrastructure.Authorization;
using Nemera.Application.Common.Infrastructure.Validation;
using Nemera.Domain.Common.Primitives;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Nemera.Application.Core.Admin.Authorization.Roles.Commands.DeleteRole;

/// <summary>
/// Handler for the command to delete an authorization role.
/// </summary>
public class DeleteRoleCommandHandler : ICommandHandler<DeleteRoleCommand, Result<Deleted>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthorizationService _authorizationService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IValidator<DeleteRoleCommand> _validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteRoleCommandHandler"/> class.
    /// </summary>
    /// <param name="authorizationService">Injected service for authorization related functionality.</param>
    /// <param name="currentUserService">Injected service to retrieve the current user information.</param>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    /// <param name="validator">Injected validator for application validation rules.</param>
    public DeleteRoleCommandHandler(IAuthorizationService authorizationService, ICurrentUserService currentUserService, IUnitOfWork unitOfWork, IValidator<DeleteRoleCommand> validator)
    {
        _unitOfWork = unitOfWork;
        _authorizationService = authorizationService;
        _currentUserService = currentUserService;
        _validator = validator;
    }

    /// <summary>
    /// Handles the command to delete an authorization role.
    /// </summary>
    /// <param name="command">The request to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public async Task<Result<Deleted>> HandleAsync(DeleteRoleCommand command, CancellationToken cancellationToken)
    {
        List<Error> validationResult = _validator.Validate(command);
        if (validationResult.Count > 0)
            return validationResult;

        // An authenticated request must always carry a user identity.
        Guid? currentUserId = _currentUserService.UserId;
        if (currentUserId is null)
            return Errors.Authorization.NotAuthorized;
        Guid userId = currentUserId.Value;

        // Only admins can delete authorization roles.
        bool isAdmin = await _authorizationService.IsInRoleAsync(userId, AuthorizationRoles.ADMIN, cancellationToken).ConfigureAwait(false);
        if (!isAdmin)
            return Errors.Authorization.NotAuthorized;

        // Check if a role with the requested Id exists.
        Result<RoleEntity?> getExistingRoleResult = await _unitOfWork.RoleRepository.GetByIdAsync(command.RoleId, cancellationToken: cancellationToken).ConfigureAwait(false);
        if (getExistingRoleResult.IsFailure)
            return getExistingRoleResult.Errors;
        else if (getExistingRoleResult.Value is null)
            return Errors.Authorization.RoleNotFound;
        else if (getExistingRoleResult.Value.RoleName == AuthorizationRoles.ADMIN)
            return Errors.Authorization.AdminRoleCannotBeDeleted;

        // Delete the role and its permissions.
        Result<Deleted> deleteRoleResult = await _unitOfWork.RoleRepository.DeleteByIdAsync(command.RoleId, cancellationToken).ConfigureAwait(false);
        if (deleteRoleResult.IsFailure)
            return deleteRoleResult.Errors;

        Result<Success> saveChangesResult = await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        if (saveChangesResult.IsFailure)
            return saveChangesResult.Errors;

        return Result.Deleted;
    }
}
