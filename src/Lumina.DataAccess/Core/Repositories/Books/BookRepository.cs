#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Common;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Common.DataAccess.Repositories.Books;
using Lumina.Application.Common.DTO.Filtering;
using Lumina.Application.Common.DTO.Pagination;
using Lumina.Application.Common.Specifications;
using Lumina.DataAccess.Core.Repositories.Books.Specifications;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.BookLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.Common;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.DataAccess.Core.Repositories.Books;

/// <summary>
/// Repository for books.
/// </summary>
internal sealed class BookRepository : IBookRepository
{
    private static readonly MethodInfo s_toLowerMethod = typeof(string).GetMethod(nameof(string.ToLower), Type.EmptyTypes)!;
    private static readonly MethodInfo s_startsWithMethod = typeof(string).GetMethod(nameof(string.StartsWith), [typeof(string)])!;
    private static readonly MethodInfo s_substringMethod = typeof(string).GetMethod(nameof(string.Substring), [typeof(int)])!;

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
        book.Tags = [.. book.Tags.Select(tag => existingTags.FirstOrDefault(existingTag => existingTag.Name == tag.Name) ?? tag)];
        book.Genres = [.. book.Genres.Select(genre => existingGenres.FirstOrDefault(existingGenre => existingGenre.Name == genre.Name) ?? genre)];

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
    /// Gets paginated books.
    /// </summary>
    /// <typeparam name="TFilter">The type of the filter used for filtering the data.</typeparam>
    /// <param name="paginationData">The pagination data that includes the current page and the number of items per page to retrieve. If <see langword="null"/>, all matching books are returned.</param>
    /// <param name="sortBy">The name of the fields by which to sort the results.</param>
    /// <param name="sortOrder">The direction in which to sort the results.</param>
    /// <param name="filterModel">The model containing the parameters used to filter the results.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either a <see cref="PaginatedResultDto{BookEntity}"/>, or an error.</returns>
    public async Task<Result<PaginatedResultDto<BookEntity>>> GetPaginatedAsync<TFilter>(PaginationDataDto? paginationData, string? sortBy = null, SortOrder? sortOrder = null, TFilter? filterModel = null, CancellationToken cancellationToken = default) where TFilter : BaseFilterDto
    {
        IQueryable<BookEntity> booksQuery = _luminaDbContext.Books
            .Include(book => book.Tags)
            .Include(book => book.Genres)
            .Include(book => book.ISBNs)
            .Include(book => book.Ratings)
            .AsNoTracking();

        // books should always be retrieved only per owning libraries
        if (filterModel is not LibraryFilterDto libraryFilter || libraryFilter.LibraryId == Guid.Empty)
            return Errors.Library.FilterMustIncludeLibraryId;

        booksQuery = booksQuery.Where(book => book.LibraryId == libraryFilter.LibraryId);

        FilterSpecification<BookEntity>? filterSpecification = BuildFilterSpecification(libraryFilter);
        // apply filtering
        if (filterSpecification is not null)
            booksQuery = booksQuery.Where(filterSpecification.ToExpression());

        // apply sorting based on the specified sortBy and sortOrder parameters
        booksQuery = ApplySorting(booksQuery, sortBy, sortOrder ?? SortOrder.Ascending, libraryFilter.IgnoreThePrefixForAlphaPicker);

        // if no pagination was requested, return all the books of the library
        if (paginationData is null)
        {
            IReadOnlyList<BookEntity> allBooks = await booksQuery.ToListAsync(cancellationToken).ConfigureAwait(false);
            return new PaginatedResultDto<BookEntity>
            {
                Data = allBooks,
                CurrentPage = 1,
                PerPage = allBooks.Count,
                Count = allBooks.Count,
                NumberOfPages = 1
            };
        }

        int count = await booksQuery.Select(book => book.Id).CountAsync(cancellationToken).ConfigureAwait(false);
        int numberOfPages = (int)Math.Ceiling((double)count / paginationData.PerPage);
        int currentPage = Math.Min(paginationData.CurrentPage, Math.Max(1, numberOfPages)); // make sure current page doesn't exceed maximum number of pages

        // apply pagination
        IReadOnlyList<BookEntity> paginatedResult = await booksQuery
            .Skip((currentPage - 1) * paginationData.PerPage)
            .Take(paginationData.PerPage)
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        return new PaginatedResultDto<BookEntity>
        {
            Data = paginatedResult,
            CurrentPage = currentPage,
            PerPage = paginationData.PerPage,
            Count = count,
            NumberOfPages = numberOfPages
        };
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

    /// <summary>
    /// Sorts a books query by the given field name, defaulting to <see cref="BookEntity.Title"/>.
    /// </summary>
    /// <param name="booksQuery">The query to sort.</param>
    /// <param name="sortBy">The field to sort by (case-insensitive).</param>
    /// <param name="sortOrder">The direction of the sorting.</param>
    /// <param name="ignoreThePrefixForAlphaPicker">Whether a leading "the " prefix of a title is ignored when deriving the title sort key, or not.</param>
    private static IOrderedQueryable<BookEntity> ApplySorting(IQueryable<BookEntity> booksQuery, string? sortBy, SortOrder sortOrder, bool ignoreThePrefixForAlphaPicker)
    {
        return sortBy?.ToLower() switch
        {
            "languagecode" => sortOrder == SortOrder.Descending
                ? booksQuery.OrderByDescending(book => book.LanguageCode)
                : booksQuery.OrderBy(book => book.LanguageCode),
            "format" => sortOrder == SortOrder.Descending
                ? booksQuery.OrderBy(book => book.Format == null).ThenByDescending(book => book.Format)
                : booksQuery.OrderBy(book => book.Format == null).ThenBy(book => book.Format),
            "metadataprovider" => sortOrder == SortOrder.Descending
                ? booksQuery.OrderByDescending(book => book.MetadataProvider)
                : booksQuery.OrderBy(book => book.MetadataProvider),
            _ => sortOrder == SortOrder.Descending
                ? booksQuery.OrderByDescending(BuildTitleSortKey(ignoreThePrefixForAlphaPicker))
                : booksQuery.OrderBy(BuildTitleSortKey(ignoreThePrefixForAlphaPicker)),
        };
    }

    /// <summary>
    /// Builds the expression that derives the title sort key of a book, matching the effective title used by the alpha filter:
    /// the title lowercased, falling back to the original title when the title is <see langword="null"/> or empty, and optionally
    /// stripped of a leading "the " prefix.
    /// </summary>
    /// <param name="ignoreThePrefixForAlphaPicker">Whether a leading "the " prefix of a title is ignored when deriving the title sort key, or not.</param>
    /// <returns>An expression that evaluates to the title sort key of a book.</returns>
    private static Expression<Func<BookEntity, string>> BuildTitleSortKey(bool ignoreThePrefixForAlphaPicker)
    {
        ParameterExpression book = Expression.Parameter(typeof(BookEntity), "book");

        Expression titleProperty = Expression.Property(book, nameof(BookEntity.Title));

        // the raw title: the title, unless it is null or empty, in which case the original title (or an empty string) is used
        BinaryExpression isTitleMissing = Expression.OrElse(
            Expression.Equal(titleProperty, Expression.Constant(null, typeof(string))),
            Expression.Equal(titleProperty, Expression.Constant(string.Empty)));
        Expression rawTitle = Expression.Condition(isTitleMissing,
            Expression.Coalesce(Expression.Property(book, nameof(BookEntity.OriginalTitle)), Expression.Constant(string.Empty)),
            titleProperty);

        MethodCallExpression lowerTitle = Expression.Call(rawTitle, s_toLowerMethod);

        // when ignoring the "The " prefix, strip a leading "the " from the lowercased title
        Expression effectiveTitle = lowerTitle;
        if (ignoreThePrefixForAlphaPicker)
        {
            MethodCallExpression startsWithThe = Expression.Call(lowerTitle, s_startsWithMethod, Expression.Constant("the "));
            MethodCallExpression strippedTitle = Expression.Call(lowerTitle, s_substringMethod, Expression.Constant(4));
            effectiveTitle = Expression.Condition(startsWithThe, strippedTitle, lowerTitle);
        }

        return Expression.Lambda<Func<BookEntity, string>>(effectiveTitle, book);
    }

    /// <summary>
    /// Builds a filter specification for querying books.
    /// </summary>
    /// <param name="libraryFilter">The model containing the parameters used to filter the results.</param>
    /// <returns>A filter specification that can be used to query books matching the provided criteria.</returns>
    private static FilterSpecification<BookEntity>? BuildFilterSpecification(LibraryFilterDto libraryFilter)
    {
        FilterSpecification<BookEntity>? filterSpecification = null;

        // include the search term filter, if provided
        if (!string.IsNullOrWhiteSpace(libraryFilter.SearchTerm))
            filterSpecification = new BookSearchSpecification(libraryFilter.SearchTerm);

        // include the alpha key filter, if provided
        if (libraryFilter.FilterAlphaKey is not null)
        {
            BookAlphaFilterSpecification alphaFilterSpecification = new(libraryFilter.FilterAlphaKey, libraryFilter.IgnoreThePrefixForAlphaPicker);
            filterSpecification = filterSpecification is null
                ? alphaFilterSpecification
                : filterSpecification.And(alphaFilterSpecification);
        }

        return filterSpecification;
    }
}
