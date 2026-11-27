#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.Infrastructure.Validation;
using Nemera.Application.Common.Utilities;
using Nemera.Domain.Common.Errors;
using System;
#endregion

namespace Nemera.Application.Core.Plugins.Queries.GetPluginSettings;

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
