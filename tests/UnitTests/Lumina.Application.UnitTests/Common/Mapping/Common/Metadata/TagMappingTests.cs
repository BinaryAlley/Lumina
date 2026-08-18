#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Common;
using Lumina.Application.Common.Mapping.Common.Metadata;
using Lumina.Domain.Common.ValueObjects.Metadata;
using Lumina.Domain.Fixtures.Common.ValueObjects.Metadata;
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
    private readonly TagFixture _tagFixture = new();

    [Fact]
    public void ToRepositoryEntity_WhenMappingTag_ShouldMapCorrectly()
    {
        // Arrange
        Tag tag = _tagFixture.Create("indie");

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
        Tag tag = _tagFixture.Create(name);

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
            _tagFixture.Create("indie"),
            _tagFixture.Create("electronic"),
            _tagFixture.Create("instrumental"),
            _tagFixture.Create("live"),
            _tagFixture.Create("acoustic")
        ];

        // Act
        IEnumerable<TagEntity> results = tags.ToRepositoryEntities();

        // Assert
        Assert.NotNull(results);
        Assert.Equal(tags.Count, results.Count());
        List<TagEntity> resultList = [.. results];
        for (int i = 0; i < tags.Count; i++)
            Assert.Equal(tags[i].Name, resultList[i].Name);
    }
}
