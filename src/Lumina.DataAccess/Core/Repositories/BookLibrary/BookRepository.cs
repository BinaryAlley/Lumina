#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Common;
using Lumina.Application.Common.DataAccess.Entities.MediaContributors;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Common.DataAccess.Repositories.BookLibrary;
using Lumina.Application.Common.DTO.Filtering;
using Lumina.Application.Common.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Common.DTO.Pagination;
using Lumina.Application.Common.Specifications;
using Lumina.DataAccess.Common.Persistence;
using Lumina.DataAccess.Core.Repositories.BookLibrary.Specifications;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.BookLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.Common;
using Lumina.Domain.SharedKernel.Common.Enums.MediaContributors;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.DataAccess.Core.Repositories.BookLibrary;

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
        bool doesBookExist = await _luminaDbContext.Books.AnyAsync(repositoryBook => repositoryBook.Id == book.Id, cancellationToken: cancellationToken).ConfigureAwait(false);
        if (doesBookExist)
            return Errors.WrittenContent.BookAlreadyExists;

        // A book is unique within its library by its file system path, so the same file can never be registered twice in the same library.
        bool doesBookPathExist = await _luminaDbContext.Books.AnyAsync(repositoryBook => repositoryBook.LibraryId == book.LibraryId && repositoryBook.Path == book.Path, cancellationToken: cancellationToken).ConfigureAwait(false);
        if (doesBookPathExist)
            return Errors.WrittenContent.BookAlreadyExists;

        // Resolve the tags and genres of the book to a single instance per name, replacing the stored ones where they exist, so that the shared tables are not duplicated.
        Result<IReadOnlyDictionary<string, TagEntity>> resolveTagsResult = await SharedReferenceResolver.ResolveAsync(_luminaDbContext, book.Tags, Errors.Metadata.TagNameCannotBeEmpty, cancellationToken).ConfigureAwait(false);
        if (resolveTagsResult.IsFailure)
            return resolveTagsResult.Errors;
        Result<IReadOnlyDictionary<string, GenreEntity>> resolveGenresResult = await SharedReferenceResolver.ResolveAsync(_luminaDbContext, book.Genres, Errors.Metadata.GenreNameCannotBeEmpty, cancellationToken).ConfigureAwait(false);
        if (resolveGenresResult.IsFailure)
            return resolveGenresResult.Errors;

        book.Tags = [.. SharedReferenceResolver.Normalize(book.Tags, resolveTagsResult.Value)];
        book.Genres = [.. SharedReferenceResolver.Normalize(book.Genres, resolveGenresResult.Value)];

        _luminaDbContext.Books.Add(book);
        return Result.Created;
    }

    /// <summary>
    /// Gets a book by its Id.
    /// </summary>
    /// <param name="id">The Id of the book to get.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either a <see cref="BookEntity"/>, or an error.</returns>
    public async Task<Result<BookEntity?>> GetByIdAsync(Guid id, bool shouldIncludeNavigationProperties = true, bool shouldTrackEntities = true, CancellationToken cancellationToken = default)
    {
        IQueryable<BookEntity> query = _luminaDbContext.Books;
        if (!shouldTrackEntities)
            query = query.AsNoTracking();
        if (shouldIncludeNavigationProperties)
        {
            query = query
                .Include(book => book.Tags)
                .Include(book => book.Genres)
                .Include(book => book.ISBNs)
                .Include(book => book.Ratings)
                .Include(book => book.Contributors)
                .Include(book => book.Artwork)
                .AsSplitQuery();
        }
        return await query.FirstOrDefaultAsync(book => book.Id == id, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets paginated books, or all the books of the matching filters when the pagination data is <see langword="null"/>.
    /// </summary>
    /// <typeparam name="TFilter">The type of the filter used for filtering the data.</typeparam>
    /// <param name="paginationData">The pagination data that includes the current page and the number of items per page to retrieve. If <see langword="null"/>, all matching books are returned.</param>
    /// <param name="sortBy">The name of the fields by which to sort the results.</param>
    /// <param name="sortOrder">The direction in which to sort the results.</param>
    /// <param name="filterModel">The model containing the parameters used to filter the results.</param>
    /// <param name="shouldIncludeNavigationProperties">Whether the navigation properties of the entities should be loaded together with the entities themselves. Pass <see langword="false"/> to retrieve only the data stored directly on the entity rows.</param>
    /// <param name="shouldTrackEntities">Whether the retrieved entities should be tracked by the persistence medium, so that changes to them can be saved. Pass <see langword="false"/> for read-only scenarios, to avoid the tracking overhead.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either a <see cref="PaginatedResultDto{BookEntity}"/>, or an error.</returns>
    public async Task<Result<PaginatedResultDto<BookEntity>>> GetAllAsync<TFilter>(PaginationDataDto? paginationData = null, string? sortBy = null, SortOrder? sortOrder = null, TFilter? filterModel = null, bool shouldIncludeNavigationProperties = true, bool shouldTrackEntities = true, CancellationToken cancellationToken = default) where TFilter : BaseFilterDto
    {
        IQueryable<BookEntity> booksQuery = _luminaDbContext.Books;
        if (!shouldTrackEntities)
            booksQuery = booksQuery.AsNoTracking();
        if (shouldIncludeNavigationProperties)
        {
            booksQuery = booksQuery
                .Include(book => book.Tags)
                .Include(book => book.Genres)
                .Include(book => book.ISBNs)
                .Include(book => book.Ratings)
                .Include(book => book.Contributors)
                .Include(book => book.Artwork)
                .AsSplitQuery();
        }

        // Books should always be retrieved only per owning libraries.
        if (filterModel is not LibraryFilterDto libraryFilter || libraryFilter.LibraryId == Guid.Empty)
            return Errors.Library.FilterMustIncludeLibraryId;

        booksQuery = booksQuery.Where(book => book.LibraryId == libraryFilter.LibraryId);

        FilterSpecification<BookEntity>? filterSpecification = BuildFilterSpecification(libraryFilter);
        // Apply filtering.
        if (filterSpecification is not null)
            booksQuery = booksQuery.Where(filterSpecification.ToExpression());

        // Apply sorting based on the specified sortBy and sortOrder parameters.
        booksQuery = ApplySorting(booksQuery, sortBy, sortOrder ?? SortOrder.Ascending, libraryFilter.ShouldIgnoreThePrefixForAlphaPicker);

        // If no pagination was requested, return all the books of the library.
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
        int currentPage = Math.Min(paginationData.CurrentPage, Math.Max(1, numberOfPages)); // Make sure current page doesn't exceed maximum number of pages.

        // Apply pagination.
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
    /// Gets paginated lightweight read models of the books of the media library identified by the provided <paramref name="filterModel"/>,
    /// or all of them when the pagination data is <see langword="null"/>, projecting only the fields needed to display the books in a grid or a list.
    /// </summary>
    /// <typeparam name="TFilter">The type of the filter used for filtering the data.</typeparam>
    /// <param name="paginationData">The pagination data that includes the current page and the number of items per page to retrieve. If <see langword="null"/>, all matching books are returned.</param>
    /// <param name="sortBy">The name of the fields by which to sort the results.</param>
    /// <param name="sortOrder">The direction in which to sort the results.</param>
    /// <param name="filterModel">The model containing the parameters used to filter the results.</param>
    /// <param name="shouldIncludeNavigationProperties">Whether the navigation properties of the entities should be loaded together with the entities themselves. Pass <see langword="false"/> to retrieve only the data stored directly on the entity rows.</param>
    /// <param name="shouldTrackEntities">Whether the retrieved entities should be tracked by the persistence medium, so that changes to them can be saved. Pass <see langword="false"/> for read-only scenarios, to avoid the tracking overhead.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either a <see cref="PaginatedResultDto{BookLiteRow}"/>, or an error.</returns>
    public async Task<Result<PaginatedResultDto<BookLiteRow>>> GetAllLiteAsync<TFilter>(PaginationDataDto? paginationData = null, string? sortBy = null, SortOrder? sortOrder = null, TFilter? filterModel = null, bool shouldIncludeNavigationProperties = false, bool shouldTrackEntities = false, CancellationToken cancellationToken = default) where TFilter : BaseFilterDto
    {
        // Books should always be retrieved only per owning libraries.
        if (filterModel is not LibraryFilterDto libraryFilter || libraryFilter.LibraryId == Guid.Empty)
            return Errors.Library.FilterMustIncludeLibraryId;

        // The lightweight read models are retrieved without tracking by default, because they are never modified by the caller.
        IQueryable<BookEntity> booksQuery = _luminaDbContext.Books;
        if (!shouldTrackEntities)
            booksQuery = booksQuery.AsNoTracking();
        if (shouldIncludeNavigationProperties)
        {
            booksQuery = booksQuery
                .Include(book => book.Tags)
                .Include(book => book.Genres)
                .Include(book => book.ISBNs)
                .Include(book => book.Ratings)
                .Include(book => book.Contributors)
                .Include(book => book.Artwork)
                .AsSplitQuery();
        }

        booksQuery = booksQuery.Where(book => book.LibraryId == libraryFilter.LibraryId);

        FilterSpecification<BookEntity>? filterSpecification = BuildFilterSpecification(libraryFilter);
        if (filterSpecification is not null)
            booksQuery = booksQuery.Where(filterSpecification.ToExpression());

        // Apply sorting based on the specified sortBy and sortOrder parameters.
        booksQuery = ApplySorting(booksQuery, sortBy, sortOrder ?? SortOrder.Ascending, libraryFilter.ShouldIgnoreThePrefixForAlphaPicker);

        IQueryable<BookLiteRow> liteRowsQuery = booksQuery.Select(book => new BookLiteRow
        {
            Id = book.Id,
            Title = book.Title,
            ReleaseYear = book.ReReleaseYear ?? book.OriginalReleaseYear,
            CoverPath = book.Artwork
                .Where(artwork => artwork.ArtworkType == ArtworkType.Cover)
                .Select(artwork => artwork.FileName)
                .FirstOrDefault()
        });

        // If no pagination was requested, return all the books of the library.
        if (paginationData is null)
        {
            IReadOnlyList<BookLiteRow> allBooks = await liteRowsQuery.ToListAsync(cancellationToken).ConfigureAwait(false);
            return new PaginatedResultDto<BookLiteRow>
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
        int currentPage = Math.Min(paginationData.CurrentPage, Math.Max(1, numberOfPages)); // Make sure current page doesn't exceed maximum number of pages.

        // Apply pagination.
        IReadOnlyList<BookLiteRow> paginatedResult = await liteRowsQuery
            .Skip((currentPage - 1) * paginationData.PerPage)
            .Take(paginationData.PerPage)
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        return new PaginatedResultDto<BookLiteRow>
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
            .Include(book => book.Contributors)
            .Include(book => book.Artwork)
            .AsSplitQuery()
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
            .Include(book => book.Artwork)
            .AsSplitQuery()
            .FirstOrDefaultAsync(book => book.LibraryId == libraryId && book.Path == path, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets the subset of <paramref name="paths"/> that is already used by a book of the library identified by <paramref name="libraryId"/>.
    /// </summary>
    /// <param name="libraryId">The Id of the library whose books are searched.</param>
    /// <param name="paths">The book paths to check.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the paths that are already used, or an error.</returns>
    public async Task<Result<IReadOnlyCollection<string>>> GetExistingPathsAsync(Guid libraryId, IReadOnlyCollection<string> paths, CancellationToken cancellationToken)
    {
        if (paths.Count == 0)
            return Result.From<IReadOnlyCollection<string>>([]);

        // The stored paths are compared ordinally, matching the case sensitive comparison used by the unique index of the storage medium.
        List<string> distinctPaths = [.. paths.Distinct(StringComparer.Ordinal)];
        List<string> existingPaths = await _luminaDbContext.Books
            .Where(repositoryBook => repositoryBook.LibraryId == libraryId && distinctPaths.Contains(repositoryBook.Path))
            .Select(repositoryBook => repositoryBook.Path)
            .ToListAsync(cancellationToken).ConfigureAwait(false);
        return Result.From<IReadOnlyCollection<string>>(existingPaths);
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
            .Include(book => book.Contributors)
            .AsSplitQuery()
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
    /// Gets a page of the books of the media library identified by <paramref name="libraryId"/> that need their artwork resolved,
    /// meaning they lack at least one required piece of artwork with an enriched status, ordered by path, using keyset pagination.
    /// </summary>
    /// <param name="libraryId">The Id of the media library whose books are retrieved.</param>
    /// <param name="lastPath">The path of the last retrieved book, used for keyset pagination. Pass <see langword="null"/> to get the first page.</param>
    /// <param name="pageSize">The maximum number of books to retrieve.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either a page of books needing their artwork resolved, or an error.</returns>
    public async Task<Result<IReadOnlyList<BookEntity>>> GetBooksNeedingArtworkAsync(Guid libraryId, string? lastPath, int pageSize, CancellationToken cancellationToken)
    {
        return await _luminaDbContext.Books
            .Include(book => book.Tags)
            .Include(book => book.Genres)
            .Include(book => book.ISBNs)
            .Include(book => book.Artwork)
            .AsSplitQuery()
            .Where(book => book.LibraryId == libraryId
                        && !book.Artwork.Any(artwork => artwork.ArtworkType == ArtworkType.Cover && artwork.Status == ArtworkStatus.Enriched)
                        && (lastPath == null || book.Path.CompareTo(lastPath) > 0))
            .OrderBy(book => book.Path)
            .Take(pageSize)
            .ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets the number of books of the media library identified by <paramref name="libraryId"/> that need their artwork resolved.
    /// </summary>
    /// <param name="libraryId">The Id of the media library whose books are counted.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the number of books needing their artwork resolved, or an error.</returns>
    public async Task<Result<int>> GetBooksNeedingArtworkCountAsync(Guid libraryId, CancellationToken cancellationToken)
    {
        return await _luminaDbContext.Books
            .CountAsync(book => book.LibraryId == libraryId
                             && !book.Artwork.Any(artwork => artwork.ArtworkType == ArtworkType.Cover && artwork.Status == ArtworkStatus.Enriched), cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Resets the enrichment state of the books stored at the provided <paramref name="paths"/> in the media library identified by
    /// <paramref name="libraryId"/>, so that they are re-enriched, because their content changed since the last scan.
    /// </summary>
    /// <param name="libraryId">The Id of the media library whose books are reset.</param>
    /// <param name="paths">The file system paths of the books whose enrichment state is reset.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public async Task<Result<Updated>> ResetEnrichmentStateForPathsAsync(Guid libraryId, IReadOnlyCollection<string> paths, CancellationToken cancellationToken)
    {
        await _luminaDbContext.Books
            .Where(book => book.LibraryId == libraryId && paths.Contains(book.Path))
            .ExecuteUpdateAsync(setters => setters.SetProperty(book => book.MetadataStatus, MetadataStatus.Pending), cancellationToken)
            .ConfigureAwait(false);

        await _luminaDbContext.Set<BookArtworkEntity>()
            .Where(artwork => _luminaDbContext.Books.Any(book => book.Id == artwork.BookId && book.LibraryId == libraryId && paths.Contains(book.Path)))
            .ExecuteUpdateAsync(setters => setters.SetProperty(artwork => artwork.Status, ArtworkStatus.Pending), cancellationToken)
            .ConfigureAwait(false);

        return Result.Updated;
    }

    /// <summary>
    /// Resets the metadata enrichment status of all the books of the media library identified by <paramref name="libraryId"/>,
    /// so that they are re-enriched, because the metadata provider configuration of the library changed.
    /// </summary>
    /// <param name="libraryId">The Id of the media library whose books are reset.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public async Task<Result<Updated>> ResetMetadataStatusForLibraryAsync(Guid libraryId, CancellationToken cancellationToken)
    {
        await _luminaDbContext.Books
            .Where(book => book.LibraryId == libraryId)
            .ExecuteUpdateAsync(setters => setters.SetProperty(book => book.MetadataStatus, MetadataStatus.Pending), cancellationToken)
            .ConfigureAwait(false);

        return Result.Updated;
    }

    /// <summary>
    /// Resets the artwork status of all the artwork of the books of the media library identified by <paramref name="libraryId"/>,
    /// so that they are re-resolved, because the artwork provider configuration of the library changed.
    /// </summary>
    /// <param name="libraryId">The Id of the media library whose artwork is reset.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public async Task<Result<Updated>> ResetArtworkStatusForLibraryAsync(Guid libraryId, CancellationToken cancellationToken)
    {
        await _luminaDbContext.Set<BookArtworkEntity>()
            .Where(artwork => _luminaDbContext.Books.Any(book => book.Id == artwork.BookId && book.LibraryId == libraryId))
            .ExecuteUpdateAsync(setters => setters.SetProperty(artwork => artwork.Status, ArtworkStatus.Pending), cancellationToken)
            .ConfigureAwait(false);

        return Result.Updated;
    }

    /// <summary>
    /// Gets the display names of the author of the books identified by the provided <paramref name="bookIds"/>,
    /// keyed by the Id of the book, or <see langword="null"/> when a book has no author.
    /// </summary>
    /// <param name="bookIds">The unique identifiers of the books whose authors are retrieved.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the display names of the authors of the books, keyed by book Id, or an error.</returns>
    public async Task<Result<IReadOnlyDictionary<Guid, string?>>> GetAuthorsDisplayNamesByBookIdsAsync(IReadOnlyCollection<Guid> bookIds, CancellationToken cancellationToken)
    {
        List<AuthorRow> authorRows = await _luminaDbContext.BookContributors
            .Where(bookContributor => bookIds.Contains(bookContributor.BookId) && bookContributor.Role == MediaContributorRole.Author)
            .Join(_luminaDbContext.MediaContributors,
                bookContributor => bookContributor.MediaContributorId,
                contributor => contributor.Id,
                (bookContributor, contributor) => new AuthorRow(bookContributor.BookId, contributor.DisplayName))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return authorRows
            .GroupBy(row => row.BookId)
            .ToDictionary(group => group.Key, group => group.Min(row => row.DisplayName));
    }

    /// <summary>
    /// The author row of a book, joining the participation of an author contributor in a book with the display name of the contributor.
    /// </summary>
    /// <param name="BookId">The Id of the book.</param>
    /// <param name="DisplayName">The display name of the author contributor.</param>
    private sealed record AuthorRow(Guid BookId, string DisplayName);

    /// <summary>
    /// Deletes the book identified by <paramref name="id"/>.
    /// </summary>
    /// <param name="id">The Id of the book to delete.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public async Task<Result<Deleted>> DeleteByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        BookEntity? book = await _luminaDbContext.Books
            .FirstOrDefaultAsync(repositoryBook => repositoryBook.Id == id, cancellationToken).ConfigureAwait(false);
        if (book is null)
            return Errors.WrittenContent.BookNotFound;

        _luminaDbContext.Books.Remove(book);
        return Result.Deleted;
    }

    /// <summary>
    /// Updates a book, replacing only the editable data that actually changed, while preserving its identity, the identity of its children, its enrichment columns and its audit columns.
    /// </summary>
    /// <param name="data">The book whose editable data is applied to the stored book.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public async Task<Result<Updated>> UpdateAsync(BookEntity data, CancellationToken cancellationToken)
    {
        BookEntity? foundBook = await _luminaDbContext.Books
            .Include(book => book.Tags)
            .Include(book => book.Genres)
            .Include(book => book.ISBNs)
            .Include(book => book.Ratings)
            .Include(book => book.Contributors)
            .AsSplitQuery()
            .FirstOrDefaultAsync(book => book.Id == data.Id, cancellationToken).ConfigureAwait(false);
        if (foundBook is null)
            return Errors.WrittenContent.BookNotFound;

        // The stored enrichment and identity columns are never overwritten by an edit: they are owned by the scan and by the record itself. The audit columns are
        // only ever written by the auditing interceptor, which is why the copier restores them after copying the editable values.
        Guid libraryId = foundBook.LibraryId;
        string path = foundBook.Path;
        MetadataStatus metadataStatus = foundBook.MetadataStatus;
        DateTime? lastMetadataUpdateUtc = foundBook.LastMetadataUpdateUtc;
        string? metadataProvider = foundBook.MetadataProvider;
        EditableValuesCopier.CopyEditableValues(_luminaDbContext, foundBook, data);
        foundBook.LibraryId = libraryId;
        foundBook.Path = path;
        foundBook.MetadataStatus = metadataStatus;
        foundBook.LastMetadataUpdateUtc = lastMetadataUpdateUtc;
        foundBook.MetadataProvider = metadataProvider;

        // Tags and genres are shared across the whole database, so the stored rows whose names already exist are reused, and only the missing names are inserted.
        Result<Updated> reconcileTagsResult = await SharedReferenceResolver.ReconcileAsync(_luminaDbContext, foundBook.Tags, data.Tags, Errors.Metadata.TagNameCannotBeEmpty, cancellationToken).ConfigureAwait(false);
        if (reconcileTagsResult.IsFailure)
            return reconcileTagsResult.Errors;
        Result<Updated> reconcileGenresResult = await SharedReferenceResolver.ReconcileAsync(_luminaDbContext, foundBook.Genres, data.Genres, Errors.Metadata.GenreNameCannotBeEmpty, cancellationToken).ConfigureAwait(false);
        if (reconcileGenresResult.IsFailure)
            return reconcileGenresResult.Errors;

        // ISBNs are owned value objects with no identity anyone could reference, so an ISBN whose format changed is replaced as a whole, while the ones that did not
        // change are left exactly as they are stored. Matching by the ISBN value keeps an update in place instead of recreating every ISBN of the book.
        CollectionReconciler.Reconcile(
            foundBook.ISBNs,
            data.ISBNs,
            existingIsbn => existingIsbn.Value ?? string.Empty,
            incomingIsbn => incomingIsbn.Value ?? string.Empty,
            shouldReplace: (existingIsbn, incomingIsbn) => !existingIsbn.Equals(incomingIsbn),
            createNew: incomingIsbn => incomingIsbn);

        // Ratings are owned value objects with no identity anyone could reference, so a changed rating is replaced as a whole, while the ratings that did not change
        // are left exactly as they are stored. Matching by the rating source keeps an update in place instead of recreating every rating of the book.
        CollectionReconciler.Reconcile(
            foundBook.Ratings,
            data.Ratings,
            existingRating => existingRating.Source?.ToString() ?? string.Empty,
            incomingRating => incomingRating.Source?.ToString() ?? string.Empty,
            shouldReplace: (existingRating, incomingRating) => !existingRating.Equals(incomingRating),
            createNew: incomingRating => incomingRating);

        // A contributor participation is matched by the contributor and the role they played. Matched participations keep their identity and their audit columns, so a
        // contributor that is displayed or linked elsewhere is never deleted and re-inserted just because another field of the book was edited.
        CollectionReconciler.Reconcile(
            foundBook.Contributors,
            data.Contributors,
            existingContributor => (existingContributor.MediaContributorId, existingContributor.Role),
            incomingContributor => (incomingContributor.MediaContributorId, incomingContributor.Role),
            shouldReplace: (existingContributor, incomingContributor) => false,
            createNew: incomingContributor => incomingContributor);

        return Result.Updated;
    }

    /// <summary>
    /// Sorts a books query by the given field name, defaulting to <see cref="BookEntity.Title"/>.
    /// </summary>
    /// <param name="booksQuery">The query to sort.</param>
    /// <param name="sortBy">The field to sort by (case-insensitive).</param>
    /// <param name="sortOrder">The direction of the sorting.</param>
    /// <param name="shouldIgnoreThePrefixForAlphaPicker">Whether a leading "the " prefix of a title is ignored when deriving the title sort key, or not.</param>
    private static IOrderedQueryable<BookEntity> ApplySorting(IQueryable<BookEntity> booksQuery, string? sortBy, SortOrder sortOrder, bool shouldIgnoreThePrefixForAlphaPicker)
    {
        return sortBy?.ToLowerInvariant() switch
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
                ? booksQuery.OrderByDescending(BuildTitleSortKey(shouldIgnoreThePrefixForAlphaPicker))
                : booksQuery.OrderBy(BuildTitleSortKey(shouldIgnoreThePrefixForAlphaPicker)),
        };
    }

    /// <summary>
    /// Builds the expression that derives the title sort key of a book, matching the effective title used by the alpha filter:
    /// the title lowercased, falling back to the original title when the title is <see langword="null"/> or empty, and optionally
    /// stripped of a leading "the " prefix.
    /// </summary>
    /// <param name="shouldIgnoreThePrefixForAlphaPicker">Whether a leading "the " prefix of a title is ignored when deriving the title sort key, or not.</param>
    /// <returns>An expression that evaluates to the title sort key of a book.</returns>
    private static Expression<Func<BookEntity, string>> BuildTitleSortKey(bool shouldIgnoreThePrefixForAlphaPicker)
    {
        ParameterExpression book = Expression.Parameter(typeof(BookEntity), "book");

        Expression titleProperty = Expression.Property(book, nameof(BookEntity.Title));

        // The raw title: the title, unless it is null or empty, in which case the original title (or an empty string) is used.
        BinaryExpression isTitleMissing = Expression.OrElse(
            Expression.Equal(titleProperty, Expression.Constant(null, typeof(string))),
            Expression.Equal(titleProperty, Expression.Constant(string.Empty)));
        Expression rawTitle = Expression.Condition(isTitleMissing,
            Expression.Coalesce(Expression.Property(book, nameof(BookEntity.OriginalTitle)), Expression.Constant(string.Empty)),
            titleProperty);

        MethodCallExpression lowerTitle = Expression.Call(rawTitle, s_toLowerMethod);

        // When ignoring the "The " prefix, strip a leading "the " from the lowercased title.
        Expression effectiveTitle = lowerTitle;
        if (shouldIgnoreThePrefixForAlphaPicker)
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

        // Include the search term filter, if provided.
        if (!string.IsNullOrWhiteSpace(libraryFilter.SearchTerm))
            filterSpecification = new BookSearchSpecification(libraryFilter.SearchTerm);

        // Include the alpha key filter, if provided.
        if (libraryFilter.FilterAlphaKey is not null)
        {
            BookAlphaFilterSpecification alphaFilterSpecification = new(libraryFilter.FilterAlphaKey, libraryFilter.ShouldIgnoreThePrefixForAlphaPicker);
            filterSpecification = filterSpecification is null
                ? alphaFilterSpecification
                : filterSpecification.And(alphaFilterSpecification);
        }

        return filterSpecification;
    }
}
