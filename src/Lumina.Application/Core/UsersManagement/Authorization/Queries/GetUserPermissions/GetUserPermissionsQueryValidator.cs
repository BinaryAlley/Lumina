#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Utilities;
using Lumina.Domain.SharedKernel.Common.Errors;
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
        RuleFor(query => query.UserId)
            .NotEmpty()
            .WithError(Errors.Users.UserIdCannotBeEmpty)
            .Must(id => id != Guid.Empty)
            .WithError(Errors.Users.UserIdCannotBeEmpty);
    }
}
