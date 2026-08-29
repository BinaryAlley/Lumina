#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.DTO.Themes;
using Lumina.Presentation.Web.Common.Primitives;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Themes;
using Lumina.Presentation.Web.Fixtures.Common.DTO.Themes;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.UnitTests.Core.Themes;

/// <summary>
/// Contains unit tests for the <see cref="ThemeFileSystemBrowserRenderer"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ThemeFileSystemBrowserRendererTests
{
    private readonly IApiHttpClient _mockApiHttpClient;
    private readonly IHttpContextAccessor _mockHttpContextAccessor;
    private readonly IStringLocalizerFactory _mockStringLocalizerFactory;
    private readonly IStringLocalizer _mockStringLocalizer;
    private readonly ThemeFileSystemBrowserRenderer _sut;
    private readonly ThemeResponseDtoFixture _themeResponseDtoFixture = new();
    private readonly ThemeTemplateResponseDtoFixture _themeTemplateResponseDtoFixture = new();
    private readonly ThemeFileSystemBrowserConfigurationDtoFixture _themeFileSystemBrowserConfigurationDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ThemeFileSystemBrowserRendererTests"/> class.
    /// </summary>
    public ThemeFileSystemBrowserRendererTests()
    {
        _mockApiHttpClient = Substitute.For<IApiHttpClient>();
        _mockHttpContextAccessor = Substitute.For<IHttpContextAccessor>();
        _mockStringLocalizer = CreateLocalizer();
        _mockStringLocalizerFactory = Substitute.For<IStringLocalizerFactory>();
        _mockStringLocalizerFactory.Create(Arg.Any<string>(), Arg.Any<string>()).Returns(_mockStringLocalizer);
        ThemeService themeService = new(_mockApiHttpClient);
        ThemeFileSystemBrowserBuilder builder = new(_mockStringLocalizerFactory, _mockHttpContextAccessor);
        _sut = new ThemeFileSystemBrowserRenderer(themeService, new ThemeTemplateEngine(), builder);
    }

    [Fact]
    public async Task RenderAsync_WhenShellTemplateRendered_ShouldReturnThemedContentWithSubTemplatesAndInitScript()
    {
        // Arrange
        ThemeResponseDto theme = _themeResponseDtoFixture.Create(themeId: "editorial");
        SetupRenderDocuments(theme,
            shellTemplate: "<div id=\"file-system-browser-dialog\">{{assetBase}}|{{iconBaseUrl}}|{{fileIconsUrl}}|{{strings.cancel}}</div><template id=\"fsb-template-tree-node\">{{{treeNodeTemplate}}}</template>",
            treeNodeTemplate: "<div class=\"tree-node\" data-path=\"{{path}}\">{{name}}</div>",
            explorerItemTemplate: "<div class=\"e\">{{name}}</div>",
            pathSegmentTemplate: "<li>{{path}}</li>");
        ThemeFileSystemBrowserConfigurationDto configuration = _themeFileSystemBrowserConfigurationDtoFixture.Create(path: "C:\\Users\\", viewMode: "list", iconSize: "large");

        // Act
        Result<string> result = await _sut.RenderAsync(configuration, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Contains("/theme-assets/editorial/assets", result.Value);
        Assert.Contains("/theme-assets/editorial/assets/images/icons", result.Value);
        Assert.Contains("data-path=\"{{path}}\"", result.Value);
        Assert.Contains("initFileSystemBrowser(", result.Value);
    }

    [Fact]
    public async Task RenderAsync_WhenSubTemplateIsMissing_ShouldRenderEmptySubTemplate()
    {
        // Arrange
        ThemeResponseDto theme = _themeResponseDtoFixture.Create(themeId: "editorial");
        SetupRenderDocuments(theme,
            shellTemplate: "<template id=\"fsb-template-tree-node\">{{{treeNodeTemplate}}}</template>",
            treeNodeTemplate: "not-returned",
            explorerItemTemplate: "not-returned",
            pathSegmentTemplate: "not-returned");
        StubSubTemplateFailure(theme.ThemeId, "shared/file-system-browser/tree-node");
        ThemeFileSystemBrowserConfigurationDto configuration = _themeFileSystemBrowserConfigurationDtoFixture.Create();

        // Act
        Result<string> result = await _sut.RenderAsync(configuration, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Contains("<template id=\"fsb-template-tree-node\"></template>", result.Value);
    }

    [Fact]
    public async Task RenderAsync_WhenShellDocumentCannotBeLoaded_ShouldReturnTemplateUnavailableError()
    {
        // Arrange
        _mockApiHttpClient.GetAsync<ThemeResponseDto>(ApiRoutes.Themes.GET_CURRENT_THEME, Arg.Any<CancellationToken>())
            .Returns(Task.FromException<ThemeResponseDto>(new HttpRequestException("The theme API is unavailable.")));
        ThemeFileSystemBrowserConfigurationDto configuration = _themeFileSystemBrowserConfigurationDtoFixture.Create();

        // Act
        Result<string> result = await _sut.RenderAsync(configuration, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Theme.Template.Unavailable", result.FirstError.Code);
    }

    [Fact]
    public async Task RenderAsync_WhenShellTemplateIsInvalid_ShouldReturnTemplateUnavailableError()
    {
        // Arrange
        ThemeResponseDto theme = _themeResponseDtoFixture.Create(themeId: "editorial");
        SetupRenderDocuments(theme,
            shellTemplate: "{{#unclosed}}",
            treeNodeTemplate: string.Empty,
            explorerItemTemplate: string.Empty,
            pathSegmentTemplate: string.Empty);
        ThemeFileSystemBrowserConfigurationDto configuration = _themeFileSystemBrowserConfigurationDtoFixture.Create();

        // Act
        Result<string> result = await _sut.RenderAsync(configuration, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Theme.Template.Invalid", result.FirstError.Code);
    }

    [Fact]
    public async Task RenderAsync_WhenCalled_ShouldFetchShellAndAllSubTemplatesFromActiveTheme()
    {
        // Arrange
        ThemeResponseDto theme = _themeResponseDtoFixture.Create(themeId: "editorial");
        SetupRenderDocuments(theme, "shell", "tree", "item", "segment");
        ThemeFileSystemBrowserConfigurationDto configuration = _themeFileSystemBrowserConfigurationDtoFixture.Create();

        // Act
        await _sut.RenderAsync(configuration, CancellationToken.None);

        // Assert
        string[] pageKeys =
        [
            "shared/file-system-browser",
            "shared/file-system-browser/tree-node",
            "shared/file-system-browser/explorer-item",
            "shared/file-system-browser/path-segment"
        ];
        foreach (string pageKey in pageKeys)
        {
            string endpoint = ApiRoutes.Themes.GET_THEME_TEMPLATE
                .Replace("{themeId}", "editorial")
                .Replace("{*pageKey}", pageKey);
            await _mockApiHttpClient.Received(1).GetAsync<ThemeTemplateResponseDto>(endpoint, Arg.Any<CancellationToken>());
        }
    }

    [Fact]
    public async Task RenderAsync_WhenCalled_ShouldEscapeInitialPathInInitializationScript()
    {
        // Arrange
        ThemeResponseDto theme = _themeResponseDtoFixture.Create(themeId: "editorial");
        SetupRenderDocuments(theme, "shell", "tree", "item", "segment");
        ThemeFileSystemBrowserConfigurationDto configuration = _themeFileSystemBrowserConfigurationDtoFixture.Create(path: "C:\\Users\\", viewMode: "list", iconSize: "large");

        // Act
        Result<string> result = await _sut.RenderAsync(configuration, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Contains(@"C:\\Users\\", result.Value);
    }

    /// <summary>
    /// Creates a substitute string localizer that returns a string prefixed with "Localized-".
    /// </summary>
    /// <returns>The created substitute string localizer.</returns>
    private static IStringLocalizer CreateLocalizer()
    {
        IStringLocalizer localizer = Substitute.For<IStringLocalizer>();
        localizer[Arg.Any<string>()].Returns(callInfo => new LocalizedString(callInfo.Arg<string>(), $"Localized-{callInfo.Arg<string>()}"));
        return localizer;
    }

    /// <summary>
    /// Stubs the API responses that return the component shell and sub templates of the given theme, together with the current theme.
    /// </summary>
    /// <param name="theme">The theme whose templates are stubbed.</param>
    /// <param name="shellTemplate">The template returned for the component shell document.</param>
    /// <param name="treeNodeTemplate">The template returned for the tree node document.</param>
    /// <param name="explorerItemTemplate">The template returned for the explorer item document.</param>
    /// <param name="pathSegmentTemplate">The template returned for the path segment document.</param>
    private void SetupRenderDocuments(ThemeResponseDto theme, string shellTemplate, string treeNodeTemplate, string explorerItemTemplate, string pathSegmentTemplate)
    {
        _mockApiHttpClient.GetAsync<ThemeResponseDto>(ApiRoutes.Themes.GET_CURRENT_THEME, Arg.Any<CancellationToken>())
            .Returns(theme);
        StubTemplateResponse(theme.ThemeId, "shared/file-system-browser", shellTemplate);
        StubTemplateResponse(theme.ThemeId, "shared/file-system-browser/tree-node", treeNodeTemplate);
        StubTemplateResponse(theme.ThemeId, "shared/file-system-browser/explorer-item", explorerItemTemplate);
        StubTemplateResponse(theme.ThemeId, "shared/file-system-browser/path-segment", pathSegmentTemplate);
    }

    /// <summary>
    /// Stubs the API response that returns a single theme template document.
    /// </summary>
    /// <param name="themeId">The manifest id of the theme.</param>
    /// <param name="pageKey">The page key of the template.</param>
    /// <param name="template">The template returned for the document.</param>
    private void StubTemplateResponse(string themeId, string pageKey, string template)
    {
        string endpoint = ApiRoutes.Themes.GET_THEME_TEMPLATE
            .Replace("{themeId}", themeId)
            .Replace("{*pageKey}", pageKey);
        _mockApiHttpClient.GetAsync<ThemeTemplateResponseDto>(endpoint, Arg.Any<CancellationToken>())
            .Returns(_themeTemplateResponseDtoFixture.Create(theme: _themeResponseDtoFixture.Create(themeId: themeId), template: template));
    }

    /// <summary>
    /// Stubs the API response of a theme template document to fail, as when the theme does not provide the template.
    /// </summary>
    /// <param name="themeId">The manifest id of the theme.</param>
    /// <param name="pageKey">The page key of the template.</param>
    private void StubSubTemplateFailure(string themeId, string pageKey)
    {
        string endpoint = ApiRoutes.Themes.GET_THEME_TEMPLATE
            .Replace("{themeId}", themeId)
            .Replace("{*pageKey}", pageKey);
        _mockApiHttpClient.GetAsync<ThemeTemplateResponseDto>(endpoint, Arg.Any<CancellationToken>())
            .Returns(Task.FromException<ThemeTemplateResponseDto>(new HttpRequestException("The sub template is unavailable.")));
    }
}
