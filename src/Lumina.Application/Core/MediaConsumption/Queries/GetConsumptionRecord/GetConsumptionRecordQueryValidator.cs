#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Utilities;
using Lumina.Domain.Common.Errors;
using System;
#endregion

namespace Lumina.Application.Core.MediaConsumption.Queries.GetConsumptionRecord;

/// <summary>
/// Validates the needed validation rules for <see cref="GetConsumptionRecordQuery"/>.
/// </summary>
public class GetConsumptionRecordQueryValidator : AbstractValidator<GetConsumptionRecordQuery>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetConsumptionRecordQueryValidator"/> class.
    /// </summary>
    public GetConsumptionRecordQueryValidator()
    {
        RuleFor(query => query.ConsumptionRecordId)
            .NotEmpty()
            .WithError(Errors.MediaConsumption.ConsumptionRecordIdCannotBeEmpty);

        RuleFor(query => query.ConsumptionRecordId)
            .Must(consumptionRecordId => Guid.TryParse(consumptionRecordId, out Guid parsedConsumptionRecordId) && parsedConsumptionRecordId != Guid.Empty)
            .When(query => query.ConsumptionRecordId is not null && query.ConsumptionRecordId.Length > 0)
            .WithError(Errors.MediaConsumption.ConsumptionRecordIdCannotBeEmpty);
    }
}
