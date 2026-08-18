#region ========================================================================= USING =====================================================================================
using DomainErrors = Lumina.Domain.Common.Errors.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Common.ValueObjects.Metadata;
using Lumina.Domain.Fixtures.Common.ValueObjects.Metadata;
using System.Diagnostics.CodeAnalysis;
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
    public void Create_WhenCalledWithValidName_ShouldCreateTagWithTrimmedName()
    {
        // Act
        Result<Tag> result = Tag.Create("  bestseller  ");

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal("bestseller", result.Value.Name);
    }

    [Theory]
    [InlineData(null)] // null name
    [InlineData("")] // empty name
    [InlineData("   ")] // whitespace name
    public void Create_WhenNameIsNullOrWhitespace_ShouldReturnError(string? name)
    {
        // Act
        Result<Tag> result = Tag.Create(name);

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
