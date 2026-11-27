#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Utilities;
using Lumina.Domain.Common.Errors;
using System;
#endregion

namespace Lumina.Application.Core.MediaConsumption.Commands.DeleteConsumptionRecord;

/// <summary>
/// Validates the needed validation rules for <see cref="DeleteConsumptionRecordCommand"/>.
/// </summary>
public class DeleteConsumptionRecordCommandValidator : AbstractValidator<DeleteConsumptionRecordCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteConsumptionRecordCommandValidator"/> class.
    /// </summary>
    public DeleteConsumptionRecordCommandValidator()
    {
        RuleFor(command => command.ConsumptionRecordId)
            .NotEmpty()
            .WithError(Errors.MediaConsumption.ConsumptionRecordIdCannotBeEmpty);

        RuleFor(command => command.ConsumptionRecordId)
            .Must(consumptionRecordId => Guid.TryParse(consumptionRecordId, out Guid parsedConsumptionRecordId) && parsedConsumptionRecordId != Guid.Empty)
            .When(command => command.ConsumptionRecordId is not null && command.ConsumptionRecordId.Length > 0)
            .WithError(Errors.MediaConsumption.ConsumptionRecordIdCannotBeEmpty);

        // The target defaults to the current user, so an absent target is valid, but a supplied one must be a real user Id.
        RuleFor(command => command.TargetUserId)
            .Must(targetUserId => targetUserId is null ||
                (Guid.TryParse(targetUserId, out Guid parsedTargetUserId) && parsedTargetUserId != Guid.Empty))
            .WithError(Errors.MediaConsumption.TargetUserIdIsInvalid);
    }
}
