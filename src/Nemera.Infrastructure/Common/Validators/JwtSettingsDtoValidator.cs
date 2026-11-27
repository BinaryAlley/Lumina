#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.Infrastructure.Validation;
using Nemera.Application.Common.Utilities;
using Nemera.Infrastructure.Common.Models.DTO.Configuration;
using System;
#endregion

namespace Nemera.Infrastructure.Common.Validators;

/// <summary>
/// Validates the needed validation rules for the JwtSettings application configuration settings section.
/// </summary>
public class JwtSettingsDtoValidator : AbstractValidator<JwtSettingsDto>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="JwtSettingsDtoValidator"/> class.
    /// </summary>
    public JwtSettingsDtoValidator()
    {
        RuleFor(settings => settings.SecretKey)
            .NotEmpty()
            .WithError(Errors.Errors.Configuration.JwtSecretKeyCannotBeEmpty)
            .MinimumLength(32)
            .WithError(Errors.Errors.Configuration.JwtSecretKeyTooShort);

        RuleFor(settings => settings.ExpiryMinutes)
            .GreaterThan(0)
            .WithError(Errors.Errors.Configuration.JwtExpiryMinutesMustBePositive);

        RuleFor(settings => settings.Issuer)
            .NotEmpty()
            .WithError(Errors.Errors.Configuration.JwtIssuerCannotBeEmpty);

        RuleFor(settings => settings.Audience)
            .NotEmpty()
            .WithError(Errors.Errors.Configuration.JwtAudienceCannotBeEmpty);
    }
}
