#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.DataAccess.Entities.Plugins;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.DTO.Filtering;
using Lumina.Application.Common.DTO.Pagination;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Infrastructure.Authorization.Policies.LibraryOwnership;
using Lumina.Application.Common.Infrastructure.Plugins;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Mapping.Plugins;
using Lumina.Contracts.Responses.Plugins;
using Lumina.Domain.Common.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
#endregion

namespace Lumina.Application.Core.Plugins.Queries.GetLibraryMetadataProviders;

/// <summary>
/// Handler for the query to get the metadata providers configured for a media library.
/// </summary>
public class GetLibraryMetadataProvidersQueryHandler : IQueryHandler<GetLibraryMetadataProvidersQuery, Result<IReadOnlyList<LibraryMetadataProviderResponse>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthorizationService _authorizationService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMediaLibraryProviderConfigurationStore _providerConfigurationStore;
    private readonly IValidator<GetLibraryMetadataProvidersQuery> _validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetLibraryMetadataProvidersQueryHandler"/> class.
    /// </summary>
    /// <param name="authorizationService">Injected service for authorization related functionality.</param>
    /// <param name="currentUserService">Injected service to retrieve the current user information.</param>
    /// <param name="providerConfigurationStore">Injected store of the provider configurations of the media libraries.</param>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    /// <param name="validator">Injected validator for application validation rules.</param>
    public GetLibraryMetadataProvidersQueryHandler(IAuthorizationService authorizationService, ICurrentUserService currentUserService, IMediaLibraryProviderConfigurationStore providerConfigurationStore, IUnitOfWork unitOfWork, IValidator<GetLibraryMetadataProvidersQuery> validator)
    {
        _authorizationService = authorizationService;
        _currentUserService = currentUserService;
        _providerConfigurationStore = providerConfigurationStore;
        _unitOfWork = unitOfWork;
        _validator = validator;
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
        List<Error> validationResult = _validator.Validate(query);
        if (validationResult.Count > 0)
            return validationResult;

        // An authenticated request must always carry a user identity.
        Guid? currentUserId = _currentUserService.UserId;
        if (currentUserId is null)
            return ApplicationErrors.Authorization.NotAuthorized;
        Guid userId = currentUserId.Value;

        // Admins can read the metadata providers of any library; for everyone else, only their own libraries.
        bool canAccessLibrary = await _authorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(
            userId, new LibraryOwnershipPolicyContext(query.LibraryId), cancellationToken).ConfigureAwait(false);
        if (!canAccessLibrary)
            return ApplicationErrors.Authorization.NotAuthorized;

        // Load the metadata provider configurations registered for the library, so that only the providers it actually has are returned.
        Result<IReadOnlyList<LibraryMetadataProviderConfigurationEntity>> getConfigurationsResult = await _providerConfigurationStore.GetConfigurationsAsync(query.LibraryId, cancellationToken).ConfigureAwait(false);
        if (getConfigurationsResult.IsFailure)
            return getConfigurationsResult.Errors;

        // Build a plugin name lookup from the detected plugins, so that each configuration can be returned with its provider's display name.
        Result<PaginatedResultDto<PluginEntity>> getPluginsResult = await _unitOfWork.PluginRepository.GetAllAsync<BaseFilterDto>(cancellationToken: cancellationToken).ConfigureAwait(false);
        if (getPluginsResult.IsFailure)
            return getPluginsResult.Errors;
        Dictionary<Guid, string> pluginNames = getPluginsResult.Value.Data.ToDictionary(plugin => plugin.Id, plugin => plugin.Name);

        // Return the configurations ordered by their rank, so the client sees the providers in the order they are applied.
        return getConfigurationsResult.Value
            .OrderBy(configuration => configuration.Rank)
            .Select(configuration => configuration.ToResponse(pluginNames.GetValueOrDefault(configuration.PluginId) ?? string.Empty))
            .ToList();
    }
}
