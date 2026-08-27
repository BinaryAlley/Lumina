#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BooksLibrary.Artwork;
using Lumina.Domain.Common.Events;
using Lumina.Domain.Common.Exceptions;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Events;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Core.MediaLibrary.Management.Events;

/// <summary>
/// Handler for the domain event raised when a media library item is no longer present in the media library scan snapshot.
/// The book stored at the deleted path is removed, together with its stored artwork.
/// </summary>
public class LibraryMediaItemDeletedDomainEventHandler : IDomainEventHandler<LibraryMediaItemDeletedDomainEvent>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBookArtworkService _bookArtworkService;
    private readonly ILogger<LibraryMediaItemDeletedDomainEventHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="LibraryMediaItemDeletedDomainEventHandler"/> class.
    /// </summary>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    /// <param name="bookArtworkService">Injected service for storing the artwork of the books.</param>
    /// <param name="logger">Injected logger used to report the issues encountered while deleting the media library item.</param>
    public LibraryMediaItemDeletedDomainEventHandler(IUnitOfWork unitOfWork, IBookArtworkService bookArtworkService, ILogger<LibraryMediaItemDeletedDomainEventHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _bookArtworkService = bookArtworkService;
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

        // load the book stored at the deleted path, which might have been already removed
        Result<BookEntity?> getBookResult = await _unitOfWork.BookRepository.GetByPathAsync(libraryId, domainEvent.Path, cancellationToken).ConfigureAwait(false);
        if (getBookResult.IsFailure)
            throw new EventualConsistencyException(getBookResult.FirstError, getBookResult.Errors);
        BookEntity? book = getBookResult.Value;
        if (book is null)
            return;

        // delete the stored artwork of the book, best-effort, since a stale cover must not prevent the book from being removed
        Result<LibraryEntity?> getLibraryResult = await _unitOfWork.LibraryRepository.GetByIdAsync(libraryId, cancellationToken).ConfigureAwait(false);
        if (getLibraryResult.IsFailure || getLibraryResult.Value is null)
            throw new EventualConsistencyException(getLibraryResult.FirstError, getLibraryResult.Errors);
        LibraryEntity library = getLibraryResult.Value;

        Result<IReadOnlyDictionary<Guid, string?>> getAuthorsResult = await _unitOfWork.BookRepository.GetAuthorsDisplayNamesByBookIdsAsync([book.Id], cancellationToken).ConfigureAwait(false);
        if (getAuthorsResult.IsFailure)
            throw new EventualConsistencyException(getAuthorsResult.FirstError, getAuthorsResult.Errors);
        string authorName = getAuthorsResult.Value.TryGetValue(book.Id, out string? authorDisplayName) && authorDisplayName is not null ? authorDisplayName : string.Empty;

        Result<Deleted> deleteArtworkResult = _bookArtworkService.DeleteBookArtwork(libraryId, book.Id, library.Title, authorName, book.Title);
        if (deleteArtworkResult.IsFailure)
            _logger.LogWarning("Failed to delete the stored artwork of the book with Id '{BookId}' at path '{BookPath}', the artwork might remain orphaned.", book.Id, book.Path);

        // delete the book, whose stored artwork and participations are removed by the database cascade
        Result<Deleted> deleteBookResult = await _unitOfWork.BookRepository.DeleteAsync(book.Id, cancellationToken).ConfigureAwait(false);
        if (deleteBookResult.IsFailure)
            throw new EventualConsistencyException(deleteBookResult.FirstError, deleteBookResult.Errors);

        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
