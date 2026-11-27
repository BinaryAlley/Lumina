#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.Infrastructure.Validation;
using Nemera.Application.Common.Utilities;
using Nemera.Domain.Common.Errors;
using System;
#endregion

namespace Nemera.Application.Core.MediaConsumption.Commands.ClearConsumptionHistory;

/// <summary>
/// Validates the needed validation rules for <see cref="ClearConsumptionHistoryCommand"/>.
/// </summary>
public class ClearConsumptionHistoryCommandValidator : AbstractValidator<ClearConsumptionHistoryCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ClearConsumptionHistoryCommandValidator"/> class.
    /// </summary>
    public ClearConsumptionHistoryCommandValidator()
    {
        // The target defaults to the current user, so an absent target is valid, but a supplied one must be a real user Id.
        RuleFor(command => command.TargetUserId)
            .Must(targetUserId => targetUserId is null ||
                (Guid.TryParse(targetUserId, out Guid parsedTargetUserId) && parsedTargetUserId != Guid.Empty))
            .WithError(Errors.MediaConsumption.TargetUserIdIsInvalid);
    }
}
