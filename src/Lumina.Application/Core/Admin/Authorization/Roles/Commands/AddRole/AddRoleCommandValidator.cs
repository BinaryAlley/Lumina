#region ========================================================================= USING =====================================================================================
using FluentValidation;
using Lumina.Application.Common.Errors;
using System;
#endregion

namespace Lumina.Application.Core.Admin.Authorization.Roles.Commands.AddRole;

/// <summary>
/// Validates the needed validation rules for <see cref="AddRoleCommand"/>.
/// </summary>
public class AddRoleCommandValidator : AbstractValidator<AddRoleCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AddRoleCommandValidator"/> class.
    /// </summary>
    public AddRoleCommandValidator()
    {
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
