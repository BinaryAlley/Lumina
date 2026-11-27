#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Utilities;
using Lumina.Domain.Common.Errors;
#endregion

namespace Lumina.Application.Core.Scheduling.Commands.StartScheduledJob;

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
