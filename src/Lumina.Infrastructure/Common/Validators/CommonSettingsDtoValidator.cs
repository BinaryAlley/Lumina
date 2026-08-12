#region ========================================================================= USING =====================================================================================
using FluentValidation;
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
        RuleFor(x => x.Theme).NotEmpty().WithMessage(Errors.Errors.Configuration.ApplicationThemeCannotBeEmpty.Description);
    }
}
