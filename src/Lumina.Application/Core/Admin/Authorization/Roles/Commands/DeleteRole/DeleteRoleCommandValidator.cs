#region ========================================================================= USING =====================================================================================
using FluentValidation;
using Lumina.Application.Common.Errors;
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
        RuleFor(x => x.RoleId)
            .NotEmpty().WithMessage(Errors.Authorization.RoleIdCannotBeEmpty.Description)
            .Must(id => id != Guid.Empty).WithMessage(Errors.Authorization.RoleIdCannotBeEmpty.Description);
    }
}
