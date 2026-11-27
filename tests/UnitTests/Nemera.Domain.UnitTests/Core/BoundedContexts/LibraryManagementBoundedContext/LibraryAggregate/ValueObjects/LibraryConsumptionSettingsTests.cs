#region ========================================================================= USING =====================================================================================
using Nemera.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using Nemera.Domain.Fixtures.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using Nemera.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Nemera.Domain.UnitTests.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;

/// <summary>
/// Contains unit tests for the <see cref="LibraryConsumptionSettings"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class LibraryConsumptionSettingsTests
{
    private readonly LibraryConsumptionSettingsFixture _libraryConsumptionSettingsFixture = new();

    [Fact]
    public void Create_WhenProgressTrackingIsEnabledAndThresholdIsSupplied_ShouldRetainThreshold()
    {
        // Act
        LibraryConsumptionSettings settings = _libraryConsumptionSettingsFixture.Create(
            shouldTrackConsumptionProgress: true,
            completionThresholdPercentage: 65);

        // Assert
        Assert.True(settings.ShouldTrackConsumptionProgress);
        Assert.True(settings.CompletionThresholdPercentage.HasValue);
        Assert.Equal(65, settings.CompletionThresholdPercentage.Value);
    }

    [Fact]
    public void Create_WhenProgressTrackingIsEnabledAndThresholdIsAbsent_ShouldHaveNoThreshold()
    {
        // Act
        LibraryConsumptionSettings settings = _libraryConsumptionSettingsFixture.Create(
            shouldTrackConsumptionProgress: true,
            includeCompletionThreshold: false);

        // Assert
        Assert.True(settings.ShouldTrackConsumptionProgress);
        Assert.False(settings.CompletionThresholdPercentage.HasValue);
    }

    [Fact]
    public void Create_WhenProgressTrackingIsDisabledAndThresholdIsSupplied_ShouldNotRetainThreshold()
    {
        // Act
        LibraryConsumptionSettings settings = _libraryConsumptionSettingsFixture.Create(
            shouldTrackConsumptionProgress: false,
            completionThresholdPercentage: 65);

        // Assert
        Assert.False(settings.ShouldTrackConsumptionProgress);
        Assert.False(settings.CompletionThresholdPercentage.HasValue);
    }

    [Fact]
    public void Create_WhenProgressTrackingIsDisabledAndThresholdIsAbsent_ShouldHaveNoThreshold()
    {
        // Act
        LibraryConsumptionSettings settings = _libraryConsumptionSettingsFixture.Create(
            shouldTrackConsumptionProgress: false,
            includeCompletionThreshold: false);

        // Assert
        Assert.False(settings.ShouldTrackConsumptionProgress);
        Assert.False(settings.CompletionThresholdPercentage.HasValue);
    }

    [Fact]
    public void Equals_WhenSettingsHaveSameValues_ShouldReturnTrue()
    {
        // Arrange
        LibraryConsumptionSettings firstSettings = _libraryConsumptionSettingsFixture.Create(
            shouldTrackConsumptionProgress: true,
            completionThresholdPercentage: 65);
        LibraryConsumptionSettings secondSettings = _libraryConsumptionSettingsFixture.Create(
            shouldTrackConsumptionProgress: true,
            completionThresholdPercentage: 65);

        // Act
        bool areEqual = firstSettings.Equals(secondSettings);

        // Assert
        Assert.True(areEqual);
    }

    [Fact]
    public void Equals_WhenProgressTrackingDiffers_ShouldReturnFalse()
    {
        // Arrange
        LibraryConsumptionSettings trackingSettings = _libraryConsumptionSettingsFixture.Create(
            shouldTrackConsumptionProgress: true,
            completionThresholdPercentage: 65);
        LibraryConsumptionSettings notTrackingSettings = _libraryConsumptionSettingsFixture.Create(
            shouldTrackConsumptionProgress: false,
            completionThresholdPercentage: 65);

        // Act
        bool areEqual = trackingSettings.Equals(notTrackingSettings);

        // Assert
        Assert.False(areEqual);
    }

    [Fact]
    public void Equals_WhenThresholdsDiffer_ShouldReturnFalse()
    {
        // Arrange
        LibraryConsumptionSettings firstSettings = _libraryConsumptionSettingsFixture.Create(
            shouldTrackConsumptionProgress: true,
            completionThresholdPercentage: 40);
        LibraryConsumptionSettings secondSettings = _libraryConsumptionSettingsFixture.Create(
            shouldTrackConsumptionProgress: true,
            completionThresholdPercentage: 60);

        // Act
        bool areEqual = firstSettings.Equals(secondSettings);

        // Assert
        Assert.False(areEqual);
    }

    [Fact]
    public void Equals_WhenProgressTrackingIsDisabled_ShouldIgnoreAnySuppliedThreshold()
    {
        // Arrange
        LibraryConsumptionSettings settingsWithThreshold = _libraryConsumptionSettingsFixture.Create(
            shouldTrackConsumptionProgress: false,
            completionThresholdPercentage: 40);
        LibraryConsumptionSettings settingsWithoutThreshold = _libraryConsumptionSettingsFixture.Create(
            shouldTrackConsumptionProgress: false,
            includeCompletionThreshold: false);

        // Act
        bool areEqual = settingsWithThreshold.Equals(settingsWithoutThreshold);

        // Assert
        Assert.False(settingsWithThreshold.CompletionThresholdPercentage.HasValue);
        Assert.False(settingsWithoutThreshold.CompletionThresholdPercentage.HasValue);
        Assert.True(areEqual);
    }

    [Fact]
    public void Equals_WhenComparedWithNull_ShouldReturnFalse()
    {
        // Arrange
        LibraryConsumptionSettings settings = _libraryConsumptionSettingsFixture.Create(
            shouldTrackConsumptionProgress: true,
            completionThresholdPercentage: 65);

        // Act
        bool areEqual = settings.Equals((object?)null);

        // Assert
        Assert.False(areEqual);
    }

    [Fact]
    public void GetHashCode_WhenSettingsAreEqual_ShouldReturnSameHashCode()
    {
        // Arrange
        LibraryConsumptionSettings firstSettings = _libraryConsumptionSettingsFixture.Create(
            shouldTrackConsumptionProgress: true,
            completionThresholdPercentage: 65);
        LibraryConsumptionSettings secondSettings = _libraryConsumptionSettingsFixture.Create(
            shouldTrackConsumptionProgress: true,
            completionThresholdPercentage: 65);

        // Act
        int firstHashCode = firstSettings.GetHashCode();
        int secondHashCode = secondSettings.GetHashCode();

        // Assert
        Assert.Equal(firstHashCode, secondHashCode);
    }

    [Theory]
    [InlineData(LibraryType.Book, true, 100)] // Written content.
    [InlineData(LibraryType.EBook, true, 100)] // Written content.
    [InlineData(LibraryType.ComicBook, true, 100)] // Written content.
    [InlineData(LibraryType.Magazine, true, 100)] // Written content.
    [InlineData(LibraryType.Newspaper, true, 100)] // Written content.
    [InlineData(LibraryType.Manga, true, 100)] // Written content.
    [InlineData(LibraryType.GraphicNovel, true, 100)] // Written content.
    [InlineData(LibraryType.AcademicPaper, true, 100)] // Written content.
    [InlineData(LibraryType.SheetMusic, true, 100)] // Written content.
    [InlineData(LibraryType.TvShow, true, 90)] // Video.
    [InlineData(LibraryType.Movie, true, 90)] // Video.
    [InlineData(LibraryType.Documentary, true, 90)] // Video.
    [InlineData(LibraryType.Anime, true, 90)] // Video.
    [InlineData(LibraryType.ConcertVideo, true, 90)] // Video.
    [InlineData(LibraryType.TutorialVideo, true, 90)] // Video.
    [InlineData(LibraryType.HomeVideo, true, 90)] // Video.
    [InlineData(LibraryType.YouTubeVideo, true, 90)] // Video.
    [InlineData(LibraryType.MusicVideo, true, 90)] // Video.
    [InlineData(LibraryType.LiveRecordingVideo, true, 90)] // Video.
    [InlineData(LibraryType.InterviewVideo, true, 90)] // Video.
    [InlineData(LibraryType.CoverSongVideo, true, 90)] // Video.
    [InlineData(LibraryType.Audiobook, true, 95)] // Audiobooks and podcasts.
    [InlineData(LibraryType.PodcastAudio, true, 95)] // Audiobooks and podcasts.
    [InlineData(LibraryType.PodcastVideo, true, 95)] // Audiobooks and podcasts.
    [InlineData(LibraryType.Music, false, null)] // Music audio and photos.
    [InlineData(LibraryType.Remix, false, null)] // Music audio and photos.
    [InlineData(LibraryType.SoundEffect, false, null)] // Music audio and photos.
    [InlineData(LibraryType.LiveRecordingAudio, false, null)] // Music audio and photos.
    [InlineData(LibraryType.InterviewAudio, false, null)] // Music audio and photos.
    [InlineData(LibraryType.CoverSongAudio, false, null)] // Music audio and photos.
    [InlineData(LibraryType.Photo, false, null)] // Music audio and photos.
    [InlineData(LibraryType.Playlist, false, null)] // Library types that are not media libraries.
    [InlineData(LibraryType.Collection, false, null)] // Library types that are not media libraries.
    [InlineData(LibraryType.Subtitles, false, null)] // Library types that are not media libraries.
    [InlineData(LibraryType.Lyrics, false, null)] // Library types that are not media libraries.
    [InlineData(LibraryType.Other, false, null)] // Library types that are not media libraries.
    public void CreateDefault_WhenLibraryTypeIsKnown_ShouldReturnTheTypeDefaults(
        LibraryType libraryType,
        bool expectedShouldTrackConsumptionProgress,
        int? expectedCompletionThresholdPercentage)
    {
        // Act
        LibraryConsumptionSettings settings = LibraryConsumptionSettings.CreateDefault(libraryType);

        // Assert
        Assert.Equal(expectedShouldTrackConsumptionProgress, settings.ShouldTrackConsumptionProgress);
        if (expectedCompletionThresholdPercentage is null)
            Assert.False(settings.CompletionThresholdPercentage.HasValue);
        else
        {
            Assert.True(settings.CompletionThresholdPercentage.HasValue);
            Assert.Equal(expectedCompletionThresholdPercentage.Value, settings.CompletionThresholdPercentage.Value);
        }
    }

    [Fact]
    public void CreateDefault_WhenLibraryTypeIsNotClassified_ShouldReturnTrackingOffWithoutThreshold()
    {
        // Act
        LibraryConsumptionSettings settings = LibraryConsumptionSettings.CreateDefault((LibraryType)999);

        // Assert
        Assert.False(settings.ShouldTrackConsumptionProgress);
        Assert.False(settings.CompletionThresholdPercentage.HasValue);
    }

    [Theory]
    [InlineData(MediaKind.None, false, null)] // No media kind.
    [InlineData(MediaKind.MusicTrack, false, null)] // Music tracks.
    [InlineData(MediaKind.PodcastEpisode, true, 95)] // Audiobooks and podcast episodes.
    [InlineData(MediaKind.Audiobook, true, 95)] // Audiobooks and podcast episodes.
    [InlineData(MediaKind.Movie, true, 90)] // Video.
    [InlineData(MediaKind.TvEpisode, true, 90)] // Video.
    [InlineData(MediaKind.Book, true, 100)] // Written content.
    [InlineData(MediaKind.Photo, false, null)] // Photos.
    [InlineData(MediaKind.Video, true, 90)] // Video.
    public void CreateDefault_WhenMediaKindIsKnown_ShouldReturnTheKindDefaults(
        MediaKind mediaKind,
        bool expectedShouldTrackConsumptionProgress,
        int? expectedCompletionThresholdPercentage)
    {
        // Act
        LibraryConsumptionSettings settings = LibraryConsumptionSettings.CreateDefault(mediaKind);

        // Assert
        Assert.Equal(expectedShouldTrackConsumptionProgress, settings.ShouldTrackConsumptionProgress);
        if (expectedCompletionThresholdPercentage is null)
            Assert.False(settings.CompletionThresholdPercentage.HasValue);
        else
        {
            Assert.True(settings.CompletionThresholdPercentage.HasValue);
            Assert.Equal(expectedCompletionThresholdPercentage.Value, settings.CompletionThresholdPercentage.Value);
        }
    }

    [Fact]
    public void CreateDefault_WhenMediaKindIsNotClassified_ShouldReturnTrackingOffWithoutThreshold()
    {
        // Act
        LibraryConsumptionSettings settings = LibraryConsumptionSettings.CreateDefault((MediaKind)999);

        // Assert
        Assert.False(settings.ShouldTrackConsumptionProgress);
        Assert.False(settings.CompletionThresholdPercentage.HasValue);
    }

    [Fact]
    public void CreateDefault_WhenReturnedSettingsAreTrackingOff_ShouldNotRetainAThreshold()
    {
        // Act
        LibraryConsumptionSettings settings = LibraryConsumptionSettings.CreateDefault(LibraryType.Music);

        // Assert
        Assert.False(settings.ShouldTrackConsumptionProgress);
        Assert.False(settings.CompletionThresholdPercentage.HasValue);
    }
}
