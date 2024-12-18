#region ========================================================================= USING =====================================================================================
using FluentValidation;
using Lumina.Application.Common.Errors;
using System;
#endregion

namespace Lumina.Application.Core.Admin.Authorization.Roles.Commands.UpdateRole;

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
        RuleFor(x => x.RoleId)
            .NotEmpty().WithMessage(Errors.Authorization.RoleIdCannotBeEmpty.Description)
            .Must(id => id != Guid.Empty).WithMessage(Errors.Authorization.RoleIdCannotBeEmpty.Description);

        RuleFor(x => x.RoleName)
            .NotNull().WithMessage(Errors.Authorization.RoleNameCannotBeNull.Description)
            .NotEmpty().WithMessage(Errors.Authorization.RoleNameCannotBeEmpty.Description);

        RuleFor(x => x.Permissions)
            .NotNull().WithMessage(Errors.Authorization.PermissionsListCannotBeNull.Description)
            .NotEmpty().WithMessage(Errors.Authorization.PermissionsListCannotBeEmpty.Description);
        RuleForEach(x => x.Permissions)
            .NotEmpty().WithMessage(Errors.Authorization.PermissionIdCannotBeEmpty.Description)
            .Must(id => id != Guid.Empty).WithMessage(Errors.Authorization.PermissionIdCannotBeEmpty.Description);
    }
}
