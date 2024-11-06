#region ========================================================================= USING =====================================================================================
using FluentValidation;
using Lumina.Infrastructure.Common.Models.Configuration;
#endregion

namespace Lumina.Infrastructure.Common.Validators;

/// <summary>
/// Validates the needed validation rules for the JwtSettings application configuration settings section.
/// </summary>
public class JwtSettingsModelValidator : AbstractValidator<JwtSettingsModel>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="JwtSettingsModelValidator"/> class.
    /// </summary>
    public JwtSettingsModelValidator()
    {
        RuleFor(x => x.SecretKey)
            .NotEmpty()
            .WithMessage(Errors.Errors.Configuration.JwtSecretKeyCannotBeEmpty.Description)
            .MinimumLength(32)
            .WithMessage(Errors.Errors.Configuration.JwtSecretKeyTooShort.Description);

        RuleFor(x => x.ExpiryMinutes)
            .GreaterThan(0)
            .WithMessage(Errors.Errors.Configuration.JwtExpiryMinutesMustBePositive.Description);

        RuleFor(x => x.Issuer)
            .NotEmpty()
            .WithMessage(Errors.Errors.Configuration.JwtIssuerCannotBeEmpty.Description);

        RuleFor(x => x.Audience)
            .NotEmpty()
            .WithMessage(Errors.Errors.Configuration.JwtAudienceCannotBeEmpty.Description);
    }
}
