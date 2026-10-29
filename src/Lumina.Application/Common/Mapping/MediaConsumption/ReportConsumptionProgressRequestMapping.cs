#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.MediaConsumption.Commands.ReportConsumptionProgress;
using Lumina.Contracts.Requests.MediaConsumption;
#endregion

namespace Lumina.Application.Common.Mapping.MediaConsumption;

/// <summary>
/// Extension methods for converting <see cref="ReportConsumptionProgressRequest"/>.
/// </summary>
public static class ReportConsumptionProgressRequestMapping
{
    /// <summary>
    /// Converts <paramref name="request"/> to <see cref="ReportConsumptionProgressCommand"/>.
    /// </summary>
    /// <param name="request">The request to be converted.</param>
    /// <param name="consumptionRecordId">The Id of the consumption record, taken from the route.</param>
    /// <returns>The converted command.</returns>
    public static ReportConsumptionProgressCommand ToCommand(this ReportConsumptionProgressRequest request, string? consumptionRecordId)
    {
        return new ReportConsumptionProgressCommand(
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
