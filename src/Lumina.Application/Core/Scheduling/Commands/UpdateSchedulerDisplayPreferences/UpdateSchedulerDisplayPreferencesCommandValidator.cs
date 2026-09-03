#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Utilities;
using Lumina.Domain.Common.Errors;
#endregion

namespace Lumina.Application.Core.Scheduling.Commands.UpdateSchedulerDisplayPreferences;

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
