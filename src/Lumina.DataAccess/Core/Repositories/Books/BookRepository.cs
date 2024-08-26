#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.DataAccess.Repositories.Books;
using Lumina.Application.Common.Models.Books;
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
        return await _luminaDbContext.Books.ToListAsync(cancellationToken).ConfigureAwait(false);
    }
    #endregion
}