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
/// Contains unit tests for the <see cref="GenreMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GenreMappingTests
{
    [Fact]
    public void ToRepositoryEntity_WhenMappingGenre_ShouldMapCorrectly()
    {
        // Arrange
        ErrorOr<Genre> createResult = Genre.Create("Rock");
        createResult.IsError.Should().BeFalse();
        Genre genre = createResult.Value;

        // Act
        GenreEntity result = genre.ToRepositoryEntity();

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(genre.Name);
    }

    [Theory]
    [InlineData("Rock")]
    [InlineData("Jazz")]
    [InlineData("Classical")]
    [InlineData("Pop")]
    public void ToRepositoryEntity_WhenMappingDifferentGenres_ShouldMapCorrectly(string name)
    {
        // Arrange
        ErrorOr<Genre> createResult = Genre.Create(name);
        createResult.IsError.Should().BeFalse();
        Genre genre = createResult.Value;

        // Act
        GenreEntity result = genre.ToRepositoryEntity();

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(genre.Name);
    }

    [Fact]
    public void ToRepositoryEntities_WhenMappingMultipleGenres_ShouldMapAllCorrectly()
    {
        // Arrange
        List<Genre> genres =
        [
            Genre.Create("Rock").Value,
            Genre.Create("Jazz").Value,
            Genre.Create("Classical").Value,
            Genre.Create("Pop").Value
        ];

        // Act
        IEnumerable<GenreEntity> results = genres.ToRepositoryEntities();

        // Assert
        results.Should().NotBeNull();
        results.Should().HaveCount(genres.Count);
        List<GenreEntity> resultList = results.ToList();
        for (int i = 0; i < genres.Count; i++)
            resultList[i].Name.Should().Be(genres[i].Name);
    }
}
