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
/// Contains unit tests for the <see cref="GenreEntityMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GenreEntityMappingTests
{
    private readonly GenreEntityFixture _genreEntityFixture = new();

    [Fact]
    public void ToResponse_WhenMappingValidGenreEntity_ShouldMapCorrectly()
    {
        // Arrange
        GenreEntity entity = _genreEntityFixture.Create(name: "Fiction");

        // Act
        GenreDto result = entity.ToResponse();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(entity.Name, result.Name);
    }

    [Theory]
    [InlineData("Fiction")]
    [InlineData("Science Fiction")]
    [InlineData("Mystery")]
    [InlineData("Romance")]
    public void ToResponse_WhenMappingDifferentValidGenreEntities_ShouldMapCorrectly(string name)
    {
        // Arrange
        GenreEntity entity = _genreEntityFixture.Create(name: name);

        // Act
        GenreDto result = entity.ToResponse();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(entity.Name, result.Name);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void ToResponse_WhenMappingInvalidGenreEntity_ShouldMapToDefault(string? invalidName)
    {
        // Arrange
        GenreEntity entity = _genreEntityFixture.Create(name: invalidName, includeName: invalidName is not null);

        // Act
        GenreDto result = entity.ToResponse();

        // Assert
        Assert.Equal(invalidName, result.Name);
    }

    [Fact]
    public void ToDomainEntity_WhenMappingValidGenreEntity_ShouldMapCorrectly()
    {
        // Arrange
        GenreEntity entity = _genreEntityFixture.Create(name: "Fiction");

        // Act
        Result<Genre> result = entity.ToDomainEntity();

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotNull(result.Value);
        Assert.Equal(entity.Name, result.Value.Name);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void ToDomainEntity_WhenMappingInvalidGenreEntity_ShouldMapToDefault(string? invalidName)
    {
        // Arrange
        GenreEntity entity = _genreEntityFixture.Create(name: invalidName, includeName: invalidName is not null);

        // Act
        Result<Genre> result = entity.ToDomainEntity();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Metadata.GenreNameCannotBeEmpty, result.FirstError);
    }

    [Fact]
    public void ToDomainEntities_WhenMappingMultipleValidGenreEntities_ShouldMapAllCorrectly()
    {
        // Arrange
        List<GenreEntity> entities =
        [
            _genreEntityFixture.Create(name: "Fiction"),
            _genreEntityFixture.Create(name: "Mystery"),
            _genreEntityFixture.Create(name: "Romance"),
            _genreEntityFixture.Create(name: "Thriller")
        ];

        // Act
        IEnumerable<Result<Genre>> results = entities.ToDomainEntities();

        // Assert
        Assert.NotNull(results);
        Assert.Equal(entities.Count, results.Count());

        List<Result<Genre>> resultList = results.ToList();
        for (int i = 0; i < entities.Count; i++)
        {
            Assert.False(resultList[i].IsFailure);
            Assert.Equal(entities[i].Name, resultList[i].Value.Name);
        }
    }

    [Fact]
    public void ToResponses_WhenMappingMultipleValidGenreEntities_ShouldMapAllCorrectly()
    {
        // Arrange
        List<GenreEntity> entities =
        [
            _genreEntityFixture.Create(name: "Fiction"),
            _genreEntityFixture.Create(name: "Mystery"),
            _genreEntityFixture.Create(name: "Romance"),
            _genreEntityFixture.Create(name: "Thriller")
        ];

        // Act
        IEnumerable<GenreDto> results = entities.ToResponses();

        // Assert
        Assert.NotNull(results);
        Assert.Equal(entities.Count, results.Count());

        List<GenreDto> resultList = results.ToList();
        for (int i = 0; i < entities.Count; i++)
            Assert.Equal(entities[i].Name, resultList[i].Name);
    }
}
