#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.DataAccess.Entities.Plugins;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.DTO.Filtering;
using Lumina.Application.Common.DTO.Pagination;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Plugins;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Contracts.Responses.Plugins;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
#endregion

namespace Lumina.Application.Core.Plugins.Queries.GetLibraryTypeMetadataProviders;

/// <summary>
/// Handler for the query to get the metadata providers available for a media library type.
/// </summary>
public class GetLibraryTypeMetadataProvidersQueryHandler : IQueryHandler<GetLibraryTypeMetadataProvidersQuery, Result<IReadOnlyList<LibraryMetadataProviderResponse>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMediaLibraryProviderConfigurationStore _providerConfigurationStore;
    private readonly IValidator<GetLibraryTypeMetadataProvidersQuery> _validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetLibraryTypeMetadataProvidersQueryHandler"/> class.
    /// </summary>
    /// <param name="currentUserService">Injected service to retrieve the current user information.</param>
    /// <param name="providerConfigurationStore">Injected store of the provider configurations of the media libraries.</param>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    /// <param name="validator">Injected validator for application validation rules.</param>
    public GetLibraryTypeMetadataProvidersQueryHandler(ICurrentUserService currentUserService, IMediaLibraryProviderConfigurationStore providerConfigurationStore, IUnitOfWork unitOfWork, IValidator<GetLibraryTypeMetadataProvidersQuery> validator)
    {
        _currentUserService = currentUserService;
        _providerConfigurationStore = providerConfigurationStore;
        _unitOfWork = unitOfWork;
        _validator = validator;
    }

    /// <summary>
    /// Handles the query to get the metadata providers available for a media library type.
    /// </summary>
    /// <param name="query">The query to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a collection of <see cref="LibraryMetadataProviderResponse"/>, or an error.
    /// </returns>
    public async Task<Result<IReadOnlyList<LibraryMetadataProviderResponse>>> HandleAsync(GetLibraryTypeMetadataProvidersQuery query, CancellationToken cancellationToken)
    {
        List<Error> validationResult = _validator.Validate(query);
        if (validationResult.Count > 0)
            return validationResult;

        // An authenticated request must always carry a user identity.
        if (_currentUserService.UserId is null)
            return ApplicationErrors.Authorization.NotAuthorized;

        LibraryType libraryType = Enum.Parse<LibraryType>(query.LibraryType!, ignoreCase: true);

        // Build a plugin name lookup from the detected plugins, so that each provider is returned with its display name.
        Result<PaginatedResultDto<PluginEntity>> getPluginsResult = await _unitOfWork.PluginRepository.GetAllAsync<BaseFilterDto>(cancellationToken: cancellationToken).ConfigureAwait(false);
        if (getPluginsResult.IsFailure)
            return getPluginsResult.Errors;
        Dictionary<Guid, string> pluginNames = getPluginsResult.Value.Data.ToDictionary(plugin => plugin.Id, plugin => plugin.Name);

        // The providers are returned the way a new media library of this type is configured with them: disabled, and ranked in alphabetical order.
        IReadOnlyList<Guid> pluginIds = _providerConfigurationStore.GetSupportedMetadataProviderPluginIds(libraryType);
        return pluginIds
            .Select((pluginId, index) => new LibraryMetadataProviderResponse(pluginId, pluginNames.GetValueOrDefault(pluginId) ?? string.Empty, IsEnabled: false, Rank: index + 1))
            .ToList();
    }
}
