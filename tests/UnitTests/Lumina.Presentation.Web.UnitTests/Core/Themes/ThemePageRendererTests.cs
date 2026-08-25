#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.DTO.Themes;
using Lumina.Presentation.Web.Common.Primitives;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Themes;
using Lumina.Presentation.Web.Fixtures.Common.DTO.Themes;
using NSubstitute;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.UnitTests.Core.Themes;

/// <summary>
/// Contains unit tests for the <see cref="ThemePageRenderer"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ThemePageRendererTests
{
    private readonly IApiHttpClient _mockApiHttpClient;
    private readonly ThemePageRenderer _sut;
    private readonly ThemeResponseDtoFixture _themeResponseDtoFixture = new();
    private readonly ThemePageDtoFixture _themePageDtoFixture = new();
    private readonly ThemeTemplateResponseDtoFixture _themeTemplateResponseDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ThemePageRendererTests"/> class.
    /// </summary>
    public ThemePageRendererTests()
    {
        _mockApiHttpClient = Substitute.For<IApiHttpClient>();
        ThemeService themeService = new(_mockApiHttpClient);
        _sut = new ThemePageRenderer(themeService, new ThemeTemplateEngine());
    }

    [Fact]
    public async Task RenderAsync_WhenThemeIdProvided_ShouldRenderPageAndPopulateThemeMetadata()
    {
        // Arrange
        ThemeResponseDto theme = _themeResponseDtoFixture.Create(themeId: "editorial-paper");
        string pageKey = "home/index";
        string template = "<h1>{{title}}</h1>{{#scripts}}<script>init();</script>{{/scripts}}";
        string expectedEndpoint = ApiRoutes.Themes.GET_THEME_TEMPLATE
            .Replace("{themeId}", "editorial-paper")
            .Replace("{*pageKey}", pageKey);
        _mockApiHttpClient.GetAsync<ThemeTemplateResponseDto>(expectedEndpoint, Arg.Any<CancellationToken>())
            .Returns(_themeTemplateResponseDtoFixture.Create(theme: theme, template: template));
        ThemePageDto model = _themePageDtoFixture.Create(pageKey: pageKey, title: "Home");

        // Act
        Result<ThemePageRenderResultDto> result = await _sut.RenderAsync(model, "editorial-paper", CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("<h1>Home</h1>", result.Value.Content);
        Assert.Equal("<script>init();</script>", result.Value.Script);
        Assert.Equal("editorial-paper", model.ThemeId);
        Assert.Equal("/theme-assets/editorial-paper/assets", model.AssetBase);
        Assert.StartsWith("script_", model.ScriptId);
        await _mockApiHttpClient.DidNotReceive().GetAsync<ThemeResponseDto>(ApiRoutes.Themes.GET_CURRENT_THEME, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RenderAsync_WhenThemeIdMissing_ShouldResolveCurrentThemeBeforeRendering()
    {
        // Arrange
        ThemeResponseDto currentTheme = _themeResponseDtoFixture.Create(themeId: "lumina-default");
        string pageKey = "home/index";
        string expectedEndpoint = ApiRoutes.Themes.GET_THEME_TEMPLATE
            .Replace("{themeId}", "lumina-default")
            .Replace("{*pageKey}", pageKey);
        _mockApiHttpClient.GetAsync<ThemeResponseDto>(ApiRoutes.Themes.GET_CURRENT_THEME, Arg.Any<CancellationToken>())
            .Returns(currentTheme);
        _mockApiHttpClient.GetAsync<ThemeTemplateResponseDto>(expectedEndpoint, Arg.Any<CancellationToken>())
            .Returns(_themeTemplateResponseDtoFixture.Create(theme: currentTheme, template: "{{title}}"));
        ThemePageDto model = _themePageDtoFixture.Create(pageKey: pageKey, title: "Home");

        // Act
        Result<ThemePageRenderResultDto> result = await _sut.RenderAsync(model, null, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("lumina-default", model.ThemeId);
        Assert.Equal("Home", result.Value.Content);
        await _mockApiHttpClient.Received(1).GetAsync<ThemeResponseDto>(ApiRoutes.Themes.GET_CURRENT_THEME, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RenderAsync_WhenTemplateRenderingFails_ShouldReturnFailure()
    {
        // Arrange
        ThemeResponseDto theme = _themeResponseDtoFixture.Create(themeId: "editorial-paper");
        string pageKey = "home/index";
        string expectedEndpoint = ApiRoutes.Themes.GET_THEME_TEMPLATE
            .Replace("{themeId}", "editorial-paper")
            .Replace("{*pageKey}", pageKey);
        _mockApiHttpClient.GetAsync<ThemeTemplateResponseDto>(expectedEndpoint, Arg.Any<CancellationToken>())
            .Returns(_themeTemplateResponseDtoFixture.Create(theme: theme, template: "{{#unclosed}}"));
        ThemePageDto model = _themePageDtoFixture.Create(pageKey: pageKey, title: "Home");

        // Act
        Result<ThemePageRenderResultDto> result = await _sut.RenderAsync(model, "editorial-paper", CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Theme.Template.Invalid", result.FirstError.Code);
    }
}
