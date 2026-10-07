#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Common;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Contracts.DTO.MediaLibrary.AudioLibrary;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.Common.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Common;

/// <summary>
/// Contains unit tests for the <see cref="AudioRatingEntityMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class AudioRatingEntityMappingTests
{
    private readonly AudioRatingEntityFixture _audioRatingEntityFixture = new();

    [Fact]
    public void ToResponse_WhenMappingValidAudioRatingEntity_ShouldMapCorrectly()
    {
        // Arrange
        AudioRatingEntity entity = _audioRatingEntityFixture.Create();

        // Act
        AudioRatingDto result = entity.ToResponse();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(entity.Value, result.Value);
        Assert.Equal(entity.MaxValue, result.MaxValue);
        Assert.Equal(entity.Source, result.Source);
        Assert.Equal(entity.VoteCount, result.VoteCount);
    }

    [Fact]
    public void ToResponse_WhenMappingAudioRatingEntityWithNulls_ShouldMapToDefaults()
    {
        // Arrange
        AudioRatingEntity entity = _audioRatingEntityFixture.Create(includeValue: false, includeMaxValue: false, includeSource: false, includeVoteCount: false);

        // Act
        AudioRatingDto result = entity.ToResponse();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(0, result.Value);
        Assert.Equal(0, result.MaxValue);
        Assert.Null(result.Source);
        Assert.Null(result.VoteCount);
    }

    [Fact]
    public void ToResponses_WhenMappingMultipleAudioRatingEntities_ShouldMapAllCorrectly()
    {
        // Arrange
        List<AudioRatingEntity> entities = _audioRatingEntityFixture.CreateMany(2);

        // Act
        List<AudioRatingDto> results = [.. entities.ToResponses()];

        // Assert
        Assert.Equal(entities.Count, results.Count);
        for (int i = 0; i < entities.Count; i++)
        {
            Assert.Equal(entities[i].Value, results[i].Value);
            Assert.Equal(entities[i].MaxValue, results[i].MaxValue);
        }
    }

    [Fact]
    public void ToDomainValueObject_WhenMappingValidAudioRatingEntity_ShouldMapCorrectly()
    {
        // Arrange
        AudioRatingEntity entity = _audioRatingEntityFixture.Create();

        // Act
        Result<AudioRating> result = entity.ToDomainValueObject();

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotNull(result.Value);
        Assert.Equal(entity.Value, result.Value.Value);
        Assert.Equal(entity.MaxValue, result.Value.MaxValue);
        Assert.True(result.Value.Source.HasValue);
        Assert.Equal(entity.Source, result.Value.Source.Value);
        Assert.True(result.Value.VoteCount.HasValue);
        Assert.Equal(entity.VoteCount, result.Value.VoteCount.Value);
    }

    [Fact]
    public void ToDomainValueObject_WhenMappingAudioRatingEntityWithNulls_ShouldMapToDefaults()
    {
        // Arrange
        AudioRatingEntity entity = _audioRatingEntityFixture.Create(includeValue: false, includeMaxValue: false, includeSource: false, includeVoteCount: false);

        // Act
        Result<AudioRating> result = entity.ToDomainValueObject();

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(0, result.Value.Value);
        Assert.Equal(0, result.Value.MaxValue);
        Assert.False(result.Value.Source.HasValue);
        Assert.False(result.Value.VoteCount.HasValue);
    }

    [Fact]
    public void ToDomainValueObject_WhenValueIsGreaterThanMaxValue_ShouldReturnError()
    {
        // Arrange
        AudioRatingEntity entity = _audioRatingEntityFixture.Create(value: 6, maxValue: 5, source: AudioRatingSource.MusicBrainz, voteCount: 10);

        // Act
        Result<AudioRating> result = entity.ToDomainValueObject();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Metadata.RatingValueCannotBeGreaterThanMaxValue, result.FirstError);
    }

    [Fact]
    public void ToDomainValueObjects_WhenMappingMultipleValidAudioRatingEntities_ShouldMapAllCorrectly()
    {
        // Arrange
        List<AudioRatingEntity> entities = _audioRatingEntityFixture.CreateMany(2);

        // Act
        List<Result<AudioRating>> results = [.. entities.ToDomainValueObjects()];

        // Assert
        Assert.Equal(entities.Count, results.Count);
        for (int i = 0; i < entities.Count; i++)
        {
            Assert.False(results[i].IsFailure);
            Assert.Equal(entities[i].Value, results[i].Value.Value);
            Assert.Equal(entities[i].MaxValue, results[i].Value.MaxValue);
        }
    }

    [Fact]
    public void ToDomainValueObjects_WhenMappingMixedValidAndInvalidAudioRatingEntities_ShouldReturnMixedResults()
    {
        // Arrange
        List<AudioRatingEntity> entities =
        [
            _audioRatingEntityFixture.Create(value: 4, maxValue: 5, source: AudioRatingSource.MusicBrainz, voteCount: 100),
            _audioRatingEntityFixture.Create(value: 6, maxValue: 5, source: AudioRatingSource.MusicBrainz, voteCount: 100),
            _audioRatingEntityFixture.Create(value: 3, maxValue: 5, source: AudioRatingSource.MusicBrainz, voteCount: 100)
        ];

        // Act
        List<Result<AudioRating>> results = [.. entities.ToDomainValueObjects()];

        // Assert
        Assert.Equal(entities.Count, results.Count);
        Assert.False(results[0].IsFailure);
        Assert.True(results[1].IsFailure);
        Assert.False(results[2].IsFailure);
    }
}
