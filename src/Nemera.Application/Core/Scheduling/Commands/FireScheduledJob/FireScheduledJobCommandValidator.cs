#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.Infrastructure.Validation;
using Nemera.Application.Common.Utilities;
using Nemera.Domain.Common.Errors;
#endregion

namespace Nemera.Application.Core.Scheduling.Commands.FireScheduledJob;

/// <summary>
/// Validates the needed validation rules for <see cref="FireScheduledJobCommand"/>.
/// </summary>
public class FireScheduledJobCommandValidator : AbstractValidator<FireScheduledJobCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FireScheduledJobCommandValidator"/> class.
    /// </summary>
    public FireScheduledJobCommandValidator()
    {
        RuleFor(command => command.ScheduledJobId)
            .NotEmpty()
            .WithError(Errors.Scheduling.ScheduledJobNotFound);
    }
}
