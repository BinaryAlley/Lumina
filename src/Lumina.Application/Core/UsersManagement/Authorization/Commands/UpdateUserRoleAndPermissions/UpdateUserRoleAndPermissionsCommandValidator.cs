#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Utilities;
using System;
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
        RuleFor(command => command.UserId)
            .NotEmpty()
            .WithError(DomainErrors.Users.UserIdCannotBeEmpty)
            .Must(id => id != Guid.Empty)
            .WithError(DomainErrors.Users.UserIdCannotBeEmpty);

        // validate each permission Id when the list is provided
        RuleForEach(command => command.Permissions)
            .NotEmpty()
            .WithError(ApplicationErrors.Authorization.PermissionIdCannotBeEmpty)
            .Must(id => id != Guid.Empty)
            .WithError(ApplicationErrors.Authorization.PermissionIdCannotBeEmpty);
    }
}
