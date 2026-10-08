#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Artwork;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Core.MediaLibrary.Management.Deletion;

/// <summary>
/// Deletion strategy for the media library items of the books media library type. The book stored at the deleted path is removed, together with its stored artwork.
/// </summary>
internal sealed class BooksMediaLibraryItemDeletionStrategy : IMediaLibraryItemDeletionStrategy
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBookArtworkService _bookArtworkService;
    private readonly ILogger<BooksMediaLibraryItemDeletionStrategy> _logger;

    /// <summary>
    /// The media library type that this deletion strategy supports.
    /// </summary>
    public LibraryType SupportedLibraryType => LibraryType.Book;

    /// <summary>
    /// Initializes a new instance of the <see cref="BooksMediaLibraryItemDeletionStrategy"/> class.
    /// </summary>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    /// <param name="bookArtworkService">Injected service for storing the artwork of the books.</param>
    /// <param name="logger">Injected logger used to report the issues encountered while deleting the media library item.</param>
    public BooksMediaLibraryItemDeletionStrategy(IUnitOfWork unitOfWork, IBookArtworkService bookArtworkService, ILogger<BooksMediaLibraryItemDeletionStrategy> logger)
    {
        _unitOfWork = unitOfWork;
        _bookArtworkService = bookArtworkService;
        _logger = logger;
    }

    /// <summary>
    /// Deletes the book stored at the provided <paramref name="path"/> in the media library identified by <paramref name="libraryId"/>, together with its stored artwork.
    /// </summary>
    /// <param name="libraryId">The Id of the media library whose book is deleted.</param>
    /// <param name="path">The file system path of the book to delete.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public async Task<Result<Success>> DeleteItemAsync(Guid libraryId, string path, CancellationToken cancellationToken)
    {
        // Load the book stored at the deleted path, which might have been already removed.
        Result<BookEntity?> getBookResult = await _unitOfWork.BookRepository.GetByPathAsync(libraryId, path, cancellationToken).ConfigureAwait(false);
        if (getBookResult.IsFailure)
            return getBookResult.Errors;
        BookEntity? book = getBookResult.Value;
        if (book is null)
            return Result.Success;

        // Delete the stored artwork of the book, best-effort, since a stale cover must not prevent the book from being removed.
        Result<LibraryEntity?> getLibraryResult = await _unitOfWork.LibraryRepository.GetByIdAsync(libraryId, cancellationToken: cancellationToken).ConfigureAwait(false);
        if (getLibraryResult.IsFailure || getLibraryResult.Value is null)
            return getLibraryResult.IsFailure ? getLibraryResult.Errors : Errors.Library.LibraryNotFound;
        LibraryEntity library = getLibraryResult.Value;

        Result<IReadOnlyDictionary<Guid, string?>> getAuthorsResult = await _unitOfWork.BookRepository.GetAuthorsDisplayNamesByBookIdsAsync([book.Id], cancellationToken).ConfigureAwait(false);
        if (getAuthorsResult.IsFailure)
            return getAuthorsResult.Errors;
        string authorName = getAuthorsResult.Value.TryGetValue(book.Id, out string? authorDisplayName) && authorDisplayName is not null ? authorDisplayName : string.Empty;

        Result<Deleted> deleteArtworkResult = _bookArtworkService.DeleteBookArtwork(libraryId, book.Id, library.Title, authorName, book.Title);
        if (deleteArtworkResult.IsFailure)
            _logger.LogWarning("Failed to delete the stored artwork of the book with Id '{BookId}' at path '{BookPath}', the artwork might remain orphaned.", book.Id, book.Path);

        // Delete the book, whose stored artwork and participations are removed by the database cascade.
        Result<Deleted> deleteBookResult = await _unitOfWork.BookRepository.DeleteByIdAsync(book.Id, cancellationToken).ConfigureAwait(false);
        if (deleteBookResult.IsFailure)
            return deleteBookResult.Errors;

        Result<Success> saveChangesResult = await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        if (saveChangesResult.IsFailure)
            return saveChangesResult.Errors;

        return Result.Success;
    }
}
