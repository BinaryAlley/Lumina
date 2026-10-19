#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.PlayHistory;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.MusicPlayHistoryBoundedContext.ExternalIdentifiers.AudioLibraryBoundedContext.MusicLibraryAggregate;
using Lumina.Domain.Core.BoundedContexts.MusicPlayHistoryBoundedContext.ExternalIdentifiers.LibraryManagementBoundedContext.LibraryAggregate;
using Lumina.Domain.Core.BoundedContexts.MusicPlayHistoryBoundedContext.ExternalIdentifiers.UserManagementBoundedContext.UserAggregate;
using Lumina.Domain.Core.BoundedContexts.MusicPlayHistoryBoundedContext.MusicPlayRecordAggregate;
using Lumina.Domain.Core.BoundedContexts.MusicPlayHistoryBoundedContext.MusicPlayRecordAggregate.ValueObjects;
using System;
#endregion

namespace Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.PlayHistory;

/// <summary>
/// Extension methods for converting <see cref="MusicPlayRecordEntity"/>.
/// </summary>
public static class MusicPlayRecordEntityMapping
{
    /// <summary>
    /// Converts <paramref name="repositoryEntity"/> to <see cref="MusicPlayRecord"/>.
    /// </summary>
    /// <param name="repositoryEntity">The repository entity to be converted.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a successfully converted <see cref="MusicPlayRecord"/>, or an error message.
    /// </returns>
    public static Result<MusicPlayRecord> ToDomainEntity(this MusicPlayRecordEntity repositoryEntity)
    {
        return MusicPlayRecord.Create(
            MusicPlayRecordId.Create(repositoryEntity.Id),
            UserId.Create(repositoryEntity.UserId),
            TrackId.Create(repositoryEntity.TrackId),
            LibraryId.Create(repositoryEntity.LibraryId),
            AlbumId.Create(repositoryEntity.AlbumId),
            ArtistId.Create(repositoryEntity.ArtistId),
            repositoryEntity.Status,
            MusicPlayPosition.Create(TimeSpan.FromMilliseconds(repositoryEntity.LastPositionMilliseconds)),
            TimeSpan.FromMilliseconds(repositoryEntity.ListenedDurationMilliseconds),
            Optional<DateTime>.FromNullable(repositoryEntity.EndedOnUtc),
            repositoryEntity.CreatedOnUtc,
            Optional<DateTime>.FromNullable(repositoryEntity.UpdatedOnUtc)
        );
    }

    /// <summary>
    /// Converts <paramref name="repositoryEntity"/> to <see cref="PlayRecordResponse"/>.
    /// </summary>
    /// <param name="repositoryEntity">The repository entity to be converted.</param>
    /// <returns>The converted response.</returns>
    public static PlayRecordResponse ToResponse(this MusicPlayRecordEntity repositoryEntity)
    {
        return new PlayRecordResponse(
            repositoryEntity.Id,
            repositoryEntity.TrackId,
            repositoryEntity.LibraryId,
            repositoryEntity.AlbumId,
            repositoryEntity.ArtistId,
            repositoryEntity.Status,
            TimeSpan.FromMilliseconds(repositoryEntity.LastPositionMilliseconds).TotalSeconds,
            TimeSpan.FromMilliseconds(repositoryEntity.ListenedDurationMilliseconds).TotalSeconds,
            repositoryEntity.CreatedOnUtc,
            repositoryEntity.EndedOnUtc,
            repositoryEntity.UpdatedOnUtc
        );
    }
}
