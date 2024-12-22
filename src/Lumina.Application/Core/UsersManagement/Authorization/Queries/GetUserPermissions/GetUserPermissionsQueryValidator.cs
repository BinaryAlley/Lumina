#region ========================================================================= USING =====================================================================================
using FluentValidation;
using Lumina.Domain.Common.Errors;
using System;
#endregion

namespace Lumina.Application.Core.UsersManagement.Authorization.Queries.GetUserPermissions;

/// <summary>
/// Validates the needed validation rules for <see cref="GetUserPermissionsQuery"/>.
/// </summary>
public class GetUserPermissionsQueryValidator : AbstractValidator<GetUserPermissionsQuery>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetUserPermissionsQueryValidator"/> class.
    /// </summary>
    public GetUserPermissionsQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage(Errors.Users.UserIdCannotBeEmpty.Description)
            .Must(id => id != Guid.Empty).WithMessage(Errors.Users.UserIdCannotBeEmpty.Description);
    }
}
