#region ========================================================================= USING =====================================================================================
using DomainErrors = Lumina.Domain.Common.Errors.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.BookLibrary;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.UnitTests.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate.ValueObjects;

/// <summary>
/// Contains unit tests for the <see cref="Isbn"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class IsbnTests
{
    private readonly IsbnFixture _isbnFixture = new();

    [Fact]
    public void Create_WhenCalledWithValidIsbn10_ShouldCreateIsbn10()
    {
        // Act
        Result<Isbn> result = Isbn.Create("0-306-40615-2", IsbnFormat.Isbn10);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal("0-306-40615-2", result.Value.Value);
        Assert.Equal(IsbnFormat.Isbn10, result.Value.Format);
    }

    [Fact]
    public void Create_WhenCalledWithValidIsbn13_ShouldCreateIsbn13()
    {
        // Act
        Result<Isbn> result = Isbn.Create("978-0-306-40615-7", IsbnFormat.Isbn13);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal("978-0-306-40615-7", result.Value.Value);
        Assert.Equal(IsbnFormat.Isbn13, result.Value.Format);
    }

    [Theory]
    [InlineData(null)] // null ISBN value
    [InlineData("")] // empty ISBN value
    [InlineData("   ")] // whitespace ISBN value
    public void Create_WhenValueIsNullOrWhitespace_ShouldReturnError(string? value)
    {
        // Act
        Result<Isbn> result = Isbn.Create(value, IsbnFormat.Isbn10);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.WrittenContent.IsbnValueCannotBeEmpty, result.FirstError);
    }

    [Fact]
    public void Create_WhenFormatIsUnknown_ShouldReturnError()
    {
        // Act
        Result<Isbn> result = Isbn.Create("0-306-40615-2", (IsbnFormat)999);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.WrittenContent.UnknownIsbnFormat, result.FirstError);
    }

    [Fact]
    public void Create_WhenIsbn10HasInvalidCheckDigit_ShouldReturnError()
    {
        // Act
        Result<Isbn> result = Isbn.Create("0-306-40615-3", IsbnFormat.Isbn10);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.WrittenContent.InvalidIsbn10Format, result.FirstError);
    }

    [Fact]
    public void Create_WhenIsbn13HasInvalidCheckDigit_ShouldReturnError()
    {
        // Act
        Result<Isbn> result = Isbn.Create("978-0-306-40615-8", IsbnFormat.Isbn13);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.WrittenContent.InvalidIsbn13Format, result.FirstError);
    }

    [Fact]
    public void Create_WhenIsbn10HasWrongLength_ShouldReturnError()
    {
        // Act
        Result<Isbn> result = Isbn.Create("123456789", IsbnFormat.Isbn10);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.WrittenContent.InvalidIsbn10Format, result.FirstError);
    }

    [Theory]
    [InlineData("0-306-40615-2")] // valid ISBN-10 with hyphens
    [InlineData("0306406152")] // valid ISBN-10 without hyphens
    public void IsValidIsbn10_WhenValueIsValid_ShouldReturnTrue(string isbn)
    {
        // Act
        bool result = Isbn.IsValidIsbn10(isbn);

        // Assert
        Assert.True(result);
    }

    [Theory]
    [InlineData("0-306-40615-3")] // invalid check digit
    [InlineData("030640615")] // too short
    [InlineData("03064061523")] // too long
    [InlineData("")] // empty value
    [InlineData("ISBN 0-306-40615-2")] // ISBN prefix is not stripped by the checksum computation
    public void IsValidIsbn10_WhenValueIsInvalid_ShouldReturnFalse(string isbn)
    {
        // Act
        bool result = Isbn.IsValidIsbn10(isbn);

        // Assert
        Assert.False(result);
    }

    [Theory]
    [InlineData("978-0-306-40615-7")] // valid ISBN-13 with hyphens
    [InlineData("9780306406157")] // valid ISBN-13 without hyphens
    [InlineData("979-0-306-40615-6")] // valid ISBN-13 with 979 prefix
    public void IsValidIsbn13_WhenValueIsValid_ShouldReturnTrue(string isbn)
    {
        // Act
        bool result = Isbn.IsValidIsbn13(isbn);

        // Assert
        Assert.True(result);
    }

    [Theory]
    [InlineData("978-0-306-40615-8")] // invalid check digit
    [InlineData("978030640615")] // too short
    [InlineData("97803064061578")] // too long
    [InlineData("")] // empty value
    [InlineData("ISBN 978-0-306-40615-7")] // ISBN prefix is not stripped by the checksum computation
    public void IsValidIsbn13_WhenValueIsInvalid_ShouldReturnFalse(string isbn)
    {
        // Act
        bool result = Isbn.IsValidIsbn13(isbn);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Equals_WithSameValueAndFormat_ShouldReturnTrue()
    {
        // Arrange
        Isbn firstIsbn = _isbnFixture.Create(value: "0-306-40615-2", format: IsbnFormat.Isbn10);
        Isbn secondIsbn = _isbnFixture.Create(value: "0-306-40615-2", format: IsbnFormat.Isbn10);

        // Act
        bool result = firstIsbn.Equals(secondIsbn);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Equals_WithDifferentValue_ShouldReturnFalse()
    {
        // Arrange
        Isbn firstIsbn = _isbnFixture.Create();
        Isbn secondIsbn = _isbnFixture.Create();

        // Act
        bool result = firstIsbn.Equals(secondIsbn);

        // Assert
        Assert.False(result);
    }
}
