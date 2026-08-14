#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
using Lumina.Application.Common.DataAccess.Repositories.Books;
using Lumina.Application.Common.DataAccess.Entities.Common;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.SharedKernel.Common.Enums.BookLibrary;
using Lumina.Domain.Common.Errors;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.DataAccess.Core.Repositories.Books;

/// <summary>
/// Repository for books.
/// </summary>
internal sealed class BookRepository : IBookRepository
{
    private readonly LuminaDbContext _luminaDbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="BookRepository"/> class.
    /// </summary>
    /// <param name="luminaDbContext">Injected Entity Framework DbContext.</param>
    public BookRepository(LuminaDbContext luminaDbContext)
    {
        _luminaDbContext = luminaDbContext;
    }

    /// <summary>
    /// Adds a new book.
    /// </summary>
    /// <param name="book">The book to add.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public async Task<Result<Created>> InsertAsync(BookEntity book, CancellationToken cancellationToken)
    {
        bool bookExists = await _luminaDbContext.Books.AnyAsync(repositoryBook => repositoryBook.Id == book.Id, cancellationToken: cancellationToken).ConfigureAwait(false);
        if (bookExists)
            return Errors.WrittenContent.BookAlreadyExists;

        // fetch existing tags and genres
        List<TagEntity> existingTags = await _luminaDbContext.Set<TagEntity>()
            .Where(t => book.Tags.Select(bt => bt.Name).Contains(t.Name))
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        List<GenreEntity> existingGenres = await _luminaDbContext.Set<GenreEntity>()
            .Where(g => book.Genres.Select(bg => bg.Name).Contains(g.Name))
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        // replace tags and genres in the book with existing ones
        book.Tags = new HashSet<TagEntity>(book.Tags.Select(tag => existingTags.FirstOrDefault(existingTag => existingTag.Name == tag.Name) ?? tag));
        book.Genres = new HashSet<GenreEntity>(book.Genres.Select(genre => existingGenres.FirstOrDefault(existingGenre => existingGenre.Name == genre.Name) ?? genre));

        _luminaDbContext.Books.Add(book);
        return Result.Created;
    }

    /// <summary>
    /// Gets a book by its Id.
    /// </summary>
    /// <param name="id">The Id of the book to get.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either a <see cref="BookEntity"/>, or an error.</returns>
    public async Task<Result<BookEntity?>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _luminaDbContext.Books
            .Include(book => book.Tags)
            .Include(book => book.Genres)
            .Include(book => book.ISBNs)
            .FirstOrDefaultAsync(book => book.Id == id, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets all books.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either a collection of <see cref="BookEntity"/>, or an error.</returns>
    public async Task<Result<IEnumerable<BookEntity>>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _luminaDbContext.Books
            .Include(book => book.Tags)
            .Include(book => book.Genres)
            .Include(book => book.ISBNs)
            .ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets all the books of the media library identified by <paramref name="libraryId"/>.
    /// </summary>
    /// <param name="libraryId">The Id of the media library whose books are retrieved.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either a collection of <see cref="BookEntity"/>, or an error.</returns>
    public async Task<Result<IEnumerable<BookEntity>>> GetByLibraryIdAsync(Guid libraryId, CancellationToken cancellationToken)
    {
        return await _luminaDbContext.Books
            .Include(book => book.Tags)
            .Include(book => book.Genres)
            .Include(book => book.ISBNs)
            .Where(book => book.LibraryId == libraryId)
            .ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets the book of the media library identified by <paramref name="libraryId"/> that is stored at the provided <paramref name="path"/>.
    /// </summary>
    /// <param name="libraryId">The Id of the media library whose book is retrieved.</param>
    /// <param name="path">The file system path of the book to be retrieved.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the <see cref="BookEntity"/> stored at the provided path, or an error.</returns>
    public async Task<Result<BookEntity?>> GetByPathAsync(Guid libraryId, string path, CancellationToken cancellationToken)
    {
        return await _luminaDbContext.Books
            .Include(book => book.Tags)
            .Include(book => book.Genres)
            .Include(book => book.ISBNs)
            .FirstOrDefaultAsync(book => book.LibraryId == libraryId && book.Path == path, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets a page of the books of the media library identified by <paramref name="libraryId"/> whose metadata has not been enriched yet,
    /// ordered by path, using keyset pagination.
    /// </summary>
    /// <param name="libraryId">The Id of the media library whose books are retrieved.</param>
    /// <param name="lastPath">The path of the last retrieved book, used for keyset pagination. Pass <see langword="null"/> to get the first page.</param>
    /// <param name="pageSize">The maximum number of books to retrieve.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either a page of books needing their metadata enriched, or an error.</returns>
    public async Task<Result<IReadOnlyList<BookEntity>>> GetBooksNeedingMetadataAsync(Guid libraryId, string? lastPath, int pageSize, CancellationToken cancellationToken)
    {
        return await _luminaDbContext.Books
            .Include(book => book.Tags)
            .Include(book => book.Genres)
            .Include(book => book.ISBNs)
            .Where(book => book.LibraryId == libraryId
                        && book.MetadataStatus != MetadataStatus.Enriched
                        && (lastPath == null || book.Path.CompareTo(lastPath) > 0))
            .OrderBy(book => book.Path)
            .Take(pageSize)
            .ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets the number of books of the media library identified by <paramref name="libraryId"/> whose metadata has not been enriched yet.
    /// </summary>
    /// <param name="libraryId">The Id of the media library whose books are counted.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the number of books needing their metadata enriched, or an error.</returns>
    public async Task<Result<int>> GetBooksNeedingMetadataCountAsync(Guid libraryId, CancellationToken cancellationToken)
    {
        return await _luminaDbContext.Books
            .CountAsync(book => book.LibraryId == libraryId && book.MetadataStatus != MetadataStatus.Enriched, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Updates a book.
    /// </summary>
    /// <param name="data">The book to update.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public async Task<Result<Updated>> UpdateAsync(BookEntity data, CancellationToken cancellationToken)
    {
        BookEntity? foundBook = await _luminaDbContext.Books
            .Include(book => book.Tags)
            .Include(book => book.Genres)
            .Include(book => book.ISBNs)
            .FirstOrDefaultAsync(book => book.Id == data.Id, cancellationToken).ConfigureAwait(false);
        if (foundBook is null)
            return Errors.WrittenContent.BookNotFound;
        // update the scalar properties of the book
        _luminaDbContext.Entry(foundBook).CurrentValues.SetValues(data);
        return Result.Updated;
    }
}
