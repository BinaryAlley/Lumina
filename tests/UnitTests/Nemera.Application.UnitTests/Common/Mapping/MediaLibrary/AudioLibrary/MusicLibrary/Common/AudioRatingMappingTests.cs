#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Common;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.Common.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.AudioLibraryBoundedContext.Common.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Common;

/// <summary>
/// Contains unit tests for the <see cref="AudioRatingMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class AudioRatingMappingTests
{
    private readonly AudioRatingFixture _audioRatingFixture = new();

    [Fact]
    public void ToRepositoryEntity_WhenMappingCompleteAudioRating_ShouldMapAllPropertiesCorrectly()
    {
        // Arrange
        AudioRating audioRating = _audioRatingFixture.Create();

        // Act
        AudioRatingEntity result = audioRating.ToRepositoryEntity();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(audioRating.Value, result.Value);
        Assert.Equal(audioRating.MaxValue, result.MaxValue);
        Assert.Equal(audioRating.Source.Value, result.Source);
        Assert.Equal(audioRating.VoteCount.Value, result.VoteCount);
    }

    [Fact]
    public void ToRepositoryEntity_WhenMappingAudioRatingWithoutSource_ShouldMapCorrectly()
    {
        // Arrange
        AudioRating audioRating = _audioRatingFixture.Create(source: Optional<AudioRatingSource>.None());

        // Act
        AudioRatingEntity result = audioRating.ToRepositoryEntity();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(audioRating.Value, result.Value);
        Assert.Equal(audioRating.MaxValue, result.MaxValue);
        Assert.Null(result.Source);
        Assert.Equal(audioRating.VoteCount.Value, result.VoteCount);
    }

    [Fact]
    public void ToRepositoryEntity_WhenMappingAudioRatingWithoutVoteCount_ShouldMapCorrectly()
    {
        // Arrange
        AudioRating audioRating = _audioRatingFixture.Create(voteCount: Optional<int>.None());

        // Act
        AudioRatingEntity result = audioRating.ToRepositoryEntity();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(audioRating.Value, result.Value);
        Assert.Equal(audioRating.MaxValue, result.MaxValue);
        Assert.Equal(audioRating.Source.Value, result.Source);
        Assert.Null(result.VoteCount);
    }

    [Fact]
    public void ToRepositoryEntities_WhenMappingMultipleAudioRatings_ShouldMapAllCorrectly()
    {
        // Arrange
        List<AudioRating> audioRatings = _audioRatingFixture.CreateMany(2);

        // Act
        List<AudioRatingEntity> results = [.. audioRatings.ToRepositoryEntities()];

        // Assert
        Assert.Equal(audioRatings.Count, results.Count);
        for (int i = 0; i < audioRatings.Count; i++)
        {
            Assert.Equal(audioRatings[i].Value, results[i].Value);
            Assert.Equal(audioRatings[i].MaxValue, results[i].MaxValue);
            Assert.Equal(audioRatings[i].Source.Value, results[i].Source);
            Assert.Equal(audioRatings[i].VoteCount.Value, results[i].VoteCount);
        }
    }
}
