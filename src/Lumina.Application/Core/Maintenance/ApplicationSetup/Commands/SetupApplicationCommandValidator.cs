#region ========================================================================= USING =====================================================================================
using FluentValidation;
using Lumina.Application.Common.Errors;
#endregion

namespace Lumina.Application.Core.Maintenance.ApplicationSetup.Commands;

/// <summary>
/// Validates the needed validation rules for <see cref="SetupApplicationCommand"/>.
/// </summary>
public class SetupApplicationCommandValidator : AbstractValidator<SetupApplicationCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SetupApplicationCommandValidator"/> class.
    /// </summary>
    public SetupApplicationCommandValidator()
    {
        RuleFor(x => x.Username).NotEmpty().WithMessage(Errors.Authentication.UsernameCannotBeEmpty.Description);
        RuleFor(x => x.Password).NotEmpty().WithMessage(Errors.Authentication.PasswordCannotBeEmpty.Description)
            .Matches("^(?=.*[A-Z])(?=.*[0-9])(?=.*[^a-zA-Z0-9]).{8,}$").WithMessage(Errors.Authentication.InvalidPassword.Description);
        RuleFor(x => x.PasswordConfirm).NotEmpty().WithMessage(Errors.Authentication.PasswordConfirmCannotBeEmpty.Description);
        RuleFor(x => x.Password).Equal(x => x.PasswordConfirm).WithMessage(Errors.Authentication.PasswordsNotMatch.Description);
    }
}
