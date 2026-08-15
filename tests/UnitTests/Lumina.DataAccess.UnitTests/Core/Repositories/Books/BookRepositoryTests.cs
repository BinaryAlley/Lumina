#region ========================================================================= USING =====================================================================================
using EntityFrameworkCore.Testing.NSubstitute;
using Lumina.Application.Common.DataAccess.Entities.Common;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.DataAccess.Core.Repositories.Books;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.BookLibrary;
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
        TagEntity existingTag = new("Existing");
        _mockContext.Set<TagEntity>().Add(existingTag);
        await _mockContext.SaveChangesAsync();

        BookEntity bookModel = _bookEntityFixture.Create();
        bookModel.Tags = [new("Existing"), new("New")];

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
        GenreEntity existingGenre = new("Existing");
        _mockContext.Set<GenreEntity>().Add(existingGenre);
        await _mockContext.SaveChangesAsync();

        BookEntity bookModel = _bookEntityFixture.Create();
        bookModel.Genres = [new("Existing"), new("New")];

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
        book.Tags = [new TagEntity("Tag1"), new TagEntity("Tag2")];
        book.Genres = [new GenreEntity("Genre1"), new GenreEntity("Genre2")];
        book.ISBNs = [new IsbnEntity("1234567890", IsbnFormat.Isbn10), new IsbnEntity("1234567890123", IsbnFormat.Isbn13)];
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
}
