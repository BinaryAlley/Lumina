#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.DataAccess.Entities.Plugins;
using Lumina.Application.Common.DataAccess.Repositories.Plugins;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Mapping.Plugins;
using Lumina.Contracts.Responses.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Core.Plugins.Queries.GetLibraryMetadataProviders;

/// <summary>
/// Handler for the query to get the metadata providers configured for a media library.
/// </summary>
public class GetLibraryMetadataProvidersQueryHandler : IQueryHandler<GetLibraryMetadataProvidersQuery, Result<IReadOnlyList<LibraryMetadataProviderResponse>>>
{
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetLibraryMetadataProvidersQueryHandler"/> class.
    /// </summary>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    public GetLibraryMetadataProvidersQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Handles the query to get the metadata providers configured for a media library.
    /// </summary>
    /// <param name="query">The request to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a collection of <see cref="LibraryMetadataProviderResponse"/>, or an error.
    /// </returns>
    public async Task<Result<IReadOnlyList<LibraryMetadataProviderResponse>>> HandleAsync(GetLibraryMetadataProvidersQuery query, CancellationToken cancellationToken)
    {
        ILibraryMetadataProviderConfigurationRepository configurationRepository = _unitOfWork.GetRepository<ILibraryMetadataProviderConfigurationRepository>();
        Result<IReadOnlyList<LibraryMetadataProviderConfigurationEntity>> getConfigurationsResult = await configurationRepository.GetByLibraryIdAsync(query.LibraryId, cancellationToken).ConfigureAwait(false);
        if (getConfigurationsResult.IsFailure)
            return getConfigurationsResult.Errors;

        // build a plugin name lookup from the detected plugins
        IPluginRepository pluginRepository = _unitOfWork.GetRepository<IPluginRepository>();
        Result<IEnumerable<PluginEntity>> getPluginsResult = await pluginRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
        if (getPluginsResult.IsFailure)
            return getPluginsResult.Errors;
        Dictionary<Guid, string> pluginNames = getPluginsResult.Value.ToDictionary(plugin => plugin.Id, plugin => plugin.Name);

        return getConfigurationsResult.Value
            .OrderBy(configuration => configuration.Rank)
            .Select(configuration => configuration.ToResponse(pluginNames.GetValueOrDefault(configuration.PluginId) ?? string.Empty))
            .ToList();
    }
}
