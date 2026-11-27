#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.CQRS;
using Nemera.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Nemera.Application.Common.DataAccess.UoW;
using Nemera.Application.Common.DomainEvents;
using Nemera.Application.Common.Infrastructure.Authentication;
using Nemera.Application.Common.Infrastructure.Authorization;
using Nemera.Application.Common.Infrastructure.Plugins;
using Nemera.Application.Common.Infrastructure.Validation;
using Nemera.Application.Common.Mapping.MediaLibrary.Management;
using Nemera.Contracts.Responses.MediaLibrary.Management;
using Nemera.Domain.Common.Events;
using Nemera.Domain.Common.Primitives;
using Nemera.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using Nemera.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.PathTemplate;
using Nemera.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Strategies.Environment;
using Nemera.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.ValueObjects;
using Nemera.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate;
using Nemera.Domain.Core.BoundedContexts.UserManagementBoundedContext.UserAggregate.ValueObjects;
using Nemera.Domain.SharedKernel.Common.Enums.Authorization;
using Nemera.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Nemera.Domain.SharedKernel.Common.Enums.PhotoLibrary;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Nemera.Application.Common.Errors.Errors;
using DomainErrors = Nemera.Domain.Common.Errors.Errors;
#endregion

namespace Nemera.Application.Core.MediaLibrary.Management.Commands.AddLibrary;

/// <summary>
/// Handler for the command to add a media library.
/// </summary>
public class AddLibraryCommandHandler : ICommandHandler<AddLibraryCommand, Result<LibraryResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDomainEventsQueue _domainEventsQueue;
    private readonly IEnvironmentContext _environmentContext;
    private readonly IAuthorizationService _authorizationService;
    private readonly IMediaLibraryProviderConfigurationStore _providerConfigurationStore;
    private readonly ILibraryPathTemplateService _pathTemplateService;
    private readonly IValidator<AddLibraryCommand> _validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="AddLibraryCommandHandler"/> class.
    /// </summary>
    /// <param name="authorizationService">Injected service for authorization related functionality.</param>
    /// <param name="currentUserService">Injected service to retrieve the current user information.</param>
    /// <param name="domainEventsQueue">Injected service for the queue of domain events.</param>
    /// <param name="environmentContext">Injected facade service for environment contextual services.</param>
    /// <param name="providerConfigurationStore">Injected store of the provider configurations of the media libraries.</param>
    /// <param name="pathTemplateService">Injected service for validating and resolving the path templates of the media libraries.</param>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    /// <param name="validator">Injected validator for application validation rules.</param>
    public AddLibraryCommandHandler(
        IAuthorizationService authorizationService,
        ICurrentUserService currentUserService,
        IDomainEventsQueue domainEventsQueue,
        IEnvironmentContext environmentContext,
        IMediaLibraryProviderConfigurationStore providerConfigurationStore,
        ILibraryPathTemplateService pathTemplateService,
        IUnitOfWork unitOfWork,
        IValidator<AddLibraryCommand> validator)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _domainEventsQueue = domainEventsQueue;
        _environmentContext = environmentContext;
        _authorizationService = authorizationService;
        _providerConfigurationStore = providerConfigurationStore;
        _pathTemplateService = pathTemplateService;
        _validator = validator;
    }

    /// <summary>
    /// Handles the command to add a media library.
    /// </summary>
    /// <param name="command">The command to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a successfully created <see cref="LibraryResponse"/>, or an error message.
    /// </returns>
    public async Task<Result<LibraryResponse>> HandleAsync(AddLibraryCommand command, CancellationToken cancellationToken)
    {
        List<Error> validationResult = _validator.Validate(command);
        if (validationResult.Count > 0)
            return validationResult;

        // An authenticated request must always carry a user identity.
        Guid? currentUserId = _currentUserService.UserId;
        if (currentUserId is null)
            return ApplicationErrors.Authorization.NotAuthorized;
        Guid userId = currentUserId.Value;

        // Only admins or users with the permission to manage media libraries can create them.
        if (!await _authorizationService.IsInRoleAsync(userId, AuthorizationRoles.ADMIN, cancellationToken).ConfigureAwait(false) &&
            !await _authorizationService.HasPermissionAsync(userId, AuthorizationPermission.CanCreateLibraries, cancellationToken).ConfigureAwait(false))
            return ApplicationErrors.Authorization.NotAuthorized;

        // Make sure the file is an actual supported image.
        if (command.CoverImage is not null)
        {
            Result<FileSystemPathId> fileSystemPathIdResult = FileSystemPathId.Create(command.CoverImage);
            if (fileSystemPathIdResult.IsFailure)
                return fileSystemPathIdResult.Errors;

            Result<ImageType> imageCheckResult = await _environmentContext.FileTypeService.GetImageTypeAsync(fileSystemPathIdResult.Value, cancellationToken).ConfigureAwait(false);
            if (imageCheckResult.IsFailure)
                return imageCheckResult.Errors;
            if (imageCheckResult.Value == ImageType.None)
                return DomainErrors.Library.CoverFileMustBeAnImage;
        }

        LibraryType libraryType = Enum.Parse<LibraryType>(command.LibraryType!, ignoreCase: true);

        // A library created without explicit consumption settings uses the defaults of its type. When progress tracking is on and no threshold is
        // supplied, the default threshold of the type is used; a type without one keeps no threshold, which means completion is recorded explicitly.
        LibraryConsumptionSettings defaultConsumptionSettings = LibraryConsumptionSettings.CreateDefault(libraryType);
        bool shouldTrackConsumptionProgress = command.ShouldTrackConsumptionProgress ?? defaultConsumptionSettings.ShouldTrackConsumptionProgress;
        Optional<int> completionThresholdPercentage = Optional<int>.FromNullable(command.CompletionThresholdPercentage);
        if (completionThresholdPercentage.HasValue && !shouldTrackConsumptionProgress)
            return DomainErrors.Library.CompletionThresholdPercentageMustBeNullWhenProgressTrackingIsOff;
        if (!completionThresholdPercentage.HasValue && shouldTrackConsumptionProgress)
            completionThresholdPercentage = defaultConsumptionSettings.CompletionThresholdPercentage;
        LibraryConsumptionSettings consumptionSettings = LibraryConsumptionSettings.Create(shouldTrackConsumptionProgress, completionThresholdPercentage);

        // A library always carries a path template: the one provided by the user, or the ideal structure of the library type as its default.
        Result<IReadOnlyList<LibraryPathPart>> pathTemplatePartsResult = command.PathTemplateParts.ToDomainParts();
        if (pathTemplatePartsResult.IsFailure)
            return pathTemplatePartsResult.Errors;
        Result<LibraryPathTemplate> pathTemplateResult = _pathTemplateService.ResolveTemplate(libraryType, pathTemplatePartsResult.Value);
        if (pathTemplateResult.IsFailure)
            return pathTemplateResult.Errors;

        // Create a domain library object.
        Result<Library> createLibraryResult = Library.Create(
            UserId.Create(userId),
            command.Title!,
            libraryType,
            command.ContentLocations!,
            Optional<string>.FromNullable(command.CoverImage),
            command.IsEnabled,
            command.IsLocked,
            command.CanDownloadMetadataFromWeb,
            command.ShouldSaveMetadataInMediaDirectories,
            command.ShouldSkipUnchangedDirectoriesDuringScan,
            command.ShouldSkipHiddenFilesAndDirectoriesDuringScan ?? false,
            command.IsContentOnSsd,
            consumptionSettings,
            pathTemplateResult.Value,
            []
        );

        if (createLibraryResult.IsFailure)
            return createLibraryResult.Errors;

        // Convert the domain library entity to a repository library entity.
        LibraryEntity persistenceLibrary = createLibraryResult.Value.ToRepositoryEntity();
        // Insert the repository entity, seed the provider configurations of the new library so that it lists the plugins providing metadata,
        // artwork or book readers for its type, with the choices made for them, and save the changes.
        Result<Created> insertLibraryResult = await _unitOfWork.LibraryRepository.InsertAsync(persistenceLibrary, cancellationToken).ConfigureAwait(false);
        if (insertLibraryResult.IsFailure)
            return insertLibraryResult.Errors;
        Result<Success> configureProvidersResult = await _providerConfigurationStore.ConfigureProvidersAsync(
            createLibraryResult.Value.Id.Value, libraryType, command.MetadataProviders, command.ArtworkProviders, command.BookReaders, shouldReconcileOmittedKinds: true, cancellationToken).ConfigureAwait(false);
        if (configureProvidersResult.IsFailure)
            return configureProvidersResult.Errors;

        Result<Success> saveChangesResult = await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        if (saveChangesResult.IsFailure)
            return saveChangesResult.Errors;

        // Retrieve the newly saved media library from the persistence medium and return it.
        Result<LibraryEntity?> getCreatedLibraryResult = await _unitOfWork.LibraryRepository.GetByIdAsync(createLibraryResult.Value.Id.Value, cancellationToken: cancellationToken).ConfigureAwait(false);
        if (getCreatedLibraryResult.IsFailure)
            return getCreatedLibraryResult.Errors;
        if (getCreatedLibraryResult.Value is null)
            return ApplicationErrors.Persistence.ErrorPersistingMediaLibrary;

        // Mark the media library as saved.
        createLibraryResult.Value.Save();
        
        // Queue any domain events.
        foreach (IDomainEvent domainEvent in createLibraryResult.Value.GetDomainEvents())
            _domainEventsQueue.Enqueue(domainEvent);

        return getCreatedLibraryResult.Value.ToResponse();
    }
}
