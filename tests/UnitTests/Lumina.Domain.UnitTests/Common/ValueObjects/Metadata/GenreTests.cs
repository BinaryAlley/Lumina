#region ========================================================================= USING =====================================================================================
using DomainErrors = Lumina.Domain.Common.Errors.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Common.ValueObjects.Metadata;
using Lumina.Domain.Fixtures.Common.ValueObjects.Metadata;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.UnitTests.Common.ValueObjects.Metadata;

/// <summary>
/// Contains unit tests for the <see cref="Genre"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GenreTests
{
    private readonly GenreFixture _genreFixture = new();

    [Fact]
    public void Create_WhenCalledWithValidName_ShouldCreateGenreWithTrimmedName()
    {
        // Act
        Result<Genre> result = Genre.Create("  Science Fiction  ");

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal("Science Fiction", result.Value.Name);
    }

    [Theory]
    [InlineData(null)] // null name
    [InlineData("")] // empty name
    [InlineData("   ")] // whitespace name
    public void Create_WhenNameIsNullOrWhitespace_ShouldReturnError(string? name)
    {
        // Act
        Result<Genre> result = Genre.Create(name);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Metadata.GenreNameCannotBeEmpty, result.FirstError);
    }

    [Fact]
    public void Equals_WithSameName_ShouldReturnTrue()
    {
        // Arrange
        Genre firstGenre = _genreFixture.Create(name: "Science Fiction");
        Genre secondGenre = _genreFixture.Create(name: "Science Fiction");

        // Act
        bool result = firstGenre.Equals(secondGenre);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Equals_WithDifferentName_ShouldReturnFalse()
    {
        // Arrange
        Genre firstGenre = _genreFixture.Create(name: "Science Fiction");
        Genre secondGenre = _genreFixture.Create(name: "Fantasy");

        // Act
        bool result = firstGenre.Equals(secondGenre);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ToString_WhenCalled_ShouldReturnName()
    {
        // Arrange
        Genre genre = _genreFixture.Create(name: "Science Fiction");

        // Act
        string result = genre.ToString();

        // Assert
        Assert.Equal("Science Fiction", result);
    }
}
