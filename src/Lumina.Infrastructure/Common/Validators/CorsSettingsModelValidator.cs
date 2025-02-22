#region ========================================================================= USING =====================================================================================
using FluentValidation;
using Lumina.Infrastructure.Common.Models.Configuration;
using System;
#endregion

namespace Lumina.Infrastructure.Common.Validators;

/// <summary>
/// Validates the needed validation rules for the JwtSettings application configuration settings section.
/// </summary>
public class CorsSettingsModelValidator : AbstractValidator<CorsSettingsModel>
{
    /// <summary>
    /// Initializes validation rules for CORS configuration
    /// </summary>
    public CorsSettingsModelValidator()
    {
        RuleFor(x => x.AllowedOrigins)
            .NotEmpty()
            .WithMessage(Errors.Errors.Configuration.CorsOriginsCannotBeEmpty.Description)
            .ForEach(originRule =>
            {
                originRule.NotEmpty()
                    .WithMessage(Errors.Errors.Configuration.CorsOriginsCannotBeEmpty.Description)
                    .Must(BeValidOrigin)
                    .WithMessage(Errors.Errors.Configuration.CorsOriginIsInvalid.Description);
            });
    }

    /// <summary>
    /// Validates individual origin format
    /// </summary>
    private bool BeValidOrigin(string origin)
    {
        if (origin == "*")
            return true;  // wildcard allowed without credentials
        return Uri.TryCreate(origin, UriKind.Absolute, out Uri? uri) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps) && !origin.EndsWith('/');
    }
}
