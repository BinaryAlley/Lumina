#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.DataAccess.Repositories.Books;
using Lumina.Application.Common.DataAccess.Entities.Common;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.Common.Errors;
using Microsoft.EntityFrameworkCore;
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
    /// <returns>An <see cref="ErrorOr{TValue}"/> representing either a successful operation, or an error.</returns>
    public async Task<ErrorOr<Created>> InsertAsync(BookEntity book, CancellationToken cancellationToken)
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
    /// Gets all books.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing either a collection of <see cref="BookEntity"/>, or an error.</returns>
    public async Task<ErrorOr<IEnumerable<BookEntity>>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _luminaDbContext.Books
            .Include(book => book.Tags)
            .Include(book => book.Genres)
            .Include(book => book.ISBNs)
            .ToListAsync(cancellationToken).ConfigureAwait(false);
    }
}
