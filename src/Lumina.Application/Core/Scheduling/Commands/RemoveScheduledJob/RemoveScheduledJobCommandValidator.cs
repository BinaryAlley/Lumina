#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Utilities;
using Lumina.Domain.Common.Errors;
#endregion

namespace Lumina.Application.Core.Scheduling.Commands.RemoveScheduledJob;

/// <summary>
/// Validates the needed validation rules for <see cref="RemoveScheduledJobCommand"/>.
/// </summary>
public class RemoveScheduledJobCommandValidator : AbstractValidator<RemoveScheduledJobCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RemoveScheduledJobCommandValidator"/> class.
    /// </summary>
    public RemoveScheduledJobCommandValidator()
    {
        RuleFor(command => command.ScheduledJobId)
            .NotEmpty()
            .WithError(Errors.Scheduling.ScheduledJobNotFound);
    }
}
