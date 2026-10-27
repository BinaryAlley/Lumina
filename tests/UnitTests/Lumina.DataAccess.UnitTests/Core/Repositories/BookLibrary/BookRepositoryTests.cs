#region ========================================================================= USING =====================================================================================
using EntityFrameworkCore.Testing.NSubstitute;
using Lumina.Application.Common.DataAccess.Entities.Common;
using Lumina.Application.Common.DataAccess.Entities.MediaContributors;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Common.DTO.Filtering;
using Lumina.Application.Common.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Common.DTO.Pagination;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Common;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaContributors;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Fixtures.Common.DTO.Filtering;
using Lumina.Application.Fixtures.Common.DTO.Pagination;
using Lumina.DataAccess.Core.Repositories.BookLibrary;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.BookLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.Common;
using Lumina.Domain.SharedKernel.Common.Enums.MediaContributors;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.DataAccess.UnitTests.Core.Repositories.BookLibrary;

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

        // Check if the book was added to the context's ChangeTracker.
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
        Assert.Single(_mockContext.ChangeTracker.Entries<BookEntity>()); // Only the existing book should be in the context.
    }

    [Fact]
    public async Task InsertAsync_WhenAnotherBookOfTheSameLibraryHasTheSamePath_ShouldReturnError()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        BookEntity existingBook = _bookEntityFixture.Create();
        existingBook.LibraryId = libraryId;
        existingBook.Path = "/books/existing.epub";
        _mockContext.Books.Add(existingBook);
        await _mockContext.SaveChangesAsync();

        BookEntity bookModel = _bookEntityFixture.Create();
        bookModel.LibraryId = libraryId;
        bookModel.Path = "/books/existing.epub";

        // Act
        Result<Created> result = await _sut.InsertAsync(bookModel, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.WrittenContent.BookAlreadyExists, result.FirstError);
    }

    [Fact]
    public async Task InsertAsync_WhenAnotherLibraryHasABookWithTheSamePath_ShouldAddBookToContextAndReturnCreated()
    {
        // Arrange
        BookEntity existingBook = _bookEntityFixture.Create();
        existingBook.Path = "/books/existing.epub";
        _mockContext.Books.Add(existingBook);
        await _mockContext.SaveChangesAsync();

        BookEntity bookModel = _bookEntityFixture.Create();
        bookModel.LibraryId = Guid.NewGuid();
        bookModel.Path = "/books/existing.epub";

        // Act
        Result<Created> result = await _sut.InsertAsync(bookModel, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Created, result.Value);
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
    public async Task GetAllAsync_WhenCalled_ShouldReturnAllBooksOfTheLibrary()
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
        Result<PaginatedResultDto<BookEntity>> result = await _sut.GetAllAsync<LibraryFilterDto>(filterModel: filter, cancellationToken: CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotNull(result.Value);
        Assert.Equal(3, result.Value.Data.Count);
        Assert.Equal([.. books.OrderBy(book => book.Id)], [.. result.Value.Data.OrderBy(book => book.Id)]);
    }

    [Fact]
    public async Task GetAllAsync_WhenNoBooksExist_ShouldReturnEmptyList()
    {
        // Arrange
        LibraryFilterDto filter = _libraryFilterDtoFixture.Create();

        // Act
        Result<PaginatedResultDto<BookEntity>> result = await _sut.GetAllAsync<LibraryFilterDto>(filterModel: filter, cancellationToken: CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotNull(result.Value);
        Assert.Empty(result.Value.Data);
    }

    [Fact]
    public async Task GetAllAsync_WhenPaginationIsNotProvided_ShouldIncludeRelatedEntities()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        BookEntity book = _bookEntityFixture.Create();
        book.LibraryId = libraryId;
        book.Tags = [_tagEntityFixture.Create(name: "Tag1"), _tagEntityFixture.Create(name: "Tag2")];
        book.Genres = [_genreEntityFixture.Create(name: "Genre1"), _genreEntityFixture.Create(name: "Genre2")];
        book.ISBNs = [_isbnEntityFixture.Create(value: "1234567890", format: IsbnFormat.Isbn10), _isbnEntityFixture.Create(value: "1234567890123", format: IsbnFormat.Isbn13)];
        _mockContext.Books.Add(book);
        await _mockContext.SaveChangesAsync();

        LibraryFilterDto filter = _libraryFilterDtoFixture.Create(libraryId: libraryId);

        // Act
        Result<PaginatedResultDto<BookEntity>> result = await _sut.GetAllAsync<LibraryFilterDto>(filterModel: filter, cancellationToken: CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotNull(result.Value);
        Assert.Single(result.Value.Data);
        BookEntity retrievedBook = result.Value.Data.First();
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
    public async Task GetExistingPathsAsync_WhenPathsIsEmpty_ShouldReturnAnEmptyCollection()
    {
        // Arrange
        IReadOnlyCollection<string> paths = [];

        // Act
        Result<IReadOnlyCollection<string>> result = await _sut.GetExistingPathsAsync(Guid.NewGuid(), paths, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task GetExistingPathsAsync_WhenCalled_ShouldReturnOnlyThePathsUsedByBooksOfTheLibrary()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        BookEntity bookOfLibrary = _bookEntityFixture.Create(libraryId: libraryId, path: "/books/tolkien/the-hobbit.epub");
        BookEntity bookOfAnotherLibrary = _bookEntityFixture.Create(libraryId: Guid.NewGuid(), path: "/books/tolkien/the-lord-of-the-rings.epub");
        _mockContext.Books.AddRange(bookOfLibrary, bookOfAnotherLibrary);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<IReadOnlyCollection<string>> result = await _sut.GetExistingPathsAsync(libraryId, ["/books/tolkien/the-hobbit.epub", "/books/tolkien/the-lord-of-the-rings.epub"], CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        string existingPath = Assert.Single(result.Value);
        Assert.Equal("/books/tolkien/the-hobbit.epub", existingPath);
    }

    [Fact]
    public async Task GetExistingPathsAsync_WhenInputContainsDuplicatePaths_ShouldDeDuplicateTheResult()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        BookEntity book = _bookEntityFixture.Create(libraryId: libraryId, path: "/books/tolkien/the-hobbit.epub");
        _mockContext.Books.Add(book);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<IReadOnlyCollection<string>> result = await _sut.GetExistingPathsAsync(libraryId, ["/books/tolkien/the-hobbit.epub", "/books/tolkien/the-hobbit.epub", "/books/tolkien/the-hobbit.epub"], CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Single(result.Value);
        Assert.Equal("/books/tolkien/the-hobbit.epub", result.Value.First());
    }

    [Fact]
    public async Task GetExistingPathsAsync_WhenPathDiffersOnlyByCase_ShouldNotReportItAsExisting()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        BookEntity book = _bookEntityFixture.Create(libraryId: libraryId, path: "/Books/Tolkien/The-Hobbit.epub");
        _mockContext.Books.Add(book);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<IReadOnlyCollection<string>> result = await _sut.GetExistingPathsAsync(libraryId, ["/books/tolkien/the-hobbit.epub"], CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Empty(result.Value);
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
    public async Task UpdateAsync_WhenChangingIdentityAndEnrichmentColumns_ShouldPreserveTheStoredValues()
    {
        // Arrange
        BookEntity storedBook = _bookEntityFixture.Create();
        _mockContext.Books.Add(storedBook);
        await _mockContext.SaveChangesAsync();

        Guid storedLibraryId = storedBook.LibraryId;
        string storedPath = storedBook.Path;
        MetadataStatus storedMetadataStatus = storedBook.MetadataStatus;
        DateTime? storedLastMetadataUpdateUtc = storedBook.LastMetadataUpdateUtc;
        string? storedMetadataProvider = storedBook.MetadataProvider;
        DateTime storedCreatedOnUtc = storedBook.CreatedOnUtc;
        Guid storedCreatedBy = storedBook.CreatedBy;

        BookEntity data = _bookEntityFixture.Create(id: storedBook.Id);
        data.Title = "Updated Title";
        data.LibraryId = Guid.NewGuid();
        data.Path = "/books/updated.epub";
        data.MetadataStatus = MetadataStatus.Enriched;
        data.LastMetadataUpdateUtc = DateTime.UtcNow;
        data.MetadataProvider = "UpdatedProvider";
        data.CreatedOnUtc = DateTime.UtcNow.AddYears(-10);
        data.CreatedBy = Guid.NewGuid();

        // Act
        Result<Updated> result = await _sut.UpdateAsync(data, CancellationToken.None);
        await _mockContext.SaveChangesAsync();

        // Assert
        Assert.False(result.IsFailure);
        BookEntity? retrievedBook = await _mockContext.Books
            .Include(book => book.Tags)
            .Include(book => book.Genres)
            .Include(book => book.ISBNs)
            .Include(book => book.Ratings)
            .Include(book => book.Contributors)
            .FirstOrDefaultAsync(book => book.Id == storedBook.Id);
        Assert.NotNull(retrievedBook);
        Assert.Equal("Updated Title", retrievedBook!.Title);
        // The identity, enrichment and audit columns of the stored book are never overwritten by an edit.
        Assert.Equal(storedLibraryId, retrievedBook.LibraryId);
        Assert.Equal(storedPath, retrievedBook.Path);
        Assert.Equal(storedMetadataStatus, retrievedBook.MetadataStatus);
        Assert.Equal(storedLastMetadataUpdateUtc, retrievedBook.LastMetadataUpdateUtc);
        Assert.Equal(storedMetadataProvider, retrievedBook.MetadataProvider);
        Assert.Equal(storedCreatedOnUtc, retrievedBook.CreatedOnUtc);
        Assert.Equal(storedCreatedBy, retrievedBook.CreatedBy);
    }

    [Fact]
    public async Task UpdateAsync_WhenStoredBookHasArtwork_ShouldPreserveTheStoredArtwork()
    {
        // Arrange
        BookEntity storedBook = _bookEntityFixture.Create();
        storedBook.Artwork = [_bookArtworkEntityFixture.Create(bookId: storedBook.Id, artworkType: ArtworkType.Cover, ordinal: 0, status: ArtworkStatus.Enriched, fileName: "cover.jpg")];
        _mockContext.Books.Add(storedBook);
        await _mockContext.SaveChangesAsync();

        BookEntity data = _bookEntityFixture.Create(id: storedBook.Id);
        data.Title = "Updated Title";

        // Act
        Result<Updated> result = await _sut.UpdateAsync(data, CancellationToken.None);
        await _mockContext.SaveChangesAsync();

        // Assert
        Assert.False(result.IsFailure);
        BookEntity? retrievedBook = await _mockContext.Books
            .Include(book => book.Artwork)
            .FirstOrDefaultAsync(book => book.Id == storedBook.Id);
        Assert.NotNull(retrievedBook);
        BookArtworkEntity retrievedArtwork = Assert.Single(retrievedBook!.Artwork);
        Assert.Equal("cover.jpg", retrievedArtwork.FileName);
        Assert.Equal(ArtworkStatus.Enriched, retrievedArtwork.Status);
    }

    [Fact]
    public async Task UpdateAsync_WhenDataContainsExistingTagAndGenreNames_ShouldReuseTheStoredTagAndGenreEntities()
    {
        // Arrange
        TagEntity existingTag = _tagEntityFixture.Create(name: "ExistingTag");
        GenreEntity existingGenre = _genreEntityFixture.Create(name: "ExistingGenre");
        BookEntity storedBook = _bookEntityFixture.Create();
        storedBook.Tags = [existingTag];
        storedBook.Genres = [existingGenre];
        _mockContext.Books.Add(storedBook);
        await _mockContext.SaveChangesAsync();

        BookEntity data = _bookEntityFixture.Create(id: storedBook.Id);
        data.Tags = [_tagEntityFixture.Create(name: "ExistingTag")];
        data.Genres = [_genreEntityFixture.Create(name: "ExistingGenre")];

        // Act
        Result<Updated> result = await _sut.UpdateAsync(data, CancellationToken.None);
        await _mockContext.SaveChangesAsync();

        // Assert
        Assert.False(result.IsFailure);
        BookEntity? retrievedBook = await _mockContext.Books
            .Include(book => book.Tags)
            .Include(book => book.Genres)
            .FirstOrDefaultAsync(book => book.Id == storedBook.Id);
        Assert.NotNull(retrievedBook);
        TagEntity retrievedTag = Assert.Single(retrievedBook!.Tags);
        GenreEntity retrievedGenre = Assert.Single(retrievedBook.Genres);
        Assert.Equal("ExistingTag", retrievedTag.Name);
        Assert.Equal("ExistingGenre", retrievedGenre.Name);
        // The stored rows of the shared tables are not duplicated by the update.
        Assert.Single(_mockContext.Set<TagEntity>().Where(tag => tag.Name == "ExistingTag"));
        Assert.Single(_mockContext.Set<GenreEntity>().Where(genre => genre.Name == "ExistingGenre"));
    }

    [Fact]
    public async Task UpdateAsync_WhenReplacingOwnedCollections_ShouldReplaceIsbnsRatingsAndBookContributors()
    {
        // Arrange
        BookEntity storedBook = _bookEntityFixture.Create();
        storedBook.ISBNs = [_isbnEntityFixture.Create(value: "9780395272237", format: IsbnFormat.Isbn13)];
        storedBook.Ratings = [_bookRatingEntityFixture.Create(value: 4.5M, maxValue: 5, source: BookRatingSource.Goodreads, voteCount: 100)];
        storedBook.Contributors = [_bookContributorEntityFixture.Create(bookId: storedBook.Id, role: MediaContributorRole.Author)];
        _mockContext.Books.Add(storedBook);
        await _mockContext.SaveChangesAsync();

        BookEntity data = _bookEntityFixture.Create(id: storedBook.Id);
        data.ISBNs = [_isbnEntityFixture.Create(value: "9780261102693", format: IsbnFormat.Isbn13)];
        data.Ratings = [_bookRatingEntityFixture.Create(value: 3.5M, maxValue: 5, source: BookRatingSource.Amazon, voteCount: 50)];
        data.Contributors = [_bookContributorEntityFixture.Create(bookId: storedBook.Id, mediaContributorId: Guid.NewGuid(), role: MediaContributorRole.Illustrator)];

        // Act
        Result<Updated> result = await _sut.UpdateAsync(data, CancellationToken.None);
        await _mockContext.SaveChangesAsync();

        // Assert
        Assert.False(result.IsFailure);
        BookEntity? retrievedBook = await _mockContext.Books
            .Include(book => book.ISBNs)
            .Include(book => book.Ratings)
            .Include(book => book.Contributors)
            .FirstOrDefaultAsync(book => book.Id == storedBook.Id);
        Assert.NotNull(retrievedBook);
        IsbnEntity retrievedIsbn = Assert.Single(retrievedBook!.ISBNs);
        BookRatingEntity retrievedRating = Assert.Single(retrievedBook.Ratings);
        BookContributorEntity retrievedContributor = Assert.Single(retrievedBook.Contributors);
        Assert.Equal("9780261102693", retrievedIsbn.Value);
        Assert.Equal(3.5M, retrievedRating.Value);
        Assert.Equal(MediaContributorRole.Illustrator, retrievedContributor.Role);
    }

    [Fact]
    public async Task GetAllAsync_WhenFilterIncludesLibraryId_ShouldReturnPaginatedBooks()
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
        Result<PaginatedResultDto<BookEntity>> result = await _sut.GetAllAsync(paginationData, null, null, filter, cancellationToken: CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(3, result.Value.Count);
        Assert.Equal(3, result.Value.Data.Count);
        Assert.Equal(1, result.Value.CurrentPage);
        Assert.Equal(10, result.Value.PerPage);
        Assert.Equal(1, result.Value.NumberOfPages);
    }

    [Fact]
    public async Task GetAllAsync_WhenFilterDoesNotIncludeLibraryId_ShouldReturnError()
    {
        // Arrange
        PaginationDataDto paginationData = _paginationDataDtoFixture.Create(currentPage: 1, perPage: 10);
        BaseFilterDto filter = _baseFilterDtoFixture.Create();

        // Act
        Result<PaginatedResultDto<BookEntity>> result = await _sut.GetAllAsync(paginationData, null, null, filter, cancellationToken: CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Library.FilterMustIncludeLibraryId, result.FirstError);
    }

    [Fact]
    public async Task GetAllAsync_WhenFilterLibraryIdIsEmpty_ShouldReturnError()
    {
        // Arrange
        PaginationDataDto paginationData = _paginationDataDtoFixture.Create(currentPage: 1, perPage: 10);
        LibraryFilterDto filter = _libraryFilterDtoFixture.Create(libraryId: Guid.Empty);

        // Act
        Result<PaginatedResultDto<BookEntity>> result = await _sut.GetAllAsync(paginationData, null, null, filter, cancellationToken: CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Library.FilterMustIncludeLibraryId, result.FirstError);
    }

    [Fact]
    public async Task GetAllAsync_WhenPaginationDataIsNull_ShouldReturnAllBooks()
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
        Result<PaginatedResultDto<BookEntity>> result = await _sut.GetAllAsync(null, null, null, filter, cancellationToken: CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(3, result.Value.Count);
        Assert.Equal(3, result.Value.Data.Count);
        Assert.Equal(1, result.Value.CurrentPage);
        Assert.Equal(3, result.Value.PerPage);
        Assert.Equal(1, result.Value.NumberOfPages);
    }

    [Fact]
    public async Task GetAllAsync_WhenSearchTermProvided_ShouldReturnOnlyMatchingBooks()
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
        Result<PaginatedResultDto<BookEntity>> result = await _sut.GetAllAsync(paginationData, null, null, filter, cancellationToken: CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        BookEntity book = Assert.Single(result.Value.Data);
        Assert.Equal("The Fellowship of the Ring", book.Title);
        Assert.Equal(1, result.Value.Count);
    }

    [Fact]
    public async Task GetAllAsync_WhenPageSizeSmallerThanBookCount_ShouldReturnCorrectPageMetadata()
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
        Result<PaginatedResultDto<BookEntity>> result = await _sut.GetAllAsync(paginationData, null, null, filter, cancellationToken: CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(5, result.Value.Count);
        Assert.Equal(3, result.Value.NumberOfPages);
        Assert.Equal(2, result.Value.CurrentPage);
        Assert.Equal(2, result.Value.PerPage);
        Assert.Equal(2, result.Value.Data.Count);
    }

    [Fact]
    public async Task GetAllAsync_WhenSortByTitleDescending_ShouldReturnBooksInDescendingOrder()
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
        Result<PaginatedResultDto<BookEntity>> result = await _sut.GetAllAsync(paginationData, "title", SortOrder.Descending, filter, cancellationToken: CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(["Book B", "Book A"], result.Value.Data.Select(book => book.Title));
    }

    [Fact]
    public async Task GetAllAsync_WhenPaginationIsProvided_ShouldIncludeRelatedEntities()
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
        Result<PaginatedResultDto<BookEntity>> result = await _sut.GetAllAsync(paginationData, null, null, filter, cancellationToken: CancellationToken.None);

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
        bookWithoutCover.Artwork = [];
        BookEntity bookWithEnrichedCover = _bookEntityFixture.Create();
        bookWithEnrichedCover.LibraryId = libraryId;
        bookWithEnrichedCover.Path = "/books/b.epub";
        bookWithEnrichedCover.Artwork = [_bookArtworkEntityFixture.Create(bookId: bookWithEnrichedCover.Id, artworkType: ArtworkType.Cover, ordinal: 0, status: ArtworkStatus.Enriched)];
        BookEntity bookWithFailedCover = _bookEntityFixture.Create();
        bookWithFailedCover.LibraryId = libraryId;
        bookWithFailedCover.Path = "/books/c.epub";
        bookWithFailedCover.Artwork = [_bookArtworkEntityFixture.Create(bookId: bookWithFailedCover.Id, artworkType: ArtworkType.Cover, ordinal: 0, status: ArtworkStatus.Failed)];
        BookEntity bookOfAnotherLibrary = _bookEntityFixture.Create();
        bookOfAnotherLibrary.LibraryId = Guid.NewGuid();
        bookOfAnotherLibrary.Artwork = [];
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
        bookWithoutCover.Artwork = [];
        BookEntity bookWithEnrichedCover = _bookEntityFixture.Create();
        bookWithEnrichedCover.LibraryId = libraryId;
        bookWithEnrichedCover.Artwork = [_bookArtworkEntityFixture.Create(bookId: bookWithEnrichedCover.Id, artworkType: ArtworkType.Cover, ordinal: 0, status: ArtworkStatus.Enriched)];
        BookEntity bookOfAnotherLibrary = _bookEntityFixture.Create();
        bookOfAnotherLibrary.LibraryId = Guid.NewGuid();
        bookOfAnotherLibrary.Artwork = [];
        _mockContext.Books.AddRange(bookWithoutCover, bookWithEnrichedCover, bookOfAnotherLibrary);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<int> result = await _sut.GetBooksNeedingArtworkCountAsync(libraryId, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(1, result.Value);
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
            _bookContributorEntityFixture.Create(bookId: firstBook.Id, mediaContributorId: author.Id, role: MediaContributorRole.Author),
            _bookContributorEntityFixture.Create(bookId: secondBook.Id, mediaContributorId: translator.Id, role: MediaContributorRole.Translator));
        await _mockContext.SaveChangesAsync();

        // Act
        Result<IReadOnlyDictionary<Guid, string?>> result = await _sut.GetAuthorsDisplayNamesByBookIdsAsync([firstBook.Id, secondBook.Id, bookWithoutAuthor.Id], CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        // Only the author contributor of the first book is returned; the book with no author is not included.
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
        Result<Deleted> result = await _sut.DeleteByIdAsync(book.Id, CancellationToken.None);

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
        Result<Deleted> result = await _sut.DeleteByIdAsync(Guid.NewGuid(), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.WrittenContent.BookNotFound, result.FirstError);
    }

    [Fact]
    public async Task GetByIdAsync_WhenBookExists_ShouldReturnTheBook()
    {
        // Arrange
        BookEntity book = _bookEntityFixture.Create();
        _mockContext.Books.Add(book);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<BookEntity?> result = await _sut.GetByIdAsync(book.Id, cancellationToken: CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(book.Id, result.Value!.Id);
    }

    [Fact]
    public async Task GetByIdAsync_WhenBookDoesNotExist_ShouldReturnNull()
    {
        // Act
        Result<BookEntity?> result = await _sut.GetByIdAsync(Guid.NewGuid(), cancellationToken: CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Null(result.Value);
    }

    [Fact]
    public async Task GetByIdAsync_WhenBookExists_ShouldIncludeRelatedEntities()
    {
        // Arrange
        BookEntity book = _bookEntityFixture.Create();
        book.Tags = [_tagEntityFixture.Create(name: "Tag1"), _tagEntityFixture.Create(name: "Tag2")];
        book.Genres = [_genreEntityFixture.Create(name: "Genre1"), _genreEntityFixture.Create(name: "Genre2")];
        book.ISBNs = [_isbnEntityFixture.Create(value: "1234567890", format: IsbnFormat.Isbn10), _isbnEntityFixture.Create(value: "1234567890123", format: IsbnFormat.Isbn13)];
        book.Artwork = [_bookArtworkEntityFixture.Create(bookId: book.Id, artworkType: ArtworkType.Cover, ordinal: 0, status: ArtworkStatus.Enriched)];
        _mockContext.Books.Add(book);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<BookEntity?> result = await _sut.GetByIdAsync(book.Id, cancellationToken: CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        BookEntity retrievedBook = result.Value!;
        Assert.Equal(2, retrievedBook.Tags.Count);
        Assert.Equal(2, retrievedBook.Genres.Count);
        Assert.Equal(2, retrievedBook.ISBNs.Count);
        Assert.Single(retrievedBook.Artwork);
    }

    [Fact]
    public async Task GetBooksNeedingMetadataCountAsync_WhenCalled_ShouldCountOnlyBooksWhoseMetadataIsNotEnriched()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        BookEntity pendingBook = _bookEntityFixture.Create();
        pendingBook.LibraryId = libraryId;
        pendingBook.MetadataStatus = MetadataStatus.Pending;
        BookEntity enrichedBook = _bookEntityFixture.Create();
        enrichedBook.LibraryId = libraryId;
        enrichedBook.MetadataStatus = MetadataStatus.Enriched;
        BookEntity failedBook = _bookEntityFixture.Create();
        failedBook.LibraryId = libraryId;
        failedBook.MetadataStatus = MetadataStatus.Failed;
        BookEntity bookOfAnotherLibrary = _bookEntityFixture.Create();
        bookOfAnotherLibrary.LibraryId = Guid.NewGuid();
        bookOfAnotherLibrary.MetadataStatus = MetadataStatus.Pending;
        _mockContext.Books.AddRange(pendingBook, enrichedBook, failedBook, bookOfAnotherLibrary);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<int> result = await _sut.GetBooksNeedingMetadataCountAsync(libraryId, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(2, result.Value);
    }

    [Fact]
    public async Task GetAllAsync_WhenSortByLanguageCodeAscending_ShouldReturnBooksInAscendingOrder()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        BookEntity frenchBook = _bookEntityFixture.Create(title: "French Book");
        frenchBook.LibraryId = libraryId;
        frenchBook.LanguageCode = "fr";
        BookEntity englishBook = _bookEntityFixture.Create(title: "English Book");
        englishBook.LibraryId = libraryId;
        englishBook.LanguageCode = "en";
        _mockContext.Books.AddRange(frenchBook, englishBook);
        await _mockContext.SaveChangesAsync();

        PaginationDataDto paginationData = _paginationDataDtoFixture.Create(currentPage: 1, perPage: 10);
        LibraryFilterDto filter = _libraryFilterDtoFixture.Create(libraryId: libraryId);

        // Act
        Result<PaginatedResultDto<BookEntity>> result = await _sut.GetAllAsync(paginationData, "languageCode", SortOrder.Ascending, filter, cancellationToken: CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(["English Book", "French Book"], result.Value.Data.Select(book => book.Title));
    }

    [Fact]
    public async Task GetAllAsync_WhenSortByLanguageCodeDescending_ShouldReturnBooksInDescendingOrder()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        BookEntity englishBook = _bookEntityFixture.Create(title: "English Book");
        englishBook.LibraryId = libraryId;
        englishBook.LanguageCode = "en";
        BookEntity frenchBook = _bookEntityFixture.Create(title: "French Book");
        frenchBook.LibraryId = libraryId;
        frenchBook.LanguageCode = "fr";
        _mockContext.Books.AddRange(englishBook, frenchBook);
        await _mockContext.SaveChangesAsync();

        PaginationDataDto paginationData = _paginationDataDtoFixture.Create(currentPage: 1, perPage: 10);
        LibraryFilterDto filter = _libraryFilterDtoFixture.Create(libraryId: libraryId);

        // Act
        Result<PaginatedResultDto<BookEntity>> result = await _sut.GetAllAsync(paginationData, "languageCode", SortOrder.Descending, filter, cancellationToken: CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(["French Book", "English Book"], result.Value.Data.Select(book => book.Title));
    }

    [Fact]
    public async Task GetAllAsync_WhenSortByFormatAscending_ShouldReturnBooksInAscendingOrder()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        BookEntity paperbackBook = _bookEntityFixture.Create(title: "Paperback Book");
        paperbackBook.LibraryId = libraryId;
        paperbackBook.Format = BookFormat.Paperback;
        BookEntity hardcoverBook = _bookEntityFixture.Create(title: "Hardcover Book");
        hardcoverBook.LibraryId = libraryId;
        hardcoverBook.Format = BookFormat.Hardcover;
        _mockContext.Books.AddRange(paperbackBook, hardcoverBook);
        await _mockContext.SaveChangesAsync();

        PaginationDataDto paginationData = _paginationDataDtoFixture.Create(currentPage: 1, perPage: 10);
        LibraryFilterDto filter = _libraryFilterDtoFixture.Create(libraryId: libraryId);

        // Act
        Result<PaginatedResultDto<BookEntity>> result = await _sut.GetAllAsync(paginationData, "format", SortOrder.Ascending, filter, cancellationToken: CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(["Hardcover Book", "Paperback Book"], result.Value.Data.Select(book => book.Title));
    }

    [Fact]
    public async Task GetAllAsync_WhenSortByFormatDescending_ShouldReturnBooksInDescendingOrder()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        BookEntity hardcoverBook = _bookEntityFixture.Create(title: "Hardcover Book");
        hardcoverBook.LibraryId = libraryId;
        hardcoverBook.Format = BookFormat.Hardcover;
        BookEntity paperbackBook = _bookEntityFixture.Create(title: "Paperback Book");
        paperbackBook.LibraryId = libraryId;
        paperbackBook.Format = BookFormat.Paperback;
        _mockContext.Books.AddRange(hardcoverBook, paperbackBook);
        await _mockContext.SaveChangesAsync();

        PaginationDataDto paginationData = _paginationDataDtoFixture.Create(currentPage: 1, perPage: 10);
        LibraryFilterDto filter = _libraryFilterDtoFixture.Create(libraryId: libraryId);

        // Act
        Result<PaginatedResultDto<BookEntity>> result = await _sut.GetAllAsync(paginationData, "format", SortOrder.Descending, filter, cancellationToken: CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(["Paperback Book", "Hardcover Book"], result.Value.Data.Select(book => book.Title));
    }

    [Fact]
    public async Task GetAllAsync_WhenSortByMetadataProviderAscending_ShouldReturnBooksInAscendingOrder()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        BookEntity openLibraryBook = _bookEntityFixture.Create(title: "OpenLibrary Book");
        openLibraryBook.LibraryId = libraryId;
        openLibraryBook.MetadataProvider = "OpenLibrary";
        BookEntity calibreBook = _bookEntityFixture.Create(title: "Calibre Book");
        calibreBook.LibraryId = libraryId;
        calibreBook.MetadataProvider = "Calibre";
        _mockContext.Books.AddRange(openLibraryBook, calibreBook);
        await _mockContext.SaveChangesAsync();

        PaginationDataDto paginationData = _paginationDataDtoFixture.Create(currentPage: 1, perPage: 10);
        LibraryFilterDto filter = _libraryFilterDtoFixture.Create(libraryId: libraryId);

        // Act
        Result<PaginatedResultDto<BookEntity>> result = await _sut.GetAllAsync(paginationData, "metadataProvider", SortOrder.Ascending, filter, cancellationToken: CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(["Calibre Book", "OpenLibrary Book"], result.Value.Data.Select(book => book.Title));
    }

    [Fact]
    public async Task GetAllAsync_WhenSortByMetadataProviderDescending_ShouldReturnBooksInDescendingOrder()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        BookEntity calibreBook = _bookEntityFixture.Create(title: "Calibre Book");
        calibreBook.LibraryId = libraryId;
        calibreBook.MetadataProvider = "Calibre";
        BookEntity openLibraryBook = _bookEntityFixture.Create(title: "OpenLibrary Book");
        openLibraryBook.LibraryId = libraryId;
        openLibraryBook.MetadataProvider = "OpenLibrary";
        _mockContext.Books.AddRange(calibreBook, openLibraryBook);
        await _mockContext.SaveChangesAsync();

        PaginationDataDto paginationData = _paginationDataDtoFixture.Create(currentPage: 1, perPage: 10);
        LibraryFilterDto filter = _libraryFilterDtoFixture.Create(libraryId: libraryId);

        // Act
        Result<PaginatedResultDto<BookEntity>> result = await _sut.GetAllAsync(paginationData, "metadataProvider", SortOrder.Descending, filter, cancellationToken: CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(["OpenLibrary Book", "Calibre Book"], result.Value.Data.Select(book => book.Title));
    }

    [Fact]
    public async Task GetAllAsync_WhenIgnoringTheTitlePrefixForSorting_ShouldSortByTitleStrippedOfItsPrefix()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        BookEntity beneathBook = _bookEntityFixture.Create(title: "Beneath the Surface", includeMetadata: false);
        beneathBook.LibraryId = libraryId;
        BookEntity artOfWarBook = _bookEntityFixture.Create(title: "The Art of War", includeMetadata: false);
        artOfWarBook.LibraryId = libraryId;
        _mockContext.Books.AddRange(beneathBook, artOfWarBook);
        await _mockContext.SaveChangesAsync();

        PaginationDataDto paginationData = _paginationDataDtoFixture.Create(currentPage: 1, perPage: 10);
        LibraryFilterDto filter = _libraryFilterDtoFixture.Create(libraryId: libraryId, shouldIgnoreThePrefixForAlphaPicker: true);

        // Act
        Result<PaginatedResultDto<BookEntity>> result = await _sut.GetAllAsync(paginationData, null, null, filter, cancellationToken: CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        // Without stripping the prefix, "The Art of War" would be sorted after "Beneath the Surface", since its sort key would start with "the" instead of "art".
        Assert.Equal(["The Art of War", "Beneath the Surface"], result.Value.Data.Select(book => book.Title));
    }

    [Fact]
    public async Task GetAllLiteAsync_WhenFilterIncludesLibraryId_ShouldReturnPaginatedLiteRows()
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
        Result<PaginatedResultDto<BookLiteRow>> result = await _sut.GetAllLiteAsync(paginationData, null, null, filter, cancellationToken: CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(3, result.Value.Count);
        Assert.Equal(3, result.Value.Data.Count);
        Assert.Equal(1, result.Value.CurrentPage);
        Assert.Equal(10, result.Value.PerPage);
        Assert.Equal(1, result.Value.NumberOfPages);
    }

    [Fact]
    public async Task GetAllLiteAsync_WhenFilterDoesNotIncludeLibraryId_ShouldReturnError()
    {
        // Arrange
        PaginationDataDto paginationData = _paginationDataDtoFixture.Create(currentPage: 1, perPage: 10);
        BaseFilterDto filter = _baseFilterDtoFixture.Create();

        // Act
        Result<PaginatedResultDto<BookLiteRow>> result = await _sut.GetAllLiteAsync(paginationData, null, null, filter, cancellationToken: CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Library.FilterMustIncludeLibraryId, result.FirstError);
    }

    [Fact]
    public async Task GetAllLiteAsync_WhenFilterLibraryIdIsEmpty_ShouldReturnError()
    {
        // Arrange
        PaginationDataDto paginationData = _paginationDataDtoFixture.Create(currentPage: 1, perPage: 10);
        LibraryFilterDto filter = _libraryFilterDtoFixture.Create(libraryId: Guid.Empty);

        // Act
        Result<PaginatedResultDto<BookLiteRow>> result = await _sut.GetAllLiteAsync(paginationData, null, null, filter, cancellationToken: CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Library.FilterMustIncludeLibraryId, result.FirstError);
    }

    [Fact]
    public async Task GetAllLiteAsync_WhenPaginationDataIsNull_ShouldReturnAllLiteRows()
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
        Result<PaginatedResultDto<BookLiteRow>> result = await _sut.GetAllLiteAsync(null, null, null, filter, cancellationToken: CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(3, result.Value.Count);
        Assert.Equal(3, result.Value.Data.Count);
        Assert.Equal(1, result.Value.CurrentPage);
        Assert.Equal(3, result.Value.PerPage);
        Assert.Equal(1, result.Value.NumberOfPages);
    }

    [Fact]
    public async Task GetAllLiteAsync_WhenSearchTermProvided_ShouldReturnOnlyMatchingLiteRows()
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
        Result<PaginatedResultDto<BookLiteRow>> result = await _sut.GetAllLiteAsync(paginationData, null, null, filter, cancellationToken: CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        BookLiteRow row = Assert.Single(result.Value.Data);
        Assert.Equal("The Fellowship of the Ring", row.Title);
        Assert.Equal(1, result.Value.Count);
    }

    [Fact]
    public async Task GetAllLiteAsync_WhenPageSizeSmallerThanBookCount_ShouldReturnCorrectPageMetadata()
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
        Result<PaginatedResultDto<BookLiteRow>> result = await _sut.GetAllLiteAsync(paginationData, null, null, filter, cancellationToken: CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(5, result.Value.Count);
        Assert.Equal(3, result.Value.NumberOfPages);
        Assert.Equal(2, result.Value.CurrentPage);
        Assert.Equal(2, result.Value.PerPage);
        Assert.Equal(2, result.Value.Data.Count);
    }

    [Fact]
    public async Task GetAllLiteAsync_WhenSortByTitleDescending_ShouldReturnLiteRowsInDescendingOrder()
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
        Result<PaginatedResultDto<BookLiteRow>> result = await _sut.GetAllLiteAsync(paginationData, "title", SortOrder.Descending, filter, cancellationToken: CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(["Book B", "Book A"], result.Value.Data.Select(row => row.Title));
    }

    [Fact]
    public async Task GetAllLiteAsync_WhenIgnoringTheTitlePrefixForSorting_ShouldSortByTitleStrippedOfItsPrefix()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        BookEntity beneathBook = _bookEntityFixture.Create(title: "Beneath the Surface", includeMetadata: false);
        beneathBook.LibraryId = libraryId;
        BookEntity artOfWarBook = _bookEntityFixture.Create(title: "The Art of War", includeMetadata: false);
        artOfWarBook.LibraryId = libraryId;
        _mockContext.Books.AddRange(beneathBook, artOfWarBook);
        await _mockContext.SaveChangesAsync();

        PaginationDataDto paginationData = _paginationDataDtoFixture.Create(currentPage: 1, perPage: 10);
        LibraryFilterDto filter = _libraryFilterDtoFixture.Create(libraryId: libraryId, shouldIgnoreThePrefixForAlphaPicker: true);

        // Act
        Result<PaginatedResultDto<BookLiteRow>> result = await _sut.GetAllLiteAsync(paginationData, null, null, filter, cancellationToken: CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        // Without stripping the prefix, "The Art of War" would be sorted after "Beneath the Surface", since its sort key would start with "the" instead of "art".
        Assert.Equal(["The Art of War", "Beneath the Surface"], result.Value.Data.Select(row => row.Title));
    }

    [Fact]
    public async Task GetAllLiteAsync_WhenBookHasReReleaseYear_ShouldProjectReReleaseYearAsReleaseYear()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        BookEntity book = _bookEntityFixture.Create();
        book.LibraryId = libraryId;
        _mockContext.Books.Add(book);
        await _mockContext.SaveChangesAsync();

        LibraryFilterDto filter = _libraryFilterDtoFixture.Create(libraryId: libraryId);

        // Act
        Result<PaginatedResultDto<BookLiteRow>> result = await _sut.GetAllLiteAsync(null, null, null, filter, cancellationToken: CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        BookLiteRow row = Assert.Single(result.Value.Data);
        Assert.Equal(book.ReReleaseYear, row.ReleaseYear);
    }

    [Fact]
    public async Task GetAllLiteAsync_WhenBookHasNoReReleaseYear_ShouldProjectOriginalReleaseYearAsReleaseYear()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        BookEntity book = _bookEntityFixture.Create();
        book.LibraryId = libraryId;
        book.ReReleaseYear = null;
        _mockContext.Books.Add(book);
        await _mockContext.SaveChangesAsync();

        LibraryFilterDto filter = _libraryFilterDtoFixture.Create(libraryId: libraryId);

        // Act
        Result<PaginatedResultDto<BookLiteRow>> result = await _sut.GetAllLiteAsync(null, null, null, filter, cancellationToken: CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        BookLiteRow row = Assert.Single(result.Value.Data);
        Assert.Equal(book.OriginalReleaseYear, row.ReleaseYear);
    }

    [Fact]
    public async Task GetAllLiteAsync_WhenBookHasCoverArtwork_ShouldProjectTheCoverFileName()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        BookEntity book = _bookEntityFixture.Create();
        book.LibraryId = libraryId;
        book.Artwork = [_bookArtworkEntityFixture.Create(bookId: book.Id, artworkType: ArtworkType.Cover, fileName: "cover.jpg")];
        _mockContext.Books.Add(book);
        await _mockContext.SaveChangesAsync();

        LibraryFilterDto filter = _libraryFilterDtoFixture.Create(libraryId: libraryId);

        // Act
        Result<PaginatedResultDto<BookLiteRow>> result = await _sut.GetAllLiteAsync(null, null, null, filter, cancellationToken: CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        BookLiteRow row = Assert.Single(result.Value.Data);
        Assert.Equal("cover.jpg", row.CoverPath);
    }

    [Fact]
    public async Task GetAllLiteAsync_WhenBookHasNoCoverArtwork_ShouldProjectNullCoverPath()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        BookEntity book = _bookEntityFixture.Create();
        book.LibraryId = libraryId;
        book.Artwork = [];
        _mockContext.Books.Add(book);
        await _mockContext.SaveChangesAsync();

        LibraryFilterDto filter = _libraryFilterDtoFixture.Create(libraryId: libraryId);

        // Act
        Result<PaginatedResultDto<BookLiteRow>> result = await _sut.GetAllLiteAsync(null, null, null, filter, cancellationToken: CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        BookLiteRow row = Assert.Single(result.Value.Data);
        Assert.Null(row.CoverPath);
    }

    [Fact]
    public async Task GetByIdsAsync_WhenIdsIsEmpty_ShouldReturnAnEmptyList()
    {
        // Arrange
        BookEntity book = _bookEntityFixture.Create(includeMetadata: false);
        _mockContext.Books.Add(book);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<IReadOnlyList<BookEntity>> result = await _sut.GetByIdsAsync([], false, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotNull(result.Value);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task GetByIdsAsync_WhenCalled_ShouldReturnOnlyTheRequestedBooksOnceEach()
    {
        // Arrange
        BookEntity firstBook = _bookEntityFixture.Create(includeMetadata: false);
        BookEntity secondBook = _bookEntityFixture.Create(includeMetadata: false);
        BookEntity thirdBook = _bookEntityFixture.Create(includeMetadata: false);
        _mockContext.Books.AddRange(firstBook, secondBook, thirdBook);
        await _mockContext.SaveChangesAsync();

        // Act
        // A repeated Id is only ever part of a set, so the same book is never returned twice.
        Result<IReadOnlyList<BookEntity>> result = await _sut.GetByIdsAsync([firstBook.Id, thirdBook.Id, firstBook.Id], false, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(2, result.Value.Count);
        Assert.Contains(result.Value, book => book.Id == firstBook.Id);
        Assert.Contains(result.Value, book => book.Id == thirdBook.Id);
        Assert.DoesNotContain(result.Value, book => book.Id == secondBook.Id);
    }

    [Fact]
    public async Task GetByIdsAsync_WhenNavigationPropertiesShouldNotBeIncluded_ShouldReturnTheRequestedBooks()
    {
        // Arrange
        BookEntity book = _bookEntityFixture.Create(includeMetadata: false);
        _mockContext.Books.Add(book);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<IReadOnlyList<BookEntity>> result = await _sut.GetByIdsAsync([book.Id], false, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        BookEntity returnedBook = Assert.Single(result.Value);
        Assert.Equal(book.Id, returnedBook.Id);
        // The mocked in-memory provider may reuse the seeded instance, so the emptiness of the navigation collections of a non-including
        // read is asserted against the real SQLite provider in the integration tests.
    }

    [Fact]
    public async Task GetByIdsAsync_WhenNavigationPropertiesShouldBeIncluded_ShouldLoadTheRelatedCollections()
    {
        // Arrange
        MediaContributorEntity author = _mediaContributorEntityFixture.Create(displayName: "Frank Herbert");
        BookEntity book = _bookEntityFixture.Create(includeMetadata: false);
        book.Tags = [_tagEntityFixture.Create(name: "Tag1"), _tagEntityFixture.Create(name: "Tag2")];
        book.Genres = [_genreEntityFixture.Create(name: "Genre1")];
        book.ISBNs = [_isbnEntityFixture.Create(value: "9783161484100", format: IsbnFormat.Isbn13)];
        book.Ratings = [_bookRatingEntityFixture.Create(value: 4.5M, maxValue: 5, source: BookRatingSource.Goodreads, voteCount: 100)];
        book.Contributors = [_bookContributorEntityFixture.Create(bookId: book.Id, mediaContributorId: author.Id, role: MediaContributorRole.Author)];
        book.Artwork = [_bookArtworkEntityFixture.Create(bookId: book.Id, artworkType: ArtworkType.Cover, ordinal: 0, status: ArtworkStatus.Enriched)];
        _mockContext.Books.Add(book);
        _mockContext.MediaContributors.Add(author);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<IReadOnlyList<BookEntity>> result = await _sut.GetByIdsAsync([book.Id], true, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        BookEntity returnedBook = Assert.Single(result.Value);
        Assert.Equal(2, returnedBook.Tags.Count);
        Assert.Single(returnedBook.Genres);
        Assert.Single(returnedBook.ISBNs);
        Assert.Single(returnedBook.Ratings);
        Assert.Single(returnedBook.Contributors);
        Assert.Single(returnedBook.Artwork);
    }

    [Fact]
    public async Task GetByIdsAsync_WhenExactlyFiveHundredIdsAreRequested_ShouldReturnEveryBook()
    {
        // Arrange
        const int bookCount = 500;
        List<BookEntity> books = CreateBooks(bookCount);
        _mockContext.Books.AddRange(books);
        await _mockContext.SaveChangesAsync();

        // Act
        // A single full chunk proves the boundary does not drop the last chunk.
        Result<IReadOnlyList<BookEntity>> result = await _sut.GetByIdsAsync([.. books.Select(book => book.Id)], false, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(bookCount, result.Value.Count);
        Assert.Equal(
            books.Select(book => book.Id).OrderBy(id => id),
            result.Value.Select(book => book.Id).OrderBy(id => id));
    }

    [Fact]
    public async Task GetByIdsAsync_WhenMoreThanFiveHundredIdsAreRequested_ShouldReturnEveryBook()
    {
        // Arrange
        const int bookCount = 501;
        List<BookEntity> books = CreateBooks(bookCount);
        _mockContext.Books.AddRange(books);
        await _mockContext.SaveChangesAsync();

        // Act
        // The lookup is chunked in batches of 500, so the 501st Id proves the second chunk is queried as well.
        Result<IReadOnlyList<BookEntity>> result = await _sut.GetByIdsAsync([.. books.Select(book => book.Id)], false, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(bookCount, result.Value.Count);
        Assert.Equal(
            books.Select(book => book.Id).OrderBy(id => id),
            result.Value.Select(book => book.Id).OrderBy(id => id));
    }

    [Fact]
    public async Task GetAuthorsDisplayNamesByBookIdsAsync_WhenBookHasMultipleAuthors_ShouldReturnTheMinimumDisplayName()
    {
        // Arrange
        BookEntity book = _bookEntityFixture.Create(includeMetadata: false);
        _mockContext.Books.Add(book);
        MediaContributorEntity firstAuthor = _mediaContributorEntityFixture.Create(displayName: "Zoe Author");
        MediaContributorEntity secondAuthor = _mediaContributorEntityFixture.Create(displayName: "Alice Author");
        _mockContext.MediaContributors.AddRange(firstAuthor, secondAuthor);
        await _mockContext.SaveChangesAsync();
        _mockContext.BookContributors.AddRange(
            _bookContributorEntityFixture.Create(bookId: book.Id, mediaContributorId: firstAuthor.Id, role: MediaContributorRole.Author),
            _bookContributorEntityFixture.Create(bookId: book.Id, mediaContributorId: secondAuthor.Id, role: MediaContributorRole.Author));
        await _mockContext.SaveChangesAsync();

        // Act
        Result<IReadOnlyDictionary<Guid, string?>> result = await _sut.GetAuthorsDisplayNamesByBookIdsAsync([book.Id], CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal("Alice Author", result.Value[book.Id]);
    }

    [Fact]
    public async Task GetAuthorsDisplayNamesByBookIdsAsync_WhenABookIsNotRequested_ShouldNotReturnIt()
    {
        // Arrange
        BookEntity requestedBook = _bookEntityFixture.Create(includeMetadata: false);
        BookEntity bookNotRequested = _bookEntityFixture.Create(includeMetadata: false);
        _mockContext.Books.AddRange(requestedBook, bookNotRequested);
        MediaContributorEntity requestedAuthor = _mediaContributorEntityFixture.Create(displayName: "Requested Author");
        MediaContributorEntity otherAuthor = _mediaContributorEntityFixture.Create(displayName: "Other Author");
        _mockContext.MediaContributors.AddRange(requestedAuthor, otherAuthor);
        await _mockContext.SaveChangesAsync();
        _mockContext.BookContributors.AddRange(
            _bookContributorEntityFixture.Create(bookId: requestedBook.Id, mediaContributorId: requestedAuthor.Id, role: MediaContributorRole.Author),
            _bookContributorEntityFixture.Create(bookId: bookNotRequested.Id, mediaContributorId: otherAuthor.Id, role: MediaContributorRole.Author));
        await _mockContext.SaveChangesAsync();

        // Act
        Result<IReadOnlyDictionary<Guid, string?>> result = await _sut.GetAuthorsDisplayNamesByBookIdsAsync([requestedBook.Id], CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal("Requested Author", result.Value[requestedBook.Id]);
        Assert.DoesNotContain(bookNotRequested.Id, result.Value.Keys);
    }

    [Fact]
    public async Task GetAuthorsDisplayNamesByBookIdsAsync_WhenBookIdsIsEmpty_ShouldReturnAnEmptyDictionary()
    {
        // Arrange
        BookEntity book = _bookEntityFixture.Create(includeMetadata: false);
        _mockContext.Books.Add(book);
        MediaContributorEntity author = _mediaContributorEntityFixture.Create();
        _mockContext.MediaContributors.Add(author);
        await _mockContext.SaveChangesAsync();
        _mockContext.BookContributors.Add(
            _bookContributorEntityFixture.Create(bookId: book.Id, mediaContributorId: author.Id, role: MediaContributorRole.Author));
        await _mockContext.SaveChangesAsync();

        // Act
        Result<IReadOnlyDictionary<Guid, string?>> result = await _sut.GetAuthorsDisplayNamesByBookIdsAsync([], CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotNull(result.Value);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task GetAuthorsDisplayNamesByBookIdsAsync_WhenMoreThanFiveHundredIdsAreRequested_ShouldReturnTheAuthorOfEveryRequestedBook()
    {
        // Arrange
        const int bookCount = 501;
        List<BookEntity> books = CreateBooks(bookCount);
        _mockContext.Books.AddRange(books);
        MediaContributorEntity author = _mediaContributorEntityFixture.Create(displayName: "Shared Author");
        _mockContext.MediaContributors.Add(author);
        await _mockContext.SaveChangesAsync();
        _mockContext.BookContributors.AddRange([.. books.Select(book =>
            _bookContributorEntityFixture.Create(bookId: book.Id, mediaContributorId: author.Id, role: MediaContributorRole.Author))]);
        await _mockContext.SaveChangesAsync();

        // Act
        // The lookup is chunked in batches of 500, so the 501st book proves the second chunk is queried as well.
        Result<IReadOnlyDictionary<Guid, string?>> result = await _sut.GetAuthorsDisplayNamesByBookIdsAsync(
            [.. books.Select(book => book.Id)], CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(bookCount, result.Value.Count);
        Assert.All(result.Value.Keys, bookId => Assert.Equal("Shared Author", result.Value[bookId]));
    }

    /// <summary>
    /// Creates a list of books with distinct paths and empty metadata collections, so that the chunked lookups can be exercised without extra graph data.
    /// </summary>
    /// <param name="count">The number of books to create.</param>
    /// <returns>The created books.</returns>
    private List<BookEntity> CreateBooks(int count)
    {
        return [.. Enumerable.Range(0, count).Select(_ => _bookEntityFixture.Create(
            path: $"/books/generated-{Guid.NewGuid():N}.epub", includeMetadata: false))];
    }
}
