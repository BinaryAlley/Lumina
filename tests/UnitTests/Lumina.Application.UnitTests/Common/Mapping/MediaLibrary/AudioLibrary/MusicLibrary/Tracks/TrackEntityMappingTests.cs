#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Common;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Contracts.DTO.MediaContributors;
using Lumina.Contracts.DTO.MediaLibrary.AudioLibrary;
using Lumina.Contracts.DTO.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;

/// <summary>
/// Contains unit tests for the <see cref="TrackEntityMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class TrackEntityMappingTests
{
    private readonly TrackEntityFixture _trackEntityFixture = new();
    private readonly AudioRatingEntityFixture _audioRatingEntityFixture = new();

    [Fact]
    public void ToDomainEntity_WhenMappingValidTrackEntity_ShouldMapAllPropertiesCorrectly()
    {
        // Arrange
        TrackEntity entity = _trackEntityFixture.Create();

        // Act
        Result<Track> result = entity.ToDomainEntity();

        // Assert
        Assert.False(result.IsFailure);
        Track track = result.Value;
        Assert.Equal(entity.Id, track.Id.Value);
        Assert.Equal(entity.Path, track.Path);
        Assert.Equal(entity.Title, track.Metadata.Title);
        Assert.True(track.Metadata.OriginalTitle.HasValue);
        Assert.Equal(entity.OriginalTitle, track.Metadata.OriginalTitle.Value);
        Assert.True(track.Metadata.Description.HasValue);
        Assert.Equal(entity.Description, track.Metadata.Description.Value);
        Assert.Equal(entity.OriginalReleaseDate, track.Metadata.ReleaseInfo.OriginalReleaseDate.Value);
        Assert.Equal(entity.OriginalReleaseYear, track.Metadata.ReleaseInfo.OriginalReleaseYear.Value);
        Assert.Equal(entity.ReReleaseDate, track.Metadata.ReleaseInfo.ReReleaseDate.Value);
        Assert.Equal(entity.ReReleaseYear, track.Metadata.ReleaseInfo.ReReleaseYear.Value);
        Assert.Equal(entity.ReleaseCountry, track.Metadata.ReleaseInfo.ReleaseCountry.Value);
        Assert.Equal(entity.ReleaseVersion, track.Metadata.ReleaseInfo.ReleaseVersion.Value);
        Assert.True(track.Metadata.Language.HasValue);
        Assert.Equal(entity.LanguageCode!.ToLowerInvariant(), track.Metadata.Language.Value.LanguageCode);
        Assert.Equal(entity.LanguageName, track.Metadata.Language.Value.LanguageName);
        Assert.Equal(entity.LanguageNativeName, track.Metadata.Language.Value.NativeName.Value);
        Assert.True(track.Metadata.OriginalLanguage.HasValue);
        Assert.Equal(entity.OriginalLanguageCode!.ToLowerInvariant(), track.Metadata.OriginalLanguage.Value.LanguageCode);
        Assert.Equal(entity.OriginalLanguageName, track.Metadata.OriginalLanguage.Value.LanguageName);
        Assert.Equal(entity.OriginalLanguageNativeName, track.Metadata.OriginalLanguage.Value.NativeName.Value);
        Assert.Equal(entity.DurationInSeconds, track.Metadata.DurationInSeconds);
        Assert.Equal(entity.SampleRate, track.Metadata.SampleRate);
        Assert.Equal(entity.Channels, track.Metadata.Channels);
        Assert.Equal(entity.BitDepth, track.Metadata.BitDepth.Value);
        Assert.Equal(entity.AudioCodec, track.Metadata.AudioCodec.Value);
        Assert.Equal(entity.Bitrate, track.Metadata.Bitrate.Value);
        Assert.Equal(entity.Genres.Select(genre => genre.Name).OrderBy(name => name), track.Metadata.Genres.Select(genre => genre.Name).OrderBy(name => name));
        Assert.Equal(entity.Tags.Select(tag => tag.Name).OrderBy(name => name), track.Metadata.Tags.Select(tag => tag.Name).OrderBy(name => name));
        Assert.Equal(entity.TrackNumber, track.TrackNumber);
        Assert.Equal(entity.DiscNumber, track.DiscNumber.Value);
        Assert.Equal(entity.Script, track.Script.Value);
        Assert.Equal(entity.Key, track.Key.Value);
        Assert.Equal(entity.Bpm, track.Bpm.Value);
        Assert.Equal(entity.Work, track.Work.Value);
        Assert.Equal(entity.MusicBrainzRecordingId, track.MusicBrainzRecordingId.Value.Value);
        Assert.Equal(entity.MusicBrainzTrackId, track.MusicBrainzTrackId.Value.Value);
        Assert.Equal(entity.MusicBrainzWorkId, track.MusicBrainzWorkId.Value.Value);
        Assert.Equal(entity.Moods.Select(mood => mood.Name), track.Moods.Select(mood => mood.Name));
        Assert.Equal(entity.Isrcs.Select(isrc => isrc.Value), track.Isrcs.Select(isrc => isrc.Value));
        Assert.Equal(entity.Ratings.Select(rating => rating.Value), track.Ratings.Select(rating => (decimal?)rating.Value));
        Assert.Equal(entity.Contributors.Select(contributor => contributor.MediaContributorId), track.Contributors.Select(contributor => contributor.ContributorId.Value));
        Assert.Equal(entity.Contributors.Select(contributor => contributor.Role), track.Contributors.Select(contributor => contributor.Role));
        Assert.Equal(entity.CreatedOnUtc, track.CreatedOnUtc);
        Assert.Equal(entity.UpdatedOnUtc, track.UpdatedOnUtc.Value);
    }

    [Fact]
    public void ToDomainEntity_WhenReleaseDateAndYearDoNotMatch_ShouldReturnError()
    {
        // Arrange
        TrackEntity entity = _trackEntityFixture.Create(
            originalReleaseDate: new DateOnly(2000, 1, 1),
            originalReleaseYear: 1999);

        // Act
        Result<Track> result = entity.ToDomainEntity();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Metadata.OriginalReleaseDateAndYearMustMatch, result.FirstError);
    }

    [Fact]
    public void ToDomainEntity_WhenGenreIsInvalid_ShouldReturnError()
    {
        // Arrange
        TrackEntity entity = _trackEntityFixture.Create();
        entity.Genres = [new GenreEntity(string.Empty)];

        // Act
        Result<Track> result = entity.ToDomainEntity();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Metadata.GenreNameCannotBeEmpty, result.FirstError);
    }

    [Fact]
    public void ToDomainEntity_WhenTagIsInvalid_ShouldReturnError()
    {
        // Arrange
        TrackEntity entity = _trackEntityFixture.Create();
        entity.Tags = [new TagEntity("   ")];

        // Act
        Result<Track> result = entity.ToDomainEntity();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Metadata.TagNameCannotBeEmpty, result.FirstError);
    }

    [Fact]
    public void ToDomainEntity_WhenMoodIsInvalid_ShouldReturnError()
    {
        // Arrange
        TrackEntity entity = _trackEntityFixture.Create();
        entity.Moods = [new TrackMoodEntity(string.Empty)];

        // Act
        Result<Track> result = entity.ToDomainEntity();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Metadata.MoodNameCannotBeEmpty, result.FirstError);
    }

    [Fact]
    public void ToDomainEntity_WhenIsrcIsInvalid_ShouldReturnError()
    {
        // Arrange
        TrackEntity entity = _trackEntityFixture.Create();
        entity.Isrcs = [new TrackIsrcEntity("not-an-isrc")];

        // Act
        Result<Track> result = entity.ToDomainEntity();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.IsrcInvalidFormat, result.FirstError);
    }

    [Fact]
    public void ToDomainEntity_WhenRatingIsInvalid_ShouldReturnError()
    {
        // Arrange
        TrackEntity entity = _trackEntityFixture.Create();
        entity.Ratings = [_audioRatingEntityFixture.Create(value: 6, maxValue: 5, includeSource: false, includeVoteCount: false)];

        // Act
        Result<Track> result = entity.ToDomainEntity();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Metadata.RatingValueCannotBeGreaterThanMaxValue, result.FirstError);
    }

    [Fact]
    public void ToResponse_WhenMappingTrackEntity_ShouldMapAllPropertiesCorrectly()
    {
        // Arrange
        TrackEntity entity = _trackEntityFixture.Create();

        // Act
        TrackResponse result = entity.ToResponse();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(entity.Id, result.Id);
        Assert.Equal(entity.AlbumId, result.AlbumId);
        Assert.Equal(entity.LibraryId, result.LibraryId);
        Assert.Equal(entity.Path, result.Path);
        Assert.Equal(entity.Title, result.Metadata.Title);
        Assert.Equal(entity.OriginalTitle, result.Metadata.OriginalTitle);
        Assert.Equal(entity.Description, result.Metadata.Description);
        Assert.Equal(entity.OriginalReleaseDate, result.Metadata.ReleaseInfo!.OriginalReleaseDate);
        Assert.Equal(entity.OriginalReleaseYear, result.Metadata.ReleaseInfo.OriginalReleaseYear);
        Assert.Equal(entity.ReReleaseDate, result.Metadata.ReleaseInfo.ReReleaseDate);
        Assert.Equal(entity.ReReleaseYear, result.Metadata.ReleaseInfo.ReReleaseYear);
        Assert.Equal(entity.ReleaseCountry, result.Metadata.ReleaseInfo.ReleaseCountry);
        Assert.Equal(entity.ReleaseVersion, result.Metadata.ReleaseInfo.ReleaseVersion);
        Assert.NotNull(result.Metadata.Language);
        Assert.Equal(entity.LanguageCode, result.Metadata.Language!.LanguageCode);
        Assert.Equal(entity.LanguageName, result.Metadata.Language.LanguageName);
        Assert.Equal(entity.LanguageNativeName, result.Metadata.Language.NativeName);
        Assert.NotNull(result.Metadata.OriginalLanguage);
        Assert.Equal(entity.OriginalLanguageCode, result.Metadata.OriginalLanguage!.LanguageCode);
        Assert.Equal(entity.OriginalLanguageName, result.Metadata.OriginalLanguage.LanguageName);
        Assert.Equal(entity.OriginalLanguageNativeName, result.Metadata.OriginalLanguage.NativeName);
        Assert.Equal(entity.DurationInSeconds, result.Metadata.DurationInSeconds);
        Assert.Equal(entity.SampleRate, result.Metadata.SampleRate);
        Assert.Equal(entity.Channels, result.Metadata.Channels);
        Assert.Equal(entity.BitDepth, result.Metadata.BitDepth);
        Assert.Equal(entity.AudioCodec, result.Metadata.AudioCodec);
        Assert.Equal(entity.Bitrate, result.Metadata.Bitrate);
        Assert.Equal(entity.Tags.Select(tag => tag.Name).OrderBy(name => name), result.Metadata.Tags!.Select(tag => tag.Name).OrderBy(name => name));
        Assert.Equal(entity.Genres.Select(genre => genre.Name).OrderBy(name => name), result.Metadata.Genres!.Select(genre => genre.Name).OrderBy(name => name));
        Assert.Equal(entity.TrackNumber, result.TrackNumber);
        Assert.Equal(entity.DiscNumber, result.DiscNumber);
        Assert.Equal(entity.Script, result.Script);
        Assert.Equal(entity.Key, result.Key);
        Assert.Equal(entity.Bpm, result.Bpm);
        Assert.Equal(entity.Work, result.Work);
        Assert.Equal(entity.MusicBrainzRecordingId, result.MusicBrainzRecordingId);
        Assert.Equal(entity.MusicBrainzTrackId, result.MusicBrainzTrackId);
        Assert.Equal(entity.MusicBrainzWorkId, result.MusicBrainzWorkId);
        Assert.Equal(entity.CreatedOnUtc, result.CreatedOnUtc);
        Assert.Equal(entity.UpdatedOnUtc, result.UpdatedOnUtc);
        Assert.Equal(entity.Moods.Select(mood => mood.Name), result.Moods!.Select(mood => mood.Name));
        Assert.Equal(entity.Isrcs.Select(isrc => isrc.Value), result.Isrcs!.Select(isrc => isrc.Value));
        Assert.Equal(entity.Contributors.Select(contributor => new MediaContributorReferenceDto(contributor.MediaContributorId, contributor.Role)), result.Contributors);
        Assert.Equal(entity.Ratings.Select(rating => new AudioRatingDto(rating.Value, rating.MaxValue, rating.Source, rating.VoteCount)), result.Ratings);
    }

    [Fact]
    public void ToResponse_WhenLanguageSubpropertiesAreBlank_ShouldMapLanguagesToNull()
    {
        // Arrange
        TrackEntity entity = _trackEntityFixture.Create();
        entity.LanguageCode = null;
        entity.LanguageName = null;
        entity.LanguageNativeName = null;
        entity.OriginalLanguageCode = null;
        entity.OriginalLanguageName = null;
        entity.OriginalLanguageNativeName = null;

        // Act
        TrackResponse result = entity.ToResponse();

        // Assert
        Assert.Null(result.Metadata.Language);
        Assert.Null(result.Metadata.OriginalLanguage);
    }

    [Fact]
    public void ToResponses_WhenMappingMultipleTrackEntities_ShouldMapAllCorrectly()
    {
        // Arrange
        List<TrackEntity> entities = _trackEntityFixture.CreateMany(2);

        // Act
        IReadOnlyList<TrackResponse> results = entities.ToResponses();

        // Assert
        Assert.NotNull(results);
        Assert.Equal(entities.Count, results.Count);
        for (int i = 0; i < entities.Count; i++)
        {
            Assert.Equal(entities[i].Id, results[i].Id);
            Assert.Equal(entities[i].Title, results[i].Metadata.Title);
        }
    }
}
