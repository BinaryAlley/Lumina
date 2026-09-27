#region ========================================================================= USING =====================================================================================
using DomainErrors = Lumina.Domain.Common.Errors.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.UnitTests.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;

/// <summary>
/// Contains unit tests for the <see cref="Isrc"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class IsrcTests
{
    private readonly IsrcFixture _isrcFixture = new();

    [Fact]
    public void Create_WhenCalledWithValidIsrc_ShouldCreateIsrc()
    {
        // Act
        Result<Isrc> result = Isrc.Create("USRC17607839");

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal("USRC17607839", result.Value.Value);
    }

    [Fact]
    public void Create_WhenValueHasLowercaseAndWhitespace_ShouldNormalizeToUppercaseTrimmed()
    {
        // Act
        Result<Isrc> result = Isrc.Create("  usrc17607839  ");

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal("USRC17607839", result.Value.Value);
    }

    [Theory]
    [InlineData(null)] // null ISRC value
    [InlineData("")] // empty ISRC value
    [InlineData("   ")] // whitespace ISRC value
    public void Create_WhenValueIsNullOrWhitespace_ShouldReturnError(string? value)
    {
        // Act
        Result<Isrc> result = Isrc.Create(value!);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Music.IsrcValueCannotBeEmpty, result.FirstError);
    }

    [Theory]
    [InlineData("USRC1760783")] // too short
    [InlineData("USRC176078399")] // too long
    [InlineData("1SRC17607839")] // country code does not start with two letters
    [InlineData("USRC1760783X")] // designation segment is not purely numeric
    [InlineData("USRC-7607839")] // contains an invalid character
    public void Create_WhenValueHasInvalidFormat_ShouldReturnError(string value)
    {
        // Act
        Result<Isrc> result = Isrc.Create(value);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Music.IsrcInvalidFormat, result.FirstError);
    }

    [Fact]
    public void Equals_WithSameValue_ShouldReturnTrue()
    {
        // Arrange
        Isrc firstIsrc = _isrcFixture.Create(value: "USRC17607839");
        Isrc secondIsrc = _isrcFixture.Create(value: "USRC17607839");

        // Act
        bool result = firstIsrc.Equals(secondIsrc);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Equals_WithDifferentValue_ShouldReturnFalse()
    {
        // Arrange
        Isrc firstIsrc = _isrcFixture.Create(value: "USRC17607839");
        Isrc secondIsrc = _isrcFixture.Create(value: "GBAYE0000001");

        // Act
        bool result = firstIsrc.Equals(secondIsrc);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Create_WhenRegistrantCodeContainsOnlyLetters_ShouldCreateIsrc()
    {
        // Act
        Result<Isrc> result = Isrc.Create("GBAAA0000001");

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal("GBAAA0000001", result.Value.Value);
    }

    [Fact]
    public void Equals_WhenValuesDifferOnlyByCase_ShouldNormalizeThemBeforeComparing()
    {
        // Arrange
        Isrc firstIsrc = _isrcFixture.Create(value: "USRC17607839");
        Isrc secondIsrc = _isrcFixture.Create(value: "usrc17607839");

        // Act
        bool result = firstIsrc.Equals(secondIsrc);

        // Assert
        Assert.True(result);
    }
}
