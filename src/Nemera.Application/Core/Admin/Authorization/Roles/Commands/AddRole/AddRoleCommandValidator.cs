#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Errors;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Utilities;
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
            .NotNull().WithError(Errors.Authorization.RoleNameCannotBeNull)
            .NotEmpty().WithError(Errors.Authorization.RoleNameCannotBeEmpty);

        RuleFor(x => x.Permissions)
            .NotNull().WithError(Errors.Authorization.PermissionsListCannotBeNull)
            .NotEmpty().WithError(Errors.Authorization.PermissionsListCannotBeEmpty);

        RuleForEach(x => x.Permissions)
            .NotEmpty().WithError(Errors.Authorization.PermissionIdCannotBeEmpty)
            .Must(id => id != Guid.Empty).WithError(Errors.Authorization.PermissionIdCannotBeEmpty);
    }
}
