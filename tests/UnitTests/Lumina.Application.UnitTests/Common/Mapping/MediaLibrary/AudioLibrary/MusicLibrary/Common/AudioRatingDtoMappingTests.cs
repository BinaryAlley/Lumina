#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Common;
using Lumina.Contracts.DTO.MediaLibrary.AudioLibrary;
using Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.AudioLibrary;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.Common.ValueObjects;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Common;

/// <summary>
/// Contains unit tests for the <see cref="AudioRatingDtoMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class AudioRatingDtoMappingTests
{
    private readonly AudioRatingDtoFixture _audioRatingDtoFixture = new();

    [Fact]
    public void ToDomainEntity_WhenMappingCompleteAudioRatingDto_ShouldMapAllPropertiesCorrectly()
    {
        // Arrange
        AudioRatingDto dto = _audioRatingDtoFixture.Create();

        // Act
        Result<AudioRating> result = dto.ToDomainEntity();

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotNull(result.Value);
        Assert.Equal(dto.Value, result.Value.Value);
        Assert.Equal(dto.MaxValue, result.Value.MaxValue);
        Assert.True(result.Value.Source.HasValue);
        Assert.Equal(dto.Source, result.Value.Source.Value);
        Assert.True(result.Value.VoteCount.HasValue);
        Assert.Equal(dto.VoteCount, result.Value.VoteCount.Value);
    }

    [Fact]
    public void ToDomainEntity_WhenOptionalPropertiesAreMissing_ShouldMapTheRequiredValuesAndNones()
    {
        // Arrange
        AudioRatingDto dto = _audioRatingDtoFixture.Create(includeSource: false, includeVoteCount: false);

        // Act
        Result<AudioRating> result = dto.ToDomainEntity();

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(dto.Value, result.Value.Value);
        Assert.Equal(dto.MaxValue, result.Value.MaxValue);
        Assert.False(result.Value.Source.HasValue);
        Assert.False(result.Value.VoteCount.HasValue);
    }

    [Fact]
    public void ToDomainEntity_WhenValueIsGreaterThanMaxValue_ShouldReturnError()
    {
        // Arrange
        AudioRatingDto dto = _audioRatingDtoFixture.Create(value: 10, maxValue: 5);

        // Act
        Result<AudioRating> result = dto.ToDomainEntity();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Metadata.RatingValueCannotBeGreaterThanMaxValue, result.FirstError);
    }

    [Fact]
    public void ToDomainEntities_WhenMappingMultipleValidAudioRatingDtos_ShouldMapAllCorrectly()
    {
        // Arrange
        List<AudioRatingDto> dtos = _audioRatingDtoFixture.CreateMany(2);

        // Act
        List<Result<AudioRating>> results = [.. dtos.ToDomainEntities()];

        // Assert
        Assert.Equal(dtos.Count, results.Count);
        for (int i = 0; i < dtos.Count; i++)
        {
            Assert.False(results[i].IsFailure);
            Assert.Equal(dtos[i].Value, results[i].Value.Value);
            Assert.Equal(dtos[i].MaxValue, results[i].Value.MaxValue);
        }
    }

    [Fact]
    public void ToDomainEntities_WhenMappingMixedValidAndInvalidAudioRatingDtos_ShouldReturnMixedResults()
    {
        // Arrange
        List<AudioRatingDto> dtos =
        [
            _audioRatingDtoFixture.Create(value: 4, maxValue: 5),
            _audioRatingDtoFixture.Create(value: 10, maxValue: 5),
            _audioRatingDtoFixture.Create(value: 3, maxValue: 5)
        ];

        // Act
        List<Result<AudioRating>> results = [.. dtos.ToDomainEntities()];

        // Assert
        Assert.Equal(dtos.Count, results.Count);
        Assert.False(results[0].IsFailure);
        Assert.True(results[1].IsFailure);
        Assert.False(results[2].IsFailure);
    }
}
