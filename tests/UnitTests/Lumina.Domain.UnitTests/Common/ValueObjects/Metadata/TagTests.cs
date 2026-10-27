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
/// Contains unit tests for the <see cref="Tag"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class TagTests
{
    private readonly TagFixture _tagFixture = new();

    [Fact]
    public void Create_WhenCalledWithValidName_ShouldCreateTagWithTrimmedLowercaseInvariantName()
    {
        // Act
        Result<Tag> result = Tag.Create("  BeStSeLLeR  ");

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal("bestseller", result.Value.Name);
    }

    [Theory]
    [InlineData("  BeStSeLLeR  ", "bestseller")] // ASCII mixed case with surrounding whitespace
    [InlineData("ÉLECTRO", "électro")] // non-ASCII Latin uppercase
    [InlineData("ЖАНР", "жанр")] // non-Latin uppercase
    [InlineData("   indie   ", "indie")] // surrounding whitespace
    public void Create_WhenNameHasMixedCaseOrSurroundingWhitespace_ShouldNormalizeIt(string name, string expectedName)
    {
        // Act
        Result<Tag> result = Tag.Create(name);

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
            Result<Tag> result = Tag.Create("INDIE");

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
        Result<Tag> result = Tag.Create(name!);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Metadata.TagNameCannotBeEmpty, result.FirstError);
    }

    [Fact]
    public void Equals_WithSameName_ShouldReturnTrue()
    {
        // Arrange
        Tag firstTag = _tagFixture.Create(name: "bestseller");
        Tag secondTag = _tagFixture.Create(name: "bestseller");

        // Act
        bool result = firstTag.Equals(secondTag);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Equals_WithNamesDifferingOnlyByCasing_ShouldReturnTrue()
    {
        // Arrange
        Tag firstTag = _tagFixture.Create(name: "BestSeller");
        Tag secondTag = _tagFixture.Create(name: "bestseller");

        // Act
        bool result = firstTag.Equals(secondTag);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Equals_WithDifferentName_ShouldReturnFalse()
    {
        // Arrange
        Tag firstTag = _tagFixture.Create(name: "bestseller");
        Tag secondTag = _tagFixture.Create(name: "award-winning");

        // Act
        bool result = firstTag.Equals(secondTag);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ToString_WhenCalled_ShouldReturnName()
    {
        // Arrange
        Tag tag = _tagFixture.Create(name: "bestseller");

        // Act
        string result = tag.ToString();

        // Assert
        Assert.Equal("bestseller", result);
    }
}
