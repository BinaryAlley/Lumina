#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
using Lumina.Application.Common.DataAccess.Entities.Plugins;
using Lumina.Application.Common.DataAccess.Repositories.Plugins;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Mapping.Plugins;
using Lumina.Plugins.Contracts.Core.Plugins;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Infrastructure.Core.Plugins;

/// <summary>
/// Reads the settings persisted by the host for a plugin.
/// </summary>
internal sealed class PluginSettingsStore : IPluginSettingsStore
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<PluginSettingsStore> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="PluginSettingsStore"/> class.
    /// </summary>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    /// <param name="logger">Injected logger used to report the failures to read the plugin settings.</param>
    public PluginSettingsStore(IUnitOfWork unitOfWork, ILogger<PluginSettingsStore> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyDictionary<string, string>?> GetSettingsAsync(Guid pluginId, CancellationToken cancellationToken)
    {
        Result<PluginEntity?> getPluginResult = await _unitOfWork.PluginRepository.GetByIdAsync(pluginId, cancellationToken).ConfigureAwait(false);
        if (getPluginResult.IsFailure)
        {
            _logger.LogWarning("Failed to read the settings of the plugin with Id '{PluginId}': {Error}", pluginId, getPluginResult.FirstError.Description);
            return null;
        }

        return getPluginResult.Value?.ToSettings();
    }
}
