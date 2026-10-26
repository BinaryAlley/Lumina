#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.MediaConsumption.Commands.CompleteConsumption;
using Lumina.Contracts.Requests.MediaConsumption;
#endregion

namespace Lumina.Application.Common.Mapping.MediaConsumption;

/// <summary>
/// Extension methods for converting <see cref="CompleteConsumptionRequest"/>.
/// </summary>
public static class CompleteConsumptionRequestMapping
{
    /// <summary>
    /// Converts <paramref name="request"/> to <see cref="CompleteConsumptionCommand"/>.
    /// </summary>
    /// <param name="request">The request to be converted.</param>
    /// <param name="consumptionRecordId">The Id of the consumption record, taken from the route.</param>
    /// <returns>The converted command.</returns>
    public static CompleteConsumptionCommand ToCommand(this CompleteConsumptionRequest request, string? consumptionRecordId)
    {
        return new CompleteConsumptionCommand(
            consumptionRecordId,
            request.Position?.Kind,
            request.Position?.TimeOffsetSeconds,
            request.Position?.Location,
            request.ProgressFraction,
            request.ConsumedDurationInSeconds,
            request.ConsecutivePagesRead
        );
    }
}
