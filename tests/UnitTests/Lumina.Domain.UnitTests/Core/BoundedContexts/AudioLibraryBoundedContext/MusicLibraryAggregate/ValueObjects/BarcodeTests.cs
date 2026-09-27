#region ========================================================================= USING =====================================================================================
using DomainErrors = Lumina.Domain.Common.Errors.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.UnitTests.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;

/// <summary>
/// Contains unit tests for the <see cref="Barcode"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class BarcodeTests
{
    private readonly BarcodeFixture _barcodeFixture = new();

    [Fact]
    public void Create_WhenCalledWithValidTwelveDigitBarcode_ShouldCreateBarcode()
    {
        // Act
        Result<Barcode> result = Barcode.Create("123456789012");

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal("123456789012", result.Value.Value);
    }

    [Fact]
    public void Create_WhenCalledWithValidThirteenDigitBarcode_ShouldCreateBarcode()
    {
        // Act
        Result<Barcode> result = Barcode.Create("1234567890123");

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal("1234567890123", result.Value.Value);
    }

    [Fact]
    public void Create_WhenValueHasSurroundingWhitespace_ShouldTrimIt()
    {
        // Act
        Result<Barcode> result = Barcode.Create("   123456789012   ");

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal("123456789012", result.Value.Value);
    }

    [Theory]
    [InlineData(null)] // null barcode value
    [InlineData("")] // empty barcode value
    [InlineData("   ")] // whitespace barcode value
    public void Create_WhenValueIsNullOrWhitespace_ShouldReturnBarcodeValueCannotBeEmptyError(string? value)
    {
        // Act
        Result<Barcode> result = Barcode.Create(value!);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Music.BarcodeValueCannotBeEmpty, result.FirstError);
    }

    [Theory]
    [InlineData("12345678901")] // eleven digits, one too few
    [InlineData("12345678901234")] // fourteen digits, one too many
    [InlineData("12345678901A")] // contains a letter
    [InlineData("1234-56789012")] // contains a separator
    [InlineData("1234567890.2")] // contains a decimal separator
    public void Create_WhenValueHasInvalidFormat_ShouldReturnInvalidFormatForBarcodeError(string value)
    {
        // Act
        Result<Barcode> result = Barcode.Create(value);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Music.InvalidFormatForBarcode, result.FirstError);
    }

    [Fact]
    public void Equals_WithSameValue_ShouldReturnTrue()
    {
        // Arrange
        Barcode firstBarcode = _barcodeFixture.Create(value: "123456789012");
        Barcode secondBarcode = _barcodeFixture.Create(value: "123456789012");

        // Act
        bool result = firstBarcode.Equals(secondBarcode);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Equals_WithDifferentValue_ShouldReturnFalse()
    {
        // Arrange
        Barcode firstBarcode = _barcodeFixture.Create(value: "123456789012");
        Barcode secondBarcode = _barcodeFixture.Create(value: "1234567890123");

        // Act
        bool result = firstBarcode.Equals(secondBarcode);

        // Assert
        Assert.False(result);
    }
}
