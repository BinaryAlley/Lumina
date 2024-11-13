#region ========================================================================= USING =====================================================================================
using FluentValidation;
using Lumina.Application.Common.Errors;
#endregion

namespace Lumina.Application.Core.UsersManagement.Authentication.Commands.ChangePassword;

/// <summary>
/// Validates the needed validation rules for <see cref="ChangePasswordCommand"/>.
/// </summary>
public class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ChangePasswordCommandValidator"/> class.
    /// </summary>
    public ChangePasswordCommandValidator()
    {
        RuleFor(x => x.Username).NotEmpty().WithMessage(Errors.Authentication.UsernameCannotBeEmpty.Description);
        RuleFor(x => x.CurrentPassword).NotEmpty().WithMessage(Errors.Authentication.CurrentPasswordCannotBeEmpty.Description)
            .Matches("^(?=.*[A-Z])(?=.*[0-9])(?=.*[^a-zA-Z0-9]).{8,}$").WithMessage(Errors.Authentication.InvalidPassword.Description);
        RuleFor(x => x.NewPassword).NotEmpty().WithMessage(Errors.Authentication.NewPasswordCannotBeEmpty.Description)
            .Matches("^(?=.*[A-Z])(?=.*[0-9])(?=.*[^a-zA-Z0-9]).{8,}$").WithMessage(Errors.Authentication.InvalidPassword.Description);
        RuleFor(x => x.NewPasswordConfirm).NotEmpty().WithMessage(Errors.Authentication.NewPasswordConfirmCannotBeEmpty.Description);
        RuleFor(x => x.NewPassword).Equal(x => x.NewPasswordConfirm).WithMessage(Errors.Authentication.PasswordsNotMatch.Description);
    }
}
