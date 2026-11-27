#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.Infrastructure.Validation;
using Nemera.Application.Common.Utilities;
using Nemera.Domain.Common.Errors;
#endregion

namespace Nemera.Application.Core.Scheduling.Commands.StopScheduledJob;

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
