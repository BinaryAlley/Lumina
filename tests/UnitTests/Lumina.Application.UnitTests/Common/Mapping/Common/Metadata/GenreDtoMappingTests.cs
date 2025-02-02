#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.Mapping.Common.Metadata;
using Lumina.Contracts.DTO.Common;
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
        ErrorOr<Genre> result = genreDto.ToDomainEntity();

        // Assert
        Assert.False(result.IsError);
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
        ErrorOr<Genre> result = genreDto.ToDomainEntity();

        // Assert
        Assert.False(result.IsError);
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
        ErrorOr<Genre> result = genreDto.ToDomainEntity();

        // Assert
        Assert.True(result.IsError);
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
        IEnumerable<ErrorOr<Genre>> results = genreDtos.ToDomainEntities();

        // Assert
        Assert.NotNull(results);
        Assert.Equal(genreDtos.Count, results.Count());

        List<ErrorOr<Genre>> resultList = results.ToList();
        for (int i = 0; i < genreDtos.Count; i++)
        {
            Assert.False(resultList[i].IsError);
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
        IEnumerable<ErrorOr<Genre>> results = genreDtos.ToDomainEntities();

        // Assert
        Assert.NotNull(results);
        Assert.Equal(genreDtos.Count, results.Count());

        List<ErrorOr<Genre>> resultList = results.ToList();
        Assert.False(resultList[0].IsError);
        Assert.Equal("Rock", resultList[0].Value.Name);

        Assert.True(resultList[1].IsError);

        Assert.False(resultList[2].IsError);
        Assert.Equal("Jazz", resultList[2].Value.Name);

        Assert.True(resultList[3].IsError);
    }
}
