#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Errors;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Utilities;
#endregion

namespace Lumina.Application.Core.UsersManagement.Authentication.Commands.RegisterUser;

/// <summary>
/// Validates the needed validation rules for <see cref="RegisterUserCommand"/>.
/// </summary>
public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RegisterUserCommandValidator"/> class.
    /// </summary>
    public RegisterUserCommandValidator()
    {
        RuleFor(command => command.Username)
            .NotEmpty()
            .WithError(Errors.Authentication.UsernameCannotBeEmpty)
            .Length(3, 255)
            .WithError(Errors.Authentication.UsernameMustBeBetween3And255CharactersLong)
            .Matches("^[a-zA-Z0-9][a-zA-Z0-9._-]*[a-zA-Z0-9]$") // only allow letters, numbers, dots, underscores and hyphens, and must start and end with letter or number
            .WithError(Errors.Authentication.InvalidUsername);
       
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
