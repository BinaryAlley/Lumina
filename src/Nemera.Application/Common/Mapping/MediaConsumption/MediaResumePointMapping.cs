#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.DataAccess.Entities.MediaConsumption;
using Nemera.Application.Common.DTO.MediaConsumption;
using Nemera.Contracts.Responses.MediaConsumption;
using Nemera.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.MediaResumePointAggregate;
using System;
#endregion

namespace Nemera.Application.Common.Mapping.MediaConsumption;

/// <summary>
/// Extension methods for converting <see cref="MediaResumePoint"/>. The resume point is mapped directly to its repository entity and to its response.
/// Repository-driven single-record reads map the entity to its DTO and then to the response.
/// </summary>
public static class MediaResumePointMapping
{
    /// <summary>
    /// Converts <paramref name="domainEntity"/> to <see cref="MediaResumePointEntity"/>.
    /// </summary>
    /// <param name="domainEntity">The domain entity to be converted.</param>
    /// <returns>The converted repository entity.</returns>
    public static MediaResumePointEntity ToRepositoryEntity(this MediaResumePoint domainEntity)
    {
        return new MediaResumePointEntity
        {
            Id = domainEntity.Id.Value,
            UserId = domainEntity.UserId.Value,
            LibraryId = domainEntity.LibraryId.Value,
            MediaKind = domainEntity.MediaKind,
            MediaItemId = domainEntity.MediaItemId.Value,
            LastPositionKind = domainEntity.LastPosition.Kind,
            LastPositionTimeOffsetMilliseconds = domainEntity.LastPosition.TimeOffset.HasValue
                ? (long)domainEntity.LastPosition.TimeOffset.Value.TotalMilliseconds
                : null,
            LastPositionLocation = domainEntity.LastPosition.Location.HasValue
                ? domainEntity.LastPosition.Location.Value
                : null,
            LastProgressFraction = domainEntity.LastProgressFraction,
            LastConsumedOnUtc = domainEntity.LastConsumedOnUtc,
            Version = 1
        };
    }

    /// <summary>
    /// Converts <paramref name="domainEntity"/> to <see cref="ResumePointDto"/>.
    /// </summary>
    /// <param name="domainEntity">The domain entity to be converted.</param>
    /// <returns>The converted data transfer object.</returns>
    public static ResumePointDto ToDto(this MediaResumePoint domainEntity)
    {
        return new ResumePointDto(
            domainEntity.Id.Value,
            domainEntity.LibraryId.Value,
            domainEntity.MediaItemId.Value,
            domainEntity.MediaKind,
            domainEntity.LastPosition.ToDto(),
            domainEntity.LastProgressFraction,
            DateTime.SpecifyKind(domainEntity.LastConsumedOnUtc, DateTimeKind.Utc)
        );
    }

    /// <summary>
    /// Converts <paramref name="domainEntity"/> to <see cref="ResumePointResponse"/>.
    /// </summary>
    /// <param name="domainEntity">The domain entity to be converted.</param>
    /// <returns>The converted response.</returns>
    public static ResumePointResponse ToResponse(this MediaResumePoint domainEntity)
    {
        return new ResumePointResponse(
            domainEntity.Id.Value,
            domainEntity.LibraryId.Value,
            domainEntity.MediaItemId.Value,
            domainEntity.MediaKind,
            domainEntity.LastPosition.ToResponse(),
            domainEntity.LastProgressFraction,
            DateTime.SpecifyKind(domainEntity.LastConsumedOnUtc, DateTimeKind.Utc)
        );
    }
}
