#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Common.ValueObjects.Metadata;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.Common.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.Entities;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Common.ValueObjects.Metadata;
using Lumina.Domain.Fixtures.Core.BoundedContexts.AudioLibraryBoundedContext.Common.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.Entities;
using Lumina.Domain.Fixtures.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Domain.UnitTests.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.Entities;

/// <summary>
/// Contains unit tests for the <see cref="Track"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class TrackTests
{
    private readonly TrackFixture _trackFixture = new();
    private readonly MoodFixture _moodFixture = new();
    private readonly IsrcFixture _isrcFixture = new();
    private readonly MusicMediaContributorFixture _musicMediaContributorFixture = new();
    private readonly AudioRatingFixture _audioRatingFixture = new();
    private readonly AudioMetadataFixture _audioMetadataFixture = new();
    private readonly MusicBrainzIdFixture _musicBrainzIdFixture = new();

    [Fact]
    public void Create_WhenCalledWithValidData_ShouldCreateTrackWithAllPropertiesPopulated()
    {
        // Arrange
        DateTime beforeCreate = DateTime.UtcNow;

        // Act
        Track track = _trackFixture.Create();

        // Assert
        Assert.NotNull(track);
        Assert.NotNull(track.Id);
        Assert.NotEqual(Guid.Empty, track.Id.Value);
        Assert.False(string.IsNullOrWhiteSpace(track.Path));
        Assert.NotNull(track.Metadata);
        Assert.True(track.TrackNumber > 0);
        Assert.True(track.DiscNumber.HasValue);
        Assert.NotEmpty(track.Moods);
        Assert.True(track.Script.HasValue);
        Assert.True(track.Key.HasValue);
        Assert.True(track.Bpm.HasValue);
        Assert.NotEmpty(track.Isrcs);
        Assert.True(track.Work.HasValue);
        Assert.True(track.MusicBrainzRecordingId.HasValue);
        Assert.True(track.MusicBrainzTrackId.HasValue);
        Assert.True(track.MusicBrainzWorkId.HasValue);
        Assert.NotEmpty(track.Contributors);
        Assert.NotEmpty(track.Ratings);
        Assert.InRange(track.CreatedOnUtc, beforeCreate, DateTime.UtcNow);
        Assert.False(track.UpdatedOnUtc.HasValue);
    }

    [Fact]
    public void Create_WhenCalledWithPreExistingIdAndTimestamps_ShouldPreserveIdentityAndTimestamps()
    {
        // Arrange
        Track sourceTrack = _trackFixture.Create();
        DateTime createdOnUtc = DateTime.UtcNow.AddDays(-1);
        DateTime updatedOnUtc = DateTime.UtcNow;

        // Act
        Result<Track> result = Track.Create(
            sourceTrack.Id,
            sourceTrack.Path,
            sourceTrack.Metadata,
            sourceTrack.TrackNumber,
            sourceTrack.DiscNumber,
            [.. sourceTrack.Moods],
            sourceTrack.Script,
            sourceTrack.Key,
            sourceTrack.Bpm,
            [.. sourceTrack.Isrcs],
            sourceTrack.Work,
            sourceTrack.MusicBrainzRecordingId,
            sourceTrack.MusicBrainzTrackId,
            sourceTrack.MusicBrainzWorkId,
            [.. sourceTrack.Contributors],
            [.. sourceTrack.Ratings],
            createdOnUtc,
            Optional<DateTime>.Some(updatedOnUtc));

        // Assert
        Assert.False(result.IsFailure);
        Track track = result.Value;
        Assert.Equal(sourceTrack.Id, track.Id);
        Assert.Equal(sourceTrack.Path, track.Path);
        Assert.Equal(sourceTrack.Metadata, track.Metadata);
        Assert.Equal(sourceTrack.TrackNumber, track.TrackNumber);
        Assert.Equal(sourceTrack.DiscNumber, track.DiscNumber);
        Assert.Equal(sourceTrack.Moods, track.Moods);
        Assert.Equal(sourceTrack.Isrcs, track.Isrcs);
        Assert.Equal(sourceTrack.Contributors, track.Contributors);
        Assert.Equal(sourceTrack.Ratings, track.Ratings);
        Assert.Equal(createdOnUtc, track.CreatedOnUtc);
        Assert.Equal(Optional<DateTime>.Some(updatedOnUtc), track.UpdatedOnUtc);
    }

    [Fact]
    public void UpdateMoods_WhenCalledTwice_ShouldReplaceTheMoodsInsteadOfAppending()
    {
        // Arrange
        Track track = _trackFixture.Create(moods: []);
        Mood firstMood = _moodFixture.Create(name: "calm");
        Mood secondMood = _moodFixture.Create(name: "energetic");
        track.UpdateMoods([firstMood, _moodFixture.Create(name: "melancholic")]);

        // Act
        track.UpdateMoods([secondMood]);

        // Assert
        // the second call replaces the first set, it is not appended to it
        Mood storedMood = Assert.Single(track.Moods);
        Assert.Equal(secondMood, storedMood);
        Assert.DoesNotContain(firstMood, track.Moods);
    }

    [Fact]
    public void UpdateIsrcs_WhenCalledTwice_ShouldReplaceTheIsrcsInsteadOfAppending()
    {
        // Arrange
        Track track = _trackFixture.Create(isrcs: []);
        Isrc firstIsrc = _isrcFixture.Create(value: "USRC17607839");
        Isrc secondIsrc = _isrcFixture.Create(value: "GBAYE0000001");
        track.UpdateIsrcs([firstIsrc, _isrcFixture.Create(value: "FRUM71400001")]);

        // Act
        track.UpdateIsrcs([secondIsrc]);

        // Assert
        // the second call replaces the first set, it is not appended to it
        Isrc storedIsrc = Assert.Single(track.Isrcs);
        Assert.Equal(secondIsrc, storedIsrc);
        Assert.DoesNotContain(firstIsrc, track.Isrcs);
    }

    [Fact]
    public void UpdateContributors_WhenCalledTwice_ShouldReplaceTheContributorsInsteadOfAppending()
    {
        // Arrange
        Track track = _trackFixture.Create(contributors: []);
        MusicMediaContributor firstContributor = _musicMediaContributorFixture.Create();
        MusicMediaContributor secondContributor = _musicMediaContributorFixture.Create();
        track.UpdateContributors([firstContributor]);

        // Act
        track.UpdateContributors([secondContributor]);

        // Assert
        // the second call replaces the first set, it is not appended to it
        MusicMediaContributor storedContributor = Assert.Single(track.Contributors);
        Assert.Equal(secondContributor, storedContributor);
        Assert.DoesNotContain(firstContributor, track.Contributors);
    }

    [Fact]
    public void UpdateRatings_WhenCalledTwice_ShouldReplaceTheRatingsInsteadOfAppending()
    {
        // Arrange
        Track track = _trackFixture.Create(ratings: []);
        AudioRating firstRating = _audioRatingFixture.Create(value: 1m, maxValue: 5m);
        AudioRating secondRating = _audioRatingFixture.Create(value: 4m, maxValue: 5m);
        track.UpdateRatings([firstRating]);

        // Act
        track.UpdateRatings([secondRating]);

        // Assert
        // the second call replaces the first set, it is not appended to it
        AudioRating storedRating = Assert.Single(track.Ratings);
        Assert.Equal(secondRating, storedRating);
    }

    [Fact]
    public void UpdateDetails_WhenCalled_ShouldUpdateAllDetailPropertiesAndSetUpdatedOnUtc()
    {
        // Arrange
        Track track = _trackFixture.Create();
        List<Mood> originalMoods = [.. track.Moods];
        string path = "C:\\Music\\queen\\bohemian-rhapsody.flac";
        AudioMetadata metadata = _audioMetadataFixture.Create();
        Optional<int> discNumber = Optional<int>.Some(2);
        Optional<string> script = Optional<string>.Some("Latin");
        Optional<MusicKey> key = Optional<MusicKey>.Some(MusicKey.DMajor);
        Optional<int> bpm = Optional<int>.Some(128);
        Optional<string> work = Optional<string>.Some("Symphony No. 5");
        Optional<MusicBrainzId> recordingId = Optional<MusicBrainzId>.Some(_musicBrainzIdFixture.Create());
        Optional<MusicBrainzId> trackId = Optional<MusicBrainzId>.Some(_musicBrainzIdFixture.Create());
        Optional<MusicBrainzId> workId = Optional<MusicBrainzId>.Some(_musicBrainzIdFixture.Create());
        DateTime beforeUpdate = DateTime.UtcNow;

        // Act
        Result<Updated> result = track.UpdateDetails(
            path,
            metadata,
            7,
            discNumber,
            script,
            key,
            bpm,
            work,
            recordingId,
            trackId,
            workId);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(path, track.Path);
        Assert.Equal(metadata, track.Metadata);
        Assert.Equal(7, track.TrackNumber);
        Assert.Equal(discNumber, track.DiscNumber);
        Assert.Equal(script, track.Script);
        Assert.Equal(key, track.Key);
        Assert.Equal(bpm, track.Bpm);
        Assert.Equal(work, track.Work);
        Assert.Equal(recordingId, track.MusicBrainzRecordingId);
        Assert.Equal(trackId, track.MusicBrainzTrackId);
        Assert.Equal(workId, track.MusicBrainzWorkId);
        Assert.True(track.UpdatedOnUtc.HasValue);
        Assert.True(track.UpdatedOnUtc.Value >= beforeUpdate);
        // the collections are not part of the details update
        Assert.Equal(originalMoods, track.Moods);
    }

    [Fact]
    public void Create_WhenOptionalValuesAndCollectionsAreEmpty_ShouldCreateTrackWithoutThem()
    {
        // Act
        Track track = _trackFixture.Create(
            discNumber: Optional<int>.None(),
            moods: [],
            script: Optional<string>.None(),
            key: Optional<MusicKey>.None(),
            bpm: Optional<int>.None(),
            isrcs: [],
            work: Optional<string>.None(),
            musicBrainzRecordingId: Optional<MusicBrainzId>.None(),
            musicBrainzTrackId: Optional<MusicBrainzId>.None(),
            musicBrainzWorkId: Optional<MusicBrainzId>.None(),
            contributors: [],
            ratings: []);

        // Assert
        Assert.False(track.DiscNumber.HasValue);
        Assert.False(track.Script.HasValue);
        Assert.False(track.Key.HasValue);
        Assert.False(track.Bpm.HasValue);
        Assert.False(track.Work.HasValue);
        Assert.False(track.MusicBrainzRecordingId.HasValue);
        Assert.False(track.MusicBrainzTrackId.HasValue);
        Assert.False(track.MusicBrainzWorkId.HasValue);
        Assert.Empty(track.Moods);
        Assert.Empty(track.Isrcs);
        Assert.Empty(track.Contributors);
        Assert.Empty(track.Ratings);
    }

    [Fact]
    public void CreateWithId_WhenCalledWithPreExistingIdAndTimestamps_ShouldPreserveAllOptionalProperties()
    {
        // Arrange
        Track sourceTrack = _trackFixture.Create();
        DateTime createdOnUtc = DateTime.UtcNow.AddDays(-1);
        DateTime updatedOnUtc = DateTime.UtcNow;

        // Act
        Result<Track> result = Track.Create(
            sourceTrack.Id,
            sourceTrack.Path,
            sourceTrack.Metadata,
            sourceTrack.TrackNumber,
            sourceTrack.DiscNumber,
            [.. sourceTrack.Moods],
            sourceTrack.Script,
            sourceTrack.Key,
            sourceTrack.Bpm,
            [.. sourceTrack.Isrcs],
            sourceTrack.Work,
            sourceTrack.MusicBrainzRecordingId,
            sourceTrack.MusicBrainzTrackId,
            sourceTrack.MusicBrainzWorkId,
            [.. sourceTrack.Contributors],
            [.. sourceTrack.Ratings],
            createdOnUtc,
            Optional<DateTime>.Some(updatedOnUtc));

        // Assert
        Assert.False(result.IsFailure);
        Track track = result.Value;
        Assert.Equal(sourceTrack.Script, track.Script);
        Assert.Equal(sourceTrack.Key, track.Key);
        Assert.Equal(sourceTrack.Bpm, track.Bpm);
        Assert.Equal(sourceTrack.Work, track.Work);
        Assert.Equal(sourceTrack.MusicBrainzRecordingId, track.MusicBrainzRecordingId);
        Assert.Equal(sourceTrack.MusicBrainzTrackId, track.MusicBrainzTrackId);
        Assert.Equal(sourceTrack.MusicBrainzWorkId, track.MusicBrainzWorkId);
        // The pre-existing identity and the provided timestamps must survive the rehydration.
        Assert.Equal(sourceTrack.Id, track.Id);
        Assert.Equal(createdOnUtc, track.CreatedOnUtc);
        Assert.True(track.UpdatedOnUtc.HasValue);
        Assert.Equal(updatedOnUtc, track.UpdatedOnUtc.Value);
    }

    [Fact]
    public void UpdateMoods_WhenCalledWithEmptyCollection_ShouldClearTheMoods()
    {
        // Arrange
        Track track = _trackFixture.Create(moods: [_moodFixture.Create(name: "calm")]);

        // Act
        track.UpdateMoods([]);

        // Assert
        Assert.Empty(track.Moods);
    }

    [Fact]
    public void UpdateIsrcs_WhenCalledWithEmptyCollection_ShouldClearTheIsrcs()
    {
        // Arrange
        Track track = _trackFixture.Create(isrcs: [_isrcFixture.Create(value: "USRC17607839")]);

        // Act
        track.UpdateIsrcs([]);

        // Assert
        Assert.Empty(track.Isrcs);
    }

    [Fact]
    public void UpdateContributors_WhenCalledWithEmptyCollection_ShouldClearTheContributors()
    {
        // Arrange
        Track track = _trackFixture.Create(contributors: [.. _musicMediaContributorFixture.CreateMany()]);

        // Act
        track.UpdateContributors([]);

        // Assert
        Assert.Empty(track.Contributors);
    }

    [Fact]
    public void UpdateRatings_WhenCalledWithEmptyCollection_ShouldClearTheRatings()
    {
        // Arrange
        Track track = _trackFixture.Create(ratings: [_audioRatingFixture.Create(value: 4m, maxValue: 5m)]);

        // Act
        track.UpdateRatings([]);

        // Assert
        Assert.Empty(track.Ratings);
    }

    [Fact]
    public void UpdateDetails_WhenOptionalValuesAreAbsent_ShouldClearTheOptionalProperties()
    {
        // Arrange
        Track track = _trackFixture.Create();

        // Act
        Result<Updated> result = track.UpdateDetails(
            "C:\\Music\\queen\\bohemian-rhapsody.flac",
            _audioMetadataFixture.Create(),
            11,
            Optional<int>.None(),
            Optional<string>.None(),
            Optional<MusicKey>.None(),
            Optional<int>.None(),
            Optional<string>.None(),
            Optional<MusicBrainzId>.None(),
            Optional<MusicBrainzId>.None(),
            Optional<MusicBrainzId>.None());

        // Assert
        Assert.False(result.IsFailure);
        Assert.False(track.DiscNumber.HasValue);
        Assert.False(track.Script.HasValue);
        Assert.False(track.Key.HasValue);
        Assert.False(track.Bpm.HasValue);
        Assert.False(track.Work.HasValue);
        Assert.False(track.MusicBrainzRecordingId.HasValue);
        Assert.False(track.MusicBrainzTrackId.HasValue);
        Assert.False(track.MusicBrainzWorkId.HasValue);
    }
}
