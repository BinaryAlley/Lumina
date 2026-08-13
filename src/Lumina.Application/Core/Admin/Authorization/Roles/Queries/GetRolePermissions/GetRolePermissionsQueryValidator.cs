#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Errors;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Utilities;
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
            .NotEmpty().WithError(Errors.Authorization.RoleIdCannotBeEmpty)
            .Must(id => id != Guid.Empty).WithError(Errors.Authorization.RoleIdCannotBeEmpty);
    }
}
