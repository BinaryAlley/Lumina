#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.DataAccess.Entities.Plugins;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Infrastructure.Authorization.Policies.LibraryOwnership;
using Lumina.Application.Common.Infrastructure.Reading;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Domain.Common.Primitives;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
#endregion

namespace Lumina.Application.Core.Plugins.Commands.SetLibraryBookReaderEnabled;

/// <summary>
/// Handler for the command to enable or disable a book reader for a media library.
/// </summary>
public class SetLibraryBookReaderEnabledCommandHandler : ICommandHandler<SetLibraryBookReaderEnabledCommand, Result<Success>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthorizationService _authorizationService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IBookReaderEnablementCache _enablementCache;
    private readonly IValidator<SetLibraryBookReaderEnabledCommand> _validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="SetLibraryBookReaderEnabledCommandHandler"/> class.
    /// </summary>
    /// <param name="authorizationService">Injected service for authorization related functionality.</param>
    /// <param name="currentUserService">Injected service to retrieve the current user information.</param>
    /// <param name="enablementCache">Injected cache of whether the book reader of a plugin is enabled for a media library.</param>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    /// <param name="validator">Injected validator for application validation rules.</param>
    public SetLibraryBookReaderEnabledCommandHandler(IAuthorizationService authorizationService, ICurrentUserService currentUserService, IBookReaderEnablementCache enablementCache, IUnitOfWork unitOfWork, IValidator<SetLibraryBookReaderEnabledCommand> validator)
    {
        _authorizationService = authorizationService;
        _currentUserService = currentUserService;
        _enablementCache = enablementCache;
        _unitOfWork = unitOfWork;
        _validator = validator;
    }

    /// <summary>
    /// Handles the command to enable or disable a book reader for a media library.
    /// </summary>
    /// <param name="command">The request to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> representing either a successful operation, or an error.
    /// </returns>
    public async Task<Result<Success>> HandleAsync(SetLibraryBookReaderEnabledCommand command, CancellationToken cancellationToken)
    {
        List<Error> validationResult = _validator.Validate(command);
        if (validationResult.Count > 0)
            return validationResult;

        // An authenticated request must always carry a user identity.
        Guid? currentUserId = _currentUserService.UserId;
        if (currentUserId is null)
            return ApplicationErrors.Authorization.NotAuthorized;
        Guid userId = currentUserId.Value;

        // Admins can configure the book readers of any library; for everyone else, only their own libraries.
        bool canAccessLibrary = await _authorizationService.EvaluatePolicyAsync<ILibraryOwnershipPolicy>(userId, new LibraryOwnershipPolicyContext(command.LibraryId), cancellationToken).ConfigureAwait(false);
        if (!canAccessLibrary)
            return ApplicationErrors.Authorization.NotAuthorized;

        Result<LibraryBookReaderConfigurationEntity?> getConfigurationResult = await _unitOfWork.LibraryBookReaderConfigurationRepository.GetByLibraryAndPluginIdAsync(command.LibraryId, command.PluginId, cancellationToken).ConfigureAwait(false);
        if (getConfigurationResult.IsFailure)
            return getConfigurationResult.Errors;

        LibraryBookReaderConfigurationEntity configuration;
        if (getConfigurationResult.Value is not null)
        {
            configuration = getConfigurationResult.Value;
            configuration.IsEnabled = command.IsEnabled;
        }
        else
        {
            configuration = new LibraryBookReaderConfigurationEntity
            {
                Id = Guid.NewGuid(),
                LibraryId = command.LibraryId,
                PluginId = command.PluginId,
                IsEnabled = command.IsEnabled,
                CreatedOnUtc = default,
                CreatedBy = default,
                UpdatedBy = default
            };
        }

        Result<Updated> upsertResult = await _unitOfWork.LibraryBookReaderConfigurationRepository.UpsertAsync(configuration, cancellationToken).ConfigureAwait(false);
        if (upsertResult.IsFailure)
            return upsertResult.Errors;
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        // The reading service caches the enablement of a reader, because it consults it on every request; the cache is invalidated here,
        // so that disabling a reader cuts access to its books immediately, without waiting for the cache to expire.
        _enablementCache.Invalidate(command.LibraryId, command.PluginId);
        return Result.Success;
    }
}
