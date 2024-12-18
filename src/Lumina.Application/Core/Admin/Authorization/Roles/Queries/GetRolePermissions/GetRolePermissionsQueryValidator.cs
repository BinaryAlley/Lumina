#region ========================================================================= USING =====================================================================================
using FluentValidation;
using Lumina.Application.Common.Errors;
using System;
#endregion

namespace Lumina.Application.Core.Admin.Authorization.Roles.Queries.GetRolePermissions;

/// <summary>
/// Validates the needed validation rules for <see cref="GetRolePermissionsQuery"/>.
/// </summary>
public class GetRolePermissionsQueryValidator : AbstractValidator<GetRolePermissionsQuery>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetRolePermissionsQueryValidator"/> class.
    /// </summary>
    public GetRolePermissionsQueryValidator()
    {
        RuleFor(x => x.RoleId)
            .NotEmpty().WithMessage(Errors.Authorization.RoleIdCannotBeEmpty.Description)
            .Must(id => id != Guid.Empty).WithMessage(Errors.Authorization.RoleIdCannotBeEmpty.Description);
    }
}
