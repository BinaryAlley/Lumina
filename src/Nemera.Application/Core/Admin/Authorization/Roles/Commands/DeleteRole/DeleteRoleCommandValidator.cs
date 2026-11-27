#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.Errors;
using Nemera.Application.Common.Infrastructure.Validation;
using Nemera.Application.Common.Utilities;
using System;
#endregion

namespace Nemera.Application.Core.Admin.Authorization.Roles.Commands.DeleteRole;

/// <summary>
/// Validates the needed validation rules for <see cref="DeleteRoleCommand"/>.
/// </summary>
public class DeleteRoleCommandValidator : AbstractValidator<DeleteRoleCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteRoleCommandValidator"/> class.
    /// </summary>
    public DeleteRoleCommandValidator()
    {
        RuleFor(command => command.RoleId)
            .NotEmpty().WithError(Errors.Authorization.RoleIdCannotBeEmpty)
            .Must(id => id != Guid.Empty).WithError(Errors.Authorization.RoleIdCannotBeEmpty);
    }
}
