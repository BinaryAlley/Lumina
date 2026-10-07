#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Common;
using Lumina.Contracts.DTO.Common;
using Lumina.Contracts.Fixtures.Core.DTO.Common;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Common.ValueObjects.Metadata;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Common;

/// <summary>
/// Contains unit tests for the <see cref="MoodDtoMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MoodDtoMappingTests
{
    private readonly MoodDtoFixture _moodDtoFixture = new();

    [Fact]
    public void ToDomainValueObject_WhenMappingValidMoodDto_ShouldMapCorrectly()
    {
        // Arrange
        MoodDto dto = _moodDtoFixture.Create(name: "  Calm  ");

        // Act
        Result<Mood> result = dto.ToDomainValueObject();

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotNull(result.Value);
        Assert.Equal("Calm", result.Value.Name);
    }

    [Theory]
    [InlineData(null)] // missing name
    [InlineData("")] // empty name
    [InlineData("   ")] // whitespace name
    public void ToDomainValueObject_WhenNameIsNullOrWhitespace_ShouldReturnError(string? name)
    {
        // Arrange
        MoodDto dto = _moodDtoFixture.Create(name: name, includeName: name is not null);

        // Act
        Result<Mood> result = dto.ToDomainValueObject();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Metadata.MoodNameCannotBeEmpty, result.FirstError);
    }

    [Fact]
    public void ToDomainValueObjects_WhenMappingMultipleValidMoodDtos_ShouldMapAllCorrectly()
    {
        // Arrange
        List<MoodDto> dtos = _moodDtoFixture.CreateMany(2);

        // Act
        List<Result<Mood>> results = [.. dtos.ToDomainValueObjects()];

        // Assert
        Assert.Equal(dtos.Count, results.Count);
        for (int i = 0; i < dtos.Count; i++)
        {
            Assert.False(results[i].IsFailure);
            Assert.Equal(dtos[i].Name, results[i].Value.Name);
        }
    }

    [Fact]
    public void ToDomainValueObjects_WhenMappingMixedValidAndInvalidMoodDtos_ShouldReturnMixedResults()
    {
        // Arrange
        List<MoodDto> dtos =
        [
            _moodDtoFixture.Create(name: "Calm"),
            _moodDtoFixture.Create(name: string.Empty, includeName: true),
            _moodDtoFixture.Create(name: "Energetic")
        ];

        // Act
        List<Result<Mood>> results = [.. dtos.ToDomainValueObjects()];

        // Assert
        Assert.Equal(dtos.Count, results.Count);
        Assert.False(results[0].IsFailure);
        Assert.Equal("Calm", results[0].Value.Name);
        Assert.True(results[1].IsFailure);
        Assert.False(results[2].IsFailure);
        Assert.Equal("Energetic", results[2].Value.Name);
    }
}
