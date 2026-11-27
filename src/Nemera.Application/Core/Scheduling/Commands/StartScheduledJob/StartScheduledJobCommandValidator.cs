#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.Infrastructure.Validation;
using Nemera.Application.Common.Utilities;
using Nemera.Domain.Common.Errors;
#endregion

namespace Nemera.Application.Core.Scheduling.Commands.StartScheduledJob;

/// <summary>
/// Validates the needed validation rules for <see cref="StartScheduledJobCommand"/>.
/// </summary>
public class StartScheduledJobCommandValidator : AbstractValidator<StartScheduledJobCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StartScheduledJobCommandValidator"/> class.
    /// </summary>
    public StartScheduledJobCommandValidator()
    {
        RuleFor(command => command.ScheduledJobId)
            .NotEmpty()
            .WithError(Errors.Scheduling.ScheduledJobNotFound);
    }
}
