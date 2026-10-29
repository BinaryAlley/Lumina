#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.UnitTests.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;

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
    [InlineData(LibraryType.Book, true, 100)] // written content
    [InlineData(LibraryType.EBook, true, 100)] // written content
    [InlineData(LibraryType.ComicBook, true, 100)] // written content
    [InlineData(LibraryType.Magazine, true, 100)] // written content
    [InlineData(LibraryType.Newspaper, true, 100)] // written content
    [InlineData(LibraryType.Manga, true, 100)] // written content
    [InlineData(LibraryType.GraphicNovel, true, 100)] // written content
    [InlineData(LibraryType.AcademicPaper, true, 100)] // written content
    [InlineData(LibraryType.SheetMusic, true, 100)] // written content
    [InlineData(LibraryType.TvShow, true, 90)] // video
    [InlineData(LibraryType.Movie, true, 90)] // video
    [InlineData(LibraryType.Documentary, true, 90)] // video
    [InlineData(LibraryType.Anime, true, 90)] // video
    [InlineData(LibraryType.ConcertVideo, true, 90)] // video
    [InlineData(LibraryType.TutorialVideo, true, 90)] // video
    [InlineData(LibraryType.HomeVideo, true, 90)] // video
    [InlineData(LibraryType.YouTubeVideo, true, 90)] // video
    [InlineData(LibraryType.MusicVideo, true, 90)] // video
    [InlineData(LibraryType.LiveRecordingVideo, true, 90)] // video
    [InlineData(LibraryType.InterviewVideo, true, 90)] // video
    [InlineData(LibraryType.CoverSongVideo, true, 90)] // video
    [InlineData(LibraryType.Audiobook, true, 95)] // audiobooks and podcasts
    [InlineData(LibraryType.PodcastAudio, true, 95)] // audiobooks and podcasts
    [InlineData(LibraryType.PodcastVideo, true, 95)] // audiobooks and podcasts
    [InlineData(LibraryType.Music, false, null)] // music audio and photos
    [InlineData(LibraryType.Remix, false, null)] // music audio and photos
    [InlineData(LibraryType.SoundEffect, false, null)] // music audio and photos
    [InlineData(LibraryType.LiveRecordingAudio, false, null)] // music audio and photos
    [InlineData(LibraryType.InterviewAudio, false, null)] // music audio and photos
    [InlineData(LibraryType.CoverSongAudio, false, null)] // music audio and photos
    [InlineData(LibraryType.Photo, false, null)] // music audio and photos
    [InlineData(LibraryType.Playlist, false, null)] // library types that are not media libraries
    [InlineData(LibraryType.Collection, false, null)] // library types that are not media libraries
    [InlineData(LibraryType.Subtitles, false, null)] // library types that are not media libraries
    [InlineData(LibraryType.Lyrics, false, null)] // library types that are not media libraries
    [InlineData(LibraryType.Other, false, null)] // library types that are not media libraries
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
    [InlineData(MediaKind.None, false, null)] // no media kind
    [InlineData(MediaKind.MusicTrack, false, null)] // music tracks
    [InlineData(MediaKind.PodcastEpisode, true, 95)] // audiobooks and podcast episodes
    [InlineData(MediaKind.Audiobook, true, 95)] // audiobooks and podcast episodes
    [InlineData(MediaKind.Movie, true, 90)] // video
    [InlineData(MediaKind.TvEpisode, true, 90)] // video
    [InlineData(MediaKind.Book, true, 100)] // written content
    [InlineData(MediaKind.Photo, false, null)] // photos
    [InlineData(MediaKind.Video, true, 90)] // video
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
