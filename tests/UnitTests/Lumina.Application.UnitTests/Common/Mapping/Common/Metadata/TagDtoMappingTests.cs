#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.Common.Metadata;
using Lumina.Contracts.DTO.Common;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Common.ValueObjects.Metadata;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.Common.Metadata;

/// <summary>
/// Contains unit tests for the <see cref="TagDtoMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class TagDtoMappingTests
{
    [Fact]
    public void ToDomainEntity_WhenMappingValidTagDto_ShouldMapCorrectly()
    {
        // Arrange
        TagDto tagDto = new("indie");

        // Act
        Result<Tag> result = tagDto.ToDomainEntity();

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotNull(result.Value);
        Assert.Equal(tagDto.Name, result.Value.Name);
    }

    [Theory]
    [InlineData("indie")]
    [InlineData("electronic")]
    [InlineData("instrumental")]
    [InlineData("live")]
    [InlineData("acoustic")]
    public void ToDomainEntity_WhenMappingDifferentValidTagDtos_ShouldMapCorrectly(string name)
    {
        // Arrange
        TagDto tagDto = new(name);

        // Act
        Result<Tag> result = tagDto.ToDomainEntity();

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotNull(result.Value);
        Assert.Equal(tagDto.Name, result.Value.Name);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void ToDomainEntity_WhenMappingInvalidTagDto_ShouldReturnError(string? invalidName)
    {
        // Arrange
        TagDto tagDto = new(invalidName);

        // Act
        Result<Tag> result = tagDto.ToDomainEntity();

        // Assert
        Assert.True(result.IsFailure);
    }

    [Fact]
    public void ToDomainModels_WhenMappingMultipleValidTagDtos_ShouldMapAllCorrectly()
    {
        // Arrange
        List<TagDto> tagDtos =
        [
            new TagDto("indie"),
            new TagDto("electronic"),
            new TagDto("instrumental"),
            new TagDto("live")
        ];

        // Act
        IEnumerable<Result<Tag>> results = tagDtos.ToDomainEntities();

        // Assert
        Assert.NotNull(results);
        Assert.Equal(tagDtos.Count, results.Count());

        List<Result<Tag>> resultList = [.. results];
        for (int i = 0; i < tagDtos.Count; i++)
        {
            Assert.False(resultList[i].IsFailure);
            Assert.Equal(tagDtos[i].Name, resultList[i].Value.Name);
        }
    }

    [Fact]
    public void ToDomainModels_WhenMappingMixedValidAndInvalidTagDtos_ShouldReturnMixedResults()
    {
        // Arrange
        List<TagDto> tagDtos =
        [
            new TagDto("indie"),
            new TagDto(""),
            new TagDto("electronic"),
            new TagDto(" ")
        ];

        // Act
        IEnumerable<Result<Tag>> results = tagDtos.ToDomainEntities();

        // Assert
        Assert.NotNull(results);
        Assert.Equal(tagDtos.Count, results.Count());

        List<Result<Tag>> resultList = [.. results];
        Assert.False(resultList[0].IsFailure);
        Assert.Equal("indie", resultList[0].Value.Name);

        Assert.True(resultList[1].IsFailure);

        Assert.False(resultList[2].IsFailure);
        Assert.Equal("electronic", resultList[2].Value.Name);

        Assert.True(resultList[3].IsFailure);
    }
}
