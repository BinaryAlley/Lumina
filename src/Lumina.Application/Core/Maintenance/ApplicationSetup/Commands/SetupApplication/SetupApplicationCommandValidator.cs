#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Errors;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Utilities;
#endregion

namespace Lumina.Application.Core.Maintenance.ApplicationSetup.Commands.SetupApplication;

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
        RuleFor(command => command.Username)
            .NotEmpty()
            .WithError(Errors.Authentication.UsernameCannotBeEmpty);

        RuleFor(command => command.Password)
            .NotEmpty()
            .WithError(Errors.Authentication.PasswordCannotBeEmpty)
            .Matches("^(?=.*[A-Z])(?=.*[0-9])(?=.*[^a-zA-Z0-9]).{8,}$")
            .WithError(Errors.Authentication.InvalidPassword);

        RuleFor(command => command.PasswordConfirm)
            .NotEmpty()
            .WithError(Errors.Authentication.PasswordConfirmCannotBeEmpty);

        RuleFor(command => command.Password)
            .Equal(command => command.PasswordConfirm)
            .WithError(Errors.Authentication.PasswordsNotMatch);
    }
}
