#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Utilities;
using Lumina.Infrastructure.Common.Models.DTO.Configuration;
using System;
#endregion

namespace Lumina.Infrastructure.Common.Validators;

/// <summary>
/// Validates the needed validation rules for the JwtSettings application configuration settings section.
/// </summary>
public class CorsSettingsDtoValidator : AbstractValidator<CorsSettingsDto>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CorsSettingsDtoValidator"/> class.
    /// </summary>
    public CorsSettingsDtoValidator()
    {
        RuleFor(settings => settings.AllowedOrigins)
            .NotEmpty()
            .WithError(Errors.Errors.Configuration.CorsOriginsCannotBeEmpty);

        RuleForEach(settings => settings.AllowedOrigins)
            .NotEmpty()
            .WithError(Errors.Errors.Configuration.CorsOriginsCannotBeEmpty)
            .Must(BeValidOrigin)
            .WithError(Errors.Errors.Configuration.CorsOriginIsInvalid);
    }

    /// <summary>
    /// Validates the format of an individual CORS origin.
    /// </summary>
    /// <param name="origin">The origin to validate.</param>
    /// <returns><see langword="true"/> if the origin is a valid absolute HTTP or HTTPS URL, or the wildcard origin; otherwise, <see langword="false"/>.</returns>
    private bool BeValidOrigin(string origin)
    {
        if (origin == "*")
            return true; // wildcard allowed without credentials
        return Uri.TryCreate(origin, UriKind.Absolute, out Uri? uri) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps) && !origin.EndsWith('/');
    }
}
