#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.DataAccess.Entities.Plugins;
using Lumina.Application.Common.DataAccess.Repositories.Plugins;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Infrastructure.Authorization.Policies.LibraryOwnership;
using Lumina.Application.Common.Infrastructure.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
#endregion

namespace Lumina.Application.Core.Plugins.Commands.ReorderLibraryArtworkProviders;

/// <summary>
/// Handler for the command to reorder the artwork providers of a media library.
/// </summary>
public class ReorderLibraryArtworkProvidersCommandHandler : ICommandHandler<ReorderLibraryArtworkProvidersCommand, Result<Success>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthorizationService _authorizationService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IValidator<ReorderLibraryArtworkProvidersCommand> _validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReorderLibraryArtworkProvidersCommandHandler"/> class.
    /// </summary>
    /// <param name="authorizationService">Injected service for authorization related functionality.</param>
    /// <param name="currentUserService">Injected service to retrieve the current user information.</param>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    /// <param name="validator">Injected validator for application validation rules.</param>
    public ReorderLibraryArtworkProvidersCommandHandler(IAuthorizationService authorizationService, ICurrentUserService currentUserService, IUnitOfWork unitOfWork, IValidator<ReorderLibraryArtworkProvidersCommand> validator)
    {
        _authorizationService = authorizationService;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
        _validator = validator;
    }

    /// <summary>
    /// Handles the command to reorder the artwork providers of a media library.
    /// </summary>
    /// <param name="command">The request to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> representing either a successful operation, or an error.
    /// </returns>
    public async Task<Result<Success>> HandleAsync(ReorderLibraryArtworkProvidersCommand command, CancellationToken cancellationToken)
    {
        List<Error> validationResult = _validator.Validate(command);
        if (validationResult.Count > 0)
            return validationResult;

        // an authenticated request must always carry a user identity
        Guid? currentUserId = _currentUserService.UserId;
        if (currentUserId is null)
            return ApplicationErrors.Authorization.NotAuthorized;
        Guid userId = currentUserId.Value;

        // admins can reorder the artwork providers of any library; for everyone else, only their own libraries
        bool canAccessLibrary = await _authorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(
            userId, new LibraryOwnershipPolicyContext(command.LibraryId), cancellationToken).ConfigureAwait(false);
        if (!canAccessLibrary)
            return ApplicationErrors.Authorization.NotAuthorized;

        Result<IReadOnlyList<LibraryArtworkProviderConfigurationEntity>> getConfigurationsResult = await _unitOfWork.ArtworkProviderConfigurationRepository.GetByLibraryIdAsync(command.LibraryId, cancellationToken).ConfigureAwait(false);
        if (getConfigurationsResult.IsFailure)
            return getConfigurationsResult.Errors;

        Dictionary<Guid, LibraryArtworkProviderConfigurationEntity> configurationsByPluginId = getConfigurationsResult.Value.ToDictionary(configuration => configuration.PluginId);
        for (int rank = 0; rank < command.PluginIds.Count; rank++)
        {
            Guid pluginId = command.PluginIds[rank];
            if (configurationsByPluginId.TryGetValue(pluginId, out LibraryArtworkProviderConfigurationEntity? configuration))
            {
                configuration.Rank = rank + 1;
                Result<Updated> upsertResult = await _unitOfWork.ArtworkProviderConfigurationRepository.UpsertAsync(configuration, cancellationToken).ConfigureAwait(false);
                if (upsertResult.IsFailure)
                    return upsertResult.Errors;
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Success;
    }
}
