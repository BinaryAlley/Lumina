#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.Infrastructure.Validation;
using Nemera.Application.Common.Infrastructure.Models.DTO.Configuration;
using Nemera.Application.Common.Utilities;
using Nemera.Infrastructure.Common.Errors;
#endregion

namespace Nemera.Infrastructure.Common.Validators;

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
