#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.Common.Metadata;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.Entities;
using System;
using System.Linq;
using Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Common;
#endregion

namespace Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;

/// <summary>
/// Extension methods for converting <see cref="Track"/>.
/// </summary>
public static class TrackMapping
{
    /// <summary>
    /// Converts <paramref name="domainEntity"/> to <see cref="TrackEntity"/>.
    /// </summary>
    /// <param name="domainEntity">The domain entity to be converted.</param>
    /// <param name="albumId">The Id of the album the track belongs to.</param>
    /// <param name="libraryId">The Id of the media library the album of the track belongs to.</param>
    /// <returns>The converted repository entity.</returns>
    public static TrackEntity ToRepositoryEntity(this Track domainEntity, Guid albumId, Guid libraryId)
    {
        return new TrackEntity
        {
            Id = domainEntity.Id.Value,
            AlbumId = albumId,
            LibraryId = libraryId,
            Path = domainEntity.Path,
            Title = domainEntity.Metadata.Title,
            OriginalTitle = domainEntity.Metadata.OriginalTitle.HasValue ? domainEntity.Metadata.OriginalTitle.Value : null,
            Description = domainEntity.Metadata.Description.HasValue ? domainEntity.Metadata.Description.Value : null,
            Disambiguation = domainEntity.Disambiguation.HasValue ? domainEntity.Disambiguation.Value : null,
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
            DurationInSeconds = domainEntity.Metadata.DurationInSeconds,
            SampleRate = domainEntity.Metadata.SampleRate,
            Channels = domainEntity.Metadata.Channels,
            BitDepth = domainEntity.Metadata.BitDepth.HasValue ? domainEntity.Metadata.BitDepth.Value : null,
            AudioCodec = domainEntity.Metadata.AudioCodec.HasValue ? domainEntity.Metadata.AudioCodec.Value : null,
            Bitrate = domainEntity.Metadata.Bitrate.HasValue ? domainEntity.Metadata.Bitrate.Value : null,
            AcoustId = domainEntity.Metadata.AcoustId.HasValue ? domainEntity.Metadata.AcoustId.Value : null,
            ReplayGainTrackGain = domainEntity.Metadata.ReplayGainTrackGain.HasValue ? domainEntity.Metadata.ReplayGainTrackGain.Value : null,
            ReplayGainTrackPeak = domainEntity.Metadata.ReplayGainTrackPeak.HasValue ? domainEntity.Metadata.ReplayGainTrackPeak.Value : null,
            ReplayGainAlbumGain = domainEntity.Metadata.ReplayGainAlbumGain.HasValue ? domainEntity.Metadata.ReplayGainAlbumGain.Value : null,
            ReplayGainAlbumPeak = domainEntity.Metadata.ReplayGainAlbumPeak.HasValue ? domainEntity.Metadata.ReplayGainAlbumPeak.Value : null,
            TrackNumber = domainEntity.TrackNumber,
            DiscNumber = domainEntity.DiscNumber.HasValue ? domainEntity.DiscNumber.Value : null,
            Script = domainEntity.Script.HasValue ? domainEntity.Script.Value : null,
            Key = domainEntity.Key.HasValue ? domainEntity.Key.Value : null,
            Bpm = domainEntity.Bpm.HasValue ? domainEntity.Bpm.Value : null,
            IsVideo = domainEntity.IsVideo,
            WorkTitle = domainEntity.Work.HasValue ? domainEntity.Work.Value.Title : null,
            WorkType = domainEntity.Work.HasValue && domainEntity.Work.Value.Type.HasValue ? domainEntity.Work.Value.Type.Value : null,
            MusicBrainzRecordingId = domainEntity.MusicBrainzRecordingId.HasValue ? domainEntity.MusicBrainzRecordingId.Value.Value : null,
            MusicBrainzTrackId = domainEntity.MusicBrainzTrackId.HasValue ? domainEntity.MusicBrainzTrackId.Value.Value : null,
            MusicBrainzWorkId = domainEntity.Work.HasValue ? domainEntity.Work.Value.MusicBrainzWorkId.Value : null,
            Contributors = [.. domainEntity.Contributors.Select(contributor => new TrackContributorEntity
            {
                Id = Guid.NewGuid(),
                TrackId = domainEntity.Id.Value,
                MediaContributorId = contributor.ContributorId.Value,
                Role = contributor.Role,
                CreatedOnUtc = domainEntity.CreatedOnUtc,
                CreatedBy = Guid.Empty,
                UpdatedBy = null
            })],
            Moods = [.. domainEntity.Moods.ToRepositoryEntities()],
            Isrcs = [.. domainEntity.Isrcs.ToRepositoryEntities()],
            WorkLanguages = domainEntity.Work.HasValue
                ? [.. domainEntity.Work.Value.Languages.Select(language => new TrackWorkLanguageEntity(language.LanguageCode, language.LanguageName, language.NativeName.HasValue ? language.NativeName.Value : null))]
                : [],
            WorkIswcs = domainEntity.Work.HasValue
                ? [.. domainEntity.Work.Value.Iswcs.Select(iswc => new TrackWorkIswcEntity(iswc))]
                : [],
            Ratings = [.. domainEntity.Ratings.ToRepositoryEntities()],
            CreatedOnUtc = domainEntity.CreatedOnUtc,
            CreatedBy = Guid.Empty,
            // The audit columns are owned by the auditing interceptor, which stamps only the rows that actually changed, so they are never mapped here.
            UpdatedOnUtc = null,
            UpdatedBy = null
        };
    }
}
