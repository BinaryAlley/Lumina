#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.DTO.Themes;
using Lumina.Presentation.Web.Common.Enums.Primitives;
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
/// Contains unit tests for the <see cref="ThemeAudioPlayerRenderer"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ThemeAudioPlayerRendererTests
{
    private const string THEME_ID = "editorial";
    private const string SHELL_PAGE_KEY = "shared/audio-player";
    private const string QUEUE_ITEM_PAGE_KEY = "shared/audio-player/queue-item";
    private const string PLAYLIST_ITEM_PAGE_KEY = "shared/audio-player/playlist-item";

    private readonly IApiHttpClient _mockApiHttpClient;
    private readonly IHttpContextAccessor _mockHttpContextAccessor;
    private readonly IStringLocalizerFactory _mockStringLocalizerFactory;
    private readonly IStringLocalizer _mockStringLocalizer;
    private readonly ThemeAudioPlayerRenderer _sut;
    private readonly ThemeResponseDtoFixture _themeResponseDtoFixture = new();
    private readonly ThemeTemplateResponseDtoFixture _themeTemplateResponseDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ThemeAudioPlayerRendererTests"/> class.
    /// </summary>
    public ThemeAudioPlayerRendererTests()
    {
        _mockApiHttpClient = Substitute.For<IApiHttpClient>();
        _mockStringLocalizer = CreateLocalizer();
        _mockStringLocalizerFactory = Substitute.For<IStringLocalizerFactory>();
        _mockStringLocalizerFactory.Create(Arg.Any<string>(), Arg.Any<string>()).Returns(_mockStringLocalizer);
        _mockHttpContextAccessor = Substitute.For<IHttpContextAccessor>();
        ThemeService themeService = new(_mockApiHttpClient);
        ThemeAudioPlayerBuilder builder = new(_mockStringLocalizerFactory, _mockHttpContextAccessor);
        _sut = new ThemeAudioPlayerRenderer(themeService, new ThemeTemplateEngine(), builder);
    }

    [Fact]
    public async Task RenderAsync_WhenCalled_ShouldRenderTheShellWithTheThemeAssetBaseAndThePlayerModel()
    {
        // Arrange
        StubTemplate(THEME_ID, SHELL_PAGE_KEY, "<div class=\"lumina-audio-player\" data-asset-base=\"{{{assetBase}}}\" data-queue-label=\"{{strings.queue}}\"></div>");
        StubTemplate(THEME_ID, QUEUE_ITEM_PAGE_KEY, string.Empty);
        StubTemplate(THEME_ID, PLAYLIST_ITEM_PAGE_KEY, string.Empty);

        // Act
        Result<string> result = await _sut.RenderAsync(THEME_ID, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Contains("/theme-assets/editorial/assets", result.Value);
        Assert.Contains("data-queue-label=\"Localized-AudioPlayerQueue\"", result.Value);
    }

    [Fact]
    public async Task RenderAsync_WhenThemeProvidesSubTemplates_ShouldInlineThemInTheShell()
    {
        // Arrange
        StubTemplate(THEME_ID, SHELL_PAGE_KEY, "{{{queueItemTemplate}}}|{{{playlistItemTemplate}}}");
        StubTemplate(THEME_ID, QUEUE_ITEM_PAGE_KEY, "<li class=\"queue-item\">{{title}}</li>");
        StubTemplate(THEME_ID, PLAYLIST_ITEM_PAGE_KEY, "<li class=\"playlist-item\">{{name}}</li>");

        // Act
        Result<string> result = await _sut.RenderAsync(THEME_ID, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("<li class=\"queue-item\">{{title}}</li>|<li class=\"playlist-item\">{{name}}</li>", result.Value);
    }

    [Fact]
    public async Task RenderAsync_WhenSubTemplateIsMissing_ShouldInlineAnEmptySubTemplate()
    {
        // Arrange
        StubTemplate(THEME_ID, SHELL_PAGE_KEY, "{{{queueItemTemplate}}}|{{{playlistItemTemplate}}}");
        StubTemplateFailure(THEME_ID, QUEUE_ITEM_PAGE_KEY, new HttpRequestException("The sub template is unavailable."));
        StubTemplate(THEME_ID, PLAYLIST_ITEM_PAGE_KEY, string.Empty);

        // Act
        Result<string> result = await _sut.RenderAsync(THEME_ID, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("|", result.Value);
    }

    [Fact]
    public async Task RenderAsync_WhenSubTemplateResolvesToTheShellTemplate_ShouldInlineAnEmptySubTemplate()
    {
        // Arrange
        string shellTemplate = "<div class=\"lumina-audio-player\">{{{queueItemTemplate}}}|{{{playlistItemTemplate}}}</div>";
        StubTemplate(THEME_ID, SHELL_PAGE_KEY, shellTemplate);
        StubTemplate(THEME_ID, QUEUE_ITEM_PAGE_KEY, shellTemplate);
        StubTemplate(THEME_ID, PLAYLIST_ITEM_PAGE_KEY, shellTemplate);

        // Act
        Result<string> result = await _sut.RenderAsync(THEME_ID, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("<div class=\"lumina-audio-player\">|</div>", result.Value);
    }

    [Fact]
    public async Task RenderAsync_WhenTemplateDefinesScripts_ShouldConcatenateTheContentAndTheScript()
    {
        // Arrange
        StubTemplate(THEME_ID, SHELL_PAGE_KEY, "<div>content</div>{{#scripts}}<script>init();</script>{{/scripts}}");
        StubTemplate(THEME_ID, QUEUE_ITEM_PAGE_KEY, string.Empty);
        StubTemplate(THEME_ID, PLAYLIST_ITEM_PAGE_KEY, string.Empty);

        // Act
        Result<string> result = await _sut.RenderAsync(THEME_ID, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("<div>content</div><script>init();</script>", result.Value);
    }

    [Fact]
    public async Task RenderAsync_WhenTemplateEngineFails_ShouldReturnTheTemplateErrors()
    {
        // Arrange
        StubTemplate(THEME_ID, SHELL_PAGE_KEY, "{{#unclosed}}");
        StubTemplate(THEME_ID, QUEUE_ITEM_PAGE_KEY, string.Empty);
        StubTemplate(THEME_ID, PLAYLIST_ITEM_PAGE_KEY, string.Empty);

        // Act
        Result<string> result = await _sut.RenderAsync(THEME_ID, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Theme.Template.Invalid", result.FirstError.Code);
    }

    [Fact]
    public async Task RenderAsync_WhenShellDocumentCannotBeLoaded_ShouldReturnTemplateUnavailableError()
    {
        // Arrange
        StubTemplateFailure(THEME_ID, SHELL_PAGE_KEY, new HttpRequestException("The theme API is unavailable."));

        // Act
        Result<string> result = await _sut.RenderAsync(THEME_ID, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Theme.Template.Unavailable", result.FirstError.Code);
        Assert.Equal(ErrorType.NotFound, result.FirstError.Type);
    }

    [Fact]
    public async Task RenderAsync_WhenShellTemplateCannotBeRendered_ShouldReturnTemplateUnavailableError()
    {
        // Arrange
        StubTemplate(THEME_ID, SHELL_PAGE_KEY, string.Empty, includeTemplate: false);
        StubTemplate(THEME_ID, QUEUE_ITEM_PAGE_KEY, string.Empty);
        StubTemplate(THEME_ID, PLAYLIST_ITEM_PAGE_KEY, string.Empty);

        // Act
        Result<string> result = await _sut.RenderAsync(THEME_ID, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Theme.Template.Unavailable", result.FirstError.Code);
        Assert.Equal(ErrorType.NotFound, result.FirstError.Type);
    }

    [Fact]
    public async Task RenderAsync_WhenShellLoadIsCancelled_ShouldRethrowTheCancellation()
    {
        // Arrange
        StubTemplateFailure(THEME_ID, SHELL_PAGE_KEY, new OperationCanceledException());

        // Act
        Task<Result<string>> operationTask = _sut.RenderAsync(THEME_ID, CancellationToken.None);

        // Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => operationTask);
    }

    [Fact]
    public async Task RenderAsync_WhenSubTemplateLoadIsCancelled_ShouldRethrowTheCancellation()
    {
        // Arrange
        StubTemplate(THEME_ID, SHELL_PAGE_KEY, "<div>{{{queueItemTemplate}}}</div>");
        StubTemplateFailure(THEME_ID, QUEUE_ITEM_PAGE_KEY, new OperationCanceledException());
        StubTemplate(THEME_ID, PLAYLIST_ITEM_PAGE_KEY, string.Empty);

        // Act
        Task<Result<string>> operationTask = _sut.RenderAsync(THEME_ID, CancellationToken.None);

        // Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => operationTask);
    }

    [Fact]
    public async Task RenderAsync_WhenCalled_ShouldPassTheThemeIdAndTheCancellationTokenToTheTemplateLookups()
    {
        // Arrange
        using CancellationTokenSource cancellationTokenSource = new();
        StubTemplate(THEME_ID, SHELL_PAGE_KEY, "shell");
        StubTemplate(THEME_ID, QUEUE_ITEM_PAGE_KEY, "queue");
        StubTemplate(THEME_ID, PLAYLIST_ITEM_PAGE_KEY, "playlist");

        // Act
        await _sut.RenderAsync(THEME_ID, cancellationTokenSource.Token);

        // Assert
        string[] pageKeys = [SHELL_PAGE_KEY, QUEUE_ITEM_PAGE_KEY, PLAYLIST_ITEM_PAGE_KEY];
        foreach (string pageKey in pageKeys)
        {
            string endpoint = ApiRoutes.Themes.GET_THEME_TEMPLATE
                .Replace("{themeId}", THEME_ID)
                .Replace("{*pageKey}", pageKey);
            await _mockApiHttpClient.Received(1).GetAsync<ThemeTemplateResponseDto>(endpoint, Arg.Is(cancellationTokenSource.Token));
        }
        await _mockApiHttpClient.DidNotReceive().GetAsync<ThemeResponseDto>(ApiRoutes.Themes.GET_CURRENT_THEME, Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Stubs the API response that returns a single audio player template document.
    /// </summary>
    /// <param name="themeId">The manifest id of the theme.</param>
    /// <param name="pageKey">The page key of the template.</param>
    /// <param name="template">The template returned for the document.</param>
    /// <param name="includeTemplate">Whether the returned document has a template, or a <see langword="null"/> one.</param>
    private void StubTemplate(string themeId, string pageKey, string template, bool includeTemplate = true)
    {
        string endpoint = ApiRoutes.Themes.GET_THEME_TEMPLATE
            .Replace("{themeId}", themeId)
            .Replace("{*pageKey}", pageKey);
        _mockApiHttpClient.GetAsync<ThemeTemplateResponseDto>(endpoint, Arg.Any<CancellationToken>())
            .Returns(_themeTemplateResponseDtoFixture.Create(
                theme: _themeResponseDtoFixture.Create(themeId: themeId),
                template: template,
                includeTemplate: includeTemplate));
    }

    /// <summary>
    /// Stubs the API response of an audio player template document to fail.
    /// </summary>
    /// <param name="themeId">The manifest id of the theme.</param>
    /// <param name="pageKey">The page key of the template.</param>
    /// <param name="exception">The exception the API call throws.</param>
    private void StubTemplateFailure(string themeId, string pageKey, Exception exception)
    {
        string endpoint = ApiRoutes.Themes.GET_THEME_TEMPLATE
            .Replace("{themeId}", themeId)
            .Replace("{*pageKey}", pageKey);
        _mockApiHttpClient.GetAsync<ThemeTemplateResponseDto>(endpoint, Arg.Any<CancellationToken>())
            .Returns(Task.FromException<ThemeTemplateResponseDto>(exception));
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
}
