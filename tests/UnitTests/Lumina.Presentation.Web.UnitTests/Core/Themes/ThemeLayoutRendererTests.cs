#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.Authorization;
using Lumina.Presentation.Web.Common.DTO.Themes;
using Lumina.Presentation.Web.Common.Enums.Authorization;
using Lumina.Presentation.Web.Common.Primitives;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Common.Services;
using Lumina.Presentation.Web.Core.Themes;
using Lumina.Presentation.Web.Fixtures.Common.DTO.Themes;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Localization;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.UnitTests.Core.Themes;

/// <summary>
/// Contains unit tests for the <see cref="ThemeLayoutRenderer"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ThemeLayoutRendererTests
{
    private readonly IApiHttpClient _mockApiHttpClient;
    private readonly ICompositeViewEngine _mockViewEngine;
    private readonly ITempDataProvider _mockTempDataProvider;
    private readonly IServiceProvider _mockServiceProvider;
    private readonly IHttpContextAccessor _mockHttpContextAccessor;
    private readonly IUrlService _mockUrlService;
    private readonly IStringLocalizerFactory _mockStringLocalizerFactory;
    private readonly IStringLocalizer _mockStringLocalizer;
    private readonly IAuthorizationService _mockAuthorizationService;
    private readonly IView _mockHeadView;
    private readonly IView _mockAudioPlayerView;
    private readonly IView _mockScriptsView;
    private readonly IView _mockNavMenuView;
    private readonly ThemeLayoutRenderer _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="ThemeLayoutRendererTests"/> class.
    /// </summary>
    public ThemeLayoutRendererTests()
    {
        _mockApiHttpClient = Substitute.For<IApiHttpClient>();
        _mockViewEngine = Substitute.For<ICompositeViewEngine>();
        _mockTempDataProvider = Substitute.For<ITempDataProvider>();
        _mockTempDataProvider.LoadTempData(Arg.Any<HttpContext>()).Returns(new Dictionary<string, object>());
        _mockServiceProvider = Substitute.For<IServiceProvider>();
        _mockServiceProvider.GetService(typeof(IModelMetadataProvider)).Returns(new EmptyModelMetadataProvider());
        _mockHttpContextAccessor = Substitute.For<IHttpContextAccessor>();

        _mockUrlService = Substitute.For<IUrlService>();
        _mockUrlService.GetAbsoluteUrl(Arg.Any<string>(), Arg.Any<object?>()).Returns("http://localhost/en-us");
        _mockStringLocalizer = CreateLocalizer();
        _mockStringLocalizerFactory = Substitute.For<IStringLocalizerFactory>();
        _mockStringLocalizerFactory.Create(Arg.Any<string>(), Arg.Any<string>()).Returns(_mockStringLocalizer);
        _mockAuthorizationService = Substitute.For<IAuthorizationService>();
        _mockAuthorizationService.IsInRoleAsync("Admin", Arg.Any<CancellationToken>()).Returns(false);
        _mockAuthorizationService.HasPermissionAsync(Arg.Any<AuthorizationPermission>(), Arg.Any<CancellationToken>()).Returns(false);

        _mockHeadView = CreateView("<head-fragment>");
        _mockAudioPlayerView = CreateView("<audio-fragment>");
        _mockScriptsView = CreateView("<scripts-fragment>");
        _mockNavMenuView = CreateView("<nav-menu-fragment>");
        _mockViewEngine.FindView(Arg.Any<ActionContext>(), "_ThemeLayoutHead", false).Returns(ViewEngineResult.Found("_ThemeLayoutHead", _mockHeadView));
        _mockViewEngine.FindView(Arg.Any<ActionContext>(), "_AudioPlayer", false).Returns(ViewEngineResult.Found("_AudioPlayer", _mockAudioPlayerView));
        _mockViewEngine.FindView(Arg.Any<ActionContext>(), "_ThemeLayoutScripts", false).Returns(ViewEngineResult.Found("_ThemeLayoutScripts", _mockScriptsView));
        _mockViewEngine.FindView(Arg.Any<ActionContext>(), "_NavMenu", false).Returns(ViewEngineResult.Found("_NavMenu", _mockNavMenuView));

        RazorViewToStringRenderer viewRenderer = new(_mockViewEngine, _mockTempDataProvider, _mockServiceProvider, _mockHttpContextAccessor);
        ThemeNavBuilder navBuilder = new(_mockUrlService, _mockStringLocalizerFactory, _mockAuthorizationService, _mockHttpContextAccessor);
        ThemeService themeService = new(_mockApiHttpClient);
        _sut = new ThemeLayoutRenderer(themeService, new ThemeTemplateEngine(), viewRenderer, navBuilder, _mockHttpContextAccessor);
    }

    [Fact]
    public async Task RenderAsync_WhenUserIsAuthenticated_ShouldRenderLayoutWithAllPlaceholders()
    {
        // Arrange
        _mockHttpContextAccessor.HttpContext.Returns(TestHttpContextFactory.Create(user: TestHttpContextFactory.CreateAuthenticatedUser()));
        ThemeResponseDto theme = new ThemeResponseDtoFixture().Create(themeId: "editorial");
        SetupRenderDocuments(theme, layoutTemplate: "{{title}}|{{assetBase}}|{{{appHead}}}|{{{nav}}}|{{{content}}}|{{{audioPlayer}}}|{{{appScripts}}}|{{{scripts}}}|{{mainStyle}}", navTemplate: "nav:{{siteName}}");
        ThemeLayoutPageDto page = new("My Page", "<main>", "<page-script>");

        // Act
        Result<string> result = await _sut.RenderAsync(page, "editorial", CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("My Page|/theme-assets/editorial/assets|<head-fragment>|nav:Lumina|<main>|<audio-fragment>|<scripts-fragment>|<page-script>|", result.Value);
    }

    [Fact]
    public async Task RenderAsync_WhenUserIsAnonymous_ShouldRenderEmptyAudioPlayerAndBottomMainStyle()
    {
        // Arrange
        _mockHttpContextAccessor.HttpContext.Returns(TestHttpContextFactory.Create(user: null));
        ThemeResponseDto theme = new ThemeResponseDtoFixture().Create(themeId: "editorial");
        SetupRenderDocuments(theme, layoutTemplate: "{{title}}|{{assetBase}}|{{{appHead}}}|{{{nav}}}|{{{content}}}|{{{audioPlayer}}}|{{{appScripts}}}|{{{scripts}}}|{{mainStyle}}", navTemplate: "nav:{{siteName}}");
        ThemeLayoutPageDto page = new("My Page", "<main>", "<page-script>");

        // Act
        Result<string> result = await _sut.RenderAsync(page, "editorial", CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("My Page|/theme-assets/editorial/assets|<head-fragment>|nav:Lumina|<main>||<scripts-fragment>|<page-script>|bottom: 0px;", result.Value);
    }

    [Fact]
    public async Task RenderAsync_WhenNavTemplateCannotBeRendered_ShouldFallBackToApplicationNavMenu()
    {
        // Arrange
        _mockHttpContextAccessor.HttpContext.Returns(TestHttpContextFactory.Create(user: null));
        ThemeResponseDto theme = new ThemeResponseDtoFixture().Create(themeId: "editorial");
        SetupRenderDocuments(theme, layoutTemplate: "{{title}}|{{{nav}}}", navTemplate: "{{#unclosed}}");
        ThemeLayoutPageDto page = new("My Page", "<main>", string.Empty);

        // Act
        Result<string> result = await _sut.RenderAsync(page, "editorial", CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("My Page|<nav-menu-fragment>", result.Value);
        _mockViewEngine.Received(1).FindView(Arg.Any<ActionContext>(), "_NavMenu", false);
    }

    [Fact]
    public async Task RenderAsync_WhenCalled_ShouldResolveLayoutAndNavDocumentsFromTheme()
    {
        // Arrange
        _mockHttpContextAccessor.HttpContext.Returns(TestHttpContextFactory.Create(user: null));
        ThemeResponseDto theme = new ThemeResponseDtoFixture().Create(themeId: "editorial");
        SetupRenderDocuments(theme, layoutTemplate: "{{title}}", navTemplate: "nav:{{siteName}}");
        ThemeLayoutPageDto page = new("My Page", "<main>", string.Empty);
        string layoutEndpoint = ApiRoutes.Themes.GET_THEME_TEMPLATE
            .Replace("{themeId}", "editorial")
            .Replace("{*pageKey}", "shared/layout");
        string navEndpoint = ApiRoutes.Themes.GET_THEME_TEMPLATE
            .Replace("{themeId}", "editorial")
            .Replace("{*pageKey}", "shared/nav-menu");

        // Act
        await _sut.RenderAsync(page, "editorial", CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).GetAsync<ThemeTemplateResponseDto>(layoutEndpoint, Arg.Any<CancellationToken>());
        await _mockApiHttpClient.Received(1).GetAsync<ThemeTemplateResponseDto>(navEndpoint, Arg.Any<CancellationToken>());
    }

    private void SetupRenderDocuments(ThemeResponseDto theme, string layoutTemplate, string navTemplate)
    {
        string layoutEndpoint = ApiRoutes.Themes.GET_THEME_TEMPLATE
            .Replace("{themeId}", theme.ThemeId)
            .Replace("{*pageKey}", "shared/layout");
        string navEndpoint = ApiRoutes.Themes.GET_THEME_TEMPLATE
            .Replace("{themeId}", theme.ThemeId)
            .Replace("{*pageKey}", "shared/nav-menu");
        _mockApiHttpClient.GetAsync<ThemeTemplateResponseDto>(layoutEndpoint, Arg.Any<CancellationToken>())
            .Returns(new ThemeTemplateResponseDto(theme, layoutTemplate));
        _mockApiHttpClient.GetAsync<ThemeTemplateResponseDto>(navEndpoint, Arg.Any<CancellationToken>())
            .Returns(new ThemeTemplateResponseDto(theme, navTemplate));
    }

    private static IView CreateView(string markup)
    {
        IView view = Substitute.For<IView>();
        view.RenderAsync(Arg.Any<ViewContext>())
            .Returns(callInfo =>
            {
                callInfo.Arg<ViewContext>().Writer.Write(markup);
                return Task.CompletedTask;
            });
        return view;
    }

    private static IStringLocalizer CreateLocalizer()
    {
        IStringLocalizer localizer = Substitute.For<IStringLocalizer>();
        localizer[Arg.Any<string>()].Returns(callInfo => new LocalizedString(callInfo.Arg<string>(), $"Localized-{callInfo.Arg<string>()}"));
        return localizer;
    }
}
