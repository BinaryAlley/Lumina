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

namespace Lumina.Application.Core.Plugins.Commands.SetLibraryMetadataProviderEnabled;

/// <summary>
/// Handler for the command to enable or disable a metadata provider for a media library.
/// </summary>
public class SetLibraryMetadataProviderEnabledCommandHandler : IRequestHandler<SetLibraryMetadataProviderEnabledCommand, ErrorOr<Success>>
{
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes a new instance of the <see cref="SetLibraryMetadataProviderEnabledCommandHandler"/> class.
    /// </summary>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    public SetLibraryMetadataProviderEnabledCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Handles the command to enable or disable a metadata provider for a media library.
    /// </summary>
    /// <param name="request">The request to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="ErrorOr{TValue}"/> representing either a successful operation, or an error.
    /// </returns>
    public async ValueTask<ErrorOr<Success>> Handle(SetLibraryMetadataProviderEnabledCommand request, CancellationToken cancellationToken)
    {
        ILibraryMetadataProviderConfigurationRepository configurationRepository = _unitOfWork.GetRepository<ILibraryMetadataProviderConfigurationRepository>();
        ErrorOr<LibraryMetadataProviderConfigurationEntity?> getConfigurationResult = await configurationRepository.GetByLibraryAndPluginIdAsync(request.LibraryId, request.PluginId, cancellationToken).ConfigureAwait(false);
        if (getConfigurationResult.IsError)
            return getConfigurationResult.Errors;

        LibraryMetadataProviderConfigurationEntity configuration;
        if (getConfigurationResult.Value is not null)
        {
            configuration = getConfigurationResult.Value;
            configuration.IsEnabled = request.IsEnabled;
        }
        else
        {
            // compute the rank to assign to the new configuration, appending it after the existing ones
            ErrorOr<IReadOnlyList<LibraryMetadataProviderConfigurationEntity>> getConfigurationsResult = await configurationRepository.GetByLibraryIdAsync(request.LibraryId, cancellationToken).ConfigureAwait(false);
            if (getConfigurationsResult.IsError)
                return getConfigurationsResult.Errors;
            int nextRank = getConfigurationsResult.Value.Count == 0 ? 1 : getConfigurationsResult.Value.Max(configuration => configuration.Rank) + 1;
            configuration = new LibraryMetadataProviderConfigurationEntity
            {
                Id = Guid.NewGuid(),
                LibraryId = request.LibraryId,
                PluginId = request.PluginId,
                IsEnabled = request.IsEnabled,
                Rank = nextRank,
                CreatedOnUtc = default,
                CreatedBy = default,
                UpdatedBy = default
            };
        }

        ErrorOr<Updated> upsertResult = await configurationRepository.UpsertAsync(configuration, cancellationToken).ConfigureAwait(false);
        if (upsertResult.IsError)
            return upsertResult.Errors;
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Success;
    }
}
