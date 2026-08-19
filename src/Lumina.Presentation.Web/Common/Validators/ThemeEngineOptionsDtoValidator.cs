#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.Configuration;
using Lumina.Presentation.Web.Common.Primitives;
using Lumina.Presentation.Web.Common.Utilities;
using Lumina.Presentation.Web.Common.Validation;
#endregion

namespace Lumina.Presentation.Web.Common.Validators;

/// <summary>
/// Validates the needed validation rules for the ThemeEngine application configuration settings section.
/// </summary>
public class ThemeEngineOptionsDtoValidator : AbstractValidator<ThemeEngineOptionsDto>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ThemeEngineOptionsDtoValidator"/> class.
    /// </summary>
    public ThemeEngineOptionsDtoValidator()
    {
        RuleFor(themeEngineOptions => themeEngineOptions.StoragePath)
            .NotEmpty()
            .WithError(Error.Validation(description: "Theme storage path cannot be empty!"));

        RuleFor(themeEngineOptions => themeEngineOptions.SettingsPath)
            .NotEmpty()
            .WithError(Error.Validation(description: "Theme settings path cannot be empty!"));

        RuleFor(themeEngineOptions => themeEngineOptions.DefaultThemeId)
            .NotEmpty()
            .WithError(Error.Validation(description: "Default theme Id cannot be empty!"));

        RuleFor(themeEngineOptions => themeEngineOptions.MaxArchiveBytes)
            .GreaterThan(0)
            .WithError(Error.Validation(description: "Maximum archive size must be greater than 0!"));

        RuleFor(themeEngineOptions => themeEngineOptions.MaxExpandedBytes)
            .GreaterThan(0)
            .WithError(Error.Validation(description: "Maximum expanded size must be greater than 0!"));

        RuleFor(themeEngineOptions => themeEngineOptions.MaxSingleFileBytes)
            .GreaterThan(0)
            .WithError(Error.Validation(description: "Maximum single file size must be greater than 0!"));

        RuleFor(themeEngineOptions => themeEngineOptions.MaxEntries)
            .GreaterThan(0)
            .WithError(Error.Validation(description: "Maximum entries count must be greater than 0!"));
    }
}
