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
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await using AsyncServiceScope asyncServiceScope = _serviceScopeFactory.CreateAsyncScope();
        IUnitOfWork unitOfWork = asyncServiceScope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        Result<IEnumerable<ThemeEntity>> getThemesResult = await unitOfWork.ThemeRepository.GetAllAsync(stoppingToken);
        if (getThemesResult.IsFailure)
        {
            _logger.LogWarning("Failed to read the installed themes: {Error}", getThemesResult.FirstError.Description);
            return;
        }

        // themes that were soft deleted by the user must not be reinstalled automatically
        HashSet<string> knownThemeIds = [.. getThemesResult.Value.Select(theme => theme.ThemeId)];

        foreach (string archivePath in _themeService.GetBundledThemeArchivePaths())
        {
            Result<ThemeManifestDto> manifestResult = await _themeService.ReadManifestFromArchiveAsync(archivePath, stoppingToken);
            if (manifestResult.IsFailure)
            {
                _logger.LogWarning("Skipping bundled theme archive {ArchivePath}: {Error}", archivePath, manifestResult.FirstError.Description);
                continue;
            }

            if (knownThemeIds.Contains(manifestResult.Value.Id))
                continue;

            await using FileStream archive = File.OpenRead(archivePath);
            Result<ThemeManifestDto> installResult = await _themeService.InstallAsync(archive, stoppingToken);
            if (installResult.IsFailure)
            {
                _logger.LogWarning("Failed to install bundled theme '{ThemeId}': {Error}", manifestResult.Value.Id, installResult.FirstError.Description);
                continue;
            }

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

            Result<Created> insertResult = await unitOfWork.ThemeRepository.InsertAsync(themeEntity, stoppingToken);
            if (insertResult.IsFailure)
                _logger.LogWarning("Failed to persist the detection of theme '{ThemeId}': {Error}", manifestResult.Value.Id, insertResult.FirstError.Description);
        }

        await unitOfWork.SaveChangesAsync(stoppingToken);
    }
}
