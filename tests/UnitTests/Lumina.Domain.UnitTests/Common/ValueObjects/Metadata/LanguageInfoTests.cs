#region ========================================================================= USING =====================================================================================
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
        LanguageInfo result = LanguageInfo.Create("EN", "English", Optional<string>.Some("English"));

        // Assert
        Assert.Equal("en", result.LanguageCode);
        Assert.Equal("English", result.LanguageName);
        Assert.True(result.NativeName.HasValue);
        Assert.Equal("English", result.NativeName.Value);
    }

    [Fact]
    public void Create_WhenNativeNameIsMissing_ShouldCreateLanguageInfoWithoutNativeName()
    {
        // Act
        LanguageInfo result = LanguageInfo.Create("en", "English", Optional<string>.None());

        // Assert
        Assert.Equal("en", result.LanguageCode);
        Assert.Equal("English", result.LanguageName);
        Assert.False(result.NativeName.HasValue);
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

    [Fact]
    public void Equals_WithSameValues_ShouldReturnTrue()
    {
        // Arrange
        LanguageInfo firstLanguageInfo = _languageInfoFixture.Create(
            languageCode: "en",
            languageName: "English",
            nativeName: Optional<string>.Some("English"));
        LanguageInfo secondLanguageInfo = _languageInfoFixture.Create(
            languageCode: "en",
            languageName: "English",
            nativeName: Optional<string>.Some("English"));

        // Act
        bool result = firstLanguageInfo.Equals(secondLanguageInfo);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void GetHashCode_WithSameValues_ShouldReturnSameHashCode()
    {
        // Arrange
        LanguageInfo firstLanguageInfo = _languageInfoFixture.Create(
            languageCode: "en",
            languageName: "English",
            nativeName: Optional<string>.Some("English"));
        LanguageInfo secondLanguageInfo = _languageInfoFixture.Create(
            languageCode: "en",
            languageName: "English",
            nativeName: Optional<string>.Some("English"));

        // Act
        int firstHashCode = firstLanguageInfo.GetHashCode();
        int secondHashCode = secondLanguageInfo.GetHashCode();

        // Assert
        Assert.Equal(firstHashCode, secondHashCode);
    }

    [Fact]
    public void Equals_WithDifferentLanguageName_ShouldReturnFalse()
    {
        // Arrange
        LanguageInfo firstLanguageInfo = _languageInfoFixture.Create(
            languageCode: "en",
            languageName: "English",
            nativeName: Optional<string>.None());
        LanguageInfo secondLanguageInfo = _languageInfoFixture.Create(
            languageCode: "en",
            languageName: "French",
            nativeName: Optional<string>.None());

        // Act
        bool result = firstLanguageInfo.Equals(secondLanguageInfo);

        // Assert
        Assert.False(result);
    }
}
