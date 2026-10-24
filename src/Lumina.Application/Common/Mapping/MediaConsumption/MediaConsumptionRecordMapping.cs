#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaConsumption;
using Lumina.Application.Common.DTO.MediaConsumption;
using Lumina.Contracts.Responses.MediaConsumption;
using Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.MediaConsumptionRecordAggregate;
using System;
#endregion

namespace Lumina.Application.Common.Mapping.MediaConsumption;

/// <summary>
/// Extension methods for converting <see cref="MediaConsumptionRecord"/>. The record is mapped directly to its repository entity and to its response.
/// Repository-driven single-record reads map the entity to its DTO and then to the response.
/// </summary>
public static class MediaConsumptionRecordMapping
{
    /// <summary>
    /// Converts <paramref name="domainEntity"/> to <see cref="MediaConsumptionRecordEntity"/>.
    /// </summary>
    /// <param name="domainEntity">The domain entity to be converted.</param>
    /// <returns>The converted repository entity.</returns>
    public static MediaConsumptionRecordEntity ToRepositoryEntity(this MediaConsumptionRecord domainEntity)
    {
        return new MediaConsumptionRecordEntity
        {
            Id = domainEntity.Id.Value,
            UserId = domainEntity.UserId.Value,
            LibraryId = domainEntity.LibraryId.Value,
            MediaKind = domainEntity.MediaKind,
            MediaItemId = domainEntity.MediaItemId.Value,
            Status = domainEntity.Status,
            StartedOnUtc = domainEntity.StartedOnUtc,
            LastActivityOnUtc = domainEntity.LastActivityOnUtc,
            EndedOnUtc = domainEntity.EndedOnUtc.HasValue ? domainEntity.EndedOnUtc.Value : null,
            // Stored time is truncated to whole milliseconds instead of rounded, so a retried report of the same fractional second maps to
            // the same stored value and is not rejected as a decrease by the domain.
            ConsumedDurationMilliseconds = (long)domainEntity.ConsumedDuration.TotalMilliseconds,
            LastPositionKind = domainEntity.LastPosition.HasValue ? domainEntity.LastPosition.Value.Kind : null,
            LastPositionTimeOffsetMilliseconds = domainEntity.LastPosition.HasValue && domainEntity.LastPosition.Value.TimeOffset.HasValue
                ? (long)domainEntity.LastPosition.Value.TimeOffset.Value.TotalMilliseconds
                : null,
            LastPositionLocation = domainEntity.LastPosition.HasValue && domainEntity.LastPosition.Value.Location.HasValue
                ? domainEntity.LastPosition.Value.Location.Value
                : null,
            LastProgressFraction = domainEntity.LastProgressFraction,
            ConsecutivePagesRead = domainEntity.ConsecutivePagesRead,
            Version = 1,
            CreatedOnUtc = domainEntity.CreatedOnUtc,
            CreatedBy = Guid.Empty,
            // The audit columns are owned by the auditing interceptor, so they are never mapped here.
            UpdatedOnUtc = null,
            UpdatedBy = null
        };
    }

    /// <summary>
    /// Converts <paramref name="domainEntity"/> to <see cref="ConsumptionRecordDto"/>.
    /// </summary>
    /// <param name="domainEntity">The domain entity to be converted.</param>
    /// <returns>The converted data transfer object.</returns>
    public static ConsumptionRecordDto ToDto(this MediaConsumptionRecord domainEntity)
    {
        return new ConsumptionRecordDto(
            domainEntity.Id.Value,
            domainEntity.LibraryId.Value,
            domainEntity.MediaItemId.Value,
            domainEntity.MediaKind,
            domainEntity.Status,
            DateTime.SpecifyKind(domainEntity.StartedOnUtc, DateTimeKind.Utc),
            DateTime.SpecifyKind(domainEntity.LastActivityOnUtc, DateTimeKind.Utc),
            domainEntity.EndedOnUtc.HasValue ? DateTime.SpecifyKind(domainEntity.EndedOnUtc.Value, DateTimeKind.Utc) : null,
            (long)domainEntity.ConsumedDuration.TotalMilliseconds,
            domainEntity.LastPosition.HasValue ? domainEntity.LastPosition.Value.ToDto() : null,
            domainEntity.LastProgressFraction,
            domainEntity.ConsecutivePagesRead
        );
    }

    /// <summary>
    /// Converts <paramref name="domainEntity"/> to <see cref="ConsumptionRecordResponse"/>.
    /// </summary>
    /// <param name="domainEntity">The domain entity to be converted.</param>
    /// <returns>The converted response.</returns>
    public static ConsumptionRecordResponse ToResponse(this MediaConsumptionRecord domainEntity)
    {
        return new ConsumptionRecordResponse(
            domainEntity.Id.Value,
            domainEntity.LibraryId.Value,
            domainEntity.MediaItemId.Value,
            domainEntity.MediaKind,
            domainEntity.Status,
            DateTime.SpecifyKind(domainEntity.StartedOnUtc, DateTimeKind.Utc),
            DateTime.SpecifyKind(domainEntity.LastActivityOnUtc, DateTimeKind.Utc),
            domainEntity.EndedOnUtc.HasValue ? DateTime.SpecifyKind(domainEntity.EndedOnUtc.Value, DateTimeKind.Utc) : null,
            domainEntity.ConsumedDuration.TotalSeconds,
            domainEntity.LastPosition.HasValue ? domainEntity.LastPosition.Value.ToResponse() : null,
            domainEntity.LastProgressFraction,
            domainEntity.ConsecutivePagesRead
        );
    }
}
