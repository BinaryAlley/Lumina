#region ========================================================================= USING =====================================================================================
using FluentValidation;
using System;
using System.Linq;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
using DomainErrors = Lumina.Domain.Common.Errors.Errors;
#endregion

namespace Lumina.Application.Core.UsersManagement.Authorization.Commands.UpdateUserRoleAndPermissions;

/// <summary>
/// Validates the needed validation rules for <see cref="UpdateUserRoleAndPermissionsCommand"/>.
/// </summary>
public class UpdateUserRoleAndPermissionsCommandValidator : AbstractValidator<UpdateUserRoleAndPermissionsCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateUserRoleAndPermissionsCommandValidator"/> class.
    /// </summary>
    public UpdateUserRoleAndPermissionsCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage(DomainErrors.Users.UserIdCannotBeEmpty.Description)
            .Must(id => id != Guid.Empty).WithMessage(DomainErrors.Users.UserIdCannotBeEmpty.Description);

        // validate Permissions only if provided
        When(x => x.Permissions != null && x.Permissions.Count != 0, () =>
        {
            RuleForEach(x => x.Permissions)
                .NotEmpty().WithMessage(ApplicationErrors.Authorization.PermissionIdCannotBeEmpty.Description)
                .Must(id => id != Guid.Empty).WithMessage(ApplicationErrors.Authorization.PermissionIdCannotBeEmpty.Description);
        });
    }
}
