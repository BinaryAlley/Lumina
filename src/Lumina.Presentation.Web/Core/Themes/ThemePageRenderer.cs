#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.Themes;
using Lumina.Presentation.Web.Common.Primitives;
using Lumina.Presentation.Web.Common.Services;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Core.Themes;

/// <summary>
/// Renders a theme template against a page model, populating the theme metadata before rendering.
/// </summary>
public class ThemePageRenderer
{
    private readonly ThemeService _themeService;
    private readonly ThemeTemplateEngine _templateEngine;

    /// <summary>
    /// Initializes a new instance of the <see cref="ThemePageRenderer"/> class.
    /// </summary>
    /// <param name="themeService">The service that resolves the render document.</param>
    /// <param name="templateEngine">The template engine used to render the document.</param>
    public ThemePageRenderer(ThemeService themeService, ThemeTemplateEngine templateEngine)
    {
        _themeService = themeService;
        _templateEngine = templateEngine;
    }

    /// <summary>
    /// Renders the page model with the theme selected for the requested theme identifier, producing the content and script sections of the page.
    /// </summary>
    /// <param name="model">The page model to render.</param>
    /// <param name="requestedThemeId">The optional unique identifier of the theme to render with, falling back to the current theme when <see langword="null"/>.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the rendered page, or an error.</returns>
    public virtual async Task<Result<ThemePageRenderResultDto>> RenderAsync(ThemePageDto model, string? requestedThemeId = null, CancellationToken cancellationToken = default)
    {
        ThemeRenderDocumentDto document = await _themeService.GetRenderDocumentAsync(model.PageKey, requestedThemeId, cancellationToken);

        model.ThemeId = document.Theme.Id;
        model.AssetBase = $"/theme-assets/{document.Theme.Id}/assets";
        // a fresh script id per render lets the AJAX navigator unload this view's script when navigating away
        model.ScriptId = ScriptIdentifierHelper.GenerateScriptId();

        return _templateEngine.RenderPage(document.Template, model);
    }
}
