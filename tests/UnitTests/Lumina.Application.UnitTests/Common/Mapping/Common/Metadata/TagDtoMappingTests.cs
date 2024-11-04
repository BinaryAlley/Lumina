#region ========================================================================= USING =====================================================================================
using ErrorOr;
using FluentAssertions;
using Lumina.Application.Common.Mapping.Common.Metadata;
using Lumina.Contracts.DTO.Common;
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
        ErrorOr<Tag> result = tagDto.ToDomainEntity();

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().NotBeNull();
        result.Value.Name.Should().Be(tagDto.Name);
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
        ErrorOr<Tag> result = tagDto.ToDomainEntity();

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().NotBeNull();
        result.Value.Name.Should().Be(tagDto.Name);
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
        ErrorOr<Tag> result = tagDto.ToDomainEntity();

        // Assert
        result.IsError.Should().BeTrue();
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
        IEnumerable<ErrorOr<Tag>> results = tagDtos.ToDomainEntities();

        // Assert
        results.Should().NotBeNull();
        results.Should().HaveCount(tagDtos.Count);

        List<ErrorOr<Tag>> resultList = results.ToList();
        for (int i = 0; i < tagDtos.Count; i++)
        {
            resultList[i].IsError.Should().BeFalse();
            resultList[i].Value.Name.Should().Be(tagDtos[i].Name);
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
        IEnumerable<ErrorOr<Tag>> results = tagDtos.ToDomainEntities();

        // Assert
        results.Should().NotBeNull();
        results.Should().HaveCount(tagDtos.Count);

        List<ErrorOr<Tag>> resultList = results.ToList();
        resultList[0].IsError.Should().BeFalse();
        resultList[0].Value.Name.Should().Be("indie");

        resultList[1].IsError.Should().BeTrue();

        resultList[2].IsError.Should().BeFalse();
        resultList[2].Value.Name.Should().Be("electronic");

        resultList[3].IsError.Should().BeTrue();
    }
}
