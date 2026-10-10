#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Common.Mapping.Common.Metadata;
using Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Common;
using Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Common.ValueObjects.Metadata;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.Common.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.Entities;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.AudioLibraryBoundedContext.Common.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.Entities;
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.Common;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;

/// <summary>
/// Contains unit tests for the <see cref="TrackMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class TrackMappingTests
{
    private readonly TrackFixture _trackFixture = new();
    private readonly AudioMetadataFixture _audioMetadataFixture = new();

    [Fact]
    public void ToRepositoryEntity_WhenMappingCompleteTrack_ShouldMapAllPropertiesCorrectly()
    {
        // Arrange
        Guid albumId = Guid.NewGuid();
        Guid libraryId = Guid.NewGuid();
        Track track = _trackFixture.Create();

        // Act
        TrackEntity result = track.ToRepositoryEntity(albumId, libraryId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(track.Id.Value, result.Id);
        Assert.Equal(albumId, result.AlbumId);
        Assert.Equal(libraryId, result.LibraryId);
        Assert.Equal(track.Path, result.Path);
        Assert.Equal(track.Metadata.Title, result.Title);
        Assert.Equal(track.Metadata.OriginalTitle.HasValue ? track.Metadata.OriginalTitle.Value : null, result.OriginalTitle);
        Assert.Equal(track.Metadata.Description.HasValue ? track.Metadata.Description.Value : null, result.Description);
        Assert.Equal(track.Metadata.ReleaseInfo.OriginalReleaseDate.HasValue ? track.Metadata.ReleaseInfo.OriginalReleaseDate.Value : null, result.OriginalReleaseDate);
        Assert.Equal(track.Metadata.ReleaseInfo.OriginalReleaseYear.HasValue ? track.Metadata.ReleaseInfo.OriginalReleaseYear.Value : null, result.OriginalReleaseYear);
        Assert.Equal(track.Metadata.ReleaseInfo.ReReleaseDate.HasValue ? track.Metadata.ReleaseInfo.ReReleaseDate.Value : null, result.ReReleaseDate);
        Assert.Equal(track.Metadata.ReleaseInfo.ReReleaseYear.HasValue ? track.Metadata.ReleaseInfo.ReReleaseYear.Value : null, result.ReReleaseYear);
        Assert.Equal(track.Metadata.ReleaseInfo.ReleaseCountry.HasValue ? track.Metadata.ReleaseInfo.ReleaseCountry.Value : null, result.ReleaseCountry);
        Assert.Equal(track.Metadata.ReleaseInfo.ReleaseVersion.HasValue ? track.Metadata.ReleaseInfo.ReleaseVersion.Value : null, result.ReleaseVersion);
        Assert.Equal(track.Metadata.Language.HasValue ? track.Metadata.Language.Value.LanguageCode : null, result.LanguageCode);
        Assert.Equal(track.Metadata.Language.HasValue ? track.Metadata.Language.Value.LanguageName : null, result.LanguageName);
        Assert.Equal(track.Metadata.Language.HasValue && track.Metadata.Language.Value.NativeName.HasValue ? track.Metadata.Language.Value.NativeName.Value : null, result.LanguageNativeName);
        Assert.Equal(track.Metadata.OriginalLanguage.HasValue ? track.Metadata.OriginalLanguage.Value.LanguageCode : null, result.OriginalLanguageCode);
        Assert.Equal(track.Metadata.OriginalLanguage.HasValue ? track.Metadata.OriginalLanguage.Value.LanguageName : null, result.OriginalLanguageName);
        Assert.Equal(track.Metadata.OriginalLanguage.HasValue && track.Metadata.OriginalLanguage.Value.NativeName.HasValue ? track.Metadata.OriginalLanguage.Value.NativeName.Value : null, result.OriginalLanguageNativeName);
        // The repository entity stores genres and tags as sets, so duplicate names in the domain collection collapse into a single entry.
        Assert.Equal(track.Metadata.Genres.ToRepositoryEntities().Distinct().OrderBy(genre => genre.Name), result.Genres.OrderBy(genre => genre.Name));
        Assert.Equal(track.Metadata.Tags.ToRepositoryEntities().Distinct().OrderBy(tag => tag.Name), result.Tags.OrderBy(tag => tag.Name));
        Assert.Equal(track.Metadata.DurationInSeconds, result.DurationInSeconds);
        Assert.Equal(track.Metadata.SampleRate, result.SampleRate);
        Assert.Equal(track.Metadata.Channels, result.Channels);
        Assert.Equal(track.Metadata.BitDepth.HasValue ? track.Metadata.BitDepth.Value : null, result.BitDepth);
        Assert.Equal(track.Metadata.AudioCodec.HasValue ? track.Metadata.AudioCodec.Value : null, result.AudioCodec);
        Assert.Equal(track.Metadata.Bitrate.HasValue ? track.Metadata.Bitrate.Value : null, result.Bitrate);
        Assert.Equal(track.TrackNumber, result.TrackNumber);
        Assert.Equal(track.DiscNumber.HasValue ? track.DiscNumber.Value : null, result.DiscNumber);
        Assert.Equal(track.Script.HasValue ? track.Script.Value : null, result.Script);
        Assert.Equal(track.Key.HasValue ? track.Key.Value : null, result.Key);
        Assert.Equal(track.Bpm.HasValue ? track.Bpm.Value : null, result.Bpm);
        Assert.Equal(track.Work.HasValue ? track.Work.Value.Title : null, result.WorkTitle);
        Assert.Equal(track.MusicBrainzRecordingId.HasValue ? track.MusicBrainzRecordingId.Value.Value : null, result.MusicBrainzRecordingId);
        Assert.Equal(track.MusicBrainzTrackId.HasValue ? track.MusicBrainzTrackId.Value.Value : null, result.MusicBrainzTrackId);
        Assert.Equal(track.Work.HasValue ? track.Work.Value.MusicBrainzWorkId.Value : null, result.MusicBrainzWorkId);
        Assert.Equal(track.Moods.ToRepositoryEntities(), result.Moods);
        Assert.Equal(track.Isrcs.ToRepositoryEntities(), result.Isrcs);
        Assert.Equal(track.Ratings.ToRepositoryEntities(), result.Ratings);
        Assert.Equal(track.CreatedOnUtc, result.CreatedOnUtc);
        Assert.Equal(Guid.Empty, result.CreatedBy);
        Assert.Null(result.UpdatedOnUtc);
        Assert.Null(result.UpdatedBy);
    }

    [Fact]
    public void ToRepositoryEntity_WhenMappingContributors_ShouldCreateAContributorEntityPerContributor()
    {
        // Arrange
        Guid albumId = Guid.NewGuid();
        Guid libraryId = Guid.NewGuid();
        Track track = _trackFixture.Create();

        // Act
        TrackEntity result = track.ToRepositoryEntity(albumId, libraryId);

        // Assert
        Assert.Equal(track.Contributors.Count, result.Contributors.Count);
        for (int i = 0; i < track.Contributors.Count; i++)
        {
            MusicMediaContributor contributor = track.Contributors.ElementAt(i);
            TrackContributorEntity contributorEntity = result.Contributors[i];
            Assert.Equal(contributor.ContributorId.Value, contributorEntity.MediaContributorId);
            Assert.Equal(contributor.Role, contributorEntity.Role);
            Assert.Equal(track.Id.Value, contributorEntity.TrackId);
            Assert.Equal(track.CreatedOnUtc, contributorEntity.CreatedOnUtc);
            Assert.Equal(Guid.Empty, contributorEntity.CreatedBy);
            Assert.Null(contributorEntity.UpdatedOnUtc);
            Assert.Null(contributorEntity.UpdatedBy);
        }
    }

    [Fact]
    public void ToRepositoryEntity_WhenOptionalPropertiesAreMissing_ShouldMapTheRequiredValuesAndNulls()
    {
        // Arrange
        Guid albumId = Guid.NewGuid();
        Guid libraryId = Guid.NewGuid();
        AudioMetadata metadata = _audioMetadataFixture.Create(
            originalTitle: Optional<string>.None(),
            description: Optional<string>.None(),
            releaseInfo: ReleaseInfo.Create(
                Optional<DateOnly>.None(),
                Optional<int>.None(),
                Optional<DateOnly>.None(),
                Optional<int>.None(),
                Optional<ReleaseCountry>.None(),
                Optional<string>.None()).Value,
            language: Optional<LanguageInfo>.None(),
            originalLanguage: Optional<LanguageInfo>.None(),
            bitDepth: Optional<int>.None(),
            audioCodec: Optional<string>.None(),
            bitrate: Optional<int>.None());
        Track track = _trackFixture.Create(
            metadata: metadata,
            discNumber: Optional<int>.None(),
            script: Optional<string>.None(),
            key: Optional<MusicKey>.None(),
            bpm: Optional<int>.None(),
            work: Optional<MusicWork>.None(),
            moods: [],
            isrcs: [],
            musicBrainzRecordingId: Optional<MusicBrainzId>.None(),
            musicBrainzTrackId: Optional<MusicBrainzId>.None(),
            contributors: [],
            ratings: []);

        // Act
        TrackEntity result = track.ToRepositoryEntity(albumId, libraryId);

        // Assert
        Assert.Null(result.OriginalTitle);
        Assert.Null(result.Description);
        Assert.Null(result.OriginalReleaseDate);
        Assert.Null(result.OriginalReleaseYear);
        Assert.Null(result.ReReleaseDate);
        Assert.Null(result.ReReleaseYear);
        Assert.Null(result.ReleaseCountry);
        Assert.Null(result.ReleaseVersion);
        Assert.Null(result.LanguageCode);
        Assert.Null(result.LanguageName);
        Assert.Null(result.LanguageNativeName);
        Assert.Null(result.OriginalLanguageCode);
        Assert.Null(result.OriginalLanguageName);
        Assert.Null(result.OriginalLanguageNativeName);
        Assert.Null(result.BitDepth);
        Assert.Null(result.AudioCodec);
        Assert.Null(result.Bitrate);
        Assert.Null(result.DiscNumber);
        Assert.Null(result.Script);
        Assert.Null(result.Key);
        Assert.Null(result.Bpm);
        Assert.Null(result.WorkTitle);
        Assert.Null(result.MusicBrainzRecordingId);
        Assert.Null(result.MusicBrainzTrackId);
        Assert.Null(result.MusicBrainzWorkId);
        Assert.Empty(result.Moods);
        Assert.Empty(result.Isrcs);
        Assert.Empty(result.Contributors);
        Assert.Empty(result.Ratings);
    }
}
