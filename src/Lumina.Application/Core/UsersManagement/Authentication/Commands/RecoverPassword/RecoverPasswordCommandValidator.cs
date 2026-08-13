#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Errors;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Utilities;
#endregion

namespace Lumina.Application.Core.UsersManagement.Authentication.Commands.RecoverPassword;

/// <summary>
/// Validates the needed validation rules for <see cref="RecoverPasswordCommand"/>.
/// </summary>
public class RecoverPasswordCommandValidator : AbstractValidator<RecoverPasswordCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RecoverPasswordCommandValidator"/> class.
    /// </summary>
    public RecoverPasswordCommandValidator()
    {
        RuleFor(command => command.Username)
            .NotEmpty()
            .WithError(Errors.Authentication.UsernameCannotBeEmpty);
        
        RuleFor(command => command.TotpCode)
            .NotEmpty()
            .WithError(Errors.Authentication.TotpCannotBeEmpty);
       
        RuleFor(command => command.TotpCode)
            .Length(6)
            .WithError(Errors.Authentication.InvalidTotpCode);
        
        RuleFor(command => command.TotpCode)
            .Matches(@"^[0-9]{6}$")
            .WithError(Errors.Authentication.InvalidTotpCode);
    }
}
