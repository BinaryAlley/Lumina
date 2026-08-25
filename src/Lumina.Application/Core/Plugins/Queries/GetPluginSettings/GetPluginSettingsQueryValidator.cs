#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Utilities;
using Lumina.Domain.Common.Errors;
using System;
#endregion

namespace Lumina.Application.Core.Plugins.Queries.GetPluginSettings;

/// <summary>
/// Validates the needed validation rules for <see cref="GetPluginSettingsQuery"/>.
/// </summary>
public class GetPluginSettingsQueryValidator : AbstractValidator<GetPluginSettingsQuery>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetPluginSettingsQueryValidator"/> class.
    /// </summary>
    public GetPluginSettingsQueryValidator()
    {
        RuleFor(query => query.PluginId)
            .NotEmpty()
            .WithError(Errors.Plugins.PluginIdCannotBeEmpty)
            .Must(id => id != Guid.Empty)
            .WithError(Errors.Plugins.PluginIdCannotBeEmpty);
    }
}
