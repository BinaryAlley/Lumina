#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.Errors;
using Nemera.Application.Common.Infrastructure.Validation;
using Nemera.Application.Common.Utilities;
using System;
#endregion

namespace Nemera.Application.Core.Admin.Authorization.Roles.Commands.UpdateRole;

/// <summary>
/// Validates the needed validation rules for <see cref="UpdateRoleCommand"/>.
/// </summary>
public class UpdateRoleCommandValidator : AbstractValidator<UpdateRoleCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateRoleCommandValidator"/> class.
    /// </summary>
    public UpdateRoleCommandValidator()
    {
        RuleFor(command => command.RoleId)
            .NotEmpty().WithError(Errors.Authorization.RoleIdCannotBeEmpty)
            .Must(id => id != Guid.Empty).WithError(Errors.Authorization.RoleIdCannotBeEmpty);

        RuleFor(command => command.RoleName)
            .NotNull().WithError(Errors.Authorization.RoleNameCannotBeNull)
            .NotEmpty().WithError(Errors.Authorization.RoleNameCannotBeEmpty);

        RuleFor(command => command.Permissions)
            .NotNull().WithError(Errors.Authorization.PermissionsListCannotBeNull)
            .NotEmpty().WithError(Errors.Authorization.PermissionsListCannotBeEmpty);
        
        RuleForEach(command => command.Permissions)
            .NotEmpty().WithError(Errors.Authorization.PermissionIdCannotBeEmpty)
            .Must(id => id != Guid.Empty).WithError(Errors.Authorization.PermissionIdCannotBeEmpty);
    }
}
