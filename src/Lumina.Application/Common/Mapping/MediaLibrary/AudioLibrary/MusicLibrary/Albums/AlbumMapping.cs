#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.Common.Metadata;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.Entities;
using System;
using System.Linq;
using Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Common;
using Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;

#endregion

namespace Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Albums;

/// <summary>
/// Extension methods for converting <see cref="Album"/>.
/// </summary>
public static class AlbumMapping
{
    /// <summary>
    /// Converts <paramref name="domainEntity"/> to <see cref="AlbumEntity"/>.
    /// </summary>
    /// <param name="domainEntity">The domain entity to be converted.</param>
    /// <param name="artistId">The Id of the artist the album belongs to.</param>
    /// <param name="libraryId">The Id of the media library the album belongs to.</param>
    /// <returns>The converted repository entity.</returns>
    public static AlbumEntity ToRepositoryEntity(this Album domainEntity, Guid artistId, Guid libraryId)
    {
        return new AlbumEntity
        {
            Id = domainEntity.Id.Value,
            ArtistId = artistId,
            LibraryId = libraryId,
            Title = domainEntity.Metadata.Title,
            OriginalTitle = domainEntity.Metadata.OriginalTitle.HasValue ? domainEntity.Metadata.OriginalTitle.Value : null,
            Description = domainEntity.Metadata.Description.HasValue ? domainEntity.Metadata.Description.Value : null,
            OriginalReleaseDate = domainEntity.Metadata.ReleaseInfo.OriginalReleaseDate.HasValue ? domainEntity.Metadata.ReleaseInfo.OriginalReleaseDate.Value : null,
            OriginalReleaseYear = domainEntity.Metadata.ReleaseInfo.OriginalReleaseYear.HasValue ? domainEntity.Metadata.ReleaseInfo.OriginalReleaseYear.Value : null,
            ReReleaseDate = domainEntity.Metadata.ReleaseInfo.ReReleaseDate.HasValue ? domainEntity.Metadata.ReleaseInfo.ReReleaseDate.Value : null,
            ReReleaseYear = domainEntity.Metadata.ReleaseInfo.ReReleaseYear.HasValue ? domainEntity.Metadata.ReleaseInfo.ReReleaseYear.Value : null,
            ReleaseCountry = domainEntity.Metadata.ReleaseInfo.ReleaseCountry.HasValue ? domainEntity.Metadata.ReleaseInfo.ReleaseCountry.Value : null,
            ReleaseVersion = domainEntity.Metadata.ReleaseInfo.ReleaseVersion.HasValue ? domainEntity.Metadata.ReleaseInfo.ReleaseVersion.Value : null,
            LanguageCode = domainEntity.Metadata.Language.HasValue ? domainEntity.Metadata.Language.Value.LanguageCode : null,
            LanguageName = domainEntity.Metadata.Language.HasValue ? domainEntity.Metadata.Language.Value.LanguageName : null,
            LanguageNativeName = domainEntity.Metadata.Language.HasValue && domainEntity.Metadata.Language.Value.NativeName.HasValue ? domainEntity.Metadata.Language.Value.NativeName.Value : null,
            OriginalLanguageCode = domainEntity.Metadata.OriginalLanguage.HasValue ? domainEntity.Metadata.OriginalLanguage.Value.LanguageCode : null,
            OriginalLanguageName = domainEntity.Metadata.OriginalLanguage.HasValue ? domainEntity.Metadata.OriginalLanguage.Value.LanguageName : null,
            OriginalLanguageNativeName = domainEntity.Metadata.OriginalLanguage.HasValue && domainEntity.Metadata.OriginalLanguage.Value.NativeName.HasValue ? domainEntity.Metadata.OriginalLanguage.Value.NativeName.Value : null,
            Genres = [.. domainEntity.Metadata.Genres.ToRepositoryEntities()],
            Tags = [.. domainEntity.Metadata.Tags.ToRepositoryEntities()],
            ReleaseType = domainEntity.Metadata.ReleaseType.HasValue ? domainEntity.Metadata.ReleaseType.Value : null,
            ReleaseStatus = domainEntity.Metadata.ReleaseStatus.HasValue ? domainEntity.Metadata.ReleaseStatus.Value : null,
            TotalDiscs = domainEntity.Metadata.TotalDiscs.HasValue ? domainEntity.Metadata.TotalDiscs.Value : null,
            TotalTracks = domainEntity.Metadata.TotalTracks,
            MediaFormat = domainEntity.MediaFormat.HasValue ? domainEntity.MediaFormat.Value : null,
            Barcode = domainEntity.Barcode.HasValue ? domainEntity.Barcode.Value.Value : null,
            CatalogNumber = domainEntity.CatalogNumber.HasValue ? domainEntity.CatalogNumber.Value : null,
            MusicBrainzReleaseId = domainEntity.MusicBrainzReleaseId.HasValue ? domainEntity.MusicBrainzReleaseId.Value.Value : null,
            MusicBrainzReleaseGroupId = domainEntity.MusicBrainzReleaseGroupId.HasValue ? domainEntity.MusicBrainzReleaseGroupId.Value.Value : null,
            MusicBrainzReleaseArtistId = domainEntity.MusicBrainzReleaseArtistId.HasValue ? domainEntity.MusicBrainzReleaseArtistId.Value.Value : null,
            Contributors = [.. domainEntity.Contributors.Select(contributor => new AlbumContributorEntity
            {
                Id = Guid.NewGuid(),
                AlbumId = domainEntity.Id.Value,
                MediaContributorId = contributor.ContributorId.Value,
                Role = contributor.Role,
                CreatedOnUtc = domainEntity.CreatedOnUtc,
                CreatedBy = Guid.Empty,
                UpdatedBy = null
            })],
            Ratings = [.. domainEntity.Ratings.ToRepositoryEntities()],
            Tracks = [.. domainEntity.Tracks.Select(track => track.ToRepositoryEntity(domainEntity.Id.Value, libraryId))],
            CreatedOnUtc = domainEntity.CreatedOnUtc,
            CreatedBy = Guid.Empty,
            // The audit columns are owned by the auditing interceptor, which stamps only the rows that actually changed, so they are never mapped here.
            UpdatedOnUtc = null,
            UpdatedBy = null
        };
    }
}
