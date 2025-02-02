#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.DataAccess.Entities.Common;
using Lumina.Application.Common.Mapping.Common.Metadata;
using Lumina.Contracts.DTO.Common;
using Lumina.Domain.Common.ValueObjects.Metadata;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Lumina.Domain.Common.Errors;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.Common.Metadata;

/// <summary>
/// Contains unit tests for the <see cref="TagEntityMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class TagEntityMappingTests
{
    [Fact]
    public void ToResponse_WhenMappingValidTagEntity_ShouldMapCorrectly()
    {
        // Arrange
        TagEntity entity = new("Fantasy");

        // Act
        TagDto result = entity.ToResponse();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(entity.Name, result.Name);
    }

    [Theory]
    [InlineData("Fantasy")]
    [InlineData("Young Adult")]
    [InlineData("Historical")]
    [InlineData("Coming of Age")]
    public void ToResponse_WhenMappingDifferentValidTagEntities_ShouldMapCorrectly(string name)
    {
        // Arrange
        TagEntity entity = new(name);

        // Act
        TagDto result = entity.ToResponse();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(entity.Name, result.Name);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void ToResponse_WhenMappingInvalidTagEntity_ShouldMapToDefault(string? invalidName)
    {
        // Arrange
        TagEntity entity = new(invalidName);

        // Act
        TagDto result = entity.ToResponse();

        // Assert
        Assert.Equal(invalidName, result.Name);
    }

    [Fact]
    public void ToDomainEntity_WhenMappingValidTagEntity_ShouldMapCorrectly()
    {
        // Arrange
        TagEntity entity = new("Fantasy");

        // Act
        ErrorOr<Tag> result = entity.ToDomainEntity();

        // Assert
        Assert.False(result.IsError);
        Assert.NotNull(result.Value);
        Assert.Equal(entity.Name, result.Value.Name);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void ToDomainEntity_WhenMappingInvalidTagEntity_ShouldMapToDefault(string? invalidName)
    {
        // Arrange
        TagEntity entity = new(invalidName);

        // Act
        ErrorOr<Tag> result = entity.ToDomainEntity();

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(Errors.Metadata.TagNameCannotBeEmpty, result.FirstError);
    }

    [Fact]
    public void ToDomainEntities_WhenMappingMultipleValidTagEntities_ShouldMapAllCorrectly()
    {
        // Arrange
        List<TagEntity> entities =
        [
            new TagEntity("Fantasy"),
            new TagEntity("Young Adult"),
            new TagEntity("Historical"),
            new TagEntity("Coming of Age")
        ];

        // Act
        IEnumerable<ErrorOr<Tag>> results = entities.ToDomainEntities();

        // Assert
        Assert.NotNull(results);
        Assert.Equal(entities.Count, results.Count());

        List<ErrorOr<Tag>> resultList = results.ToList();
        for (int i = 0; i < entities.Count; i++)
        {
            Assert.False(resultList[i].IsError);
            Assert.Equal(entities[i].Name, resultList[i].Value.Name);
        }
    }

    [Fact]
    public void ToResponses_WhenMappingMultipleValidTagEntities_ShouldMapAllCorrectly()
    {
        // Arrange
        List<TagEntity> entities =
        [
            new TagEntity("Fantasy"),
            new TagEntity("Young Adult"),
            new TagEntity("Historical"),
            new TagEntity("Coming of Age")
        ];

        // Act
        IEnumerable<TagDto> results = entities.ToResponses();

        // Assert
        Assert.NotNull(results);
        Assert.Equal(entities.Count, results.Count());

        List<TagDto> resultList = results.ToList();
        for (int i = 0; i < entities.Count; i++)
            Assert.Equal(entities[i].Name, resultList[i].Name);
    }
}
