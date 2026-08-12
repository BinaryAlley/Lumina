#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.DataAccess.Entities.Plugins;
using Lumina.Application.Common.DataAccess.Repositories.Plugins;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Plugins;
using Lumina.Domain.SharedKernel.Common.Enums.Plugins;
using Lumina.Plugins.Contracts.Core.Plugins;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
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
    private readonly IReadOnlyList<string> _loadErrors;
    private readonly ILogger<PluginDetectionSyncJob> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="PluginDetectionSyncJob"/> class.
    /// </summary>
    /// <param name="serviceScopeFactory">Injected factory for creating scopes in which services are requested.</param>
    /// <param name="pluginManager">Injected manager of the plugins loaded by the host application.</param>
    /// <param name="loadErrors">The errors that occurred while loading the plugins.</param>
    /// <param name="logger">Injected logger used for logging.</param>
    public PluginDetectionSyncJob(IServiceScopeFactory serviceScopeFactory, IPluginManager pluginManager, IReadOnlyList<string> loadErrors, ILogger<PluginDetectionSyncJob> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _pluginManager = pluginManager;
        _loadErrors = loadErrors;
        _logger = logger;
    }

    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        foreach (string loadError in _loadErrors)
            _logger.LogWarning("Plugin load failure: {LoadError}", loadError);

        await using AsyncServiceScope asyncServiceScope = _serviceScopeFactory.CreateAsyncScope();
        IUnitOfWork unitOfWork = asyncServiceScope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        IPluginRepository pluginRepository = unitOfWork.GetRepository<IPluginRepository>();

        foreach (IPlugin plugin in _pluginManager.GetPlugins())
        {
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
            ErrorOr<Updated> upsertResult = await pluginRepository.UpsertAsync(pluginEntity, stoppingToken).ConfigureAwait(false);
            if (upsertResult.IsError)
                _logger.LogError("Failed to persist the detection of plugin '{PluginName}': {Error}", plugin.Name, upsertResult.FirstError.Description);
        }

        await unitOfWork.SaveChangesAsync(stoppingToken).ConfigureAwait(false);
    }
}
