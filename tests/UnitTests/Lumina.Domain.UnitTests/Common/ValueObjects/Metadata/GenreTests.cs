#region ========================================================================= USING =====================================================================================
using DomainErrors = Lumina.Domain.Common.Errors.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Common.ValueObjects.Metadata;
using Lumina.Domain.Fixtures.Common.ValueObjects.Metadata;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
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
    public void Create_WhenCalledWithValidName_ShouldCreateGenreWithTrimmedLowercaseInvariantName()
    {
        // Act
        Result<Genre> result = Genre.Create("  Science FICTION  ");

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal("science fiction", result.Value.Name);
    }

    [Theory]
    [InlineData("  Science FICTION  ", "science fiction")] // ASCII mixed case with surrounding whitespace
    [InlineData("ÉLECTRO", "électro")] // non-ASCII Latin uppercase
    [InlineData("ЖАНР", "жанр")] // non-Latin uppercase
    [InlineData("   indie   ", "indie")] // surrounding whitespace
    public void Create_WhenNameHasMixedCaseOrSurroundingWhitespace_ShouldNormalizeIt(string name, string expectedName)
    {
        // Act
        Result<Genre> result = Genre.Create(name);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(expectedName, result.Value.Name);
    }

    [Fact]
    public void Create_WhenCurrentCultureIsTurkish_ShouldLowercaseWithTheInvariantCulture()
    {
        CultureInfo originalCulture = CultureInfo.CurrentCulture;
        CultureInfo originalUiCulture = CultureInfo.CurrentUICulture;
        try
        {
            // Arrange
            CultureInfo.CurrentCulture = new CultureInfo("tr-TR");
            CultureInfo.CurrentUICulture = new CultureInfo("tr-TR");

            // Act
            Result<Genre> result = Genre.Create("INDIE");

            // Assert
            Assert.False(result.IsFailure);
            // The Turkish culture maps an uppercase I to a dotless lowercase i, so this proves the invariant culture is used.
            Assert.Equal("indie", result.Value.Name);
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
            CultureInfo.CurrentUICulture = originalUiCulture;
        }
    }

    [Theory]
    [InlineData(null)] // null name
    [InlineData("")] // empty name
    [InlineData("   ")] // whitespace name
    public void Create_WhenNameIsNullOrWhitespace_ShouldReturnError(string? name)
    {
        // Act
        Result<Genre> result = Genre.Create(name!);

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
    public void Equals_WithNamesDifferingOnlyByCasing_ShouldReturnTrue()
    {
        // Arrange
        Genre firstGenre = _genreFixture.Create(name: "Science Fiction");
        Genre secondGenre = _genreFixture.Create(name: "science fiction");

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
        Assert.Equal("science fiction", result);
    }
}
