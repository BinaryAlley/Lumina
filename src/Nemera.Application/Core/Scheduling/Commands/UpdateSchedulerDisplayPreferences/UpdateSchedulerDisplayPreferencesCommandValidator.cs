#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.Errors;
using Nemera.Application.Common.Infrastructure.Validation;
using Nemera.Application.Common.Utilities;
#endregion

namespace Nemera.Application.Core.Scheduling.Commands.UpdateSchedulerDisplayPreferences;

/// <summary>
/// Validates the needed validation rules for <see cref="UpdateSchedulerDisplayPreferencesCommand"/>.
/// </summary>
public class UpdateSchedulerDisplayPreferencesCommandValidator : AbstractValidator<UpdateSchedulerDisplayPreferencesCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateSchedulerDisplayPreferencesCommandValidator"/> class.
    /// </summary>
    public UpdateSchedulerDisplayPreferencesCommandValidator()
    {
        RuleFor(command => command.DisplayTimeSpan)
            .GreaterThan(0)
            .WithError(Errors.Scheduling.SchedulerDisplayTimeSpanMustBePositive);
    }
}
