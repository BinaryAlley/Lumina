#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Errors;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Utilities;
#endregion

namespace Lumina.Application.Core.UsersManagement.Authentication.Queries.LoginUser;

/// <summary>
/// Validates the needed validation rules for <see cref="LoginUserQuery"/>.
/// </summary>
public class LoginUserQueryValidator : AbstractValidator<LoginUserQuery>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LoginUserQueryValidator"/> class.
    /// </summary>
    public LoginUserQueryValidator()
    {
        RuleFor(query => query.Username)
            .NotEmpty()
            .WithError(Errors.Authentication.UsernameCannotBeEmpty);
       
        RuleFor(query => query.Password)
            .NotEmpty()
            .WithError(Errors.Authentication.PasswordCannotBeEmpty)
            .Matches("^(?=.*[A-Z])(?=.*[0-9])(?=.*[^a-zA-Z0-9]).{8,}$")
            .WithError(Errors.Authentication.InvalidPassword);
      
        RuleFor(query => query.TotpCode)
            .Matches("^[0-9]{6}$")
            .When(query => !string.IsNullOrEmpty(query.TotpCode))
            .WithError(Errors.Authentication.InvalidTotpCode);
    }
}
