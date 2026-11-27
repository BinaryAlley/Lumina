#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.Errors;
using Nemera.Application.Common.Infrastructure.Validation;
using Nemera.Application.Common.Utilities;
using System;
#endregion

namespace Nemera.Application.Core.UsersManagement.Authorization.Queries.GetUserRole;

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
        RuleFor(query => query.UserId)
            .NotEmpty()
            .WithError(Errors.Users.UserIdCannotBeEmpty)
            .Must(id => id != Guid.Empty)
            .WithError(Errors.Users.UserIdCannotBeEmpty);
    }
}
