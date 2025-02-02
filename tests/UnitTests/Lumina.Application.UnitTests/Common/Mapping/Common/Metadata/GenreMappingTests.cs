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
        Assert.False(createResult.IsError);
        Genre genre = createResult.Value;

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
        ErrorOr<Genre> createResult = Genre.Create(name);
        Assert.False(createResult.IsError);
        Genre genre = createResult.Value;

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
            Genre.Create("Rock").Value,
            Genre.Create("Jazz").Value,
            Genre.Create("Classical").Value,
            Genre.Create("Pop").Value
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
