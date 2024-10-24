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
/// Contains unit tests for the <see cref="TagMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class TagMappingTests
{
    [Fact]
    public void ToRepositoryEntity_WhenMappingTag_ShouldMapCorrectly()
    {
        // Arrange
        ErrorOr<Tag> createResult = Tag.Create("indie");
        createResult.IsError.Should().BeFalse();
        Tag tag = createResult.Value;

        // Act
        TagEntity result = tag.ToRepositoryEntity();

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(tag.Name);
    }

    [Theory]
    [InlineData("indie")]
    [InlineData("electronic")]
    [InlineData("instrumental")]
    [InlineData("live")]
    [InlineData("acoustic")]
    public void ToRepositoryEntity_WhenMappingDifferentTags_ShouldMapCorrectly(string name)
    {
        // Arrange
        ErrorOr<Tag> createResult = Tag.Create(name);
        createResult.IsError.Should().BeFalse();
        Tag tag = createResult.Value;

        // Act
        TagEntity result = tag.ToRepositoryEntity();

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(tag.Name);
    }

    [Fact]
    public void ToRepositoryEntities_WhenMappingMultipleTags_ShouldMapAllCorrectly()
    {
        // Arrange
        List<Tag> tags =
        [
            Tag.Create("indie").Value,
            Tag.Create("electronic").Value,
            Tag.Create("instrumental").Value,
            Tag.Create("live").Value,
            Tag.Create("acoustic").Value
        ];

        // Act
        IEnumerable<TagEntity> results = tags.ToRepositoryEntities();

        // Assert
        results.Should().NotBeNull();
        results.Should().HaveCount(tags.Count);
        List<TagEntity> resultList = results.ToList();
        for (int i = 0; i < tags.Count; i++)
            resultList[i].Name.Should().Be(tags[i].Name);
    }
}
