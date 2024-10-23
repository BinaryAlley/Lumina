#region ========================================================================= USING =====================================================================================
using ErrorOr;
using FluentAssertions;
using Lumina.Application.Common.Mapping.Common.Metadata;
using Lumina.Contracts.Entities.Common;
using Lumina.Domain.Common.ValueObjects.Metadata;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.Common.Metadata;

/// <summary>
/// Contains unit tests for the <see cref="TagModelMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class TagModelMappingTests
{
    [Fact]
    public void ToDomainModel_WhenMappingValidTagEntity_ShouldMapCorrectly()
    {
        // Arrange
        TagEntity tagEntity = new("indie");

        // Act
        ErrorOr<Tag> result = tagEntity.ToDomainModel();

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().NotBeNull();
        result.Value.Name.Should().Be(tagEntity.Name);
    }

    [Theory]
    [InlineData("indie")]
    [InlineData("electronic")]
    [InlineData("instrumental")]
    [InlineData("live")]
    [InlineData("acoustic")]
    public void ToDomainModel_WhenMappingDifferentValidTagEntities_ShouldMapCorrectly(string name)
    {
        // Arrange
        TagEntity tagEntity = new(name);

        // Act
        ErrorOr<Tag> result = tagEntity.ToDomainModel();

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().NotBeNull();
        result.Value.Name.Should().Be(tagEntity.Name);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void ToDomainModel_WhenMappingInvalidTagEntity_ShouldReturnError(string? invalidName)
    {
        // Arrange
        TagEntity tagEntity = new(invalidName);

        // Act
        ErrorOr<Tag> result = tagEntity.ToDomainModel();

        // Assert
        result.IsError.Should().BeTrue();
    }

    [Fact]
    public void ToDomainModels_WhenMappingMultipleValidTagEntities_ShouldMapAllCorrectly()
    {
        // Arrange
        List<TagEntity> tagEntities =
        [
            new TagEntity("indie"),
            new TagEntity("electronic"),
            new TagEntity("instrumental"),
            new TagEntity("live")
        ];

        // Act
        IEnumerable<ErrorOr<Tag>> results = tagEntities.ToDomainModels();

        // Assert
        results.Should().NotBeNull();
        results.Should().HaveCount(tagEntities.Count);

        List<ErrorOr<Tag>> resultList = results.ToList();
        for (int i = 0; i < tagEntities.Count; i++)
        {
            resultList[i].IsError.Should().BeFalse();
            resultList[i].Value.Name.Should().Be(tagEntities[i].Name);
        }
    }

    [Fact]
    public void ToDomainModels_WhenMappingMixedValidAndInvalidTagEntities_ShouldReturnMixedResults()
    {
        // Arrange
        List<TagEntity> tagEntities =
        [
            new TagEntity("indie"),
            new TagEntity(""),
            new TagEntity("electronic"),
            new TagEntity(" ")
        ];

        // Act
        IEnumerable<ErrorOr<Tag>> results = tagEntities.ToDomainModels();

        // Assert
        results.Should().NotBeNull();
        results.Should().HaveCount(tagEntities.Count);

        List<ErrorOr<Tag>> resultList = results.ToList();
        resultList[0].IsError.Should().BeFalse();
        resultList[0].Value.Name.Should().Be("indie");

        resultList[1].IsError.Should().BeTrue();

        resultList[2].IsError.Should().BeFalse();
        resultList[2].Value.Name.Should().Be("electronic");

        resultList[3].IsError.Should().BeTrue();
    }
}
