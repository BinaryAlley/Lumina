#region ========================================================================= USING =====================================================================================
using FluentValidation;
using Lumina.Application.Common.Errors;
using System.Linq;
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
        RuleFor(x => x.Username).NotEmpty().WithMessage(Errors.Authentication.UsernameCannotBeEmpty.Description);
        RuleFor(x => x.Password).NotEmpty().WithMessage(Errors.Authentication.PasswordCannotBeEmpty.Description)
            .Matches("^(?=.*[A-Z])(?=.*[0-9])(?=.*[^a-zA-Z0-9]).{8,}$").WithMessage(Errors.Authentication.InvalidPassword.Description);
        RuleFor(m => m!.TotpCode)
            .Custom((code, context) =>
            {
                if (string.IsNullOrEmpty(code))
                    return;
                if (code.Length != 6)
                    context.AddFailure("TotpCode", Errors.Authentication.InvalidTotpCode.Description);
                if (!code.All(char.IsDigit))
                    context.AddFailure("TotpCode", Errors.Authentication.InvalidTotpCode.Description);
            });
    }
}
