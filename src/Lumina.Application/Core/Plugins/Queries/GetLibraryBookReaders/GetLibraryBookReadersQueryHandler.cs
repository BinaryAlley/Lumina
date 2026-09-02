#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.DataAccess.Entities.Plugins;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Infrastructure.Authorization.Policies.LibraryOwnership;
using Lumina.Application.Common.Infrastructure.Reading;
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

namespace Lumina.Application.Core.Plugins.Queries.GetLibraryBookReaders;

/// <summary>
/// Handler for the query to get the book readers configured for a media library.
/// </summary>
public class GetLibraryBookReadersQueryHandler : IQueryHandler<GetLibraryBookReadersQuery, Result<IReadOnlyList<LibraryBookReaderResponse>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthorizationService _authorizationService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IBookReaderRegistry _bookReaderRegistry;
    private readonly IValidator<GetLibraryBookReadersQuery> _validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetLibraryBookReadersQueryHandler"/> class.
    /// </summary>
    /// <param name="authorizationService">Injected service for authorization related functionality.</param>
    /// <param name="currentUserService">Injected service to retrieve the current user information.</param>
    /// <param name="bookReaderRegistry">Injected registry of the book readers provided by the loaded plugins.</param>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    /// <param name="validator">Injected validator for application validation rules.</param>
    public GetLibraryBookReadersQueryHandler(IAuthorizationService authorizationService, ICurrentUserService currentUserService, IBookReaderRegistry bookReaderRegistry, IUnitOfWork unitOfWork, IValidator<GetLibraryBookReadersQuery> validator)
    {
        _authorizationService = authorizationService;
        _currentUserService = currentUserService;
        _bookReaderRegistry = bookReaderRegistry;
        _unitOfWork = unitOfWork;
        _validator = validator;
    }

    /// <summary>
    /// Handles the query to get the book readers configured for a media library.
    /// </summary>
    /// <param name="query">The request to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a collection of <see cref="LibraryBookReaderResponse"/>, or an error.
    /// </returns>
    public async Task<Result<IReadOnlyList<LibraryBookReaderResponse>>> HandleAsync(GetLibraryBookReadersQuery query, CancellationToken cancellationToken)
    {
        List<Error> validationResult = _validator.Validate(query);
        if (validationResult.Count > 0)
            return validationResult;

        // An authenticated request must always carry a user identity.
        Guid? currentUserId = _currentUserService.UserId;
        if (currentUserId is null)
            return ApplicationErrors.Authorization.NotAuthorized;
        Guid userId = currentUserId.Value;

        // Admins can read the book readers of any library; for everyone else, only their own libraries.
        bool canAccessLibrary = await _authorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(
            userId, new LibraryOwnershipPolicyContext(query.LibraryId), cancellationToken).ConfigureAwait(false);
        if (!canAccessLibrary)
            return ApplicationErrors.Authorization.NotAuthorized;

        Result<IReadOnlyList<LibraryBookReaderConfigurationEntity>> getConfigurationsResult = await _unitOfWork.LibraryBookReaderConfigurationRepository.GetByLibraryIdAsync(query.LibraryId, cancellationToken).ConfigureAwait(false);
        if (getConfigurationsResult.IsFailure)
            return getConfigurationsResult.Errors;

        // Build a plugin name lookup from the detected plugins.
        Result<IEnumerable<PluginEntity>> getPluginsResult = await _unitOfWork.PluginRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
        if (getPluginsResult.IsFailure)
            return getPluginsResult.Errors;
        Dictionary<Guid, string> pluginNames = getPluginsResult.Value.ToDictionary(plugin => plugin.Id, plugin => plugin.Name);

        IReadOnlyDictionary<Guid, IReadOnlyList<string>> supportedExtensionsByPluginId = _bookReaderRegistry.GetSupportedExtensionsByPluginId();

        return getConfigurationsResult.Value
            .OrderBy(configuration => configuration.IsEnabled ? 0 : 1)
            .ThenBy(configuration => pluginNames.GetValueOrDefault(configuration.PluginId) ?? string.Empty, StringComparer.OrdinalIgnoreCase)
            .Select(configuration => configuration.ToResponse(
                pluginNames.GetValueOrDefault(configuration.PluginId) ?? string.Empty,
                supportedExtensionsByPluginId.GetValueOrDefault(configuration.PluginId) ?? []))
            .ToList();
    }
}
