#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.Common.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.MediaConsumptionRecordAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Domain.UnitTests.Core.BoundedContexts.MediaConsumptionBoundedContext.Common.ValueObjects;

/// <summary>
/// Contains unit tests for the <see cref="MediaConsumptionRule"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MediaConsumptionRuleTests
{
    [Theory]
    [InlineData(MediaKind.MusicTrack, typeof(MusicTrackConsumptionRule))] // duration based rule of a music track
    [InlineData(MediaKind.PodcastEpisode, typeof(PodcastEpisodeConsumptionRule))] // duration based rule of a podcast episode
    [InlineData(MediaKind.Audiobook, typeof(AudiobookConsumptionRule))] // duration based rule of an audiobook
    [InlineData(MediaKind.Movie, typeof(MovieConsumptionRule))] // duration based rule of a movie
    [InlineData(MediaKind.TvEpisode, typeof(TvEpisodeConsumptionRule))] // duration based rule of a TV episode
    [InlineData(MediaKind.Book, typeof(BookConsumptionRule))] // page based rule of a book
    [InlineData(MediaKind.Photo, typeof(PhotoConsumptionRule))] // every session rule of a photo
    [InlineData(MediaKind.Video, typeof(VideoConsumptionRule))] // duration based rule of a video
    public void For_WhenMediaKindHasARule_ShouldReturnTheRuleOfThatKind(MediaKind mediaKind, Type expectedType)
    {
        // Act
        Optional<MediaConsumptionRule> result = MediaConsumptionRule.For(mediaKind);

        // Assert
        Assert.True(result.HasValue);
        Assert.IsType(expectedType, result.Value);
        Assert.Equal(mediaKind, result.Value.MediaKind);
    }

    [Theory]
    [InlineData(MediaKind.None)] // the default value, which is not a valid kind for a media item
    [InlineData((MediaKind)999)] // a value outside the declared enumeration members
    public void For_WhenMediaKindHasNoRule_ShouldReturnNoValue(MediaKind mediaKind)
    {
        // Act
        Optional<MediaConsumptionRule> result = MediaConsumptionRule.For(mediaKind);

        // Assert
        Assert.False(result.HasValue);
    }

    [Fact]
    public void For_WhenEveryDeclaredMediaKindIsNotNone_ShouldReturnTheRuleOfThatKind()
    {
        // Arrange
        MediaKind[] declaredMediaKinds = [.. Enum.GetValues<MediaKind>().Where(mediaKind => mediaKind != MediaKind.None)];

        // Act
        Optional<MediaConsumptionRule>[] results = [.. declaredMediaKinds.Select(MediaConsumptionRule.For)];

        // Assert
        Assert.All(results, result => Assert.True(result.HasValue));
        Assert.Equal(declaredMediaKinds, results.Select(result => result.Value.MediaKind));
    }

    [Theory]
    [InlineData(MediaKind.Movie)] // a duration based rule that resumes from a time offset
    [InlineData(MediaKind.Book)] // a page based rule that resumes from a location
    public void CanResume_WhenRuleHasAPositionKind_ShouldReturnTrue(MediaKind mediaKind)
    {
        // Act
        bool result = MediaConsumptionRule.For(mediaKind).Value.CanResume;

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void CanResume_WhenRuleHasNoPositionKind_ShouldReturnFalse()
    {
        // Act
        bool result = MediaConsumptionRule.For(MediaKind.Photo).Value.CanResume;

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Equals_WhenRulesHaveTheSameMediaKind_ShouldReturnTrue()
    {
        // Arrange
        MediaConsumptionRule firstRule = MediaConsumptionRule.For(MediaKind.MusicTrack).Value;
        MediaConsumptionRule secondRule = MediaConsumptionRule.For(MediaKind.MusicTrack).Value;

        // Act
        bool result = firstRule.Equals(secondRule);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Equals_WhenRulesHaveDifferentMediaKinds_ShouldReturnFalse()
    {
        // Arrange
        MediaConsumptionRule firstRule = MediaConsumptionRule.For(MediaKind.MusicTrack).Value;
        MediaConsumptionRule secondRule = MediaConsumptionRule.For(MediaKind.Movie).Value;

        // Act
        bool result = firstRule.Equals(secondRule);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GetHashCode_WhenRulesHaveTheSameMediaKind_ShouldBeEqual()
    {
        // Arrange
        MediaConsumptionRule firstRule = MediaConsumptionRule.For(MediaKind.MusicTrack).Value;
        MediaConsumptionRule secondRule = MediaConsumptionRule.For(MediaKind.MusicTrack).Value;

        // Act
        int firstHashCode = firstRule.GetHashCode();
        int secondHashCode = secondRule.GetHashCode();

        // Assert
        Assert.Equal(firstHashCode, secondHashCode);
    }

    [Fact]
    public void GetEqualityComponents_WhenCalled_ShouldContainOnlyTheMediaKind()
    {
        // Arrange
        MediaConsumptionRule rule = MediaConsumptionRule.For(MediaKind.Movie).Value;

        // Act
        object?[] components = [.. rule.GetEqualityComponents()];

        // Assert
        object? component = Assert.Single(components);
        Assert.Equal(rule.MediaKind, Assert.IsType<MediaKind>(component));
    }
}
