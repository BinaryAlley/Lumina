#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.MediaConsumption.Commands.StopConsumption;
using Lumina.Contracts.Requests.MediaConsumption;
#endregion

namespace Lumina.Application.Common.Mapping.MediaConsumption;

/// <summary>
/// Extension methods for converting <see cref="StopConsumptionRequest"/>.
/// </summary>
public static class StopConsumptionRequestMapping
{
    /// <summary>
    /// Converts <paramref name="request"/> to <see cref="StopConsumptionCommand"/>.
    /// </summary>
    /// <param name="request">The request to be converted.</param>
    /// <param name="consumptionRecordId">The Id of the consumption record, taken from the route.</param>
    /// <returns>The converted command.</returns>
    public static StopConsumptionCommand ToCommand(this StopConsumptionRequest request, string? consumptionRecordId)
    {
        return new StopConsumptionCommand(
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
