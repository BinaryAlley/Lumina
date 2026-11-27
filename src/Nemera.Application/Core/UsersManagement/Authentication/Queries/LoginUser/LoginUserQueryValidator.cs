#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.Errors;
using Nemera.Application.Common.Infrastructure.Validation;
using Nemera.Application.Common.Utilities;
#endregion

namespace Nemera.Application.Core.UsersManagement.Authentication.Queries.LoginUser;

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
