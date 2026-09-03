#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Utilities;
using Lumina.Domain.Common.Errors;
#endregion

namespace Lumina.Application.Core.Scheduling.Commands.StopScheduledJob;

/// <summary>
/// Validates the needed validation rules for <see cref="StopScheduledJobCommand"/>.
/// </summary>
public class StopScheduledJobCommandValidator : AbstractValidator<StopScheduledJobCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StopScheduledJobCommandValidator"/> class.
    /// </summary>
    public StopScheduledJobCommandValidator()
    {
        RuleFor(command => command.ScheduledJobId)
            .NotEmpty()
            .WithError(Errors.Scheduling.ScheduledJobNotFound);
    }
}
