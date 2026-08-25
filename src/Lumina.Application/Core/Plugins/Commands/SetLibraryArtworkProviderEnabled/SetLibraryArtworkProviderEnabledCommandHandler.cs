#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.DataAccess.Entities.Plugins;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Infrastructure.Authorization.Policies.LibraryOwnership;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Domain.Common.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
#endregion

namespace Lumina.Application.Core.Plugins.Commands.SetLibraryArtworkProviderEnabled;

/// <summary>
/// Handler for the command to enable or disable an artwork provider for a media library.
/// </summary>
public class SetLibraryArtworkProviderEnabledCommandHandler : ICommandHandler<SetLibraryArtworkProviderEnabledCommand, Result<Success>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthorizationService _authorizationService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IValidator<SetLibraryArtworkProviderEnabledCommand> _validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="SetLibraryArtworkProviderEnabledCommandHandler"/> class.
    /// </summary>
    /// <param name="authorizationService">Injected service for authorization related functionality.</param>
    /// <param name="currentUserService">Injected service to retrieve the current user information.</param>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    /// <param name="validator">Injected validator for application validation rules.</param>
    public SetLibraryArtworkProviderEnabledCommandHandler(IAuthorizationService authorizationService, ICurrentUserService currentUserService, IUnitOfWork unitOfWork, IValidator<SetLibraryArtworkProviderEnabledCommand> validator)
    {
        _authorizationService = authorizationService;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
        _validator = validator;
    }

    /// <summary>
    /// Handles the command to enable or disable an artwork provider for a media library.
    /// </summary>
    /// <param name="command">The request to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> representing either a successful operation, or an error.
    /// </returns>
    public async Task<Result<Success>> HandleAsync(SetLibraryArtworkProviderEnabledCommand command, CancellationToken cancellationToken)
    {
        List<Error> validationResult = _validator.Validate(command);
        if (validationResult.Count > 0)
            return validationResult;

        // an authenticated request must always carry a user identity
        Guid? currentUserId = _currentUserService.UserId;
        if (currentUserId is null)
            return ApplicationErrors.Authorization.NotAuthorized;
        Guid userId = currentUserId.Value;

        // admins can configure the artwork providers of any library; for everyone else, only their own libraries
        bool canAccessLibrary = await _authorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(
            userId, new LibraryOwnershipPolicyContext(command.LibraryId), cancellationToken).ConfigureAwait(false);
        if (!canAccessLibrary)
            return ApplicationErrors.Authorization.NotAuthorized;

        Result<LibraryArtworkProviderConfigurationEntity?> getConfigurationResult = await _unitOfWork.ArtworkProviderConfigurationRepository.GetByLibraryAndPluginIdAsync(command.LibraryId, command.PluginId, cancellationToken).ConfigureAwait(false);
        if (getConfigurationResult.IsFailure)
            return getConfigurationResult.Errors;

        LibraryArtworkProviderConfigurationEntity configuration;
        if (getConfigurationResult.Value is not null)
        {
            configuration = getConfigurationResult.Value;
            configuration.IsEnabled = command.IsEnabled;
        }
        else
        {
            // compute the rank to assign to the new configuration, appending it after the existing ones
            Result<IReadOnlyList<LibraryArtworkProviderConfigurationEntity>> getConfigurationsResult = await _unitOfWork.ArtworkProviderConfigurationRepository.GetByLibraryIdAsync(command.LibraryId, cancellationToken).ConfigureAwait(false);
            if (getConfigurationsResult.IsFailure)
                return getConfigurationsResult.Errors;
            int nextRank = getConfigurationsResult.Value.Count == 0 ? 1 : getConfigurationsResult.Value.Max(configuration => configuration.Rank) + 1;
            configuration = new LibraryArtworkProviderConfigurationEntity
            {
                Id = Guid.NewGuid(),
                LibraryId = command.LibraryId,
                PluginId = command.PluginId,
                IsEnabled = command.IsEnabled,
                Rank = nextRank,
                CreatedOnUtc = default,
                CreatedBy = default,
                UpdatedBy = default
            };
        }

        Result<Updated> upsertResult = await _unitOfWork.ArtworkProviderConfigurationRepository.UpsertAsync(configuration, cancellationToken).ConfigureAwait(false);
        if (upsertResult.IsFailure)
            return upsertResult.Errors;
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Success;
    }
}
