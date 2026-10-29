#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaConsumption;
using Lumina.Application.Common.DTO.MediaConsumption;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.Common.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.ExternalIdentifiers.LibraryManagementBoundedContext.LibraryAggregate;
using Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.ExternalIdentifiers.MediaLibraryBoundedContext.MediaItemAggregate;
using Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.ExternalIdentifiers.UserManagementBoundedContext.UserAggregate;
using Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.MediaResumePointAggregate;
using Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.MediaResumePointAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.MediaConsumption;
using System;
using DomainErrors = Lumina.Domain.Common.Errors.Errors;
#endregion

namespace Lumina.Application.Common.Mapping.MediaConsumption;

/// <summary>
/// Extension methods for converting <see cref="MediaResumePointEntity"/>.
/// </summary>
public static class MediaResumePointEntityMapping
{
    /// <summary>
    /// Converts <paramref name="repositoryEntity"/> to <see cref="MediaResumePoint"/>.
    /// </summary>
    /// <param name="repositoryEntity">The repository entity to be converted.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a successfully converted <see cref="MediaResumePoint"/>, or an error message.
    /// </returns>
    public static Result<MediaResumePoint> ToDomainEntity(this MediaResumePointEntity repositoryEntity)
    {
        MediaPosition lastPosition;
        if (repositoryEntity.LastPositionKind == MediaPositionKind.TimeOffset)
        {
            // A stored position outside the representable range is rejected like a negative one, instead of being silently clamped to a position that no client ever reported.
            long maximumTimeOffsetMilliseconds = TimeSpan.MaxValue.Ticks / TimeSpan.TicksPerMillisecond;
            if (!repositoryEntity.LastPositionTimeOffsetMilliseconds.HasValue ||
                repositoryEntity.LastPositionTimeOffsetMilliseconds.Value < 0 ||
                repositoryEntity.LastPositionTimeOffsetMilliseconds.Value > maximumTimeOffsetMilliseconds ||
                repositoryEntity.LastPositionLocation is not null)
                return DomainErrors.MediaConsumption.InvalidMediaPosition;
            lastPosition = MediaPosition.CreateTimeOffset(TimeSpan.FromMilliseconds(repositoryEntity.LastPositionTimeOffsetMilliseconds.Value));
        }
        else if (repositoryEntity.LastPositionKind == MediaPositionKind.Location)
        {
            if (string.IsNullOrWhiteSpace(repositoryEntity.LastPositionLocation) ||
                repositoryEntity.LastPositionTimeOffsetMilliseconds.HasValue)
                return DomainErrors.MediaConsumption.InvalidMediaPosition;
            lastPosition = MediaPosition.CreateLocation(repositoryEntity.LastPositionLocation);
        }
        else
            return DomainErrors.MediaConsumption.InvalidMediaPosition;

        return MediaResumePoint.Reconstitute(
            MediaResumePointId.Create(repositoryEntity.Id),
            UserId.Create(repositoryEntity.UserId),
            LibraryId.Create(repositoryEntity.LibraryId),
            MediaItemId.Create(repositoryEntity.MediaItemId),
            repositoryEntity.MediaKind,
            lastPosition,
            repositoryEntity.LastProgressFraction,
            DateTime.SpecifyKind(repositoryEntity.LastConsumedOnUtc, DateTimeKind.Utc),
            DateTime.SpecifyKind(repositoryEntity.CreatedOnUtc, DateTimeKind.Utc),
            repositoryEntity.UpdatedOnUtc.HasValue
                ? Optional<DateTime>.Some(DateTime.SpecifyKind(repositoryEntity.UpdatedOnUtc.Value, DateTimeKind.Utc))
                : Optional<DateTime>.None()
        );
    }

    /// <summary>
    /// Converts <paramref name="repositoryEntity"/> to <see cref="ResumePointDto"/>.
    /// </summary>
    /// <param name="repositoryEntity">The repository entity to be converted.</param>
    /// <returns>The converted data transfer object.</returns>
    public static ResumePointDto ToDto(this MediaResumePointEntity repositoryEntity)
    {
        return new ResumePointDto(
            repositoryEntity.Id,
            repositoryEntity.LibraryId,
            repositoryEntity.MediaItemId,
            repositoryEntity.MediaKind,
            new MediaPositionDto(
                repositoryEntity.LastPositionKind,
                repositoryEntity.LastPositionTimeOffsetMilliseconds,
                repositoryEntity.LastPositionLocation),
            repositoryEntity.LastProgressFraction,
            DateTime.SpecifyKind(repositoryEntity.LastConsumedOnUtc, DateTimeKind.Utc)
        );
    }
}
