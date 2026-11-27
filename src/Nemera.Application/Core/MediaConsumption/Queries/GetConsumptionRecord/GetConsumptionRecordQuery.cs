#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Core.MediaConsumption.Queries.GetConsumptionRecord;

/// <summary>
/// Query for getting a consumption record of the current user.
/// </summary>
/// <param name="ConsumptionRecordId">The Id of the consumption record, taken from the route.</param>
[DebuggerDisplay("ConsumptionRecordId: {ConsumptionRecordId}")]
public record GetConsumptionRecordQuery(
    string? ConsumptionRecordId
) : IQuery;
