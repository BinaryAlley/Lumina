#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Utilities;
using Lumina.Infrastructure.Common.Models.DTO.Configuration;
#endregion

namespace Lumina.Infrastructure.Common.Validators;

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
            .WithError(Errors.Errors.Configuration.ThemeStoragePathCannotBeEmpty);

        RuleFor(themeEngineOptions => themeEngineOptions.BundledThemesPath)
            .NotEmpty()
            .WithError(Errors.Errors.Configuration.ThemeBundledThemesPathCannotBeEmpty);

        RuleFor(themeEngineOptions => themeEngineOptions.DefaultThemeId)
            .NotEmpty()
            .WithError(Errors.Errors.Configuration.ThemeDefaultThemeIdCannotBeEmpty);

        RuleFor(themeEngineOptions => themeEngineOptions.MaxArchiveBytes)
            .GreaterThan(0)
            .WithError(Errors.Errors.Configuration.ThemeMaxArchiveBytesMustBePositive);

        RuleFor(themeEngineOptions => themeEngineOptions.MaxExpandedBytes)
            .GreaterThan(0)
            .WithError(Errors.Errors.Configuration.ThemeMaxExpandedBytesMustBePositive);

        RuleFor(themeEngineOptions => themeEngineOptions.MaxSingleFileBytes)
            .GreaterThan(0)
            .WithError(Errors.Errors.Configuration.ThemeMaxSingleFileBytesMustBePositive);

        RuleFor(themeEngineOptions => themeEngineOptions.MaxEntries)
            .GreaterThan(0)
            .WithError(Errors.Errors.Configuration.ThemeMaxEntriesMustBePositive);
    }
}
