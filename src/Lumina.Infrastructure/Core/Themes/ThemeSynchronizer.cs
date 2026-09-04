#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Themes;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Models.DTO.Themes;
using Lumina.Application.Common.Infrastructure.Themes;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.Themes;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Infrastructure.Core.Themes;

/// <summary>
/// Utility class that keeps the installed themes in sync with the bundled theme archives.
/// </summary>
internal static class ThemeSynchronizer
{
    /// <summary>
    /// Installs the bundled themes into the storage medium and the theme storage, and repairs the installed themes whose files are missing.
    /// </summary>
    /// <param name="themeService">Injected service for the server-side storage and serving of theme packs.</param>
    /// <param name="unitOfWork">The unit of work for interacting with the theme repository.</param>
    /// <param name="logger">Injected logger used for logging.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    internal static async Task SynchronizeAsync(IThemeService themeService, IUnitOfWork unitOfWork, ILogger logger, CancellationToken cancellationToken)
    {
        Result<IEnumerable<ThemeEntity>> getThemesResult = await unitOfWork.ThemeRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
        if (getThemesResult.IsFailure)
        {
            logger.LogWarning("Failed to read the installed themes: {Error}", getThemesResult.FirstError.Description);
            return;
        }

        // Themes that were soft deleted by the user must not be reinstalled automatically.
        List<ThemeEntity> themes = [.. getThemesResult.Value];

        foreach (string archivePath in themeService.GetBundledThemeArchivePaths())
        {
            Result<ThemeManifestDto> manifestResult = await themeService.ReadManifestFromArchiveAsync(archivePath, cancellationToken).ConfigureAwait(false);
            if (manifestResult.IsFailure)
            {
                logger.LogWarning("Skipping bundled theme archive {ArchivePath}: {Error}", archivePath, manifestResult.FirstError.Description);
                continue;
            }

            ThemeEntity? existingTheme = themes.Where(theme => theme.ThemeId == manifestResult.Value.Id).FirstOrDefault();

            // A theme that was soft deleted by the user must not be reinstalled automatically; an installed bundled theme
            // whose files are present is already in sync, so only a bundled theme whose files went missing gets its files reinstalled.
            if (existingTheme is not null && (existingTheme.IsDeleted || existingTheme.InstallSource != ThemeInstallSource.Bundled || themeService.HasThemePack(existingTheme.ThemeId)))
                continue;

            // Installing a bundled theme at startup is best effort: a file system failure here must never crash the host, because the default
            // BackgroundServiceExceptionBehavior (StopHost) shuts down the whole application and disposes its service provider, making every later request fail.
            try
            {
                await using FileStream archive = File.OpenRead(archivePath);
                Result<ThemeManifestDto> installResult = await themeService.InstallAsync(archive, cancellationToken).ConfigureAwait(false);
                if (installResult.IsFailure)
                {
                    logger.LogWarning("Failed to install bundled theme '{ThemeId}': {Error}", manifestResult.Value.Id, installResult.FirstError.Description);
                    continue;
                }

                // A known theme only gets its missing files reinstalled, so no new row is inserted.
                if (existingTheme is not null)
                    continue;

                ThemeEntity themeEntity = new()
                {
                    Id = Guid.NewGuid(),
                    ThemeId = manifestResult.Value.Id,
                    Name = manifestResult.Value.Name,
                    Description = manifestResult.Value.Description,
                    Author = manifestResult.Value.Author,
                    Version = manifestResult.Value.Version,
                    PreviewPath = manifestResult.Value.Preview,
                    InstallSource = ThemeInstallSource.Bundled,
                    InstalledAtUtc = DateTime.UtcNow,
                    CreatedOnUtc = DateTime.UtcNow,
                    CreatedBy = default,
                    UpdatedBy = default
                };

                Result<Created> insertResult = await unitOfWork.ThemeRepository.InsertAsync(themeEntity, cancellationToken).ConfigureAwait(false);
                if (insertResult.IsFailure)
                    logger.LogWarning("Failed to persist the detection of theme '{ThemeId}': {Error}", manifestResult.Value.Id, insertResult.FirstError.Description);
                else
                    themes.Add(themeEntity);
            }
            catch (IOException exception)
            {
                logger.LogWarning(exception, "Failed to install bundled theme '{ThemeId}': the theme files could not be written.", manifestResult.Value.Id);
            }
            catch (UnauthorizedAccessException exception)
            {
                logger.LogWarning(exception, "Failed to install bundled theme '{ThemeId}': access to the theme files was denied.", manifestResult.Value.Id);
            }
        }

        await CleanUpMissingThemePacksAsync(themeService, unitOfWork, themes, logger, cancellationToken).ConfigureAwait(false);
        await EnsureCurrentThemeExistsAsync(themeService, unitOfWork, themes, cancellationToken).ConfigureAwait(false);

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Restores the files of the damaged bundled themes that were not deleted by the user, and removes the damaged themes whose files could not be recovered.
    /// </summary>
    /// <param name="themeService">Injected service for the server-side storage and serving of theme packs.</param>
    /// <param name="unitOfWork">The unit of work for interacting with the theme repository.</param>
    /// <param name="themes">The installed themes read from the storage medium.</param>
    /// <param name="logger">Injected logger used for logging.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    private static async Task CleanUpMissingThemePacksAsync(IThemeService themeService, IUnitOfWork unitOfWork, IReadOnlyList<ThemeEntity> themes, ILogger logger, CancellationToken cancellationToken)
    {
        // A soft deleted theme was deleted intentionally from the admin interface, so it must never be restored automatically;
        // only a theme that was not deleted, but whose files went missing, was corrupted or removed externally, and is the only case where the files are restored.
        IReadOnlyList<ThemeEntity> brokenThemes = [.. themes.Where(theme => !theme.IsDeleted && !themeService.HasThemePack(theme.ThemeId))];
        if (brokenThemes.Count is 0)
            return;

        foreach (ThemeEntity brokenTheme in brokenThemes)
        {
            try
            {
                if (brokenTheme.InstallSource == ThemeInstallSource.Bundled)
                {
                    // The files of a bundled theme that was not deleted by the user are restored from its shipped archive.
                    Result<Success> restoreResult = await themeService.RestoreBundledThemeAsync(brokenTheme.ThemeId, cancellationToken).ConfigureAwait(false);
                    if (restoreResult.IsSuccess)
                        continue;

                    logger.LogWarning("Failed to restore the files of bundled theme '{ThemeId}': {Error}", brokenTheme.ThemeId, restoreResult.FirstError.Description);

                    // At least one bundled theme must always remain available, so the application never ends up without any theme;
                    // the last remaining bundled theme is kept even when its files cannot be recovered.
                    int availableBundledThemes = themes.Count(availableTheme => !availableTheme.IsDeleted && availableTheme.InstallSource == ThemeInstallSource.Bundled);
                    if (availableBundledThemes <= 1)
                    {
                        logger.LogWarning("Bundled theme '{ThemeId}' is the last remaining bundled theme and its files could not be restored, so it is kept.", brokenTheme.ThemeId);
                        continue;
                    }

                    // A bundled theme whose files could not be recovered is soft deleted, so it is not shown anymore, but can be restored later.
                    brokenTheme.IsDeleted = true;
                    brokenTheme.IsCurrent = null;
                    brokenTheme.UpdatedOnUtc = DateTime.UtcNow;
                    brokenTheme.UpdatedBy = default;
                    Result<Updated> updateResult = await unitOfWork.ThemeRepository.UpdateAsync(brokenTheme, cancellationToken).ConfigureAwait(false);
                    if (updateResult.IsFailure)
                        logger.LogWarning("Failed to delete theme '{ThemeId}': {Error}", brokenTheme.ThemeId, updateResult.FirstError.Description);
                }
                else
                {
                    // A user theme has no shipped archive to restore from, so damage is permanent and the theme is removed entirely.
                    logger.LogWarning("The stored files of user theme '{ThemeId}' are missing, so the theme is deleted.", brokenTheme.ThemeId);
                    brokenTheme.IsCurrent = null;
                    Result<Deleted> deleteResult = await unitOfWork.ThemeRepository.DeleteByIdAsync(brokenTheme.Id, cancellationToken).ConfigureAwait(false);
                    if (deleteResult.IsFailure)
                        logger.LogWarning("Failed to delete theme '{ThemeId}': {Error}", brokenTheme.ThemeId, deleteResult.FirstError.Description);
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception exception)
            {
                // Repairing a broken theme is best effort: an unexpected failure on one theme must not crash the host, so it is logged and the remaining broken themes are still processed.
                logger.LogWarning(exception, "Failed to repair the broken theme '{ThemeId}'.", brokenTheme.ThemeId);
            }
        }
    }

    /// <summary>
    /// Activates the configured default theme when no theme is currently active, so the application always has an active theme.
    /// </summary>
    /// <param name="themeService">Injected service for the server-side storage and serving of theme packs.</param>
    /// <param name="unitOfWork">The unit of work for interacting with the theme repository.</param>
    /// <param name="themes">The installed themes read from the storage medium.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    private static async Task EnsureCurrentThemeExistsAsync(IThemeService themeService, IUnitOfWork unitOfWork, IReadOnlyList<ThemeEntity> themes, CancellationToken cancellationToken)
    {
        if (themes.Any(theme => !theme.IsDeleted && theme.IsCurrent == true))
            return;

        // A theme whose files are present is preferred, so the activated theme can render immediately; a bundled theme
        // whose files could not be restored is kept as the fallback, so the application always has an active theme.
        ThemeEntity? defaultTheme = themes
            .Where(theme => !theme.IsDeleted)
            .OrderBy(theme => themeService.HasThemePack(theme.ThemeId) ? 0 : 1)
            .ThenBy(theme => string.Equals(theme.ThemeId, themeService.DefaultThemeId, StringComparison.OrdinalIgnoreCase) ? 0 : 1)
            .ThenBy(theme => theme.ThemeId, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault();
        if (defaultTheme is null)
            return;

        defaultTheme.IsCurrent = true;
        defaultTheme.UpdatedOnUtc = DateTime.UtcNow;
        defaultTheme.UpdatedBy = default;
        await unitOfWork.ThemeRepository.UpdateAsync(defaultTheme, cancellationToken).ConfigureAwait(false);
    }
}
