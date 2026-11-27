#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.Errors;
using Nemera.Application.Common.Infrastructure.Validation;
using Nemera.Application.Common.Utilities;
using System;
#endregion

namespace Nemera.Application.Core.UsersManagement.Authorization.Queries.GetAuthorization;

/// <summary>
/// Validates the needed validation rules for <see cref="GetAuthorizationQuery"/>.
/// </summary>
public class GetAuthorizationQueryValidator : AbstractValidator<GetAuthorizationQuery>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetAuthorizationQueryValidator"/> class.
    /// </summary>
    public GetAuthorizationQueryValidator()
    {
        RuleFor(query => query.UserId)
            .NotEmpty()
            .WithError(Errors.Users.UserIdCannotBeEmpty)
            .Must(id => id != Guid.Empty)
            .WithError(Errors.Users.UserIdCannotBeEmpty);
    }
}
