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
/// Contains unit tests for the <see cref="GenreEntityMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GenreModelMappingTests
{
    [Fact]
    public void ToDomainModel_WhenMappingValidGenreEntity_ShouldMapCorrectly()
    {
        // Arrange
        GenreEntity genreEntity = new("Rock");

        // Act
        ErrorOr<Genre> result = genreEntity.ToDomainEntity();

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().NotBeNull();
        result.Value.Name.Should().Be(genreEntity.Name);
    }

    [Theory]
    [InlineData("Rock")]
    [InlineData("Jazz")]
    [InlineData("Classical")]
    [InlineData("Pop")]
    public void ToDomainModel_WhenMappingDifferentValidGenreEntities_ShouldMapCorrectly(string name)
    {
        // Arrange
        GenreEntity genreEntity = new(name);

        // Act
        ErrorOr<Genre> result = genreEntity.ToDomainEntity();

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().NotBeNull();
        result.Value.Name.Should().Be(genreEntity.Name);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void ToDomainModel_WhenMappingInvalidGenreEntity_ShouldReturnError(string? invalidName)
    {
        // Arrange
        GenreEntity genreEntity = new(invalidName);

        // Act
        ErrorOr<Genre> result = genreEntity.ToDomainEntity();

        // Assert
        result.IsError.Should().BeTrue();
    }

    [Fact]
    public void ToDomainModels_WhenMappingMultipleValidGenreEntities_ShouldMapAllCorrectly()
    {
        // Arrange
        List<GenreEntity> genreEntities =
        [
            new GenreEntity("Rock"),
            new GenreEntity("Jazz"),
            new GenreEntity("Classical"),
            new GenreEntity("Pop")
        ];

        // Act
        IEnumerable<ErrorOr<Genre>> results = genreEntities.ToDomainEntities();

        // Assert
        results.Should().NotBeNull();
        results.Should().HaveCount(genreEntities.Count);

        List<ErrorOr<Genre>> resultList = results.ToList();
        for (int i = 0; i < genreEntities.Count; i++)
        {
            resultList[i].IsError.Should().BeFalse();
            resultList[i].Value.Name.Should().Be(genreEntities[i].Name);
        }
    }

    [Fact]
    public void ToDomainModels_WhenMappingMixedValidAndInvalidGenreEntities_ShouldReturnMixedResults()
    {
        // Arrange
        List<GenreEntity> genreEntities =
        [
            new GenreEntity("Rock"),
            new GenreEntity(""),
            new GenreEntity("Jazz"),
            new GenreEntity(" ")
        ];

        // Act
        IEnumerable<ErrorOr<Genre>> results = genreEntities.ToDomainEntities();

        // Assert
        results.Should().NotBeNull();
        results.Should().HaveCount(genreEntities.Count);

        List<ErrorOr<Genre>> resultList = results.ToList();
        resultList[0].IsError.Should().BeFalse();
        resultList[0].Value.Name.Should().Be("Rock");

        resultList[1].IsError.Should().BeTrue();

        resultList[2].IsError.Should().BeFalse();
        resultList[2].Value.Name.Should().Be("Jazz");

        resultList[3].IsError.Should().BeTrue();
    }
}
