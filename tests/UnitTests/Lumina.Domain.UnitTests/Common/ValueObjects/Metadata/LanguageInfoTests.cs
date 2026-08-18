#region ========================================================================= USING =====================================================================================
using DomainErrors = Lumina.Domain.Common.Errors.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Common.ValueObjects.Metadata;
using Lumina.Domain.Fixtures.Common.ValueObjects.Metadata;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.UnitTests.Common.ValueObjects.Metadata;

/// <summary>
/// Contains unit tests for the <see cref="LanguageInfo"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class LanguageInfoTests
{
    private readonly LanguageInfoFixture _languageInfoFixture = new();

    [Fact]
    public void Create_WhenCalledWithValidValues_ShouldCreateLanguageInfoWithLowercaseCode()
    {
        // Act
        Result<LanguageInfo> result = LanguageInfo.Create("EN", "English", Optional<string>.Some("English"));

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal("en", result.Value.LanguageCode);
        Assert.Equal("English", result.Value.LanguageName);
        Assert.True(result.Value.NativeName.HasValue);
        Assert.Equal("English", result.Value.NativeName.Value);
    }

    [Theory]
    [InlineData(null)] // null language code
    [InlineData("")] // empty language code
    [InlineData("   ")] // whitespace language code
    public void Create_WhenLanguageCodeIsNullOrWhitespace_ShouldReturnError(string? languageCode)
    {
        // Act
        Result<LanguageInfo> result = LanguageInfo.Create(languageCode, "English", Optional<string>.None());

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Metadata.LanguageCodeCannotBeEmpty, result.FirstError);
    }

    [Theory]
    [InlineData(null)] // null language name
    [InlineData("")] // empty language name
    [InlineData("   ")] // whitespace language name
    public void Create_WhenLanguageNameIsNullOrWhitespace_ShouldReturnError(string? languageName)
    {
        // Act
        Result<LanguageInfo> result = LanguageInfo.Create("en", languageName, Optional<string>.None());

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Metadata.LanguageNameCannotBeEmpty, result.FirstError);
    }

    [Theory]
    [InlineData("e")] // single character code
    [InlineData("eng")] // three character code
    [InlineData("english")] // long code
    public void Create_WhenLanguageCodeIsNotTwoCharactersLong_ShouldReturnError(string languageCode)
    {
        // Act
        Result<LanguageInfo> result = LanguageInfo.Create(languageCode, "English", Optional<string>.None());

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Metadata.InvalidIsoCode, result.FirstError);
    }

    [Fact]
    public void ToString_WhenCalled_ShouldReturnFormattedString()
    {
        // Arrange
        LanguageInfo languageInfo = _languageInfoFixture.Create(
            languageCode: "en",
            languageName: "English",
            nativeName: Optional<string>.None());

        // Act
        string result = languageInfo.ToString();

        // Assert
        Assert.Equal("en - English", result);
    }
}
