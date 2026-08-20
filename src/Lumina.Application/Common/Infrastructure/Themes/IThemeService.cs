#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Models.DTO.Themes;
using Lumina.Domain.Common.Primitives;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Common.Infrastructure.Themes;

/// <summary>
/// Service for the storage and serving of theme packs.
/// </summary>
public interface IThemeService
{
    /// <summary>
    /// Gets a value indicating whether theme templates may contain script elements and theme assets may include script files.
    /// </summary>
    bool AllowThemeScripts { get; }

    /// <summary>
    /// Gets the maximum allowed size of a theme archive, in bytes.
    /// </summary>
    long MaxArchiveBytes { get; }

    /// <summary>
    /// Gets the identifier of the theme selected when no valid current theme is available.
    /// </summary>
    string DefaultThemeId { get; }

    /// <summary>
    /// Installs a theme pack from the provided archive, replacing the files of an existing theme with the same manifest id.
    /// </summary>
    /// <param name="archive">The ZIP archive stream of the theme pack.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the parsed manifest of the installed theme, or an error.</returns>
    Task<Result<ThemeManifestDto>> InstallAsync(Stream archive, CancellationToken cancellationToken);

    /// <summary>
    /// Reads the manifest of a theme pack archive without installing it.
    /// </summary>
    /// <param name="archivePath">The path of the theme pack ZIP archive.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the parsed manifest, or an error.</returns>
    Task<Result<ThemeManifestDto>> ReadManifestFromArchiveAsync(string archivePath, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes the stored files of a theme pack.
    /// </summary>
    /// <param name="themeId">The manifest id of the theme.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    Task<Result<Success>> DeleteAsync(string themeId, CancellationToken cancellationToken);

    /// <summary>
    /// Loads the manifest of an installed theme pack from its storage location.
    /// </summary>
    /// <param name="themeId">The manifest id of the theme.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the parsed manifest, or an error.</returns>
    Task<Result<ThemeManifestDto>> LoadManifestAsync(string themeId, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the sanitized content of the template selected by a page key, falling back to the default template when the key is missing.
    /// </summary>
    /// <param name="themeId">The manifest id of the theme.</param>
    /// <param name="pageKey">The page key that selects the template.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the template content, or an error.</returns>
    Task<Result<string>> GetTemplateAsync(string themeId, string pageKey, CancellationToken cancellationToken);

    /// <summary>
    /// Gets a theme asset file.
    /// </summary>
    /// <param name="themeId">The manifest id of the theme.</param>
    /// <param name="assetPath">The asset path relative to the theme pack root.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the asset, or an error.</returns>
    Task<Result<ThemeAssetDto>> GetAssetAsync(string themeId, string assetPath, CancellationToken cancellationToken);

    /// <summary>
    /// Builds a downloadable ZIP archive of an installed theme pack.
    /// </summary>
    /// <param name="themeId">The manifest id of the theme.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the archive, or an error.</returns>
    Task<Result<ThemeArchiveDto>> BuildArchiveAsync(string themeId, CancellationToken cancellationToken);

    /// <summary>
    /// Restores the files of a bundled theme from its shipped archive, used when the stored files were removed externally.
    /// </summary>
    /// <param name="themeId">The manifest id of the theme.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    Task<Result<Success>> RestoreBundledThemeAsync(string themeId, CancellationToken cancellationToken);

    /// <summary>
    /// Checks whether the stored pack files of a theme still exist.
    /// </summary>
    /// <param name="themeId">The manifest id of the theme.</param>
    /// <returns><see langword="true"/> when the theme pack files exist, <see langword="false"/> otherwise.</returns>
    bool HasThemePack(string themeId);

    /// <summary>
    /// Gets the paths of the theme pack archives shipped with the application.
    /// </summary>
    /// <returns>The list of bundled theme archive paths.</returns>
    IReadOnlyList<string> GetBundledThemeArchivePaths();
}
