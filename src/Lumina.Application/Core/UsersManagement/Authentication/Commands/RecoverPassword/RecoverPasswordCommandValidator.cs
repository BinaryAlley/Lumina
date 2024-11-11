#region ========================================================================= USING =====================================================================================
using FluentValidation;
using Lumina.Application.Common.Errors;
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
        RuleFor(x => x.Username).NotEmpty().WithMessage(Errors.Authentication.UsernameCannotBeEmpty.Description);
        RuleFor(x => x.TotpCode).NotEmpty().WithMessage(Errors.Authentication.TotpCannotBeEmpty.Description);
        RuleFor(x => x.TotpCode).Length(6).WithMessage(Errors.Authentication.InvalidTotpCode.Description);
        RuleFor(x => x.TotpCode).Matches(@"^[0-9]{6}$").WithMessage(Errors.Authentication.InvalidTotpCode.Description);
    }
}
