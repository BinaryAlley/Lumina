#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Nemera.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Common;
using Nemera.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Nemera.Contracts.DTO.MediaLibrary.AudioLibrary.MusicLibrary;
using Nemera.Domain.Common.Errors;
using Nemera.Domain.Common.Primitives;
using Nemera.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Nemera.Application.UnitTests.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Common;

/// <summary>
/// Contains unit tests for the <see cref="IsrcEntityMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class IsrcEntityMappingTests
{
    private readonly TrackIsrcEntityFixture _trackIsrcEntityFixture = new();

    [Fact]
    public void ToResponse_WhenMappingValidTrackIsrcEntity_ShouldMapCorrectly()
    {
        // Arrange
        TrackIsrcEntity entity = _trackIsrcEntityFixture.Create();

        // Act
        IsrcDto result = entity.ToResponse();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(entity.Value, result.Value);
    }

    [Fact]
    public void ToResponses_WhenMappingMultipleTrackIsrcEntities_ShouldMapAllCorrectly()
    {
        // Arrange
        List<TrackIsrcEntity> entities = _trackIsrcEntityFixture.CreateMany(2);

        // Act
        List<IsrcDto> results = [.. entities.ToResponses()];

        // Assert
        Assert.Equal(entities.Count, results.Count);
        for (int i = 0; i < entities.Count; i++)
            Assert.Equal(entities[i].Value, results[i].Value);
    }

    [Fact]
    public void ToDomainValueObject_WhenMappingValidTrackIsrcEntity_ShouldMapCorrectly()
    {
        // Arrange
        TrackIsrcEntity entity = _trackIsrcEntityFixture.Create(value: "GBUM71029604");

        // Act
        Result<Isrc> result = entity.ToDomainValueObject();

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotNull(result.Value);
        Assert.Equal(entity.Value, result.Value.Value);
    }

    [Theory]
    [InlineData("")] // Empty value.
    [InlineData("   ")] // Whitespace value.
    public void ToDomainValueObject_WhenValueIsWhitespace_ShouldReturnError(string value)
    {
        // Arrange
        TrackIsrcEntity entity = _trackIsrcEntityFixture.Create(value: value);

        // Act
        Result<Isrc> result = entity.ToDomainValueObject();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.IsrcValueCannotBeEmpty, result.FirstError);
    }

    [Fact]
    public void ToDomainValueObject_WhenValueIsMalformed_ShouldReturnError()
    {
        // Arrange
        TrackIsrcEntity entity = _trackIsrcEntityFixture.Create(value: "not-an-isrc");

        // Act
        Result<Isrc> result = entity.ToDomainValueObject();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.IsrcInvalidFormat, result.FirstError);
    }

    [Fact]
    public void ToDomainValueObjects_WhenMappingMultipleValidTrackIsrcEntities_ShouldMapAllCorrectly()
    {
        // Arrange
        List<TrackIsrcEntity> entities = _trackIsrcEntityFixture.CreateMany(2);

        // Act
        List<Result<Isrc>> results = [.. entities.ToDomainValueObjects()];

        // Assert
        Assert.Equal(entities.Count, results.Count);
        for (int i = 0; i < entities.Count; i++)
        {
            Assert.False(results[i].IsFailure);
            Assert.Equal(entities[i].Value, results[i].Value.Value);
        }
    }

    [Fact]
    public void ToDomainValueObjects_WhenMappingMixedValidAndInvalidTrackIsrcEntities_ShouldReturnMixedResults()
    {
        // Arrange
        List<TrackIsrcEntity> entities =
        [
            _trackIsrcEntityFixture.Create(value: "GBUM71029604"),
            _trackIsrcEntityFixture.Create(value: "invalid"),
            _trackIsrcEntityFixture.Create(value: "USRC17607839")
        ];

        // Act
        List<Result<Isrc>> results = [.. entities.ToDomainValueObjects()];

        // Assert
        Assert.Equal(entities.Count, results.Count);
        Assert.False(results[0].IsFailure);
        Assert.True(results[1].IsFailure);
        Assert.False(results[2].IsFailure);
    }
}
