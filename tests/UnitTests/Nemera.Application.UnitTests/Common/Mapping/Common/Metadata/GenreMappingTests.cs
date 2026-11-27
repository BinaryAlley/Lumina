#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.DataAccess.Entities.Common;
using Nemera.Application.Common.Mapping.Common.Metadata;
using Nemera.Domain.Common.ValueObjects.Metadata;
using Nemera.Domain.Fixtures.Common.ValueObjects.Metadata;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Nemera.Application.UnitTests.Common.Mapping.Common.Metadata;

/// <summary>
/// Contains unit tests for the <see cref="GenreMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GenreMappingTests
{
    private readonly GenreFixture _genreFixture = new();

    [Fact]
    public void ToRepositoryEntity_WhenMappingGenre_ShouldMapCorrectly()
    {
        // Arrange
        Genre genre = _genreFixture.Create("Rock");

        // Act
        GenreEntity result = genre.ToRepositoryEntity();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(genre.Name, result.Name);
    }

    [Theory]
    [InlineData("Rock")]
    [InlineData("Jazz")]
    [InlineData("Classical")]
    [InlineData("Pop")]
    public void ToRepositoryEntity_WhenMappingDifferentGenres_ShouldMapCorrectly(string name)
    {
        // Arrange
        Genre genre = _genreFixture.Create(name);

        // Act
        GenreEntity result = genre.ToRepositoryEntity();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(genre.Name, result.Name);
    }

    [Fact]
    public void ToRepositoryEntities_WhenMappingMultipleGenres_ShouldMapAllCorrectly()
    {
        // Arrange
        List<Genre> genres =
        [
            _genreFixture.Create("Rock"),
            _genreFixture.Create("Jazz"),
            _genreFixture.Create("Classical"),
            _genreFixture.Create("Pop")
        ];

        // Act
        IEnumerable<GenreEntity> results = genres.ToRepositoryEntities();

        // Assert
        Assert.NotNull(results);
        Assert.Equal(genres.Count, results.Count());
        List<GenreEntity> resultList = results.ToList();
        for (int i = 0; i < genres.Count; i++)
            Assert.Equal(genres[i].Name, resultList[i].Name);
    }
}
