#region ========================================================================= USING =====================================================================================
using FluentValidation;
using Lumina.Domain.Common.Errors;
using System;
#endregion

namespace Lumina.Application.Core.UsersManagement.Authorization.Queries.GetUserRole;

/// <summary>
/// Validates the needed validation rules for <see cref="GetUserRoleQuery"/>.
/// </summary>
public class GetUserRoleQueryValidator : AbstractValidator<GetUserRoleQuery>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetUserRoleQueryValidator"/> class.
    /// </summary>
    public GetUserRoleQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage(Errors.Users.UserIdCannotBeEmpty.Description)
            .Must(id => id != Guid.Empty).WithMessage(Errors.Users.UserIdCannotBeEmpty.Description);
    }
}
