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
/// Contains unit tests for the <see cref="GenreDtoMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GenreDtoMappingTests
{
    [Fact]
    public void ToDomainEntity_WhenMappingValidGenreDto_ShouldMapCorrectly()
    {
        // Arrange
        GenreDto genreDto = new("Rock");

        // Act
        Result<Genre> result = genreDto.ToDomainEntity();

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotNull(result.Value);
        Assert.Equal(genreDto.Name, result.Value.Name);
    }

    [Theory]
    [InlineData("Rock")]
    [InlineData("Jazz")]
    [InlineData("Classical")]
    [InlineData("Pop")]
    public void ToDomainEntity_WhenMappingDifferentValidGenreDtos_ShouldMapCorrectly(string name)
    {
        // Arrange
        GenreDto genreDto = new(name);

        // Act
        Result<Genre> result = genreDto.ToDomainEntity();

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotNull(result.Value);
        Assert.Equal(genreDto.Name, result.Value.Name);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void ToDomainEntity_WhenMappingInvalidGenreDto_ShouldReturnError(string? invalidName)
    {
        // Arrange
        GenreDto genreDto = new(invalidName);

        // Act
        Result<Genre> result = genreDto.ToDomainEntity();

        // Assert
        Assert.True(result.IsFailure);
    }

    [Fact]
    public void ToDomainModels_WhenMappingMultipleValidGenreDtos_ShouldMapAllCorrectly()
    {
        // Arrange
        List<GenreDto> genreDtos =
        [
            new GenreDto("Rock"),
            new GenreDto("Jazz"),
            new GenreDto("Classical"),
            new GenreDto("Pop")
        ];

        // Act
        IEnumerable<Result<Genre>> results = genreDtos.ToDomainEntities();

        // Assert
        Assert.NotNull(results);
        Assert.Equal(genreDtos.Count, results.Count());

        List<Result<Genre>> resultList = results.ToList();
        for (int i = 0; i < genreDtos.Count; i++)
        {
            Assert.False(resultList[i].IsFailure);
            Assert.Equal(genreDtos[i].Name, resultList[i].Value.Name);
        }
    }

    [Fact]
    public void ToDomainModels_WhenMappingMixedValidAndInvalidGenreDtos_ShouldReturnMixedResults()
    {
        // Arrange
        List<GenreDto> genreDtos =
        [
            new GenreDto("Rock"),
            new GenreDto(""),
            new GenreDto("Jazz"),
            new GenreDto(" ")
        ];

        // Act
        IEnumerable<Result<Genre>> results = genreDtos.ToDomainEntities();

        // Assert
        Assert.NotNull(results);
        Assert.Equal(genreDtos.Count, results.Count());

        List<Result<Genre>> resultList = results.ToList();
        Assert.False(resultList[0].IsFailure);
        Assert.Equal("Rock", resultList[0].Value.Name);

        Assert.True(resultList[1].IsFailure);

        Assert.False(resultList[2].IsFailure);
        Assert.Equal("Jazz", resultList[2].Value.Name);

        Assert.True(resultList[3].IsFailure);
    }
}
