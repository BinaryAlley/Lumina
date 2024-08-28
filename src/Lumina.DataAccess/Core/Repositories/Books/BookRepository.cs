#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.DataAccess.Repositories.Books;
using Lumina.Application.Common.Models.Books;
using Lumina.Application.Common.Models.Common;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.Common.Errors;
using Microsoft.EntityFrameworkCore;
#endregion

namespace Lumina.DataAccess.Core.Repositories.Books;

/// <summary>
/// Repository for books.
/// </summary>
internal sealed class BookRepository : IBookRepository
{
    #region ================================================================== FIELD MEMBERS ================================================================================
    private readonly LuminaDbContext _luminaDbContext;
    #endregion

    #region ====================================================================== CTOR =====================================================================================
    /// <summary>
    /// Initializes a new instance of the <see cref="BookRepository"/> class.
    /// </summary>
    /// <param name="luminaDbContext">Injected Entity Framework DbContext.</param>
    public BookRepository(LuminaDbContext luminaDbContext)
    {
        _luminaDbContext = luminaDbContext;
    }
    #endregion

    #region ===================================================================== METHODS ===================================================================================
    /// <summary>
    /// Adds a new book.
    /// </summary>
    /// <param name="book">The book to add.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="ErrorOr{T}"/> representing either a successfull operation, or an error.</returns>
    public async Task<ErrorOr<Created>> InsertAsync(BookDto book, CancellationToken cancellationToken)
    {
        var jobExists = await _luminaDbContext.Books.AnyAsync(_book => _book.Id == book.Id, cancellationToken: cancellationToken).ConfigureAwait(false);
        if (jobExists)
            return Errors.WrittenContent.BookAlreadyExists;

        // fetch existing tags and genres
        var existingTags = await _luminaDbContext.Set<TagDto>()
            .Where(t => book.Tags.Select(bt => bt.Name).Contains(t.Name))
            .ToListAsync(cancellationToken);

        var existingGenres = await _luminaDbContext.Set<GenreDto>()
            .Where(g => book.Genres.Select(bg => bg.Name).Contains(g.Name))
            .ToListAsync(cancellationToken);

        // replace tags and genres in the book with existing ones
        book.Tags = new HashSet<TagDto>(book.Tags.Select(tag => existingTags.FirstOrDefault(existingTag => existingTag.Name == tag.Name) ?? tag));
        book.Genres = new HashSet<GenreDto>(book.Genres.Select(genre => existingGenres.FirstOrDefault(existingGenre => existingGenre.Name == genre.Name) ?? genre));

        _luminaDbContext.Books.Add(book);
        return Result.Created;
    }

    /// <summary>
    /// Gets all books.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="ErrorOr{T}"/> containing either a collection of <see cref="BookDto"/>, or an error.</returns>
    public async Task<ErrorOr<IEnumerable<BookDto>>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _luminaDbContext.Books
            .Include(book => book.Tags)
            .Include(book => book.Genres)
            .Include(book => book.ISBNs)
            .ToListAsync(cancellationToken).ConfigureAwait(false);
    }
    #endregion
}