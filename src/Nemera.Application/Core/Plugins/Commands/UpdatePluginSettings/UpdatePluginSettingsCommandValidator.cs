#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.Infrastructure.Validation;
using Nemera.Application.Common.Utilities;
using Nemera.Domain.Common.Errors;
using System;
#endregion

namespace Nemera.Application.Core.Plugins.Commands.UpdatePluginSettings;

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
            .WithError(Errors.Plugins.PluginIdCannotBeEmpty)
            .Must(id => id != Guid.Empty)
            .WithError(Errors.Plugins.PluginIdCannotBeEmpty);
    }
}
