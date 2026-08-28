#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Themes;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Models.DTO.Themes;
using Lumina.Application.Common.Infrastructure.Themes;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.Themes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
/// Background job that installs the bundled themes into the storage medium and the theme storage at startup.
/// </summary>
internal sealed class ThemeDetectionSyncJob : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IThemeService _themeService;
    private readonly ILogger<ThemeDetectionSyncJob> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ThemeDetectionSyncJob"/> class.
    /// </summary>
    /// <param name="serviceScopeFactory">Injected factory for creating scopes in which services are requested.</param>
    /// <param name="themeService">Injected service for the server-side storage and serving of theme packs.</param>
    /// <param name="logger">Injected logger used for logging.</param>
    public ThemeDetectionSyncJob(IServiceScopeFactory serviceScopeFactory, IThemeService themeService, ILogger<ThemeDetectionSyncJob> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _themeService = themeService;
        _logger = logger;
    }

    /// <summary>
    /// Method called when the background service starts.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await using AsyncServiceScope asyncServiceScope = _serviceScopeFactory.CreateAsyncScope();
        IUnitOfWork unitOfWork = asyncServiceScope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        Result<IEnumerable<ThemeEntity>> getThemesResult = await unitOfWork.ThemeRepository.GetAllAsync(cancellationToken);
        if (getThemesResult.IsFailure)
        {
            _logger.LogWarning("Failed to read the installed themes: {Error}", getThemesResult.FirstError.Description);
            return;
        }

        // themes that were soft deleted by the user must not be reinstalled automatically
        List<ThemeEntity> themes = [.. getThemesResult.Value];

        foreach (string archivePath in _themeService.GetBundledThemeArchivePaths())
        {
            Result<ThemeManifestDto> manifestResult = await _themeService.ReadManifestFromArchiveAsync(archivePath, cancellationToken);
            if (manifestResult.IsFailure)
            {
                _logger.LogWarning("Skipping bundled theme archive {ArchivePath}: {Error}", archivePath, manifestResult.FirstError.Description);
                continue;
            }

            ThemeEntity? existingTheme = themes.Where(theme => theme.ThemeId == manifestResult.Value.Id).FirstOrDefault();

            // a theme that was soft deleted by the user must not be reinstalled automatically; an installed bundled theme
            // whose files are present is already in sync, so only a bundled theme whose files went missing gets its files reinstalled
            if (existingTheme is not null && (existingTheme.IsDeleted || existingTheme.InstallSource != ThemeInstallSource.Bundled || _themeService.HasThemePack(existingTheme.ThemeId)))
                continue;

            // installing a bundled theme at startup is best effort: a file system failure here must never crash the host,
            // because the default BackgroundServiceExceptionBehavior (StopHost) shuts down the whole application and
            // disposes its service provider, making every later request fail
            try
            {
                await using FileStream archive = File.OpenRead(archivePath);
                Result<ThemeManifestDto> installResult = await _themeService.InstallAsync(archive, cancellationToken);
                if (installResult.IsFailure)
                {
                    _logger.LogWarning("Failed to install bundled theme '{ThemeId}': {Error}", manifestResult.Value.Id, installResult.FirstError.Description);
                    continue;
                }

                // a known theme only gets its missing files reinstalled, so no new row is inserted
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

                Result<Created> insertResult = await unitOfWork.ThemeRepository.InsertAsync(themeEntity, cancellationToken);
                if (insertResult.IsFailure)
                    _logger.LogWarning("Failed to persist the detection of theme '{ThemeId}': {Error}", manifestResult.Value.Id, insertResult.FirstError.Description);
                else
                    themes.Add(themeEntity);
            }
            catch (IOException exception)
            {
                _logger.LogWarning(exception, "Failed to install bundled theme '{ThemeId}': the theme files could not be written.", manifestResult.Value.Id);
            }
            catch (UnauthorizedAccessException exception)
            {
                _logger.LogWarning(exception, "Failed to install bundled theme '{ThemeId}': access to the theme files was denied.", manifestResult.Value.Id);
            }
        }

        await CleanUpMissingThemePacksAsync(unitOfWork, themes, cancellationToken);
        await EnsureCurrentThemeExistsAsync(unitOfWork, themes, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Restores the files of the damaged bundled themes that were not deleted by the user, and removes the damaged themes whose files could not be recovered.
    /// </summary>
    /// <param name="unitOfWork">The unit of work for interacting with the theme repository.</param>
    /// <param name="themes">The installed themes read from the storage medium.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    private async Task CleanUpMissingThemePacksAsync(IUnitOfWork unitOfWork, IReadOnlyList<ThemeEntity> themes, CancellationToken cancellationToken)
    {
        // a soft deleted theme was deleted intentionally from the admin interface, so it must never be restored
        // automatically; only a theme that was not deleted, but whose files went missing, was corrupted or removed
        // externally, and is the only case where the files are restored
        IReadOnlyList<ThemeEntity> brokenThemes = [.. themes.Where(theme => !theme.IsDeleted && !_themeService.HasThemePack(theme.ThemeId))];
        if (brokenThemes.Count is 0)
            return;

        foreach (ThemeEntity brokenTheme in brokenThemes)
        {
            if (brokenTheme.InstallSource == ThemeInstallSource.Bundled)
            {
                // the files of a bundled theme that was not deleted by the user are restored from its shipped archive
                Result<Success> restoreResult = await _themeService.RestoreBundledThemeAsync(brokenTheme.ThemeId, cancellationToken);
                if (restoreResult.IsSuccess)
                    continue;

                _logger.LogWarning("Failed to restore the files of bundled theme '{ThemeId}': {Error}", brokenTheme.ThemeId, restoreResult.FirstError.Description);

                // at least one bundled theme must always remain available, so the application never ends up without any theme;
                // the last remaining bundled theme is kept even when its files cannot be recovered
                int availableBundledThemes = themes.Count(availableTheme => !availableTheme.IsDeleted && availableTheme.InstallSource == ThemeInstallSource.Bundled);
                if (availableBundledThemes <= 1)
                {
                    _logger.LogWarning("Bundled theme '{ThemeId}' is the last remaining bundled theme and its files could not be restored, so it is kept.", brokenTheme.ThemeId);
                    continue;
                }

                // a bundled theme whose files could not be recovered is soft deleted, so it is not shown anymore, but can be restored later
                brokenTheme.IsDeleted = true;
                brokenTheme.IsCurrent = null;
                brokenTheme.UpdatedOnUtc = DateTime.UtcNow;
                brokenTheme.UpdatedBy = default;
                Result<Updated> updateResult = await unitOfWork.ThemeRepository.UpdateAsync(brokenTheme, cancellationToken);
                if (updateResult.IsFailure)
                    _logger.LogWarning("Failed to delete theme '{ThemeId}': {Error}", brokenTheme.ThemeId, updateResult.FirstError.Description);
            }
            else
            {
                // a user theme has no shipped archive to restore from, so damage is permanent and the theme is removed entirely
                _logger.LogWarning("The stored files of user theme '{ThemeId}' are missing, so the theme is deleted.", brokenTheme.ThemeId);
                brokenTheme.IsCurrent = null;
                Result<Deleted> deleteResult = await unitOfWork.ThemeRepository.DeleteByIdAsync(brokenTheme.Id, cancellationToken);
                if (deleteResult.IsFailure)
                    _logger.LogWarning("Failed to delete theme '{ThemeId}': {Error}", brokenTheme.ThemeId, deleteResult.FirstError.Description);
            }
        }
    }

    /// <summary>
    /// Activates the configured default theme when no theme is currently active, so the application always has an active theme.
    /// </summary>
    /// <param name="unitOfWork">The unit of work for interacting with the theme repository.</param>
    /// <param name="themes">The installed themes read from the storage medium.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    private async Task EnsureCurrentThemeExistsAsync(IUnitOfWork unitOfWork, IReadOnlyList<ThemeEntity> themes, CancellationToken cancellationToken)
    {
        if (themes.Any(theme => !theme.IsDeleted && theme.IsCurrent == true))
            return;

        // a theme whose files are present is preferred, so the activated theme can render immediately; a bundled theme
        // whose files could not be restored is kept as the fallback, so the application always has an active theme
        ThemeEntity? defaultTheme = themes
            .Where(theme => !theme.IsDeleted)
            .OrderBy(theme => _themeService.HasThemePack(theme.ThemeId) ? 0 : 1)
            .ThenBy(theme => string.Equals(theme.ThemeId, _themeService.DefaultThemeId, StringComparison.OrdinalIgnoreCase) ? 0 : 1)
            .ThenBy(theme => theme.ThemeId, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault();
        if (defaultTheme is null)
            return;

        defaultTheme.IsCurrent = true;
        defaultTheme.UpdatedOnUtc = DateTime.UtcNow;
        defaultTheme.UpdatedBy = default;
        await unitOfWork.ThemeRepository.UpdateAsync(defaultTheme, cancellationToken);
    }
}
