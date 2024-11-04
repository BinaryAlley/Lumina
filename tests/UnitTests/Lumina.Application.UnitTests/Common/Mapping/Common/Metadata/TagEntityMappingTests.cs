#region ========================================================================= USING =====================================================================================
using ErrorOr;
using FluentAssertions;
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
        result.Should().NotBeNull();
        result.Name.Should().Be(entity.Name);
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
        result.Should().NotBeNull();
        result.Name.Should().Be(entity.Name);
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
        result.Name.Should().Be(invalidName);
    }

    [Fact]
    public void ToDomainEntity_WhenMappingValidTagEntity_ShouldMapCorrectly()
    {
        // Arrange
        TagEntity entity = new("Fantasy");

        // Act
        ErrorOr<Tag> result = entity.ToDomainEntity();

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().NotBeNull();
        result.Value.Name.Should().Be(entity.Name);
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
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.Metadata.TagNameCannotBeEmpty);
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
        results.Should().NotBeNull();
        results.Should().HaveCount(entities.Count);

        List<ErrorOr<Tag>> resultList = results.ToList();
        for (int i = 0; i < entities.Count; i++)
        {
            resultList[i].IsError.Should().BeFalse();
            resultList[i].Value.Name.Should().Be(entities[i].Name);
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
        results.Should().NotBeNull();
        results.Should().HaveCount(entities.Count);

        List<TagDto> resultList = results.ToList();
        for (int i = 0; i < entities.Count; i++)
        {
            resultList[i].Name.Should().Be(entities[i].Name);
        }
    }
}
