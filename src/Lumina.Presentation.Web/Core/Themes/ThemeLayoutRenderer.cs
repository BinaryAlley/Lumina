#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.Themes;
using Lumina.Presentation.Web.Common.Primitives;
using Microsoft.AspNetCore.Http;
using System;
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

    private readonly ThemeService _themeService;
    private readonly ThemeTemplateEngine _templateEngine;
    private readonly RazorViewToStringRenderer _viewRenderer;
    private readonly ThemeNavBuilder _navBuilder;
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="ThemeLayoutRenderer"/> class.
    /// </summary>
    /// <param name="themeService">The service that resolves the render documents.</param>
    /// <param name="templateEngine">The template engine used to render the documents.</param>
    /// <param name="viewRenderer">The renderer used to produce the application chrome fragments.</param>
    /// <param name="navBuilder">The builder used to produce the navigation menu model.</param>
    /// <param name="httpContextAccessor">Injected accessor for the current HTTP context.</param>
    public ThemeLayoutRenderer(ThemeService themeService, ThemeTemplateEngine templateEngine, RazorViewToStringRenderer viewRenderer, ThemeNavBuilder navBuilder, IHttpContextAccessor httpContextAccessor)
    {
        _themeService = themeService;
        _templateEngine = templateEngine;
        _viewRenderer = viewRenderer;
        _navBuilder = navBuilder;
        _httpContextAccessor = httpContextAccessor;
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
            string audioPlayer = isAuthenticated ? await _viewRenderer.RenderPartialAsync("_AudioPlayer", cancellationToken) : string.Empty;
            string appScripts = await _viewRenderer.RenderPartialAsync("_ThemeLayoutScripts", cancellationToken);

            ThemeLayoutModelDto layoutModel = new(
                page.Title,
                $"/theme-assets/{themeId}/assets",
                appHead,
                nav,
                page.Content,
                audioPlayer,
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
}
