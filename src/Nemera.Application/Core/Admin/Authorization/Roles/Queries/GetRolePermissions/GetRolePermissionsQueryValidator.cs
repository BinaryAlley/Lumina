#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.Errors;
using Nemera.Application.Common.Infrastructure.Validation;
using Nemera.Application.Common.Utilities;
using System;
#endregion

namespace Nemera.Application.Core.Admin.Authorization.Roles.Queries.GetRolePermissions;

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
