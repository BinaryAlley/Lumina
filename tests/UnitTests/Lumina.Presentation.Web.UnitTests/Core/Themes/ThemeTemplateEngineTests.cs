#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.Themes;
using Lumina.Presentation.Web.Common.Primitives;
using Lumina.Presentation.Web.Core.Themes;
using Lumina.Presentation.Web.Fixtures.Common.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.UnitTests.Core.Themes;

/// <summary>
/// Contains unit tests for the <see cref="ThemeTemplateEngine"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ThemeTemplateEngineTests
{
    private readonly ThemeTemplateEngine _sut;
    private readonly SectionModelFixture _sectionModelFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ThemeTemplateEngineTests"/> class.
    /// </summary>
    public ThemeTemplateEngineTests()
    {
        _sut = new ThemeTemplateEngine();
    }

    [Fact]
    public void RenderPage_WhenVariableValueContainsHtml_ShouldEscapeOutput()
    {
        // Arrange
        Dictionary<string, object?> model = new() { ["value"] = "<b>&\"quote\"" };
        string template = "{{value}}";

        // Act
        Result<ThemePageRenderResultDto> result = _sut.RenderPage(template, model);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("&lt;b&gt;&amp;&quot;quote&quot;", result.Value.Content);
        Assert.Equal(string.Empty, result.Value.Script);
    }

    [Fact]
    public void RenderPage_WhenRawVariableTagUsed_ShouldNotEscapeOutput()
    {
        // Arrange
        Dictionary<string, object?> model = new() { ["value"] = "<b>raw</b>" };

        // Act
        Result<ThemePageRenderResultDto> ampersandResult = _sut.RenderPage("{{&value}}", model);
        Result<ThemePageRenderResultDto> tripleResult = _sut.RenderPage("{{{value}}}", model);

        // Assert
        Assert.True(ampersandResult.IsSuccess);
        Assert.Equal("<b>raw</b>", ampersandResult.Value.Content);
        Assert.True(tripleResult.IsSuccess);
        Assert.Equal("<b>raw</b>", tripleResult.Value.Content);
    }

    [Fact]
    public void RenderPage_WhenVariableMissing_ShouldRenderEmptyString()
    {
        // Arrange
        Dictionary<string, object?> model = new() { ["title"] = "Present" };

        // Act
        Result<ThemePageRenderResultDto> result = _sut.RenderPage("{{title}}|{{missing}}", model);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Present|", result.Value.Content);
    }

    [Fact]
    public void RenderPage_WhenVariableIsBoolean_ShouldRenderLowercaseLiteral()
    {
        // Arrange
        Dictionary<string, object?> model = new() { ["enabled"] = true, ["disabled"] = false };

        // Act
        Result<ThemePageRenderResultDto> result = _sut.RenderPage("{{enabled}}/{{disabled}}", model);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("true/false", result.Value.Content);
    }

    [Fact]
    public void RenderPage_WhenDottedExpressionUsed_ShouldResolveNestedProperty()
    {
        // Arrange
        Dictionary<string, object?> model = new()
        {
            ["user"] = new Dictionary<string, object?> { ["name"] = "Ada" }
        };

        // Act
        Result<ThemePageRenderResultDto> result = _sut.RenderPage("Hello {{user.name}}", model);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Hello Ada", result.Value.Content);
    }

    [Fact]
    public void RenderPage_WhenCollectionCountRequested_ShouldRenderCollectionCount()
    {
        // Arrange
        Dictionary<string, object?> model = new() { ["items"] = new List<object> { "a", "b", "c" } };

        // Act
        Result<ThemePageRenderResultDto> result = _sut.RenderPage("{{items.Count}}", model);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("3", result.Value.Content);
    }

    [Fact]
    public void RenderPage_WhenSectionIsTruthy_ShouldRenderChildren()
    {
        // Arrange
        Dictionary<string, object?> model = new() { ["section"] = true, ["text"] = "inside" };

        // Act
        Result<ThemePageRenderResultDto> result = _sut.RenderPage("{{#section}}{{text}}{{/section}}", model);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("inside", result.Value.Content);
    }

    [Fact]
    public void RenderPage_WhenSectionIsObject_ShouldRenderChildrenWithObjectScope()
    {
        // Arrange
        Dictionary<string, object?> model = new()
        {
            ["section"] = _sectionModelFixture.Create(text: "scoped")
        };

        // Act
        Result<ThemePageRenderResultDto> result = _sut.RenderPage("{{#section}}{{text}}{{/section}}", model);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("scoped", result.Value.Content);
    }

    [Theory]
    [InlineData(null)] // unresolved section
    [InlineData(false)] // boolean false
    [InlineData("")] // empty string
    public void RenderPage_WhenSectionIsFalsy_ShouldSkipChildren(object? sectionValue)
    {
        // Arrange
        Dictionary<string, object?> model = new() { ["section"] = sectionValue, ["text"] = "hidden" };

        // Act
        Result<ThemePageRenderResultDto> result = _sut.RenderPage("before{{#section}}{{text}}{{/section}}after", model);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("beforeafter", result.Value.Content);
    }

    [Fact]
    public void RenderPage_WhenSectionIsEmptyCollection_ShouldSkipChildren()
    {
        // Arrange
        Dictionary<string, object?> model = new() { ["section"] = new List<object>() };

        // Act
        Result<ThemePageRenderResultDto> result = _sut.RenderPage("a{{#section}}x{{/section}}b", model);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("ab", result.Value.Content);
    }

    [Fact]
    public void RenderPage_WhenSectionIsCollection_ShouldRenderChildrenPerItem()
    {
        // Arrange
        Dictionary<string, object?> model = new()
        {
            ["items"] = new List<object>
            {
                new Dictionary<string, object?> { ["name"] = "A" },
                new Dictionary<string, object?> { ["name"] = "B" }
            }
        };

        // Act
        Result<ThemePageRenderResultDto> result = _sut.RenderPage("{{#items}}[{{name}}]{{/items}}", model);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("[A][B]", result.Value.Content);
    }

    [Fact]
    public void RenderPage_WhenSectionIteratesScalarItems_ShouldRenderCurrentScopeValue()
    {
        // Arrange
        Dictionary<string, object?> model = new() { ["items"] = new List<object> { "one", "two" } };

        // Act
        Result<ThemePageRenderResultDto> result = _sut.RenderPage("{{#items}}{{.}};{{/items}}", model);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("one;two;", result.Value.Content);
    }

    [Fact]
    public void RenderPage_WhenParentTraversalUsed_ShouldResolveFromParentScope()
    {
        // Arrange
        Dictionary<string, object?> model = new()
        {
            ["title"] = "Parent",
            ["items"] = new List<object>
            {
                new Dictionary<string, object?> { ["name"] = "A" },
                new Dictionary<string, object?> { ["name"] = "B" }
            }
        };

        // Act
        Result<ThemePageRenderResultDto> result = _sut.RenderPage("{{#items}}{{../title}}/{{name}};{{/items}}", model);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Parent/A;Parent/B;", result.Value.Content);
    }

    [Fact]
    public void RenderPage_WhenInvertedSectionIsFalsy_ShouldRenderChildren()
    {
        // Arrange
        Dictionary<string, object?> model = new() { ["section"] = null };

        // Act
        Result<ThemePageRenderResultDto> result = _sut.RenderPage("{{^section}}fallback{{/section}}", model);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("fallback", result.Value.Content);
    }

    [Fact]
    public void RenderPage_WhenInvertedSectionIsTruthy_ShouldSkipChildren()
    {
        // Arrange
        Dictionary<string, object?> model = new() { ["section"] = "present" };

        // Act
        Result<ThemePageRenderResultDto> result = _sut.RenderPage("a{{^section}}fallback{{/section}}b", model);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("ab", result.Value.Content);
    }

    [Fact]
    public void RenderPage_WhenTopLevelScriptsSectionPresent_ShouldSeparateScriptFromContent()
    {
        // Arrange
        Dictionary<string, object?> model = new() { ["title"] = "Page", ["script"] = "console.log('x');" };

        // Act
        Result<ThemePageRenderResultDto> result = _sut.RenderPage(
            "<h1>{{title}}</h1>{{#scripts}}<script>{{{script}}}</script>{{/scripts}}",
            model);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("<h1>Page</h1>", result.Value.Content);
        Assert.Equal("<script>console.log('x');</script>", result.Value.Script);
    }

    [Fact]
    public void RenderPage_WhenScriptsSectionIsNamedDifferentlyInCase_ShouldStillSplitIt()
    {
        // Arrange
        Dictionary<string, object?> model = new() { ["title"] = "Page" };

        // Act
        Result<ThemePageRenderResultDto> result = _sut.RenderPage(
            "content{{#SCRIPTS}}script{{/SCRIPTS}}",
            model);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("content", result.Value.Content);
        Assert.Equal("script", result.Value.Script);
    }

    [Fact]
    public void RenderPage_WhenScriptsSectionIsNestedInsideAnotherSection_ShouldKeepItInContent()
    {
        // Arrange
        Dictionary<string, object?> model = new()
        {
            ["items"] = new List<object>
            {
                new Dictionary<string, object?> { ["name"] = "A", ["scripts"] = true, ["script"] = "x" }
            }
        };

        // Act
        Result<ThemePageRenderResultDto> result = _sut.RenderPage(
            "{{#items}}{{name}}{{#scripts}}{{script}}{{/scripts}}{{/items}}",
            model);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Ax", result.Value.Content);
    }

    [Fact]
    public void RenderPage_WhenTemplateContainsOnlyComments_ShouldReturnEmptyContent()
    {
        // Arrange
        Dictionary<string, object?> model = new() { ["title"] = "Page" };

        // Act
        Result<ThemePageRenderResultDto> result = _sut.RenderPage("{{! this is a comment }}", model);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(string.Empty, result.Value.Content);
        Assert.Equal(string.Empty, result.Value.Script);
    }

    [Fact]
    public void RenderPage_WhenSectionIsNotClosed_ShouldReturnTemplateError()
    {
        // Arrange
        Dictionary<string, object?> model = new() { ["section"] = true };

        // Act
        Result<ThemePageRenderResultDto> result = _sut.RenderPage("{{#section}}content", model);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Theme.Template.Invalid", result.FirstError.Code);
        Assert.Contains("not closed", result.FirstError.Description);
    }

    [Fact]
    public void RenderPage_WhenClosingTagHasNoOpeningSection_ShouldReturnTemplateError()
    {
        // Act
        Result<ThemePageRenderResultDto> result = _sut.RenderPage("{{/section}}", new Dictionary<string, object?>());

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Theme.Template.Invalid", result.FirstError.Code);
        Assert.Contains("no matching opening section", result.FirstError.Description);
    }

    [Fact]
    public void RenderPage_WhenClosingTagMismatchesOpeningSection_ShouldReturnTemplateError()
    {
        // Act
        Result<ThemePageRenderResultDto> result = _sut.RenderPage("{{#first}}x{{/second}}", new Dictionary<string, object?>());

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Theme.Template.Invalid", result.FirstError.Code);
        Assert.Contains("closed by", result.FirstError.Description);
    }

    [Fact]
    public void RenderPage_WhenEmptyTagUsed_ShouldReturnTemplateError()
    {
        // Act
        Result<ThemePageRenderResultDto> result = _sut.RenderPage("before{{}}after", new Dictionary<string, object?>());

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Theme.Template.Invalid", result.FirstError.Code);
        Assert.Contains("Empty template tags", result.FirstError.Description);
    }

    [Fact]
    public void RenderPage_WhenTagIsNotClosed_ShouldReturnTemplateError()
    {
        // Act
        Result<ThemePageRenderResultDto> result = _sut.RenderPage("before{{value", new Dictionary<string, object?>());

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Theme.Template.Invalid", result.FirstError.Code);
        Assert.Contains("not closed", result.FirstError.Description);
    }

    [Fact]
    public void RenderPage_WhenRawTagIsNotClosed_ShouldReturnTemplateError()
    {
        // Act
        Result<ThemePageRenderResultDto> result = _sut.RenderPage("before{{{value", new Dictionary<string, object?>());

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Theme.Template.Invalid", result.FirstError.Code);
        Assert.Contains("not closed", result.FirstError.Description);
    }

    [Fact]
    public void RenderPage_WhenExpressionIsTooLong_ShouldReturnTemplateError()
    {
        // Arrange
        string expression = new('a', 121);
        string template = $"{{{{{expression}}}}}";

        // Act
        Result<ThemePageRenderResultDto> result = _sut.RenderPage(template, new Dictionary<string, object?>());

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Theme.Template.Invalid", result.FirstError.Code);
        Assert.Contains("too long", result.FirstError.Description);
    }

    [Theory]
    [InlineData("{{bad-name}}")] // hyphen is not a valid name part character
    [InlineData("{{two words}}")] // whitespace inside the expression
    [InlineData("{{a..b}}")] // empty name part
    public void RenderPage_WhenExpressionIsInvalid_ShouldReturnTemplateError(string template)
    {
        // Act
        Result<ThemePageRenderResultDto> result = _sut.RenderPage(template, new Dictionary<string, object?>());

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Theme.Template.Invalid", result.FirstError.Code);
        Assert.Contains("is invalid", result.FirstError.Description);
    }

    [Fact]
    public void RenderPage_WhenSectionsAreNestedDeeperThanTheLimit_ShouldReturnTemplateError()
    {
        // Arrange
        string template = string.Concat(Enumerable.Repeat("{{#section}}", 33))
            + string.Concat(Enumerable.Repeat("{{/section}}", 33));

        // Act
        Result<ThemePageRenderResultDto> result = _sut.RenderPage(template, new Dictionary<string, object?>());

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Theme.Template.Invalid", result.FirstError.Code);
        Assert.Contains("nested more than", result.FirstError.Description);
    }

    [Fact]
    public void RenderPage_WhenRenderedOutputExceedsTheSafetyLimit_ShouldReturnTemplateError()
    {
        // Arrange
        int limit = 4 * 1024 * 1024;
        Dictionary<string, object?> model = new() { ["text"] = new string('a', limit + 1) };

        // Act
        Result<ThemePageRenderResultDto> result = _sut.RenderPage("{{text}}", model);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Theme.Template.Invalid", result.FirstError.Code);
        Assert.Contains("safety limit", result.FirstError.Description);
    }
}
