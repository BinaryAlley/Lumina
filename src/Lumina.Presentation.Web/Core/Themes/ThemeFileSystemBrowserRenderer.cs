#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.Themes;
using Lumina.Presentation.Web.Common.Primitives;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Core.Themes;

/// <summary>
/// Renders the file system browser component with the active theme, inlining the sub templates of its dynamic content so that the client script can render nodes, items and path segments from theme markup.
/// </summary>
public class ThemeFileSystemBrowserRenderer
{
    // the page keys mirror the template files of the theme packs, pinned in each theme manifest so that a missing sub template does not resolve to the component shell template
    private const string SHELL_PAGE_KEY = "shared/file-system-browser";
    private const string TREE_NODE_PAGE_KEY = "shared/file-system-browser/tree-node";
    private const string EXPLORER_ITEM_PAGE_KEY = "shared/file-system-browser/explorer-item";
    private const string PATH_SEGMENT_PAGE_KEY = "shared/file-system-browser/path-segment";

    private static readonly JsonSerializerOptions s_camelCaseJsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly ThemeService _themeService;
    private readonly ThemeTemplateEngine _templateEngine;
    private readonly ThemeFileSystemBrowserBuilder _builder;

    /// <summary>
    /// Initializes a new instance of the <see cref="ThemeFileSystemBrowserRenderer"/> class.
    /// </summary>
    /// <param name="themeService">The service that resolves the render documents.</param>
    /// <param name="templateEngine">The template engine used to render the component shell template.</param>
    /// <param name="builder">The builder used to produce the component model.</param>
    public ThemeFileSystemBrowserRenderer(ThemeService themeService, ThemeTemplateEngine templateEngine, ThemeFileSystemBrowserBuilder builder)
    {
        _themeService = themeService;
        _templateEngine = templateEngine;
        _builder = builder;
    }

    /// <summary>
    /// Renders the themed file system browser component.
    /// </summary>
    /// <param name="configuration">The runtime configuration of the file system browser.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the component HTML, or an error.</returns>
    public virtual async Task<Result<string>> RenderAsync(ThemeFileSystemBrowserConfigurationDto configuration, CancellationToken cancellationToken)
    {
        try
        {
            ThemeRenderDocumentDto shellDocument = await _themeService.GetRenderDocumentAsync(SHELL_PAGE_KEY, requestedThemeId: null, cancellationToken).ConfigureAwait(false);
            string themeId = shellDocument.Theme.Id;
            string assetBase = $"/theme-assets/{themeId}/assets";

            string treeNodeTemplate = await GetSubTemplateAsync(themeId, shellDocument.Template, TREE_NODE_PAGE_KEY, cancellationToken).ConfigureAwait(false);
            string explorerItemTemplate = await GetSubTemplateAsync(themeId, shellDocument.Template, EXPLORER_ITEM_PAGE_KEY, cancellationToken).ConfigureAwait(false);
            string pathSegmentTemplate = await GetSubTemplateAsync(themeId, shellDocument.Template, PATH_SEGMENT_PAGE_KEY, cancellationToken).ConfigureAwait(false);

            ThemeFileSystemBrowserDto model = _builder.Build(assetBase, treeNodeTemplate, explorerItemTemplate, pathSegmentTemplate);
            Result<ThemePageRenderResultDto> renderResult = _templateEngine.RenderPage(shellDocument.Template, model);
            if (renderResult.IsFailure)
                return renderResult.Errors;

            return renderResult.Value.Content + renderResult.Value.Script + BuildInitializationScript(configuration, assetBase);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception)
        {
            // the file system browser is best effort: when the component template of the active theme cannot be loaded or rendered, the application Razor fallback is rendered instead
            return Error.NotFound(code: "Theme.Template.Unavailable", description: "The file system browser theme template could not be loaded.");
        }
    }

    /// <summary>
    /// Gets the raw template source of a file system browser sub template, degrading to an empty string when the theme does not provide one.
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
            // when the theme does not ship the sub template, the mirrored template resolution walks up the path scopes
            // and returns the component shell template instead, which the client script must not render as dynamic content
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
            // each sub template is best effort: when the theme omits one, the client script renders that part with its built-in markup instead
            return string.Empty;
        }
    }

    /// <summary>
    /// Builds the initialization script of the file system browser, which passes the runtime configuration to the client script.
    /// </summary>
    /// <param name="configuration">The runtime configuration of the file system browser.</param>
    /// <param name="assetBase">The base URL of the assets of the active theme.</param>
    /// <returns>The initialization script element.</returns>
    private static string BuildInitializationScript(ThemeFileSystemBrowserConfigurationDto configuration, string assetBase)
    {
        string initConfigJson = JsonSerializer.Serialize(new
        {
            configuration.ServerBasePath,
            configuration.ClientBasePath,
            configuration.Path,
            configuration.ViewMode,
            configuration.IconSize,
            IconBaseUrl = $"{assetBase}/images/icons",
            FileIconsUrl = $"{assetBase}/file-icons.json"
        }, s_camelCaseJsonOptions);

        return $"<script defer data-component-init=\"file-system-browser\">\n" +
            "    // this outer function executes immediately and creates a scope for the variables\n" +
            "    (function() {\n" +
            "        // this inner function contains the actual initialization code that needs to run\n" +
            "        async function checkAndInit() {\n" +
            "            if (typeof initFileSystemBrowser === 'undefined' || typeof callApiGetAsync === 'undefined') {\n" +
            "                setTimeout(checkAndInit, 10);\n" +
            "                return;\n" +
            "            }\n" +
            $"            await initFileSystemBrowser({initConfigJson});\n" +
            "        }\n" +
            "        checkAndInit().catch(error => console.error('Failed to initialize file system browser:', error));\n" +
            "    })();\n" +
            "</script>";
    }
}
