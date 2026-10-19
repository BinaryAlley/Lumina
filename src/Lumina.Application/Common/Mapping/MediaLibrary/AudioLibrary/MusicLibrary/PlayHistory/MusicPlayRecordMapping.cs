#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Domain.Core.BoundedContexts.MusicPlayHistoryBoundedContext.MusicPlayRecordAggregate;
using System;
#endregion

namespace Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.PlayHistory;

/// <summary>
/// Extension methods for converting <see cref="MusicPlayRecord"/>.
/// </summary>
public static class MusicPlayRecordMapping
{
    /// <summary>
    /// Converts <paramref name="domainEntity"/> to <see cref="MusicPlayRecordEntity"/>.
    /// </summary>
    /// <param name="domainEntity">The domain entity to be converted.</param>
    /// <returns>The converted repository entity.</returns>
    public static MusicPlayRecordEntity ToRepositoryEntity(this MusicPlayRecord domainEntity)
    {
        return new MusicPlayRecordEntity
        {
            Id = domainEntity.Id.Value,
            UserId = domainEntity.UserId.Value,
            TrackId = domainEntity.TrackId.Value,
            LibraryId = domainEntity.LibraryId.Value,
            AlbumId = domainEntity.AlbumId.Value,
            ArtistId = domainEntity.ArtistId.Value,
            Status = domainEntity.Status,
            // Stored time is truncated to whole milliseconds instead of rounded, so a retried report of the same fractional second maps to
            // the same stored value and is not rejected as a decrease by the domain.
            LastPositionMilliseconds = (long)domainEntity.LastPosition.Offset.TotalMilliseconds,
            ListenedDurationMilliseconds = (long)domainEntity.ListenedDuration.TotalMilliseconds,
            EndedOnUtc = domainEntity.EndedOnUtc.HasValue ? domainEntity.EndedOnUtc.Value : null,
            Version = 1,
            CreatedOnUtc = domainEntity.CreatedOnUtc,
            CreatedBy = Guid.Empty,
            // The audit columns are owned by the auditing interceptor, which stamps only the rows that actually changed, so they are never mapped here.
            UpdatedOnUtc = null,
            UpdatedBy = null
        };
    }
}
