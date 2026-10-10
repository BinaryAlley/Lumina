#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Core.MediaLibrary.Management.Deletion;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Events;
using Lumina.Domain.Common.Exceptions;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Events;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Core.MediaLibrary.Management.Events;

/// <summary>
/// Handler for the domain event raised when a media library item is no longer present in the media library scan snapshot.
/// The media library item stored at the deleted path is removed by the deletion strategy of the media library type it belongs to.
/// </summary>
public class LibraryMediaItemDeletedDomainEventHandler : IDomainEventHandler<LibraryMediaItemDeletedDomainEvent>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEnumerable<IMediaLibraryItemDeletionStrategy> _mediaLibraryItemDeletionStrategies;
    private readonly ILogger<LibraryMediaItemDeletedDomainEventHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="LibraryMediaItemDeletedDomainEventHandler"/> class.
    /// </summary>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    /// <param name="mediaLibraryItemDeletionStrategies">Injected deletion strategies, one for each supported media library type.</param>
    /// <param name="logger">Injected logger used to report the issues encountered while deleting the media library item.</param>
    public LibraryMediaItemDeletedDomainEventHandler(IUnitOfWork unitOfWork, IEnumerable<IMediaLibraryItemDeletionStrategy> mediaLibraryItemDeletionStrategies, ILogger<LibraryMediaItemDeletedDomainEventHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _mediaLibraryItemDeletionStrategies = mediaLibraryItemDeletionStrategies;
        _logger = logger;
    }

    /// <summary>
    /// Handles the event raised when a media library item is no longer present in the media library scan snapshot.
    /// </summary>
    /// <param name="domainEvent">The domain event to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public async ValueTask HandleAsync(LibraryMediaItemDeletedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        Guid libraryId = domainEvent.LibraryId.Value;

        // Load the media library, whose type determines the deletion strategy to use.
        Result<LibraryEntity?> getLibraryResult = await _unitOfWork.LibraryRepository.GetByIdAsync(libraryId, cancellationToken: cancellationToken).ConfigureAwait(false);
        if (getLibraryResult.IsFailure)
            throw new EventualConsistencyException(getLibraryResult.FirstError, getLibraryResult.Errors);
        if (getLibraryResult.Value is null)
            throw new EventualConsistencyException(Errors.Library.LibraryNotFound);
        LibraryType libraryType = getLibraryResult.Value.LibraryType;

        IMediaLibraryItemDeletionStrategy? deletionStrategy = _mediaLibraryItemDeletionStrategies.FirstOrDefault(strategy => strategy.SupportedLibraryType == libraryType);
        if (deletionStrategy is null)
        {
            _logger.LogWarning("No deletion strategy exists for media library type '{LibraryType}', the item at path '{ItemPath}' was not deleted.", libraryType, domainEvent.Path);
            return;
        }

        Result<Success> deleteItemResult = await deletionStrategy.DeleteItemAsync(libraryId, domainEvent.Path, cancellationToken).ConfigureAwait(false);
        if (deleteItemResult.IsFailure)
            throw new EventualConsistencyException(deleteItemResult.FirstError, deleteItemResult.Errors);
    }
}
