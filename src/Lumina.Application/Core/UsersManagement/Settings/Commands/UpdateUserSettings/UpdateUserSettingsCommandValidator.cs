#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Utilities;
using Lumina.Domain.Common.Errors;
#endregion

namespace Lumina.Application.Core.UsersManagement.Settings.Commands.UpdateUserSettings;

/// <summary>
/// Validates the needed validation rules for <see cref="UpdateUserSettingsCommand"/>.
/// </summary>
public class UpdateUserSettingsCommandValidator : AbstractValidator<UpdateUserSettingsCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateUserSettingsCommandValidator"/> class.
    /// </summary>
    public UpdateUserSettingsCommandValidator()
    {
        RuleFor(command => command.ItemsPerPage)
            .GreaterThan(0)
            .WithError(Errors.UserSettings.ItemsPerPageMustBeGreaterThanZero);
    }
}
