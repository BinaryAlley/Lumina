#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.DataAccess.Entities.Common;
using Lumina.Application.Common.Mapping.Common.Metadata;
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
        Assert.False(createResult.IsError);
        Tag tag = createResult.Value;

        // Act
        TagEntity result = tag.ToRepositoryEntity();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(tag.Name, result.Name);
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
        Assert.False(createResult.IsError);
        Tag tag = createResult.Value;

        // Act
        TagEntity result = tag.ToRepositoryEntity();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(tag.Name, result.Name);
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
        Assert.NotNull(results);
        Assert.Equal(tags.Count, results.Count());
        List<TagEntity> resultList = results.ToList();
        for (int i = 0; i < tags.Count; i++)
            Assert.Equal(tags[i].Name, resultList[i].Name);
    }
}
