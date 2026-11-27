#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.Themes;
using Lumina.Presentation.Web.Common.Primitives;
using System;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Core.Themes;

/// <summary>
/// Renders the audio player bar with a theme, inlining the sub templates of its dynamic content so that the client script can render queue items and playlist items from theme markup.
/// </summary>
public class ThemeAudioPlayerRenderer
{
    // The page keys mirror the template files of the theme packs, pinned in each theme manifest so that a missing sub template does not resolve to the component shell template.
    private const string SHELL_PAGE_KEY = "shared/audio-player";
    private const string QUEUE_ITEM_PAGE_KEY = "shared/audio-player/queue-item";
    private const string PLAYLIST_ITEM_PAGE_KEY = "shared/audio-player/playlist-item";

    private readonly ThemeService _themeService;
    private readonly ThemeTemplateEngine _templateEngine;
    private readonly ThemeAudioPlayerBuilder _builder;

    /// <summary>
    /// Initializes a new instance of the <see cref="ThemeAudioPlayerRenderer"/> class.
    /// </summary>
    /// <param name="themeService">The service that resolves the render documents.</param>
    /// <param name="templateEngine">The template engine used to render the component shell template.</param>
    /// <param name="builder">The builder used to produce the component model.</param>
    public ThemeAudioPlayerRenderer(ThemeService themeService, ThemeTemplateEngine templateEngine, ThemeAudioPlayerBuilder builder)
    {
        _themeService = themeService;
        _templateEngine = templateEngine;
        _builder = builder;
    }

    /// <summary>
    /// Renders the themed audio player bar with the specified theme.
    /// </summary>
    /// <param name="themeId">The manifest id of the theme to render the bar with, normally the theme the layout was resolved with.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the component HTML, or an error.</returns>
    public virtual async Task<Result<string>> RenderAsync(string themeId, CancellationToken cancellationToken)
    {
        try
        {
            ThemeRenderDocumentDto shellDocument = await _themeService.GetRenderDocumentAsync(SHELL_PAGE_KEY, themeId, cancellationToken).ConfigureAwait(false);
            string assetBase = $"/theme-assets/{themeId}/assets";

            string queueItemTemplate = await GetSubTemplateAsync(themeId, shellDocument.Template, QUEUE_ITEM_PAGE_KEY, cancellationToken).ConfigureAwait(false);
            string playlistItemTemplate = await GetSubTemplateAsync(themeId, shellDocument.Template, PLAYLIST_ITEM_PAGE_KEY, cancellationToken).ConfigureAwait(false);

            ThemeAudioPlayerDto model = _builder.Build(assetBase, queueItemTemplate, playlistItemTemplate);
            Result<ThemePageRenderResultDto> renderResult = _templateEngine.RenderPage(shellDocument.Template, model);
            if (renderResult.IsFailure)
                return renderResult.Errors;

            return renderResult.Value.Content + renderResult.Value.Script;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception)
        {
            // The audio player bar is best effort: when the component template of the active theme cannot be loaded or rendered, the application Razor fallback is rendered instead.
            return Error.NotFound(code: "Theme.Template.Unavailable", description: "The audio player theme template could not be loaded.");
        }
    }

    /// <summary>
    /// Gets the raw template source of an audio player sub template, degrading to an empty string when the theme does not provide one.
    /// </summary>
    /// <param name="themeId">The manifest id of the active theme.</param>
    /// <param name="shellTemplate">The raw template source of the component shell, used to detect the mirrored resolution falling back to the shell when a sub template is missing.</param>
    /// <param name="pageKey">The page key of the sub template.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The raw template source, or an empty string when the theme does not provide the template.</returns>
    private async Task<string> GetSubTemplateAsync(string themeId, string shellTemplate, string pageKey, CancellationToken cancellationToken)
    {
        try
        {
            ThemeRenderDocumentDto document = await _themeService.GetRenderDocumentAsync(pageKey, themeId, cancellationToken).ConfigureAwait(false);
            // When the theme does not ship the sub template, the mirrored template resolution walks up the path scopes
            // and returns the component shell template instead, which the client script must not render as dynamic content.
            if (string.Equals(document.Template, shellTemplate, StringComparison.Ordinal))
                return string.Empty;

            return document.Template;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception)
        {
            // Each sub template is best effort: when the theme omits one, the client script renders that part with its built-in markup instead.
            return string.Empty;
        }
    }
}
