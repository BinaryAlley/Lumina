#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.DataAccess.Entities.Plugins;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.DTO.Filtering;
using Lumina.Application.Common.DTO.Pagination;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Plugins;
using Lumina.Application.Common.Infrastructure.Reading;
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

namespace Lumina.Application.Core.Plugins.Queries.GetLibraryTypeBookReaders;

/// <summary>
/// Handler for the query to get the book readers available for a media library type.
/// </summary>
public class GetLibraryTypeBookReadersQueryHandler : IQueryHandler<GetLibraryTypeBookReadersQuery, Result<IReadOnlyList<LibraryBookReaderResponse>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMediaLibraryProviderConfigurationStore _providerConfigurationStore;
    private readonly IBookReaderRegistry _bookReaderRegistry;
    private readonly IValidator<GetLibraryTypeBookReadersQuery> _validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetLibraryTypeBookReadersQueryHandler"/> class.
    /// </summary>
    /// <param name="currentUserService">Injected service to retrieve the current user information.</param>
    /// <param name="providerConfigurationStore">Injected store of the provider configurations of the media libraries.</param>
    /// <param name="bookReaderRegistry">Injected registry of the book readers provided by the loaded plugins.</param>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    /// <param name="validator">Injected validator for application validation rules.</param>
    public GetLibraryTypeBookReadersQueryHandler(ICurrentUserService currentUserService, IMediaLibraryProviderConfigurationStore providerConfigurationStore, IBookReaderRegistry bookReaderRegistry, IUnitOfWork unitOfWork, IValidator<GetLibraryTypeBookReadersQuery> validator)
    {
        _currentUserService = currentUserService;
        _providerConfigurationStore = providerConfigurationStore;
        _bookReaderRegistry = bookReaderRegistry;
        _unitOfWork = unitOfWork;
        _validator = validator;
    }

    /// <summary>
    /// Handles the query to get the book readers available for a media library type.
    /// </summary>
    /// <param name="query">The query to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a collection of <see cref="LibraryBookReaderResponse"/>, or an error.
    /// </returns>
    public async Task<Result<IReadOnlyList<LibraryBookReaderResponse>>> HandleAsync(GetLibraryTypeBookReadersQuery query, CancellationToken cancellationToken)
    {
        List<Error> validationResult = _validator.Validate(query);
        if (validationResult.Count > 0)
            return validationResult;

        // An authenticated request must always carry a user identity.
        if (_currentUserService.UserId is null)
            return ApplicationErrors.Authorization.NotAuthorized;

        LibraryType libraryType = Enum.Parse<LibraryType>(query.LibraryType!, ignoreCase: true);

        // Build a plugin name lookup from the detected plugins, so that each reader is returned with its display name.
        Result<PaginatedResultDto<PluginEntity>> getPluginsResult = await _unitOfWork.PluginRepository.GetAllAsync<BaseFilterDto>(cancellationToken: cancellationToken).ConfigureAwait(false);
        if (getPluginsResult.IsFailure)
            return getPluginsResult.Errors;
        Dictionary<Guid, string> pluginNames = getPluginsResult.Value.Data.ToDictionary(plugin => plugin.Id, plugin => plugin.Name);

        IReadOnlyDictionary<Guid, IReadOnlyList<string>> supportedExtensionsByPluginId = _bookReaderRegistry.GetSupportedExtensionsByPluginId();

        // The readers are returned the way a new media library of this type is configured with them: disabled, in alphabetical order.
        IReadOnlyList<Guid> pluginIds = _providerConfigurationStore.GetSupportedBookReaderPluginIds(libraryType);
        return pluginIds
            .Select(pluginId => new LibraryBookReaderResponse(
                pluginId,
                pluginNames.GetValueOrDefault(pluginId) ?? string.Empty,
                supportedExtensionsByPluginId.GetValueOrDefault(pluginId) ?? [],
                IsEnabled: false))
            .ToList();
    }
}
