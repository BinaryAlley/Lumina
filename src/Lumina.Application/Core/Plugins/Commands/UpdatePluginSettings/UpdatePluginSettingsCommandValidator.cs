#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Utilities;
using Lumina.Domain.SharedKernel.Common.Errors;
#endregion

namespace Lumina.Application.Core.Plugins.Commands.UpdatePluginSettings;

/// <summary>
/// Validates the needed validation rules for <see cref="UpdatePluginSettingsCommand"/>.
/// </summary>
public class UpdatePluginSettingsCommandValidator : AbstractValidator<UpdatePluginSettingsCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdatePluginSettingsCommandValidator"/> class.
    /// </summary>
    public UpdatePluginSettingsCommandValidator()
    {
        RuleFor(command => command.PluginId)
            .NotEmpty()
            .WithError(Errors.Plugins.PluginIdCannotBeEmpty);
    }
}
