#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Infrastructure.Models.DTO.Configuration;
using Lumina.Application.Common.Utilities;
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
        RuleFor(settings => settings.Directory)
            .NotEmpty()
            .WithError(Errors.Errors.Configuration.PluginsDirectoryCannotBeEmpty);
    }
}
