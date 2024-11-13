#region ========================================================================= USING =====================================================================================
using ErrorOr;
using FluentAssertions;
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
        result.IsError.Should().BeFalse();
        result.Value.Should().NotBeNull();
        result.Value.Name.Should().Be(genreDto.Name);
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
        result.IsError.Should().BeFalse();
        result.Value.Should().NotBeNull();
        result.Value.Name.Should().Be(genreDto.Name);
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
        result.IsError.Should().BeTrue();
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
        results.Should().NotBeNull();
        results.Should().HaveCount(genreDtos.Count);

        List<ErrorOr<Genre>> resultList = results.ToList();
        for (int i = 0; i < genreDtos.Count; i++)
        {
            resultList[i].IsError.Should().BeFalse();
            resultList[i].Value.Name.Should().Be(genreDtos[i].Name);
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
        results.Should().NotBeNull();
        results.Should().HaveCount(genreDtos.Count);

        List<ErrorOr<Genre>> resultList = results.ToList();
        resultList[0].IsError.Should().BeFalse();
        resultList[0].Value.Name.Should().Be("Rock");

        resultList[1].IsError.Should().BeTrue();

        resultList[2].IsError.Should().BeFalse();
        resultList[2].Value.Name.Should().Be("Jazz");

        resultList[3].IsError.Should().BeTrue();
    }
}
