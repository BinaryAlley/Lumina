#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.DTO.FileSystemManagement;
using Lumina.Presentation.Web.Common.DTO.Themes;
using Lumina.Presentation.Web.Common.Enums.Themes;
using Lumina.Presentation.Web.Common.Routes;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Core.Themes;

/// <summary>
/// Facade over the theme endpoints of the remote API, which stores and serves the theme packs, while the rendering logic stays in this project.
/// </summary>
public sealed class ThemeService
{
    private readonly IApiHttpClient _apiHttpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="ThemeService"/> class.
    /// </summary>
    /// <param name="apiHttpClient">Injected service for interactions with the API.</param>
    public ThemeService(IApiHttpClient apiHttpClient)
    {
        _apiHttpClient = apiHttpClient;
    }

    /// <summary>
    /// Gets the display metadata of all installed themes.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The list of installed themes.</returns>
    public async Task<IReadOnlyList<ThemeInfoDto>> GetThemesAsync(CancellationToken cancellationToken = default)
    {
        ThemeResponseDto[] themes = await _apiHttpClient.GetAsync<ThemeResponseDto[]>(ApiRoutes.Themes.GET_THEMES, cancellationToken).ConfigureAwait(false);
        return [.. themes.Select(ToThemeInfo)];
    }

    /// <summary>
    /// Gets the display metadata of the currently active theme.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The current theme.</returns>
    public async Task<ThemeInfoDto> GetCurrentThemeAsync(CancellationToken cancellationToken = default)
    {
        ThemeResponseDto theme = await _apiHttpClient.GetAsync<ThemeResponseDto>(ApiRoutes.Themes.GET_CURRENT_THEME, cancellationToken).ConfigureAwait(false);
        return ToThemeInfo(theme);
    }

    /// <summary>
    /// Gets the theme engine settings used by the administration interface.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The theme engine settings.</returns>
    public async Task<ThemeSettingsResponseDto> GetThemeSettingsAsync(CancellationToken cancellationToken = default)
    {
        return await _apiHttpClient.GetAsync<ThemeSettingsResponseDto>(ApiRoutes.Themes.GET_THEME_SETTINGS, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Selects the theme with the specified identifier as the current theme.
    /// </summary>
    /// <param name="themeId">The identifier of the theme to select.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public async Task SetCurrentThemeAsync(string themeId, CancellationToken cancellationToken = default)
    {
        await _apiHttpClient.PutAsync<ThemeResponseDto, object>(ApiRoutes.Themes.SET_CURRENT_THEME, new { ThemeId = themeId }, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Installs a theme from the provided archive, replacing the files of an existing theme with the same manifest id.
    /// </summary>
    /// <param name="archive">The ZIP archive stream of the theme pack.</param>
    /// <param name="fileName">The file name of the uploaded archive.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The installed theme metadata.</returns>
    public async Task<ThemeInfoDto> InstallAsync(Stream archive, string fileName, CancellationToken cancellationToken = default)
    {
        ThemeResponseDto theme = await _apiHttpClient.PostMultipartAsync<ThemeResponseDto>(ApiRoutes.Themes.INSTALL_THEME, archive, fileName, "archive", cancellationToken).ConfigureAwait(false);
        return ToThemeInfo(theme);
    }

    /// <summary>
    /// Deletes a theme.
    /// </summary>
    /// <param name="themeId">The identifier of the theme to delete.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public async Task DeleteThemeAsync(string themeId, CancellationToken cancellationToken = default)
    {
        await _apiHttpClient.DeleteAsync(ApiRoutes.Themes.DELETE_THEME.Replace("{themeId}", themeId), cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets the theme and raw template source selected for a page key.
    /// </summary>
    /// <param name="pageKey">The page key that selects the template to render.</param>
    /// <param name="requestedThemeId">The optional theme to render with, falling back to the current theme when null.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The render document.</returns>
    public async Task<ThemeRenderDocumentDto> GetRenderDocumentAsync(string pageKey, string? requestedThemeId, CancellationToken cancellationToken = default)
    {
        string themeId;
        if (string.IsNullOrWhiteSpace(requestedThemeId))
            themeId = (await GetCurrentThemeAsync(cancellationToken).ConfigureAwait(false)).Id;
        else
            themeId = requestedThemeId;

        ThemeTemplateResponseDto response = await _apiHttpClient.GetAsync<ThemeTemplateResponseDto>(
            ApiRoutes.Themes.GET_THEME_TEMPLATE.Replace("{themeId}", themeId).Replace("{pageKey}", pageKey), cancellationToken).ConfigureAwait(false);
        return new ThemeRenderDocumentDto(ToThemeInfo(response.Theme), response.Template);
    }

    /// <summary>
    /// Builds a downloadable ZIP archive of an installed theme.
    /// </summary>
    /// <param name="themeId">The identifier of the theme to archive.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The theme archive.</returns>
    public async Task<ThemeArchiveDto> BuildArchiveAsync(string themeId, CancellationToken cancellationToken = default)
    {
        BlobDataDto blob = await _apiHttpClient.GetBlobAsync(ApiRoutes.Themes.GET_THEME_ARCHIVE.Replace("{themeId}", themeId), cancellationToken).ConfigureAwait(false);
        return new ThemeArchiveDto($"{themeId}.zip", new MemoryStream(blob.Data));
    }

    /// <summary>
    /// Converts a theme response from the remote API to the display metadata of the theme.
    /// </summary>
    /// <param name="theme">The theme response to convert.</param>
    /// <returns>The converted theme display metadata.</returns>
    private static ThemeInfoDto ToThemeInfo(ThemeResponseDto theme)
    {
        string previewUrl = string.IsNullOrWhiteSpace(theme.PreviewPath) ? "/admin/theme-placeholder.svg" : $"/theme-assets/{theme.ThemeId}/{theme.PreviewPath}";
        return new ThemeInfoDto(theme.ThemeId, theme.Name, theme.Description, theme.Author, theme.Version, previewUrl, theme.InstallSource == ThemeInstallSource.Bundled);
    }
}
