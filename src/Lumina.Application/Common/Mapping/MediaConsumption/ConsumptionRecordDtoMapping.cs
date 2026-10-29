#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DTO.MediaConsumption;
using Lumina.Application.Common.Utilities;
using Lumina.Contracts.Responses.MediaConsumption;
using System;
#endregion

namespace Lumina.Application.Common.Mapping.MediaConsumption;

/// <summary>
/// Extension methods for converting <see cref="ConsumptionRecordDto"/>. This mapping is the record response builder for repository-driven
/// single-record reads.
/// </summary>
public static class ConsumptionRecordDtoMapping
{
    /// <summary>
    /// Converts <paramref name="data"/> to <see cref="ConsumptionRecordResponse"/>.
    /// </summary>
    /// <param name="data">The data transfer object to be converted.</param>
    /// <returns>The converted response.</returns>
    public static ConsumptionRecordResponse ToResponse(this ConsumptionRecordDto data)
    {
        return new ConsumptionRecordResponse(
            data.Id,
            data.LibraryId,
            data.MediaItemId,
            data.MediaKind,
            data.Status,
            DateTime.SpecifyKind(data.StartedOnUtc, DateTimeKind.Utc),
            DateTime.SpecifyKind(data.LastActivityOnUtc, DateTimeKind.Utc),
            data.EndedOnUtc.HasValue ? DateTime.SpecifyKind(data.EndedOnUtc.Value, DateTimeKind.Utc) : null,
            data.ConsumedDurationMilliseconds.ToClampedTimeSpan().TotalSeconds,
            data.LastPosition?.ToResponse(),
            data.LastProgressFraction,
            data.ConsecutivePagesRead
        );
    }
}
