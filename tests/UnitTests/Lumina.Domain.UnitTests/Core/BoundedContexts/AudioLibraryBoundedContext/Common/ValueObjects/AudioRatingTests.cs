#region ========================================================================= USING =====================================================================================
using DomainErrors = Lumina.Domain.Common.Errors.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.Common.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.AudioLibraryBoundedContext.Common.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.UnitTests.Core.BoundedContexts.AudioLibraryBoundedContext.Common.ValueObjects;

/// <summary>
/// Contains unit tests for the <see cref="AudioRating"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class AudioRatingTests
{
    private readonly AudioRatingFixture _audioRatingFixture = new();

    [Fact]
    public void Create_WhenCalledWithValidValues_ShouldCreateRatingWithAllPropertiesSet()
    {
        // Act
        Result<AudioRating> result = AudioRating.Create(4m, 5m, Optional<AudioRatingSource>.Some(AudioRatingSource.MusicBrainz), Optional<int>.Some(10));

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(4m, result.Value.Value);
        Assert.Equal(5m, result.Value.MaxValue);
        Assert.Equal(Optional<AudioRatingSource>.Some(AudioRatingSource.MusicBrainz), result.Value.Source);
        Assert.Equal(Optional<int>.Some(10), result.Value.VoteCount);
    }

    [Fact]
    public void Create_WhenOptionalValuesAreAbsent_ShouldCreateRatingWithoutThem()
    {
        // Act
        Result<AudioRating> result = AudioRating.Create(3m, 5m, Optional<AudioRatingSource>.None(), Optional<int>.None());

        // Assert
        Assert.False(result.IsFailure);
        Assert.False(result.Value.Source.HasValue);
        Assert.False(result.Value.VoteCount.HasValue);
    }

    [Theory]
    [InlineData(-1)] // negative value
    [InlineData(-0.5)] // negative fractional value
    public void Create_WhenValueIsNegative_ShouldReturnError(double value)
    {
        // Act
        Result<AudioRating> result = AudioRating.Create((decimal)value, maxValue: 5, Optional<AudioRatingSource>.None(), Optional<int>.None());

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Metadata.RatingValueMustBePositive, result.FirstError);
    }

    [Fact]
    public void Create_WhenMaxValueIsNegative_ShouldReturnError()
    {
        // Act
        Result<AudioRating> result = AudioRating.Create(value: 4m, maxValue: -5, Optional<AudioRatingSource>.None(), Optional<int>.None());

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Metadata.RatingValueMustBePositive, result.FirstError);
    }

    [Fact]
    public void Create_WhenValueIsGreaterThanMaxValue_ShouldReturnError()
    {
        // Act
        Result<AudioRating> result = AudioRating.Create(6m, 5m, Optional<AudioRatingSource>.None(), Optional<int>.None());

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Metadata.RatingValueCannotBeGreaterThanMaxValue, result.FirstError);
    }

    [Fact]
    public void ToString_WhenSourceAndVoteCountArePresent_ShouldIncludeThem()
    {
        // Arrange
        AudioRating rating = _audioRatingFixture.Create(value: 4m, maxValue: 5m, source: Optional<AudioRatingSource>.Some(AudioRatingSource.LastFm), voteCount: Optional<int>.Some(10));

        // Act
        string result = rating.ToString();

        // Assert
        Assert.Equal("4/5 (LastFm) [10 votes]", result);
    }

    [Fact]
    public void ToString_WhenSourceAndVoteCountAreAbsent_ShouldReturnValueAndMaxValue()
    {
        // Arrange
        AudioRating rating = _audioRatingFixture.Create(value: 3m, maxValue: 5m, source: Optional<AudioRatingSource>.None(), voteCount: Optional<int>.None());

        // Act
        string result = rating.ToString();

        // Assert
        Assert.Equal("3/5", result);
    }

    [Fact]
    public void Equals_WithSameValues_ShouldReturnTrue()
    {
        // Arrange
        AudioRating firstRating = _audioRatingFixture.Create(value: 4m, maxValue: 5m, source: Optional<AudioRatingSource>.Some(AudioRatingSource.User), voteCount: Optional<int>.Some(10));
        AudioRating secondRating = _audioRatingFixture.Create(value: 4m, maxValue: 5m, source: Optional<AudioRatingSource>.Some(AudioRatingSource.User), voteCount: Optional<int>.Some(10));

        // Act
        bool result = firstRating.Equals(secondRating);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Equals_WithDifferentValue_ShouldReturnFalse()
    {
        // Arrange
        AudioRating firstRating = _audioRatingFixture.Create(value: 1m, maxValue: 5m);
        AudioRating secondRating = _audioRatingFixture.Create(value: 4m, maxValue: 5m);

        // Act
        bool result = firstRating.Equals(secondRating);

        // Assert
        Assert.False(result);
    }
}
