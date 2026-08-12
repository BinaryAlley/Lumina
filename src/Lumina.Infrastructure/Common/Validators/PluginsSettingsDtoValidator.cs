#region ========================================================================= USING =====================================================================================
using FluentValidation;
using Lumina.Application.Common.Infrastructure.Models.DTO.Configuration;
using Lumina.Infrastructure.Common.Errors;
#endregion

namespace Lumina.Infrastructure.Common.Validators;

/// <summary>
/// Validates the needed validation rules for the PluginsSettings application configuration settings section.
/// </summary>
public class PluginsSettingsDtoValidator : AbstractValidator<PluginsSettingsDto>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PluginsSettingsDtoValidator"/> class.
    /// </summary>
    public PluginsSettingsDtoValidator()
    {
        RuleFor(x => x.Directory).NotEmpty().WithMessage(Errors.Errors.Configuration.PluginsDirectoryCannotBeEmpty.Description);
    }
}
