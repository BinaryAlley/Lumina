#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.DataAccess.Entities.Plugins;
using Lumina.Application.Common.DataAccess.Repositories.Plugins;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Mapping.Plugins;
using Lumina.Contracts.Responses.Plugins;
using Mediator;
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
public class GetLibraryMetadataProvidersQueryHandler : IRequestHandler<GetLibraryMetadataProvidersQuery, ErrorOr<IReadOnlyList<LibraryMetadataProviderResponse>>>
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
    /// <param name="request">The request to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="ErrorOr{TValue}"/> containing either a collection of <see cref="LibraryMetadataProviderResponse"/>, or an error.
    /// </returns>
    public async ValueTask<ErrorOr<IReadOnlyList<LibraryMetadataProviderResponse>>> Handle(GetLibraryMetadataProvidersQuery request, CancellationToken cancellationToken)
    {
        ILibraryMetadataProviderConfigurationRepository configurationRepository = _unitOfWork.GetRepository<ILibraryMetadataProviderConfigurationRepository>();
        ErrorOr<IReadOnlyList<LibraryMetadataProviderConfigurationEntity>> getConfigurationsResult = await configurationRepository.GetByLibraryIdAsync(request.LibraryId, cancellationToken).ConfigureAwait(false);
        if (getConfigurationsResult.IsError)
            return getConfigurationsResult.Errors;

        // build a plugin name lookup from the detected plugins
        IPluginRepository pluginRepository = _unitOfWork.GetRepository<IPluginRepository>();
        ErrorOr<IEnumerable<PluginEntity>> getPluginsResult = await pluginRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
        if (getPluginsResult.IsError)
            return getPluginsResult.Errors;
        Dictionary<Guid, string> pluginNames = getPluginsResult.Value.ToDictionary(plugin => plugin.Id, plugin => plugin.Name);

        return getConfigurationsResult.Value
            .OrderBy(configuration => configuration.Rank)
            .Select(configuration => configuration.ToResponse(pluginNames.GetValueOrDefault(configuration.PluginId) ?? string.Empty))
            .ToList();
    }
}
