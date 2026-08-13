#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.DataAccess.Entities.Plugins;
using Lumina.Application.Common.DataAccess.Repositories.Plugins;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Plugins;
using Lumina.Application.Common.Mapping.Plugins;
using Lumina.Contracts.Responses.Plugins;
using Lumina.Domain.SharedKernel.Common.Errors;
using Lumina.Plugins.Contracts.Core.Plugins;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Core.Plugins.Queries.GetPluginSettings;

/// <summary>
/// Handler for the query to get the settings of a plugin and their schema.
/// </summary>
public class GetPluginSettingsQueryHandler : IQueryHandler<GetPluginSettingsQuery, ErrorOr<PluginSettingsResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPluginManager _pluginManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetPluginSettingsQueryHandler"/> class.
    /// </summary>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    /// <param name="pluginManager">Injected manager of the plugins loaded by the host application.</param>
    public GetPluginSettingsQueryHandler(IUnitOfWork unitOfWork, IPluginManager pluginManager)
    {
        _unitOfWork = unitOfWork;
        _pluginManager = pluginManager;
    }

    /// <summary>
    /// Handles the query to get the settings of a plugin and their schema.
    /// </summary>
    /// <param name="query">The request to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="ErrorOr{TValue}"/> containing either a <see cref="PluginSettingsResponse"/>, or an error.
    /// </returns>
    public async Task<ErrorOr<PluginSettingsResponse>> HandleAsync(GetPluginSettingsQuery query, CancellationToken cancellationToken)
    {
        IPluginRepository pluginRepository = _unitOfWork.GetRepository<IPluginRepository>();
        ErrorOr<PluginEntity?> getPluginResult = await pluginRepository.GetByIdAsync(query.PluginId, cancellationToken).ConfigureAwait(false);
        if (getPluginResult.IsError)
            return getPluginResult.Errors;
        PluginEntity? pluginEntity = getPluginResult.Value;
        if (pluginEntity is null)
            return Errors.Plugins.PluginNotFound;

        // the schema comes from the loaded plugin, the current values from the storage medium
        IPlugin? loadedPlugin = _pluginManager.GetPlugin(query.PluginId);
        IReadOnlyList<PluginSettingDescriptorResponse> schema = loadedPlugin is not null
            ? loadedPlugin.GetSettingsSchema().Select(setting => setting.ToResponse()).ToList()
            : [];
        return new PluginSettingsResponse(query.PluginId, schema, pluginEntity.ToSettings());
    }
}
