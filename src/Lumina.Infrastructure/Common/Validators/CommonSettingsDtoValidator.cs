#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Utilities;
using Lumina.Infrastructure.Common.Models.DTO.Configuration;
#endregion

namespace Lumina.Infrastructure.Common.Validators;

/// <summary>
/// Validates the needed validation rules for the CommonSettings application configuration settings section.
/// </summary>
public class CommonSettingsDtoValidator : AbstractValidator<CommonSettingsDto>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CommonSettingsDtoValidator"/> class.
    /// </summary>
    public CommonSettingsDtoValidator()
    {
        RuleFor(settings => settings.Theme)
            .NotEmpty()
            .WithError(Errors.Errors.Configuration.ApplicationThemeCannotBeEmpty);
    }
}
