#region ========================================================================= USING =====================================================================================
using FluentValidation;
using Lumina.Application.Common.Errors;
#endregion

namespace Lumina.Application.Core.UsersManagement.Register;

/// <summary>
/// Validates the needed validation rules for <see cref="RegisterCommand"/>.
/// </summary>
public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RegisterCommandValidator"/> class.
    /// </summary>
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Username).NotEmpty().WithMessage(Errors.Authentication.UsernameCannotBeEmpty.Description);
        RuleFor(x => x.Password).NotEmpty().WithMessage(Errors.Authentication.PasswordCannotBeEmpty.Description)
            .Matches("^(?=.*[A-Z])(?=.*[0-9])(?=.*[^a-zA-Z0-9]).{8,}$").WithMessage(Errors.Authentication.InvalidPassword.Description);
        RuleFor(x => x.PasswordConfirm).NotEmpty().WithMessage(Errors.Authentication.PasswordConfirmCannotBeEmpty.Description);
        RuleFor(x => x.Password).Equal(x => x.PasswordConfirm).WithMessage(Errors.Authentication.PasswordsNotMatch.Description);
    }
}
