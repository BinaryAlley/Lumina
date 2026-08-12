#region ========================================================================= USING =====================================================================================
using FluentValidation;
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
        RuleFor(x => x.PluginId)
            .NotEmpty().WithMessage(Errors.Plugins.PluginIdCannotBeEmpty.Description);
    }
}
