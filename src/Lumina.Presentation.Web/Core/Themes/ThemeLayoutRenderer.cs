#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.Themes;
using Lumina.Presentation.Web.Common.Primitives;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Core.Themes;

/// <summary>
/// Renders the themed shell of a page, wrapping the page section in the layout and navigation templates of the active theme.
/// </summary>
public class ThemeLayoutRenderer
{
    // the page keys mirror the paths of the corresponding Razor views under Core/Views, so that theme templates can override them at the page, section or default scope
    private const string LAYOUT_PAGE_KEY = "shared/layout";
    private const string NAV_PAGE_KEY = "shared/nav-menu";
    private const string SERVER_UNREACHABLE_BANNER_PAGE_KEY = "shared/server-unreachable-banner";

    // The banner template is static for a theme, so it is cached, including the negative result of a theme that does not
    // provide one, so a missing template does not cost an API round trip and a 404 log on every page render.
    private static readonly ConcurrentDictionary<string, (string Template, DateTime ExpiresOnUtc)> s_bannerTemplateCache = [];
    private static readonly TimeSpan s_bannerTemplateCacheDuration = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan s_missingBannerTemplateCacheDuration = TimeSpan.FromMinutes(1);

    private readonly ThemeService _themeService;
    private readonly ThemeTemplateEngine _templateEngine;
    private readonly RazorViewToStringRenderer _viewRenderer;
    private readonly ThemeNavBuilder _navBuilder;
    private readonly ThemeAudioPlayerRenderer _audioPlayerRenderer;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IStringLocalizer _layoutLocalizer;

    /// <summary>
    /// Initializes a new instance of the <see cref="ThemeLayoutRenderer"/> class.
    /// </summary>
    /// <param name="themeService">The service that resolves the render documents.</param>
    /// <param name="templateEngine">The template engine used to render the documents.</param>
    /// <param name="viewRenderer">The renderer used to produce the application chrome fragments.</param>
    /// <param name="navBuilder">The builder used to produce the navigation menu model.</param>
    /// <param name="audioPlayerRenderer">The renderer used to produce the themed audio player bar.</param>
    /// <param name="httpContextAccessor">Injected accessor for the current HTTP context.</param>
    /// <param name="stringLocalizerFactory">Injected factory used to create the localizer of the layout resources.</param>
    public ThemeLayoutRenderer(ThemeService themeService, ThemeTemplateEngine templateEngine, RazorViewToStringRenderer viewRenderer, ThemeNavBuilder navBuilder, ThemeAudioPlayerRenderer audioPlayerRenderer, IHttpContextAccessor httpContextAccessor, IStringLocalizerFactory stringLocalizerFactory)
    {
        _themeService = themeService;
        _templateEngine = templateEngine;
        _viewRenderer = viewRenderer;
        _navBuilder = navBuilder;
        _audioPlayerRenderer = audioPlayerRenderer;
        _httpContextAccessor = httpContextAccessor;
        _layoutLocalizer = stringLocalizerFactory.Create("Lumina.Presentation.Web.Views.Shared._Layout", "Lumina.Presentation.Web");
    }

    /// <summary>
    /// Renders the themed shell of the page, wrapping the page section in the layout template of the active theme.
    /// </summary>
    /// <param name="page">The rendered page section to wrap.</param>
    /// <param name="requestedThemeId">The optional unique identifier of the theme to render with, falling back to the current theme when <see langword="null"/>.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the full page HTML, or an error.</returns>
    public virtual async Task<Result<string>> RenderAsync(ThemeLayoutPageDto page, string? requestedThemeId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            ThemeRenderDocumentDto layoutDocument = await _themeService.GetRenderDocumentAsync(LAYOUT_PAGE_KEY, requestedThemeId, cancellationToken);
            string themeId = layoutDocument.Theme.Id;
            bool isAuthenticated = _httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated == true;

            string appHead = await _viewRenderer.RenderPartialAsync("_ThemeLayoutHead", cancellationToken);
            string nav = await RenderNavAsync(themeId, cancellationToken);
            string audioPlayer = isAuthenticated ? await RenderAudioPlayerAsync(themeId, cancellationToken) : string.Empty;
            string serverUnreachableBanner = await RenderServerUnreachableBannerAsync(themeId, cancellationToken);
            string appScripts = await _viewRenderer.RenderPartialAsync("_ThemeLayoutScripts", cancellationToken);

            ThemeLayoutModelDto layoutModel = new(
                page.Title,
                $"/theme-assets/{themeId}/assets",
                appHead,
                nav,
                page.Content,
                audioPlayer,
                serverUnreachableBanner,
                appScripts,
                page.Script,
                isAuthenticated ? string.Empty : "bottom: 0px;");

            Result<ThemePageRenderResultDto> renderResult = _templateEngine.RenderPage(layoutDocument.Template, layoutModel);
            if (renderResult.IsFailure)
                return renderResult.Errors;

            return renderResult.Value.Content;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception)
        {
            // the themed shell is best effort: when the active theme cannot be loaded, the application layout renders the
            // page instead. The exception must not propagate, otherwise the exception handling middleware turns the page
            // into a JSON error instead of the fallback layout, so any page breaks whenever the theme API is unavailable.
            return Error.NotFound(code: "Theme.Template.Unavailable", description: "The active theme could not be loaded.");
        }
    }

    /// <summary>
    /// Renders the navigation menu of the active theme, falling back to the application Razor menu when the theme navigation template cannot be loaded or rendered.
    /// </summary>
    /// <param name="themeId">The manifest id of the active theme.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The rendered navigation menu HTML.</returns>
    private async Task<string> RenderNavAsync(string themeId, CancellationToken cancellationToken)
    {
        ThemeNavMenuDto navModel = await _navBuilder.BuildAsync(cancellationToken);
        try
        {
            ThemeRenderDocumentDto navDocument = await _themeService.GetRenderDocumentAsync(NAV_PAGE_KEY, themeId, cancellationToken);
            Result<ThemePageRenderResultDto> navRenderResult = _templateEngine.RenderPage(navDocument.Template, navModel);
            if (navRenderResult.IsSuccess)
                return navRenderResult.Value.Content;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception)
        {
            // the navigation menu is best effort: when the theme navigation template cannot be loaded or rendered,
            // the application navigation menu is rendered instead, keeping the themed shell of the page intact
        }

        return await _viewRenderer.RenderPartialAsync("_NavMenu", cancellationToken);
    }

    /// <summary>
    /// Renders the audio player bar with the theme the layout was resolved with, falling back to the application Razor markup when the theme does not provide a bar template.
    /// </summary>
    /// <param name="themeId">The manifest id of the theme the layout was rendered with, so that the bar always matches the shell, including a preview of another theme.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The rendered audio player bar HTML.</returns>
    private async Task<string> RenderAudioPlayerAsync(string themeId, CancellationToken cancellationToken)
    {
        // The audio player renderer already degrades to an error when the theme template is missing or cannot be
        // rendered, so a failure here only means the application fallback markup is rendered instead.
        Result<string> audioPlayerResult = await _audioPlayerRenderer.RenderAsync(themeId, cancellationToken);
        if (audioPlayerResult.IsSuccess)
            return audioPlayerResult.Value;

        return await _viewRenderer.RenderPartialAsync("_AudioPlayer", cancellationToken);
    }

    /// <summary>
    /// Renders the server unreachable banner of the active theme, falling back to the application Razor markup when the theme does not provide a template.
    /// </summary>
    /// <param name="themeId">The manifest id of the theme the layout was resolved with, so that the banner always matches the shell.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The rendered server unreachable banner HTML.</returns>
    private async Task<string> RenderServerUnreachableBannerAsync(string themeId, CancellationToken cancellationToken)
    {
        if (s_bannerTemplateCache.TryGetValue(themeId, out (string Template, DateTime ExpiresOnUtc) cached) && cached.ExpiresOnUtc > DateTime.UtcNow)
            return await RenderBannerTemplateAsync(cached.Template, cancellationToken).ConfigureAwait(false);

        string template = string.Empty;
        try
        {
            ThemeRenderDocumentDto bannerDocument = await _themeService.GetRenderDocumentAsync(SERVER_UNREACHABLE_BANNER_PAGE_KEY, themeId, cancellationToken);
            template = bannerDocument.Template;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception)
        {
            // The banner is best effort: when the theme template cannot be loaded or rendered, the application Razor fallback is rendered instead.
        }
        s_bannerTemplateCache[themeId] = (template, DateTime.UtcNow + (string.IsNullOrEmpty(template) ? s_missingBannerTemplateCacheDuration : s_bannerTemplateCacheDuration));
        return await RenderBannerTemplateAsync(template, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Renders a banner template, or the application fallback when the theme does not provide one.
    /// </summary>
    /// <param name="template">The raw banner template source, or an empty string when the theme does not provide one.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The rendered server unreachable banner HTML.</returns>
    private async Task<string> RenderBannerTemplateAsync(string template, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(template))
            return await _viewRenderer.RenderPartialAsync("_ServerUnreachableBanner", cancellationToken);

        ThemeServerUnreachableBannerDto bannerModel = new(_layoutLocalizer["ServerUnreachable"].Value);
        Result<ThemePageRenderResultDto> renderResult = _templateEngine.RenderPage(template, bannerModel);
        return renderResult.IsSuccess
            ? renderResult.Value.Content
            : await _viewRenderer.RenderPartialAsync("_ServerUnreachableBanner", cancellationToken);
    }
}
