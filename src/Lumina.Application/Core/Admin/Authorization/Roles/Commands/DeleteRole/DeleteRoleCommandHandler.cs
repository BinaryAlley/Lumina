#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.DataAccess.Entities.Authorization;
using Lumina.Application.Common.DataAccess.Repositories.Authorization;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Errors;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Contracts.Responses.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lumina.Application.Common.Mapping.Authorization;
using Lumina.Application.Common.Infrastructure.Authorization;
#endregion

namespace Lumina.Application.Core.Admin.Authorization.Roles.Commands.DeleteRole;

/// <summary>
/// Handler for the command to delete an authorization role.
/// </summary>
public class DeleteRoleCommandHandler : ICommandHandler<DeleteRoleCommand, ErrorOr<Deleted>>
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
    /// <returns>An <see cref="ErrorOr{TValue}"/> representing either a successful operation, or an error.</returns>
    public async Task<ErrorOr<Deleted>> HandleAsync(DeleteRoleCommand command, CancellationToken cancellationToken)
    {
        List<Error> validationResult = _validator.Validate(command);
        if (validationResult.Count > 0)
            return validationResult;

        // only admins can delete authorization roles
        bool isAdmin = await _authorizationService.IsInRoleAsync(_currentUserService.UserId!.Value, "Admin", cancellationToken).ConfigureAwait(false);
        if (!isAdmin)
            return Errors.Authorization.NotAuthorized;

        // check if a role with the requested Id exists
        IRoleRepository roleRepository = _unitOfWork.GetRepository<IRoleRepository>();
        ErrorOr<RoleEntity?> getExistingRoleResult = await roleRepository.GetByIdAsync(command.RoleId, cancellationToken).ConfigureAwait(false);
        if (getExistingRoleResult.IsError)
            return getExistingRoleResult.Errors;
        else if (getExistingRoleResult.Value is null)
            return Errors.Authorization.RoleNotFound;
        else if (getExistingRoleResult.Value.RoleName == "Admin")
            return Errors.Authorization.AdminRoleCannotBeDeleted;
        // delete the role and its permissions
        ErrorOr<Deleted> deleteRoleResult = await roleRepository.DeleteByIdAsync(command.RoleId, cancellationToken).ConfigureAwait(false);
        if (deleteRoleResult.IsError)
            return deleteRoleResult.Errors;
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return deleteRoleResult.Value;
    }
}
