#region ========================================================================= USING =====================================================================================
using Nemera.Domain.Common.Primitives;
using Nemera.Application.Common.DataAccess.Repositories.Common.Actions;
using Nemera.Application.Common.DataAccess.Repositories.Common.Base;
using Nemera.Application.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Nemera.Application.Common.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Nemera.Application.Common.DataAccess.Repositories.BookLibrary;

/// <summary>
/// Interface for the repository for books.
/// </summary>
public interface IBookRepository : IRepository<BookEntity>,
                                   IInsertRepositoryAction<BookEntity>,
                                   IUpdateRepositoryAction<BookEntity>,
                                   IGetByIdRepositoryAction<BookEntity, Guid>,
                                   IGetAllRepositoryAction<BookEntity>,
                                   IGetAllLiteRepositoryAction<BookEntity, BookLiteRow>,
                                   IDeleteByIdRepositoryAction<Guid>
{
    /// <summary>
    /// Gets the book of the media library identified by <paramref name="libraryId"/> that is stored at the provided <paramref name="path"/>.
    /// </summary>
    /// <remarks>
    /// Soft deleted books are included on purpose, so that the scan deletion strategy can permanently remove a deleted book when its file disappears from disk.
    /// </remarks>
    /// <param name="libraryId">The Id of the media library whose book is retrieved.</param>
    /// <param name="path">The file system path of the book to be retrieved.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the <see cref="BookEntity"/> stored at the provided path, or an error.</returns>
    Task<Result<BookEntity?>> GetByPathAsync(Guid libraryId, string path, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the soft deleted book identified by <paramref name="id"/>.
    /// </summary>
    /// <param name="id">The Id of the soft deleted book to get.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the soft deleted <see cref="BookEntity"/>, <see langword="null"/> when the book does not exist or is not soft deleted, or an error.</returns>
    Task<Result<BookEntity?>> GetSoftDeletedByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the soft deleted books of the media library identified by <paramref name="libraryId"/>.
    /// </summary>
    /// <param name="libraryId">The Id of the media library whose soft deleted books are retrieved.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the soft deleted books, or an error.</returns>
    Task<Result<IReadOnlyList<BookEntity>>> GetSoftDeletedByLibraryIdAsync(Guid libraryId, CancellationToken cancellationToken);

    /// <summary>
    /// Soft deletes the book identified by <paramref name="id"/>, without removing its stored metadata or artwork.
    /// </summary>
    /// <param name="id">The Id of the book to soft delete.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    Task<Result<Updated>> SoftDeleteByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// Restores the soft deleted book identified by <paramref name="id"/>.
    /// </summary>
    /// <param name="id">The Id of the book to restore.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    Task<Result<Updated>> RestoreSoftDeletedByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the subset of <paramref name="paths"/> that is already used by a book of the library identified by <paramref name="libraryId"/>.
    /// </summary>
    /// <remarks>
    /// Soft deleted books are included on purpose: their paths must still count as taken, so that a scan never materializes a deleted book again.
    /// </remarks>
    /// <param name="libraryId">The Id of the library whose books are searched.</param>
    /// <param name="paths">The book paths to check.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the paths that are already used, or an error.</returns>
    Task<Result<IReadOnlyCollection<string>>> GetExistingPathsAsync(Guid libraryId, IReadOnlyCollection<string> paths, CancellationToken cancellationToken);

    /// <summary>
    /// Gets a page of the books of the media library identified by <paramref name="libraryId"/> whose metadata has not been enriched yet,
    /// ordered by path, using keyset pagination.
    /// </summary>
    /// <param name="libraryId">The Id of the media library whose books are retrieved.</param>
    /// <param name="lastPath">The path of the last retrieved book, used for keyset pagination. Pass <see langword="null"/> to get the first page.</param>
    /// <param name="pageSize">The maximum number of books to retrieve.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either a page of books needing their metadata enriched, or an error.</returns>
    Task<Result<IReadOnlyList<BookEntity>>> GetBooksNeedingMetadataAsync(Guid libraryId, string? lastPath, int pageSize, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the number of books of the media library identified by <paramref name="libraryId"/> whose metadata has not been enriched yet.
    /// </summary>
    /// <param name="libraryId">The Id of the media library whose books are counted.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the number of books needing their metadata enriched, or an error.</returns>
    Task<Result<int>> GetBooksNeedingMetadataCountAsync(Guid libraryId, CancellationToken cancellationToken);

    /// <summary>
    /// Gets a page of the books of the media library identified by <paramref name="libraryId"/> that need their artwork resolved,
    /// meaning they lack at least one required piece of artwork with an enriched status, ordered by path, using keyset pagination.
    /// </summary>
    /// <param name="libraryId">The Id of the media library whose books are retrieved.</param>
    /// <param name="lastPath">The path of the last retrieved book, used for keyset pagination. Pass <see langword="null"/> to get the first page.</param>
    /// <param name="pageSize">The maximum number of books to retrieve.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either a page of books needing their artwork resolved, or an error.</returns>
    Task<Result<IReadOnlyList<BookEntity>>> GetBooksNeedingArtworkAsync(Guid libraryId, string? lastPath, int pageSize, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the number of books of the media library identified by <paramref name="libraryId"/> that need their artwork resolved.
    /// </summary>
    /// <param name="libraryId">The Id of the media library whose books are counted.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the number of books needing their artwork resolved, or an error.</returns>
    Task<Result<int>> GetBooksNeedingArtworkCountAsync(Guid libraryId, CancellationToken cancellationToken);

    /// <summary>
    /// Resets the enrichment state of the books stored at the provided <paramref name="paths"/> in the media library identified by
    /// <paramref name="libraryId"/>, so that they are re-enriched, because their content changed since the last scan.
    /// </summary>
    /// <remarks>
    /// Soft deleted books are included on purpose: if such a book is restored after its content changed, it must be re-enriched instead of keeping stale metadata.
    /// </remarks>
    /// <param name="libraryId">The Id of the media library whose books are reset.</param>
    /// <param name="paths">The file system paths of the books whose enrichment state is reset.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    Task<Result<Updated>> ResetEnrichmentStateForPathsAsync(Guid libraryId, IReadOnlyCollection<string> paths, CancellationToken cancellationToken);

    /// <summary>
    /// Resets the metadata enrichment status of all the books of the media library identified by <paramref name="libraryId"/>,
    /// so that they are re-enriched, because the metadata provider configuration of the library changed.
    /// </summary>
    /// <remarks>
    /// Soft deleted books are included on purpose: if such a book is restored after the provider configuration changed, it must be re-enriched with the new providers.
    /// </remarks>
    /// <param name="libraryId">The Id of the media library whose books are reset.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    Task<Result<Updated>> ResetMetadataStatusForLibraryAsync(Guid libraryId, CancellationToken cancellationToken);

    /// <summary>
    /// Resets the artwork status of all the artwork of the books of the media library identified by <paramref name="libraryId"/>,
    /// so that they are re-resolved, because the artwork provider configuration of the library changed.
    /// </summary>
    /// <remarks>
    /// Soft deleted books are included on purpose: if such a book is restored after the provider configuration changed, its artwork must be re-resolved with the new providers.
    /// </remarks>
    /// <param name="libraryId">The Id of the media library whose artwork is reset.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    Task<Result<Updated>> ResetArtworkStatusForLibraryAsync(Guid libraryId, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the display names of the author of the books identified by the provided <paramref name="bookIds"/>,
    /// keyed by the Id of the book, or <see langword="null"/> when a book has no author.
    /// </summary>
    /// <param name="bookIds">The unique identifiers of the books whose authors are retrieved.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the display names of the authors of the books, keyed by book Id, or an error.</returns>
    Task<Result<IReadOnlyDictionary<Guid, string?>>> GetAuthorsDisplayNamesByBookIdsAsync(IReadOnlyCollection<Guid> bookIds, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the books identified by <paramref name="bookIds"/>, optionally together with their navigation properties.
    /// </summary>
    /// <param name="bookIds">The Ids of the books to get.</param>
    /// <param name="shouldIncludeNavigationProperties">Whether the navigation properties of the books should be loaded together with the books themselves.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the existing books, or an error.</returns>
    Task<Result<IReadOnlyList<BookEntity>>> GetByIdsAsync(IReadOnlyCollection<Guid> bookIds, bool shouldIncludeNavigationProperties, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes all the books of the media library identified by <paramref name="libraryId"/>, together with their artwork and their related data.
    /// </summary>
    /// <param name="libraryId">The Id of the media library whose books are deleted.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    Task<Result<Deleted>> DeleteByLibraryIdAsync(Guid libraryId, CancellationToken cancellationToken);
}
