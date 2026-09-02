#region ========================================================================= USING =====================================================================================
using Lumina.Infrastructure.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Reading;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Infrastructure.UnitTests.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Reading;

/// <summary>
/// Contains unit tests for the <see cref="ReadingContentSanitizer"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ReadingContentSanitizerTests
{
    [Fact]
    public void Sanitize_WhenHtmlContainsScript_ShouldStripTheScript()
    {
        // Arrange
        string html = "<section><p>Hello</p><script>alert('xss')</script></section>";

        // Act
        string result = ReadingContentSanitizer.Sanitize(html, shouldPreserveStyles: false);

        // Assert
        Assert.DoesNotContain("script", result, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("alert", result, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Hello", result, StringComparison.Ordinal);
    }

    [Fact]
    public void Sanitize_WhenHtmlContainsInlineEventHandler_ShouldStripTheHandler()
    {
        // Arrange
        string html = "<p onclick=\"alert('xss')\">Hello</p>";

        // Act
        string result = ReadingContentSanitizer.Sanitize(html, shouldPreserveStyles: false);

        // Assert
        Assert.DoesNotContain("onclick", result, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Hello", result, StringComparison.Ordinal);
    }

    [Fact]
    public void Sanitize_WhenHtmlContainsJavascriptUrl_ShouldStripTheReference()
    {
        // Arrange
        string html = "<a href=\"javascript:alert('xss')\">Link</a>";

        // Act
        string result = ReadingContentSanitizer.Sanitize(html, shouldPreserveStyles: false);

        // Assert
        Assert.DoesNotContain("javascript", result, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Sanitize_WhenHtmlContainsIframe_ShouldStripTheIframe()
    {
        // Arrange
        string html = "<iframe src=\"https://evil.example\"></iframe><p>Hello</p>";

        // Act
        string result = ReadingContentSanitizer.Sanitize(html, shouldPreserveStyles: false);

        // Assert
        Assert.DoesNotContain("iframe", result, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Hello", result, StringComparison.Ordinal);
    }

    [Fact]
    public void Sanitize_WhenHtmlContainsDataLuminaResourceMarker_ShouldKeepTheMarker()
    {
        // Arrange
        string html = $"<img {ReadingContentSanitizer.RESOURCE_ATTRIBUTE}=\"page:1\" alt=\"Page\" />";

        // Act
        string result = ReadingContentSanitizer.Sanitize(html, shouldPreserveStyles: false);

        // Assert
        Assert.Contains(ReadingContentSanitizer.RESOURCE_ATTRIBUTE, result, StringComparison.Ordinal);
        Assert.Contains("page:1", result, StringComparison.Ordinal);
    }

    [Fact]
    public void Sanitize_WhenHtmlContainsPlainContent_ShouldKeepTheContent()
    {
        // Arrange
        string html = "<section><h1>Chapter 1</h1><p>Some text.</p></section>";

        // Act
        string result = ReadingContentSanitizer.Sanitize(html, shouldPreserveStyles: false);

        // Assert
        Assert.Contains("Chapter 1", result, StringComparison.Ordinal);
        Assert.Contains("Some text.", result, StringComparison.Ordinal);
    }

    [Fact]
    public void Sanitize_WhenShouldPreserveStylesIsTrue_ShouldKeepTheStyleAttribute()
    {
        // Arrange
        string html = "<p style=\"color:red\">Hello</p>";

        // Act
        string result = ReadingContentSanitizer.Sanitize(html, shouldPreserveStyles: true);

        // Assert
        Assert.Contains("style", result, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Hello", result, StringComparison.Ordinal);
    }

    [Fact]
    public void Sanitize_WhenShouldPreserveStylesIsFalse_ShouldStripTheStyleAttribute()
    {
        // Arrange
        string html = "<p style=\"color:red\">Hello</p>";

        // Act
        string result = ReadingContentSanitizer.Sanitize(html, shouldPreserveStyles: false);

        // Assert
        Assert.DoesNotContain("style", result, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("color:red", result, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Hello", result, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(true)] // preserve styles
    [InlineData(false)] // strip styles
    public void Sanitize_WhenHtmlContainsActiveContent_ShouldStripItRegardlessOfTheStylePreference(bool shouldPreserveStyles)
    {
        // Arrange
        string html = "<section><p style=\"color:red\">Hello</p><script>alert('xss')</script><p onclick=\"alert('xss')\">World</p></section>";

        // Act
        string result = ReadingContentSanitizer.Sanitize(html, shouldPreserveStyles);

        // Assert
        Assert.DoesNotContain("script", result, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("alert", result, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("onclick", result, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Hello", result, StringComparison.Ordinal);
        Assert.Contains("World", result, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(null)] // null input
    [InlineData("")] // empty input
    [InlineData("   ")] // whitespace input
    public void Sanitize_WhenInputIsBlank_ShouldReturnEmptyString(string? html)
    {
        // Act
        string result = ReadingContentSanitizer.Sanitize(html!, shouldPreserveStyles: false);

        // Assert
        Assert.Equal(string.Empty, result);
    }
}
