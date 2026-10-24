#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaConsumption;
using Lumina.Application.Common.DTO.MediaConsumption;
using Lumina.Application.Common.Utilities;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.Common.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.ExternalIdentifiers.LibraryManagementBoundedContext.LibraryAggregate;
using Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.ExternalIdentifiers.MediaLibraryBoundedContext.MediaItemAggregate;
using Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.ExternalIdentifiers.UserManagementBoundedContext.UserAggregate;
using Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.MediaConsumptionRecordAggregate;
using Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.MediaConsumptionRecordAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.MediaConsumption;
using System;
using DomainErrors = Lumina.Domain.Common.Errors.Errors;
#endregion

namespace Lumina.Application.Common.Mapping.MediaConsumption;

/// <summary>
/// Extension methods for converting <see cref="MediaConsumptionRecordEntity"/>.
/// </summary>
public static class MediaConsumptionRecordEntityMapping
{
    /// <summary>
    /// Converts <paramref name="repositoryEntity"/> to <see cref="MediaConsumptionRecord"/>.
    /// </summary>
    /// <param name="repositoryEntity">The repository entity to be converted.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a successfully converted <see cref="MediaConsumptionRecord"/>, or an error message.
    /// </returns>
    public static Result<MediaConsumptionRecord> ToDomainEntity(this MediaConsumptionRecordEntity repositoryEntity)
    {
        Optional<DateTime> endedOnUtc = repositoryEntity.EndedOnUtc.HasValue
            ? Optional<DateTime>.Some(DateTime.SpecifyKind(repositoryEntity.EndedOnUtc.Value, DateTimeKind.Utc))
            : Optional<DateTime>.None();

        Optional<MediaPosition> lastPosition = Optional<MediaPosition>.None();
        if (repositoryEntity.LastPositionKind.HasValue)
        {
            MediaPositionKind positionKind = repositoryEntity.LastPositionKind.Value;
            if (positionKind == MediaPositionKind.TimeOffset)
            {
                // A stored position outside the representable range is rejected like a negative one, instead of being silently clamped to a position that no client ever reported.
                long maximumTimeOffsetMilliseconds = TimeSpan.MaxValue.Ticks / TimeSpan.TicksPerMillisecond;
                if (!repositoryEntity.LastPositionTimeOffsetMilliseconds.HasValue ||
                    repositoryEntity.LastPositionTimeOffsetMilliseconds.Value < 0 ||
                    repositoryEntity.LastPositionTimeOffsetMilliseconds.Value > maximumTimeOffsetMilliseconds ||
                    repositoryEntity.LastPositionLocation is not null)
                    return DomainErrors.MediaConsumption.InvalidMediaPosition;
                lastPosition = Optional<MediaPosition>.Some(
                    MediaPosition.CreateTimeOffset(TimeSpan.FromMilliseconds(repositoryEntity.LastPositionTimeOffsetMilliseconds.Value)));
            }
            else if (positionKind == MediaPositionKind.Location)
            {
                if (string.IsNullOrWhiteSpace(repositoryEntity.LastPositionLocation) ||
                    repositoryEntity.LastPositionTimeOffsetMilliseconds.HasValue)
                    return DomainErrors.MediaConsumption.InvalidMediaPosition;
                lastPosition = Optional<MediaPosition>.Some(MediaPosition.CreateLocation(repositoryEntity.LastPositionLocation));
            }
            else
                return DomainErrors.MediaConsumption.InvalidMediaPosition;
        }

        return MediaConsumptionRecord.Reconstitute(
            MediaConsumptionRecordId.Create(repositoryEntity.Id),
            UserId.Create(repositoryEntity.UserId),
            LibraryId.Create(repositoryEntity.LibraryId),
            MediaItemId.Create(repositoryEntity.MediaItemId),
            repositoryEntity.MediaKind,
            repositoryEntity.Status,
            DateTime.SpecifyKind(repositoryEntity.StartedOnUtc, DateTimeKind.Utc),
            DateTime.SpecifyKind(repositoryEntity.LastActivityOnUtc, DateTimeKind.Utc),
            endedOnUtc,
            repositoryEntity.ConsumedDurationMilliseconds.ToClampedTimeSpan(),
            lastPosition,
            repositoryEntity.LastProgressFraction,
            repositoryEntity.ConsecutivePagesRead
        );
    }

    /// <summary>
    /// Converts <paramref name="repositoryEntity"/> to <see cref="ConsumptionRecordDto"/>.
    /// </summary>
    /// <param name="repositoryEntity">The repository entity to be converted.</param>
    /// <returns>The converted data transfer object.</returns>
    public static ConsumptionRecordDto ToDto(this MediaConsumptionRecordEntity repositoryEntity)
    {
        MediaPositionDto? lastPosition = null;
        if (repositoryEntity.LastPositionKind.HasValue)
        {
            lastPosition = new MediaPositionDto(
                repositoryEntity.LastPositionKind.Value,
                repositoryEntity.LastPositionTimeOffsetMilliseconds,
                repositoryEntity.LastPositionLocation);
        }

        return new ConsumptionRecordDto(
            repositoryEntity.Id,
            repositoryEntity.LibraryId,
            repositoryEntity.MediaItemId,
            repositoryEntity.MediaKind,
            repositoryEntity.Status,
            DateTime.SpecifyKind(repositoryEntity.StartedOnUtc, DateTimeKind.Utc),
            DateTime.SpecifyKind(repositoryEntity.LastActivityOnUtc, DateTimeKind.Utc),
            repositoryEntity.EndedOnUtc.HasValue ? DateTime.SpecifyKind(repositoryEntity.EndedOnUtc.Value, DateTimeKind.Utc) : null,
            repositoryEntity.ConsumedDurationMilliseconds,
            lastPosition,
            repositoryEntity.LastProgressFraction,
            repositoryEntity.ConsecutivePagesRead
        );
    }
}
