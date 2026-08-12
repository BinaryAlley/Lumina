#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.DataAccess.Entities.Plugins;
using Lumina.Application.Common.DataAccess.Repositories.Plugins;
using Lumina.Application.Common.DataAccess.UoW;
using Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Core.Plugins.Commands.ReorderLibraryMetadataProviders;

/// <summary>
/// Handler for the command to reorder the metadata providers of a media library.
/// </summary>
public class ReorderLibraryMetadataProvidersCommandHandler : IRequestHandler<ReorderLibraryMetadataProvidersCommand, ErrorOr<Success>>
{
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReorderLibraryMetadataProvidersCommandHandler"/> class.
    /// </summary>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    public ReorderLibraryMetadataProvidersCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Handles the command to reorder the metadata providers of a media library.
    /// </summary>
    /// <param name="request">The request to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="ErrorOr{TValue}"/> representing either a successful operation, or an error.
    /// </returns>
    public async ValueTask<ErrorOr<Success>> Handle(ReorderLibraryMetadataProvidersCommand request, CancellationToken cancellationToken)
    {
        ILibraryMetadataProviderConfigurationRepository configurationRepository = _unitOfWork.GetRepository<ILibraryMetadataProviderConfigurationRepository>();
        ErrorOr<IReadOnlyList<LibraryMetadataProviderConfigurationEntity>> getConfigurationsResult = await configurationRepository.GetByLibraryIdAsync(request.LibraryId, cancellationToken).ConfigureAwait(false);
        if (getConfigurationsResult.IsError)
            return getConfigurationsResult.Errors;

        Dictionary<Guid, LibraryMetadataProviderConfigurationEntity> configurationsByPluginId = getConfigurationsResult.Value.ToDictionary(configuration => configuration.PluginId);
        for (int rank = 0; rank < request.PluginIds.Count; rank++)
        {
            Guid pluginId = request.PluginIds[rank];
            if (configurationsByPluginId.TryGetValue(pluginId, out LibraryMetadataProviderConfigurationEntity? configuration))
            {
                configuration.Rank = rank + 1;
                ErrorOr<Updated> upsertResult = await configurationRepository.UpsertAsync(configuration, cancellationToken).ConfigureAwait(false);
                if (upsertResult.IsError)
                    return upsertResult.Errors;
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Success;
    }
}
