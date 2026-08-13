#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Errors;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Utilities;
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
        RuleFor(command => command.Username)
            .NotEmpty()
            .WithError(Errors.Authentication.UsernameCannotBeEmpty);
       
        RuleFor(command => command.CurrentPassword)
            .NotEmpty()
            .WithError(Errors.Authentication.CurrentPasswordCannotBeEmpty)
            .Matches("^(?=.*[A-Z])(?=.*[0-9])(?=.*[^a-zA-Z0-9]).{8,}$")
            .WithError(Errors.Authentication.InvalidPassword);
       
        RuleFor(command => command.NewPassword)
            .NotEmpty()
            .WithError(Errors.Authentication.NewPasswordCannotBeEmpty)
            .Matches("^(?=.*[A-Z])(?=.*[0-9])(?=.*[^a-zA-Z0-9]).{8,}$")
            .WithError(Errors.Authentication.InvalidPassword);
       
        RuleFor(command => command.NewPasswordConfirm)
            .NotEmpty()
            .WithError(Errors.Authentication.NewPasswordConfirmCannotBeEmpty);
        
        RuleFor(command => command.NewPassword)
            .Equal(command => command.NewPasswordConfirm)
            .WithError(Errors.Authentication.PasswordsNotMatch);
    }
}
