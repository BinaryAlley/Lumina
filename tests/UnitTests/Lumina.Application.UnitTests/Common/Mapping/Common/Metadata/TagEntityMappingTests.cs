#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Common;
using Lumina.Application.Common.Mapping.Common.Metadata;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Common;
using Lumina.Contracts.DTO.Common;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Common.ValueObjects.Metadata;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.Common.Metadata;

/// <summary>
/// Contains unit tests for the <see cref="TagEntityMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class TagEntityMappingTests
{
    private readonly TagEntityFixture _tagEntityFixture = new();

    [Fact]
    public void ToResponse_WhenMappingValidTagEntity_ShouldMapCorrectly()
    {
        // Arrange
        TagEntity entity = _tagEntityFixture.Create(name: "Fantasy");

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
        TagEntity entity = _tagEntityFixture.Create(name: name);

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
        TagEntity entity = _tagEntityFixture.Create(name: invalidName, includeName: invalidName is not null);

        // Act
        TagDto result = entity.ToResponse();

        // Assert
        Assert.Equal(invalidName, result.Name);
    }

    [Fact]
    public void ToDomainEntity_WhenMappingValidTagEntity_ShouldMapCorrectly()
    {
        // Arrange
        TagEntity entity = _tagEntityFixture.Create(name: "Fantasy");

        // Act
        Result<Tag> result = entity.ToDomainEntity();

        // Assert
        Assert.False(result.IsFailure);
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
        TagEntity entity = _tagEntityFixture.Create(name: invalidName, includeName: invalidName is not null);

        // Act
        Result<Tag> result = entity.ToDomainEntity();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Metadata.TagNameCannotBeEmpty, result.FirstError);
    }

    [Fact]
    public void ToDomainEntities_WhenMappingMultipleValidTagEntities_ShouldMapAllCorrectly()
    {
        // Arrange
        List<TagEntity> entities =
        [
            _tagEntityFixture.Create(name: "Fantasy"),
            _tagEntityFixture.Create(name: "Young Adult"),
            _tagEntityFixture.Create(name: "Historical"),
            _tagEntityFixture.Create(name: "Coming of Age")
        ];

        // Act
        IEnumerable<Result<Tag>> results = entities.ToDomainEntities();

        // Assert
        Assert.NotNull(results);
        Assert.Equal(entities.Count, results.Count());

        List<Result<Tag>> resultList = [.. results];
        for (int i = 0; i < entities.Count; i++)
        {
            Assert.False(resultList[i].IsFailure);
            Assert.Equal(entities[i].Name, resultList[i].Value.Name);
        }
    }

    [Fact]
    public void ToResponses_WhenMappingMultipleValidTagEntities_ShouldMapAllCorrectly()
    {
        // Arrange
        List<TagEntity> entities =
        [
            _tagEntityFixture.Create(name: "Fantasy"),
            _tagEntityFixture.Create(name: "Young Adult"),
            _tagEntityFixture.Create(name: "Historical"),
            _tagEntityFixture.Create(name: "Coming of Age")
        ];

        // Act
        IEnumerable<TagDto> results = entities.ToResponses();

        // Assert
        Assert.NotNull(results);
        Assert.Equal(entities.Count, results.Count());

        List<TagDto> resultList = [.. results];
        for (int i = 0; i < entities.Count; i++)
            Assert.Equal(entities[i].Name, resultList[i].Name);
    }
}
