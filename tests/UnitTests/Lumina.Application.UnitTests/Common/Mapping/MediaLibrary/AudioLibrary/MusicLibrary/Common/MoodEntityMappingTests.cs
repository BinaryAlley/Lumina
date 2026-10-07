#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Common;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Contracts.DTO.Common;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Common.ValueObjects.Metadata;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Common;

/// <summary>
/// Contains unit tests for the <see cref="MoodEntityMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MoodEntityMappingTests
{
    private readonly TrackMoodEntityFixture _trackMoodEntityFixture = new();

    [Fact]
    public void ToResponse_WhenMappingValidTrackMoodEntity_ShouldMapCorrectly()
    {
        // Arrange
        TrackMoodEntity entity = _trackMoodEntityFixture.Create();

        // Act
        MoodDto result = entity.ToResponse();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(entity.Name, result.Name);
    }

    [Fact]
    public void ToResponses_WhenMappingMultipleTrackMoodEntities_ShouldMapAllCorrectly()
    {
        // Arrange
        List<TrackMoodEntity> entities = _trackMoodEntityFixture.CreateMany(2);

        // Act
        List<MoodDto> results = [.. entities.ToResponses()];

        // Assert
        Assert.Equal(entities.Count, results.Count);
        for (int i = 0; i < entities.Count; i++)
            Assert.Equal(entities[i].Name, results[i].Name);
    }

    [Fact]
    public void ToDomainValueObject_WhenMappingValidTrackMoodEntity_ShouldMapCorrectly()
    {
        // Arrange
        TrackMoodEntity entity = _trackMoodEntityFixture.Create();

        // Act
        Result<Mood> result = entity.ToDomainValueObject();

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotNull(result.Value);
        Assert.Equal(entity.Name, result.Value.Name);
    }

    [Theory]
    [InlineData("")] // empty name
    [InlineData("   ")] // whitespace name
    public void ToDomainValueObject_WhenNameIsWhitespace_ShouldReturnError(string name)
    {
        // Arrange
        TrackMoodEntity entity = _trackMoodEntityFixture.Create(name: name);

        // Act
        Result<Mood> result = entity.ToDomainValueObject();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Metadata.MoodNameCannotBeEmpty, result.FirstError);
    }

    [Fact]
    public void ToDomainValueObjects_WhenMappingMultipleValidTrackMoodEntities_ShouldMapAllCorrectly()
    {
        // Arrange
        List<TrackMoodEntity> entities = _trackMoodEntityFixture.CreateMany(2);

        // Act
        List<Result<Mood>> results = [.. entities.ToDomainValueObjects()];

        // Assert
        Assert.Equal(entities.Count, results.Count);
        for (int i = 0; i < entities.Count; i++)
        {
            Assert.False(results[i].IsFailure);
            Assert.Equal(entities[i].Name, results[i].Value.Name);
        }
    }

    [Fact]
    public void ToDomainValueObjects_WhenMappingMixedValidAndInvalidTrackMoodEntities_ShouldReturnMixedResults()
    {
        // Arrange
        List<TrackMoodEntity> entities =
        [
            _trackMoodEntityFixture.Create(name: "Calm"),
            _trackMoodEntityFixture.Create(name: string.Empty),
            _trackMoodEntityFixture.Create(name: "Energetic")
        ];

        // Act
        List<Result<Mood>> results = [.. entities.ToDomainValueObjects()];

        // Assert
        Assert.Equal(entities.Count, results.Count);
        Assert.False(results[0].IsFailure);
        Assert.True(results[1].IsFailure);
        Assert.False(results[2].IsFailure);
    }
}
