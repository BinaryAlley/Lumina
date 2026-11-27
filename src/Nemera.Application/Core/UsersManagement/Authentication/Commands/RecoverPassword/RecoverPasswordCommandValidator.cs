#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.Errors;
using Nemera.Application.Common.Infrastructure.Validation;
using Nemera.Application.Common.Utilities;
#endregion

namespace Nemera.Application.Core.UsersManagement.Authentication.Commands.RecoverPassword;

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
