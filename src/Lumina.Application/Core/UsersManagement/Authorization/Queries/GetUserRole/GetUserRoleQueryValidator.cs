#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Utilities;
using Lumina.Domain.SharedKernel.Common.Errors;
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
        RuleFor(query => query.UserId)
            .NotEmpty()
            .WithError(Errors.Users.UserIdCannotBeEmpty)
            .Must(id => id != Guid.Empty)
            .WithError(Errors.Users.UserIdCannotBeEmpty);
    }
}
