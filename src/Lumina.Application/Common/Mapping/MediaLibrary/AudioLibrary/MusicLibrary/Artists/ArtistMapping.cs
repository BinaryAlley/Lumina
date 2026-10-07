#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Common.Mapping.Common.Metadata;
using Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Albums;
using Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Common;
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
            SortName = domainEntity.SortName.HasValue ? domainEntity.SortName.Value : null,
            Disambiguation = domainEntity.Disambiguation.HasValue ? domainEntity.Disambiguation.Value : null,
            Type = domainEntity.Type.HasValue ? domainEntity.Type.Value : null,
            Gender = domainEntity.Gender.HasValue ? domainEntity.Gender.Value : null,
            Country = domainEntity.Country.HasValue ? domainEntity.Country.Value : null,
            LifeSpanBegin = domainEntity.LifeSpanBegin.HasValue ? domainEntity.LifeSpanBegin.Value : null,
            LifeSpanEnd = domainEntity.LifeSpanEnd.HasValue ? domainEntity.LifeSpanEnd.Value : null,
            IsEnded = domainEntity.IsEnded,
            Website = domainEntity.Website.HasValue ? domainEntity.Website.Value : null,
            MusicBrainzArtistId = domainEntity.MusicBrainzArtistId.HasValue ? domainEntity.MusicBrainzArtistId.Value.Value : null,
            Area = domainEntity.Area.HasValue ? domainEntity.Area.Value.ToRepositoryEntity() : null,
            BeginArea = domainEntity.BeginArea.HasValue ? domainEntity.BeginArea.Value.ToRepositoryEntity() : null,
            EndArea = domainEntity.EndArea.HasValue ? domainEntity.EndArea.Value.ToRepositoryEntity() : null,
            Aliases = [.. domainEntity.Aliases.ToRepositoryEntities()],
            Ipis = [.. domainEntity.Ipis.Select(ipi => new ArtistIpiEntity(ipi))],
            Isnis = [.. domainEntity.Isnis.Select(isni => new ArtistIsniEntity(isni))],
            Genres = [.. domainEntity.Genres.ToRepositoryEntities()],
            Tags = [.. domainEntity.Tags.ToRepositoryEntities()],
            Ratings = [.. domainEntity.Ratings.ToRepositoryEntities()],
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
