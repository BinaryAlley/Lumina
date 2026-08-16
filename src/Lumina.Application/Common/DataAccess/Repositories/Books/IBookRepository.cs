#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
using Lumina.Application.Common.DataAccess.Repositories.Common.Actions;
using Lumina.Application.Common.DataAccess.Repositories.Common.Base;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Common.DataAccess.Repositories.Books;

/// <summary>
/// Interface for the repository for books.
/// </summary>
public interface IBookRepository : IRepository<BookEntity>,
                                   IInsertRepositoryAction<BookEntity>,
                                   IUpdateRepositoryAction<BookEntity>,
                                   IGetByIdRepositoryAction<BookEntity, Guid>,
                                   IGetAllRepositoryAction<BookEntity>,
                                   IGetPaginatedRepositoryAction<BookEntity>
{
    /// <summary>
    /// Gets all the books of the media library identified by <paramref name="libraryId"/>.
    /// </summary>
    /// <param name="libraryId">The Id of the media library whose books are retrieved.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either a collection of <see cref="BookEntity"/>, or an error.</returns>
    Task<Result<IEnumerable<BookEntity>>> GetByLibraryIdAsync(Guid libraryId, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the book of the media library identified by <paramref name="libraryId"/> that is stored at the provided <paramref name="path"/>.
    /// </summary>
    /// <param name="libraryId">The Id of the media library whose book is retrieved.</param>
    /// <param name="path">The file system path of the book to be retrieved.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the <see cref="BookEntity"/> stored at the provided path, or an error.</returns>
    Task<Result<BookEntity?>> GetByPathAsync(Guid libraryId, string path, CancellationToken cancellationToken);

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
}
