#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.DataAccess.Entities.Plugins;
using Lumina.Application.Common.DataAccess.Repositories.Plugins;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Validation;
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
public class ReorderLibraryMetadataProvidersCommandHandler : ICommandHandler<ReorderLibraryMetadataProvidersCommand, Result<Success>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<ReorderLibraryMetadataProvidersCommand> _validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReorderLibraryMetadataProvidersCommandHandler"/> class.
    /// </summary>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    /// <param name="validator">Injected validator for application validation rules.</param>
    public ReorderLibraryMetadataProvidersCommandHandler(IUnitOfWork unitOfWork, IValidator<ReorderLibraryMetadataProvidersCommand> validator)
    {
        _unitOfWork = unitOfWork;
        _validator = validator;
    }

    /// <summary>
    /// Handles the command to reorder the metadata providers of a media library.
    /// </summary>
    /// <param name="command">The request to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> representing either a successful operation, or an error.
    /// </returns>
    public async Task<Result<Success>> HandleAsync(ReorderLibraryMetadataProvidersCommand command, CancellationToken cancellationToken)
    {
        List<Error> validationResult = _validator.Validate(command);
        if (validationResult.Count > 0)
            return validationResult;

        Result<IReadOnlyList<LibraryMetadataProviderConfigurationEntity>> getConfigurationsResult = await _unitOfWork.LibraryMetadataProviderConfigurationRepository.GetByLibraryIdAsync(command.LibraryId, cancellationToken).ConfigureAwait(false);
        if (getConfigurationsResult.IsFailure)
            return getConfigurationsResult.Errors;

        Dictionary<Guid, LibraryMetadataProviderConfigurationEntity> configurationsByPluginId = getConfigurationsResult.Value.ToDictionary(configuration => configuration.PluginId);
        for (int rank = 0; rank < command.PluginIds.Count; rank++)
        {
            Guid pluginId = command.PluginIds[rank];
            if (configurationsByPluginId.TryGetValue(pluginId, out LibraryMetadataProviderConfigurationEntity? configuration))
            {
                configuration.Rank = rank + 1;
                Result<Updated> upsertResult = await _unitOfWork.LibraryMetadataProviderConfigurationRepository.UpsertAsync(configuration, cancellationToken).ConfigureAwait(false);
                if (upsertResult.IsFailure)
                    return upsertResult.Errors;
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Success;
    }
}
