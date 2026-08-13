#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Errors;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Utilities;
using System;
#endregion

namespace Lumina.Application.Core.Admin.Authorization.Roles.Commands.DeleteRole;

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
