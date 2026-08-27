#region ========================================================================= USING =====================================================================================
using EntityFrameworkCore.Testing.NSubstitute;
using Lumina.Application.Common.DataAccess.Entities.Common;
using Lumina.Application.Common.DataAccess.Entities.MediaContributors;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Common.DTO.Filtering;
using Lumina.Application.Common.DTO.Pagination;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Common;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaContributors;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Fixtures.Common.DTO.Filtering;
using Lumina.Application.Fixtures.Common.DTO.Pagination;
using Lumina.DataAccess.Core.Repositories.Books;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.BookLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.Common;
using Lumina.Domain.SharedKernel.Common.Enums.MediaContributors;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.DataAccess.UnitTests.Core.Repositories.Books;

/// <summary>
/// Contains unit tests for the <see cref="BookRepository"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class BookRepositoryTests
{
    private readonly LuminaDbContext _mockContext;
    private readonly BookRepository _sut;
    private readonly BookEntityFixture _bookEntityFixture = new();
    private readonly BookRatingEntityFixture _bookRatingEntityFixture = new();
    private readonly TagEntityFixture _tagEntityFixture = new();
    private readonly GenreEntityFixture _genreEntityFixture = new();
    private readonly IsbnEntityFixture _isbnEntityFixture = new();
    private readonly BookArtworkEntityFixture _bookArtworkEntityFixture = new();
    private readonly BookContributorEntityFixture _bookContributorEntityFixture = new();
    private readonly MediaContributorEntityFixture _mediaContributorEntityFixture = new();
    private readonly PaginationDataDtoFixture _paginationDataDtoFixture = new();
    private readonly LibraryFilterDtoFixture _libraryFilterDtoFixture = new();
    private readonly BaseFilterDtoFixture _baseFilterDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="BookRepositoryTests"/> class.
    /// </summary>
    public BookRepositoryTests()
    {
        _mockContext = Create.MockedDbContextFor<LuminaDbContext>();
        _sut = new BookRepository(_mockContext);
    }

    [Fact]
    public async Task InsertAsync_WhenBookDoesNotExist_ShouldAddBookToContextAndReturnCreated()
    {
        // Arrange
        BookEntity bookModel = _bookEntityFixture.Create();

        // Act
        Result<Created> result = await _sut.InsertAsync(bookModel, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Created, result.Value);

        // check if the book was added to the context's ChangeTracker
        EntityEntry<BookEntity>? addedBook = _mockContext.ChangeTracker.Entries<BookEntity>()
        .FirstOrDefault(e => e.State == EntityState.Added && e.Entity.Id == bookModel.Id);
        Assert.NotNull(addedBook);
    }

    [Fact]
    public async Task InsertAsync_WhenBookAlreadyExists_ShouldReturnError()
    {
        // Arrange
        BookEntity bookModel = _bookEntityFixture.Create();

        _mockContext.Books.Add(bookModel);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<Created> result = await _sut.InsertAsync(bookModel, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.WrittenContent.BookAlreadyExists, result.FirstError);
        Assert.Single(_mockContext.ChangeTracker.Entries<BookEntity>()); // only the existing book should be in the context
    }

    [Fact]
    public async Task InsertAsync_WhenExistingTagsFound_ShouldReplaceTagsWithExistingOnes()
    {
        // Arrange
        TagEntity existingTag = _tagEntityFixture.Create(name: "Existing");
        _mockContext.Set<TagEntity>().Add(existingTag);
        await _mockContext.SaveChangesAsync();

        BookEntity bookModel = _bookEntityFixture.Create();
        bookModel.Tags = [_tagEntityFixture.Create(name: "Existing"), _tagEntityFixture.Create(name: "New")];

        // Act
        Result<Created> result = await _sut.InsertAsync(bookModel, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Created, result.Value);

        EntityEntry<BookEntity>? addedBook = _mockContext.ChangeTracker.Entries<BookEntity>()
            .FirstOrDefault(e => e.State == EntityState.Added && e.Entity.Id == bookModel.Id);
        Assert.NotNull(addedBook);
        BookEntity addedBookEntity = addedBook!.Entity;
        Assert.Equal(2, addedBookEntity.Tags.Count);
        Assert.Contains(addedBookEntity.Tags, t => t.Name == "Existing" && t == existingTag);
        Assert.Contains(addedBookEntity.Tags, t => t.Name == "New" && t != existingTag);
    }

    [Fact]
    public async Task InsertAsync_WhenExistingGenresFound_ShouldReplaceGenresWithExistingOnes()
    {
        // Arrange
        GenreEntity existingGenre = _genreEntityFixture.Create(name: "Existing");
        _mockContext.Set<GenreEntity>().Add(existingGenre);
        await _mockContext.SaveChangesAsync();

        BookEntity bookModel = _bookEntityFixture.Create();
        bookModel.Genres = [_genreEntityFixture.Create(name: "Existing"), _genreEntityFixture.Create(name: "New")];

        // Act
        Result<Created> result = await _sut.InsertAsync(bookModel, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Created, result.Value);

        EntityEntry<BookEntity>? addedBook = _mockContext.ChangeTracker.Entries<BookEntity>()
            .FirstOrDefault(e => e.State == EntityState.Added && e.Entity.Id == bookModel.Id);
        Assert.NotNull(addedBook);
        BookEntity addedBookEntity = addedBook!.Entity;
        Assert.Equal(2, addedBookEntity.Genres.Count);
        Assert.Contains(addedBookEntity.Genres, g => g.Name == "Existing" && g == existingGenre);
        Assert.Contains(addedBookEntity.Genres, g => g.Name == "New" && g != existingGenre);
    }

    [Fact]
    public async Task GetAllAsync_WhenCalled_ShouldReturnAllBooks()
    {
        // Arrange
        List<BookEntity> books = _bookEntityFixture.CreateMany(3);
        _mockContext.Books.AddRange(books);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<IEnumerable<BookEntity>> result = await _sut.GetAllAsync(CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotNull(result.Value);
        Assert.Equal(3, result.Value.Count());
        Assert.Equal(books, result.Value);
    }

    [Fact]
    public async Task GetAllAsync_WhenNoBooksExist_ShouldReturnEmptyList()
    {
        // Act
        Result<IEnumerable<BookEntity>> result = await _sut.GetAllAsync(CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotNull(result.Value);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task GetAllAsync_WhenCalled_ShouldIncludeRelatedEntities()
    {
        // Arrange
        BookEntity book = _bookEntityFixture.Create();
        book.Tags = [_tagEntityFixture.Create(name: "Tag1"), _tagEntityFixture.Create(name: "Tag2")];
        book.Genres = [_genreEntityFixture.Create(name: "Genre1"), _genreEntityFixture.Create(name: "Genre2")];
        book.ISBNs = [_isbnEntityFixture.Create(value: "1234567890", format: IsbnFormat.Isbn10), _isbnEntityFixture.Create(value: "1234567890123", format: IsbnFormat.Isbn13)];
        _mockContext.Books.Add(book);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<IEnumerable<BookEntity>> result = await _sut.GetAllAsync(CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotNull(result.Value);
        Assert.Single(result.Value);
        BookEntity retrievedBook = result.Value.First();
        Assert.Equal(2, retrievedBook.Tags.Count);
        Assert.Equal(2, retrievedBook.Genres.Count);
        Assert.Equal(2, retrievedBook.ISBNs.Count);
    }

    [Fact]
    public async Task GetByLibraryIdAsync_WhenCalled_ShouldReturnOnlyBooksOfTheLibrary()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        BookEntity bookOfLibrary = _bookEntityFixture.Create();
        bookOfLibrary.LibraryId = libraryId;
        BookEntity bookOfAnotherLibrary = _bookEntityFixture.Create();
        bookOfAnotherLibrary.LibraryId = Guid.NewGuid();
        _mockContext.Books.AddRange(bookOfLibrary, bookOfAnotherLibrary);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<IEnumerable<BookEntity>> result = await _sut.GetByLibraryIdAsync(libraryId, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        BookEntity retrievedBook = Assert.Single(result.Value);
        Assert.Equal(bookOfLibrary.Id, retrievedBook.Id);
    }

    [Fact]
    public async Task GetByPathAsync_WhenBookExists_ShouldReturnTheBook()
    {
        // Arrange
        BookEntity book = _bookEntityFixture.Create();
        _mockContext.Books.Add(book);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<BookEntity?> result = await _sut.GetByPathAsync(book.LibraryId, book.Path, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(book.Id, result.Value!.Id);
    }

    [Fact]
    public async Task GetByPathAsync_WhenBookDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        BookEntity book = _bookEntityFixture.Create();
        _mockContext.Books.Add(book);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<BookEntity?> result = await _sut.GetByPathAsync(book.LibraryId, "/books/non-existent.epub", CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Null(result.Value);
    }

    [Fact]
    public async Task GetBooksNeedingMetadataAsync_WhenCalled_ShouldReturnOnlyBooksWhoseMetadataIsNotEnriched()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        BookEntity pendingBook = _bookEntityFixture.Create();
        pendingBook.LibraryId = libraryId;
        pendingBook.Path = "/books/a.epub";
        pendingBook.MetadataStatus = MetadataStatus.Pending;
        BookEntity enrichedBook = _bookEntityFixture.Create();
        enrichedBook.LibraryId = libraryId;
        enrichedBook.Path = "/books/b.epub";
        enrichedBook.MetadataStatus = MetadataStatus.Enriched;
        BookEntity failedBook = _bookEntityFixture.Create();
        failedBook.LibraryId = libraryId;
        failedBook.Path = "/books/c.epub";
        failedBook.MetadataStatus = MetadataStatus.Failed;
        BookEntity bookOfAnotherLibrary = _bookEntityFixture.Create();
        bookOfAnotherLibrary.LibraryId = Guid.NewGuid();
        bookOfAnotherLibrary.MetadataStatus = MetadataStatus.Pending;
        _mockContext.Books.AddRange(pendingBook, enrichedBook, failedBook, bookOfAnotherLibrary);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<IReadOnlyList<BookEntity>> result = await _sut.GetBooksNeedingMetadataAsync(libraryId, null, 10, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(2, result.Value.Count);
        Assert.Contains(result.Value, book => book.Id == pendingBook.Id);
        Assert.Contains(result.Value, book => book.Id == failedBook.Id);
    }

    [Fact]
    public async Task GetBooksNeedingMetadataAsync_WhenLastPathProvided_ShouldReturnBooksAfterIt()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        BookEntity firstBook = _bookEntityFixture.Create();
        firstBook.LibraryId = libraryId;
        firstBook.Path = "/books/a.epub";
        BookEntity secondBook = _bookEntityFixture.Create();
        secondBook.LibraryId = libraryId;
        secondBook.Path = "/books/b.epub";
        _mockContext.Books.AddRange(firstBook, secondBook);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<IReadOnlyList<BookEntity>> result = await _sut.GetBooksNeedingMetadataAsync(libraryId, firstBook.Path, 10, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        BookEntity retrievedBook = Assert.Single(result.Value);
        Assert.Equal(secondBook.Id, retrievedBook.Id);
    }

    [Fact]
    public async Task UpdateAsync_WhenBookExists_ShouldUpdateItsScalarProperties()
    {
        // Arrange
        BookEntity book = _bookEntityFixture.Create();
        _mockContext.Books.Add(book);
        await _mockContext.SaveChangesAsync();

        book.Title = "Updated Title";

        // Act
        Result<Updated> result = await _sut.UpdateAsync(book, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Updated, result.Value);
        BookEntity? retrievedBook = await _mockContext.Books.FindAsync(book.Id);
        Assert.Equal("Updated Title", retrievedBook!.Title);
    }

    [Fact]
    public async Task UpdateAsync_WhenBookDoesNotExist_ShouldReturnError()
    {
        // Arrange
        BookEntity book = _bookEntityFixture.Create();

        // Act
        Result<Updated> result = await _sut.UpdateAsync(book, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.WrittenContent.BookNotFound, result.FirstError);
    }

    [Fact]
    public async Task GetPaginatedAsync_WhenFilterIncludesLibraryId_ShouldReturnPaginatedBooks()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        List<BookEntity> books = _bookEntityFixture.CreateMany(3);
        foreach (BookEntity book in books)
            book.LibraryId = libraryId;
        _mockContext.Books.AddRange(books);
        await _mockContext.SaveChangesAsync();

        PaginationDataDto paginationData = _paginationDataDtoFixture.Create(currentPage: 1, perPage: 10);
        LibraryFilterDto filter = _libraryFilterDtoFixture.Create(libraryId: libraryId);

        // Act
        Result<PaginatedResultDto<BookEntity>> result = await _sut.GetPaginatedAsync(paginationData, null, null, filter, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(3, result.Value.Count);
        Assert.Equal(3, result.Value.Data.Count);
        Assert.Equal(1, result.Value.CurrentPage);
        Assert.Equal(10, result.Value.PerPage);
        Assert.Equal(1, result.Value.NumberOfPages);
    }

    [Fact]
    public async Task GetPaginatedAsync_WhenFilterDoesNotIncludeLibraryId_ShouldReturnError()
    {
        // Arrange
        PaginationDataDto paginationData = _paginationDataDtoFixture.Create(currentPage: 1, perPage: 10);
        BaseFilterDto filter = _baseFilterDtoFixture.Create();

        // Act
        Result<PaginatedResultDto<BookEntity>> result = await _sut.GetPaginatedAsync(paginationData, null, null, filter, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Library.FilterMustIncludeLibraryId, result.FirstError);
    }

    [Fact]
    public async Task GetPaginatedAsync_WhenFilterLibraryIdIsEmpty_ShouldReturnError()
    {
        // Arrange
        PaginationDataDto paginationData = _paginationDataDtoFixture.Create(currentPage: 1, perPage: 10);
        LibraryFilterDto filter = _libraryFilterDtoFixture.Create(libraryId: Guid.Empty);

        // Act
        Result<PaginatedResultDto<BookEntity>> result = await _sut.GetPaginatedAsync(paginationData, null, null, filter, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Library.FilterMustIncludeLibraryId, result.FirstError);
    }

    [Fact]
    public async Task GetPaginatedAsync_WhenPaginationDataIsNull_ShouldReturnAllBooks()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        List<BookEntity> books = _bookEntityFixture.CreateMany(3);
        foreach (BookEntity book in books)
            book.LibraryId = libraryId;
        _mockContext.Books.AddRange(books);
        await _mockContext.SaveChangesAsync();

        LibraryFilterDto filter = _libraryFilterDtoFixture.Create(libraryId: libraryId);

        // Act
        Result<PaginatedResultDto<BookEntity>> result = await _sut.GetPaginatedAsync(null, null, null, filter, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(3, result.Value.Count);
        Assert.Equal(3, result.Value.Data.Count);
        Assert.Equal(1, result.Value.CurrentPage);
        Assert.Equal(3, result.Value.PerPage);
        Assert.Equal(1, result.Value.NumberOfPages);
    }

    [Fact]
    public async Task GetPaginatedAsync_WhenSearchTermProvided_ShouldReturnOnlyMatchingBooks()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        BookEntity fellowshipBook = _bookEntityFixture.Create();
        fellowshipBook.LibraryId = libraryId;
        fellowshipBook.Title = "The Fellowship of the Ring";
        BookEntity towersBook = _bookEntityFixture.Create();
        towersBook.LibraryId = libraryId;
        towersBook.Title = "The Two Towers";
        _mockContext.Books.AddRange(fellowshipBook, towersBook);
        await _mockContext.SaveChangesAsync();

        PaginationDataDto paginationData = _paginationDataDtoFixture.Create(currentPage: 1, perPage: 10);
        LibraryFilterDto filter = _libraryFilterDtoFixture.Create(libraryId: libraryId, searchTerm: "Fellowship");

        // Act
        Result<PaginatedResultDto<BookEntity>> result = await _sut.GetPaginatedAsync(paginationData, null, null, filter, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        BookEntity book = Assert.Single(result.Value.Data);
        Assert.Equal("The Fellowship of the Ring", book.Title);
        Assert.Equal(1, result.Value.Count);
    }

    [Fact]
    public async Task GetPaginatedAsync_WhenPageSizeSmallerThanBookCount_ShouldReturnCorrectPageMetadata()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        List<BookEntity> books = _bookEntityFixture.CreateMany(5);
        foreach (BookEntity book in books)
            book.LibraryId = libraryId;
        _mockContext.Books.AddRange(books);
        await _mockContext.SaveChangesAsync();

        PaginationDataDto paginationData = _paginationDataDtoFixture.Create(currentPage: 2, perPage: 2);
        LibraryFilterDto filter = _libraryFilterDtoFixture.Create(libraryId: libraryId);

        // Act
        Result<PaginatedResultDto<BookEntity>> result = await _sut.GetPaginatedAsync(paginationData, null, null, filter, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(5, result.Value.Count);
        Assert.Equal(3, result.Value.NumberOfPages);
        Assert.Equal(2, result.Value.CurrentPage);
        Assert.Equal(2, result.Value.PerPage);
        Assert.Equal(2, result.Value.Data.Count);
    }

    [Fact]
    public async Task GetPaginatedAsync_WhenSortByTitleDescending_ShouldReturnBooksInDescendingOrder()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        BookEntity bookA = _bookEntityFixture.Create();
        bookA.LibraryId = libraryId;
        bookA.Title = "Book A";
        BookEntity bookB = _bookEntityFixture.Create();
        bookB.LibraryId = libraryId;
        bookB.Title = "Book B";
        _mockContext.Books.AddRange(bookA, bookB);
        await _mockContext.SaveChangesAsync();

        PaginationDataDto paginationData = _paginationDataDtoFixture.Create(currentPage: 1, perPage: 10);
        LibraryFilterDto filter = _libraryFilterDtoFixture.Create(libraryId: libraryId);

        // Act
        Result<PaginatedResultDto<BookEntity>> result = await _sut.GetPaginatedAsync(paginationData, "title", SortOrder.Descending, filter, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(["Book B", "Book A"], result.Value.Data.Select(book => book.Title));
    }

    [Fact]
    public async Task GetPaginatedAsync_WhenCalled_ShouldIncludeRelatedEntities()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        BookEntity book = _bookEntityFixture.Create();
        book.LibraryId = libraryId;
        book.Tags = [_tagEntityFixture.Create(name: "Tag1"), _tagEntityFixture.Create(name: "Tag2")];
        book.Genres = [_genreEntityFixture.Create(name: "Genre1"), _genreEntityFixture.Create(name: "Genre2")];
        book.ISBNs = [_isbnEntityFixture.Create(value: "1234567890", format: IsbnFormat.Isbn10), _isbnEntityFixture.Create(value: "1234567890123", format: IsbnFormat.Isbn13)];
        book.Ratings = [_bookRatingEntityFixture.Create(value: 4.5M, maxValue: 5, source: BookRatingSource.Goodreads, voteCount: 100)];
        _mockContext.Books.Add(book);
        await _mockContext.SaveChangesAsync();

        PaginationDataDto paginationData = _paginationDataDtoFixture.Create(currentPage: 1, perPage: 10);
        LibraryFilterDto filter = _libraryFilterDtoFixture.Create(libraryId: libraryId);

        // Act
        Result<PaginatedResultDto<BookEntity>> result = await _sut.GetPaginatedAsync(paginationData, null, null, filter, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        BookEntity retrievedBook = Assert.Single(result.Value.Data);
        Assert.Equal(2, retrievedBook.Tags.Count);
        Assert.Equal(2, retrievedBook.Genres.Count);
        Assert.Equal(2, retrievedBook.ISBNs.Count);
        Assert.Single(retrievedBook.Ratings);
        Assert.Equal(4.5M, retrievedBook.Ratings.First().Value);
    }

    [Fact]
    public async Task GetBooksNeedingArtworkAsync_WhenCalled_ShouldReturnOnlyBooksWithoutAnEnrichedCover()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        BookEntity bookWithoutCover = _bookEntityFixture.Create();
        bookWithoutCover.LibraryId = libraryId;
        bookWithoutCover.Path = "/books/a.epub";
        bookWithoutCover.BookArtwork = [];
        BookEntity bookWithEnrichedCover = _bookEntityFixture.Create();
        bookWithEnrichedCover.LibraryId = libraryId;
        bookWithEnrichedCover.Path = "/books/b.epub";
        bookWithEnrichedCover.BookArtwork = [_bookArtworkEntityFixture.Create(bookId: bookWithEnrichedCover.Id, artworkType: ArtworkType.Cover, ordinal: 0, status: ArtworkStatus.Enriched)];
        BookEntity bookWithFailedCover = _bookEntityFixture.Create();
        bookWithFailedCover.LibraryId = libraryId;
        bookWithFailedCover.Path = "/books/c.epub";
        bookWithFailedCover.BookArtwork = [_bookArtworkEntityFixture.Create(bookId: bookWithFailedCover.Id, artworkType: ArtworkType.Cover, ordinal: 0, status: ArtworkStatus.Failed)];
        BookEntity bookOfAnotherLibrary = _bookEntityFixture.Create();
        bookOfAnotherLibrary.LibraryId = Guid.NewGuid();
        bookOfAnotherLibrary.BookArtwork = [];
        _mockContext.Books.AddRange(bookWithoutCover, bookWithEnrichedCover, bookWithFailedCover, bookOfAnotherLibrary);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<IReadOnlyList<BookEntity>> result = await _sut.GetBooksNeedingArtworkAsync(libraryId, null, 10, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(2, result.Value.Count);
        Assert.Contains(result.Value, book => book.Id == bookWithoutCover.Id);
        Assert.Contains(result.Value, book => book.Id == bookWithFailedCover.Id);
    }

    [Fact]
    public async Task GetBooksNeedingArtworkCountAsync_WhenCalled_ShouldCountOnlyBooksWithoutAnEnrichedCover()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        BookEntity bookWithoutCover = _bookEntityFixture.Create();
        bookWithoutCover.LibraryId = libraryId;
        bookWithoutCover.BookArtwork = [];
        BookEntity bookWithEnrichedCover = _bookEntityFixture.Create();
        bookWithEnrichedCover.LibraryId = libraryId;
        bookWithEnrichedCover.BookArtwork = [_bookArtworkEntityFixture.Create(bookId: bookWithEnrichedCover.Id, artworkType: ArtworkType.Cover, ordinal: 0, status: ArtworkStatus.Enriched)];
        BookEntity bookOfAnotherLibrary = _bookEntityFixture.Create();
        bookOfAnotherLibrary.LibraryId = Guid.NewGuid();
        bookOfAnotherLibrary.BookArtwork = [];
        _mockContext.Books.AddRange(bookWithoutCover, bookWithEnrichedCover, bookOfAnotherLibrary);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<int> result = await _sut.GetBooksNeedingArtworkCountAsync(libraryId, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(1, result.Value);
    }

    [Fact]
    public async Task ResetEnrichmentStateForPathsAsync_WhenCalled_ShouldResetTheMetadataAndArtworkStatusesForThePaths()
    {
        // Arrange
        // the reset methods use ExecuteUpdateAsync, which is not supported by the in-memory provider, so a real SQLite database is used
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-bookrepo-reset-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        LuminaDbContext context = new(new DbContextOptionsBuilder<LuminaDbContext>().UseSqlite(anchorConnection.ConnectionString).Options);
        context.Database.EnsureCreated();
        BookRepository sut = new(context);

        Guid libraryId = Guid.NewGuid();
        BookEntity changedBook = _bookEntityFixture.Create();
        changedBook.LibraryId = libraryId;
        changedBook.Path = "/books/changed.epub";
        changedBook.MetadataStatus = MetadataStatus.Enriched;
        changedBook.BookArtwork = [_bookArtworkEntityFixture.Create(bookId: changedBook.Id, artworkType: ArtworkType.Cover, ordinal: 0, status: ArtworkStatus.Enriched)];
        BookEntity unchangedBook = _bookEntityFixture.Create();
        unchangedBook.LibraryId = libraryId;
        unchangedBook.Path = "/books/unchanged.epub";
        unchangedBook.MetadataStatus = MetadataStatus.Enriched;
        unchangedBook.BookArtwork = [_bookArtworkEntityFixture.Create(bookId: unchangedBook.Id, artworkType: ArtworkType.Cover, ordinal: 0, status: ArtworkStatus.Enriched)];
        context.Books.AddRange(changedBook, unchangedBook);
        await context.SaveChangesAsync();

        // Act
        Result<Updated> result = await sut.ResetEnrichmentStateForPathsAsync(libraryId, ["/books/changed.epub"], CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Updated, result.Value);
        BookEntity? resetBook = await context.Books.AsNoTracking().Include(book => book.BookArtwork).FirstOrDefaultAsync(book => book.Id == changedBook.Id);
        Assert.NotNull(resetBook);
        Assert.Equal(MetadataStatus.Pending, resetBook!.MetadataStatus);
        Assert.Equal(ArtworkStatus.Pending, resetBook.BookArtwork.Single().Status);
        BookEntity? keptBook = await context.Books.AsNoTracking().FirstOrDefaultAsync(book => book.Id == unchangedBook.Id);
        Assert.NotNull(keptBook);
        Assert.Equal(MetadataStatus.Enriched, keptBook!.MetadataStatus);
    }

    [Fact]
    public async Task ResetMetadataStatusForLibraryAsync_WhenCalled_ShouldResetTheMetadataStatusOfAllBooksOfTheLibrary()
    {
        // Arrange
        // the reset methods use ExecuteUpdateAsync, which is not supported by the in-memory provider, so a real SQLite database is used
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-bookrepo-reset-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        LuminaDbContext context = new(new DbContextOptionsBuilder<LuminaDbContext>().UseSqlite(anchorConnection.ConnectionString).Options);
        context.Database.EnsureCreated();
        BookRepository sut = new(context);

        Guid libraryId = Guid.NewGuid();
        BookEntity enrichedBook = _bookEntityFixture.Create();
        enrichedBook.LibraryId = libraryId;
        enrichedBook.MetadataStatus = MetadataStatus.Enriched;
        BookEntity bookOfAnotherLibrary = _bookEntityFixture.Create();
        bookOfAnotherLibrary.LibraryId = Guid.NewGuid();
        bookOfAnotherLibrary.MetadataStatus = MetadataStatus.Enriched;
        context.Books.AddRange(enrichedBook, bookOfAnotherLibrary);
        await context.SaveChangesAsync();

        // Act
        Result<Updated> result = await sut.ResetMetadataStatusForLibraryAsync(libraryId, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Updated, result.Value);
        BookEntity? resetBook = await context.Books.AsNoTracking().FirstOrDefaultAsync(book => book.Id == enrichedBook.Id);
        Assert.NotNull(resetBook);
        Assert.Equal(MetadataStatus.Pending, resetBook!.MetadataStatus);
        BookEntity? keptBook = await context.Books.AsNoTracking().FirstOrDefaultAsync(book => book.Id == bookOfAnotherLibrary.Id);
        Assert.NotNull(keptBook);
        Assert.Equal(MetadataStatus.Enriched, keptBook!.MetadataStatus);
    }

    [Fact]
    public async Task ResetArtworkStatusForLibraryAsync_WhenCalled_ShouldResetTheArtworkStatusOfTheLibraryBooks()
    {
        // Arrange
        // the reset methods use ExecuteUpdateAsync, which is not supported by the in-memory provider, so a real SQLite database is used
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-bookrepo-reset-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        LuminaDbContext context = new(new DbContextOptionsBuilder<LuminaDbContext>().UseSqlite(anchorConnection.ConnectionString).Options);
        context.Database.EnsureCreated();
        BookRepository sut = new(context);

        Guid libraryId = Guid.NewGuid();
        BookEntity book = _bookEntityFixture.Create();
        book.LibraryId = libraryId;
        book.BookArtwork = [_bookArtworkEntityFixture.Create(bookId: book.Id, artworkType: ArtworkType.Cover, ordinal: 0, status: ArtworkStatus.Enriched)];
        BookEntity bookOfAnotherLibrary = _bookEntityFixture.Create();
        bookOfAnotherLibrary.LibraryId = Guid.NewGuid();
        bookOfAnotherLibrary.BookArtwork = [_bookArtworkEntityFixture.Create(bookId: bookOfAnotherLibrary.Id, artworkType: ArtworkType.Cover, ordinal: 0, status: ArtworkStatus.Enriched)];
        context.Books.AddRange(book, bookOfAnotherLibrary);
        await context.SaveChangesAsync();

        // Act
        Result<Updated> result = await sut.ResetArtworkStatusForLibraryAsync(libraryId, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Updated, result.Value);
        BookArtworkEntity? resetArtwork = await context.Set<BookArtworkEntity>().AsNoTracking().FirstOrDefaultAsync(artwork => artwork.BookId == book.Id);
        Assert.NotNull(resetArtwork);
        Assert.Equal(ArtworkStatus.Pending, resetArtwork!.Status);
        BookArtworkEntity? keptArtwork = await context.Set<BookArtworkEntity>().AsNoTracking().FirstOrDefaultAsync(artwork => artwork.BookId == bookOfAnotherLibrary.Id);
        Assert.NotNull(keptArtwork);
        Assert.Equal(ArtworkStatus.Enriched, keptArtwork!.Status);
    }

    [Fact]
    public async Task GetAuthorsDisplayNamesByBookIdsAsync_WhenCalled_ShouldReturnTheAuthorDisplayNames()
    {
        // Arrange
        BookEntity firstBook = _bookEntityFixture.Create();
        BookEntity secondBook = _bookEntityFixture.Create();
        BookEntity bookWithoutAuthor = _bookEntityFixture.Create();
        _mockContext.Books.AddRange(firstBook, secondBook, bookWithoutAuthor);
        await _mockContext.SaveChangesAsync();

        MediaContributorEntity author = _mediaContributorEntityFixture.Create(displayName: "Frank Herbert");
        MediaContributorEntity translator = _mediaContributorEntityFixture.Create(displayName: "Jane Translator");
        _mockContext.MediaContributors.AddRange(author, translator);
        await _mockContext.SaveChangesAsync();

        _mockContext.BookContributors.AddRange(
            _bookContributorEntityFixture.Create(bookId: firstBook.Id, mediaContributorId: author.Id, roleName: "Author", roleCategory: MediaContributorRoleCategory.Author),
            _bookContributorEntityFixture.Create(bookId: secondBook.Id, mediaContributorId: translator.Id, roleName: "Translator", roleCategory: MediaContributorRoleCategory.Translator));
        await _mockContext.SaveChangesAsync();

        // Act
        Result<IReadOnlyDictionary<Guid, string?>> result = await _sut.GetAuthorsDisplayNamesByBookIdsAsync([firstBook.Id, secondBook.Id, bookWithoutAuthor.Id], CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        // only the author contributor of the first book is returned; the book with no author is not included
        Assert.Single(result.Value);
        Assert.Equal("Frank Herbert", result.Value[firstBook.Id]);
    }

    [Fact]
    public async Task DeleteAsync_WhenBookExists_ShouldRemoveItAndReturnDeleted()
    {
        // Arrange
        BookEntity book = _bookEntityFixture.Create();
        _mockContext.Books.Add(book);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<Deleted> result = await _sut.DeleteAsync(book.Id, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Deleted, result.Value);
        EntityEntry<BookEntity>? deletedBook = _mockContext.ChangeTracker.Entries<BookEntity>()
            .FirstOrDefault(entityEntry => entityEntry.State == EntityState.Deleted && entityEntry.Entity.Id == book.Id);
        Assert.NotNull(deletedBook);
    }

    [Fact]
    public async Task DeleteAsync_WhenBookDoesNotExist_ShouldReturnError()
    {
        // Act
        Result<Deleted> result = await _sut.DeleteAsync(Guid.NewGuid(), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.WrittenContent.BookNotFound, result.FirstError);
    }
}
