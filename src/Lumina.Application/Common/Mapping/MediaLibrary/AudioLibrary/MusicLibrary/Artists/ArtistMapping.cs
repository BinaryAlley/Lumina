#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Albums;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate;
using System;
using System.Linq;
#endregion

namespace Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Artists;

/// <summary>
/// Extension methods for converting <see cref="Artist"/>.
/// </summary>
public static class ArtistMapping
{
    /// <summary>
    /// Converts <paramref name="domainEntity"/> to <see cref="ArtistEntity"/>.
    /// </summary>
    /// <param name="domainEntity">The domain entity to be converted.</param>
    /// <returns>The converted repository entity.</returns>
    public static ArtistEntity ToRepositoryEntity(this Artist domainEntity)
    {
        return new ArtistEntity
        {
            Id = domainEntity.Id.Value,
            LibraryId = domainEntity.LibraryId.Value,
            Name = domainEntity.Name,
            Website = domainEntity.Website.HasValue ? domainEntity.Website.Value : null,
            MusicBrainzArtistId = domainEntity.MusicBrainzArtistId.HasValue ? domainEntity.MusicBrainzArtistId.Value.Value : null,
            Contributors = [.. domainEntity.Contributors.Select(contributor => new ArtistContributorEntity
            {
                Id = Guid.NewGuid(),
                ArtistId = domainEntity.Id.Value,
                MediaContributorId = contributor.ContributorId.Value,
                Role = contributor.Role,
                CreatedOnUtc = domainEntity.CreatedOnUtc,
                CreatedBy = Guid.Empty,
                UpdatedBy = null
            })],
            Albums = [.. domainEntity.Albums.Select(album => album.ToRepositoryEntity(domainEntity.Id.Value, domainEntity.LibraryId.Value))],
            CreatedOnUtc = domainEntity.CreatedOnUtc,
            CreatedBy = Guid.Empty,
            // The audit columns are owned by the auditing interceptor, which stamps only the rows that actually changed, so they are never mapped here.
            UpdatedOnUtc = null,
            UpdatedBy = null
        };
    }
}
