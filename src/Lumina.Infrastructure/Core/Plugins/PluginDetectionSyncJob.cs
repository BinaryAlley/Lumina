#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
using Lumina.Application.Common.DataAccess.Entities.Plugins;
using Lumina.Application.Common.DataAccess.Repositories.Plugins;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Plugins;
using Lumina.Domain.SharedKernel.Common.Enums.Plugins;
using Lumina.Infrastructure.Common.Models.DTO.Plugins;
using Lumina.Plugins.Contracts.Core.Plugins;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Infrastructure.Core.Plugins;

/// <summary>
/// Background job that persists the detection of the loaded plugins into the storage medium at startup.
/// </summary>
internal sealed class PluginDetectionSyncJob : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IPluginManager _pluginManager;
    private readonly IReadOnlyList<PluginLoadErrorDto> _loadErrors;
    private readonly ILogger<PluginDetectionSyncJob> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="PluginDetectionSyncJob"/> class.
    /// </summary>
    /// <param name="serviceScopeFactory">Injected factory for creating scopes in which services are requested.</param>
    /// <param name="pluginManager">Injected manager of the plugins loaded by the host application.</param>
    /// <param name="loadErrors">The errors that occurred while loading the plugins.</param>
    /// <param name="logger">Injected logger used for logging.</param>
    public PluginDetectionSyncJob(IServiceScopeFactory serviceScopeFactory, IPluginManager pluginManager, IReadOnlyList<PluginLoadErrorDto> loadErrors, ILogger<PluginDetectionSyncJob> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _pluginManager = pluginManager;
        _loadErrors = loadErrors;
        _logger = logger;
    }

    /// <summary>
    /// This method is called when the <see cref="IHostedService"/> starts. The implementation should return a task that represents the lifetime of the long running operation(s) being performed.
    /// </summary>
    /// <param name="stoppingToken">Triggered when <see cref="IHostedService.StopAsync(CancellationToken)"/> is called.</param>
    /// <returns>A <see cref="Task"/> that represents the long running operations.</returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        foreach (PluginLoadErrorDto loadError in _loadErrors)
            _logger.LogWarning("Plugin load failure: {LoadError}", loadError.ErrorMessage);

        await using AsyncServiceScope asyncServiceScope = _serviceScopeFactory.CreateAsyncScope();
        IUnitOfWork unitOfWork = asyncServiceScope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        // the Ids of the plugin rows that are currently detected, either loaded successfully or failed to load
        HashSet<Guid> detectedPluginIds = [];

        foreach (IPlugin plugin in _pluginManager.GetPlugins())
        {
            detectedPluginIds.Add(plugin.Id);
            PluginEntity pluginEntity = new()
            {
                Id = plugin.Id,
                Name = plugin.Name,
                Author = plugin.Author,
                Version = plugin.Version.ToString(),
                Description = plugin.Description,
                LoadStatus = PluginLoadStatus.Loaded,
                CreatedOnUtc = DateTime.UtcNow,
                CreatedBy = default,
                UpdatedBy = default
            };
            Result<Updated> upsertResult = await unitOfWork.PluginRepository.UpsertAsync(pluginEntity, stoppingToken).ConfigureAwait(false);
            if (upsertResult.IsFailure)
                _logger.LogError("Failed to persist the detection of plugin '{PluginName}': {Error}", plugin.Name, upsertResult.FirstError.Description);
        }

        foreach (PluginLoadErrorDto loadError in _loadErrors)
        {
            // a failing plugin cannot report its own Id, so it is identified by a stable Id derived from its assembly file name,
            // which keeps the same row across restarts and avoids creating a new row every time the application starts
            Guid failedPluginId = DerivePluginId(loadError.AssemblyName);
            detectedPluginIds.Add(failedPluginId);
            PluginEntity pluginEntity = new()
            {
                Id = failedPluginId,
                Name = loadError.AssemblyName,
                Author = string.Empty,
                Version = string.Empty,
                Description = string.Empty,
                LoadStatus = PluginLoadStatus.FailedToLoad,
                LoadError = loadError.ErrorMessage,
                CreatedOnUtc = DateTime.UtcNow,
                CreatedBy = default,
                UpdatedBy = default
            };
            Result<Updated> upsertResult = await unitOfWork.PluginRepository.UpsertAsync(pluginEntity, stoppingToken).ConfigureAwait(false);
            if (upsertResult.IsFailure)
                _logger.LogError("Failed to persist the detection of plugin '{PluginName}': {Error}", loadError.AssemblyName, upsertResult.FirstError.Description);
        }

        // remove the rows of the plugins that are no longer detected, so that each plugin assembly has exactly one row regardless of its load status
        Result<IEnumerable<PluginEntity>> getPluginsResult = await unitOfWork.PluginRepository.GetAllAsync(stoppingToken).ConfigureAwait(false);
        if (getPluginsResult.IsFailure)
            _logger.LogError("Failed to get the detected plugins while reconciling the plugin rows.");
        else
        {
            foreach (PluginEntity plugin in getPluginsResult.Value)
            {
                if (detectedPluginIds.Contains(plugin.Id))
                    continue;
                Result<Deleted> deleteResult = await unitOfWork.PluginRepository.DeleteByIdAsync(plugin.Id, stoppingToken).ConfigureAwait(false);
                if (deleteResult.IsFailure)
                    _logger.LogError("Failed to remove the stale detection of plugin '{PluginName}': {Error}", plugin.Name, deleteResult.FirstError.Description);
            }
        }

        await unitOfWork.SaveChangesAsync(stoppingToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Derives a stable unique identifier for a plugin from the file name of its assembly, so that a plugin that fails to load
    /// is always identified by the same row across application restarts.
    /// </summary>
    /// <param name="assemblyName">The file name of the plugin assembly, without its extension.</param>
    /// <returns>The derived plugin identifier.</returns>
    private static Guid DerivePluginId(string assemblyName)
    {
        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(assemblyName));
        return new Guid([.. hash.Take(16)]);
    }
}
