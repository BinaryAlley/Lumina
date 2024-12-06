#region ========================================================================= USING =====================================================================================
using FluentValidation;
using Lumina.Domain.Common.Errors;
using System;
#endregion

namespace Lumina.Application.Core.UsersManagement.Authorization.Queries.GetAuthorization;

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
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage(Errors.Users.UserIdCannotBeEmpty.Description)
            .Must(id => id != Guid.Empty).WithMessage(Errors.Users.UserIdCannotBeEmpty.Description);
    }
}
