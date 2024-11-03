#region ========================================================================= USING =====================================================================================
using EntityFrameworkCore.Testing.NSubstitute;
using ErrorOr;
using FluentAssertions;
using Lumina.Contracts.Entities.Common;
using Lumina.Contracts.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Domain.Common.Enums.BookLibrary;
using Lumina.DataAccess.Core.Repositories.Books;
using Lumina.DataAccess.Core.UoW;
using Lumina.DataAccess.UnitTests.Core.Repositories.Books.Fixtures;
using Lumina.Domain.Common.Errors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
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
    private readonly BookModelFixture _bookModelFixture;

    /// <summary>
    /// Initializes a new instance of the <see cref="BookRepositoryTests"/> class.
    /// </summary>
    public BookRepositoryTests()
    {
        _mockContext = Create.MockedDbContextFor<LuminaDbContext>();
        _sut = new BookRepository(_mockContext);
        _bookModelFixture = new BookModelFixture();
    }

    [Fact]
    public async Task InsertAsync_WhenBookDoesNotExist_ShouldAddBookToContextAndReturnCreated()
    {
        // Arrange
        BookEntity bookModel = _bookModelFixture.CreateBookModel();

        // Act
        ErrorOr<Created> result = await _sut.InsertAsync(bookModel, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(Result.Created);

        // Check if the book was added to the context's ChangeTracker
        EntityEntry<BookEntity>? addedBook = _mockContext.ChangeTracker.Entries<BookEntity>()
            .FirstOrDefault(e => e.State == EntityState.Added && e.Entity.Id == bookModel.Id);
        addedBook.Should().NotBeNull();
    }

    [Fact]
    public async Task InsertAsync_WhenBookAlreadyExists_ShouldReturnError()
    {
        // Arrange
        BookEntity bookModel = _bookModelFixture.CreateBookModel();

        _mockContext.Books.Add(bookModel);
        await _mockContext.SaveChangesAsync();

        // Act
        ErrorOr<Created> result = await _sut.InsertAsync(bookModel, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.WrittenContent.BookAlreadyExists);
        _mockContext.ChangeTracker.Entries<BookEntity>().Should().HaveCount(1); // Only the existing book should be in the context}
    }

    [Fact]
    public async Task InsertAsync_WhenExistingTagsFound_ShouldReplaceTagsWithExistingOnes()
    {
        // Arrange
        TagEntity existingTag = new("Existing");
        _mockContext.Set<TagEntity>().Add(existingTag);
        await _mockContext.SaveChangesAsync();

        BookEntity bookModel = _bookModelFixture.CreateBookModel();
        bookModel.Tags = [new("Existing"), new("New")];

        // Act
        ErrorOr<Created> result = await _sut.InsertAsync(bookModel, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(Result.Created);

        EntityEntry<BookEntity>? addedBook = _mockContext.ChangeTracker.Entries<BookEntity>()
            .FirstOrDefault(e => e.State == EntityState.Added && e.Entity.Id == bookModel.Id);
        addedBook.Should().NotBeNull();
        BookEntity addedBookEntity = addedBook!.Entity;
        addedBookEntity.Tags.Should().HaveCount(2);
        addedBookEntity.Tags.Should().Contain(t => t.Name == "Existing" && t == existingTag);
        addedBookEntity.Tags.Should().Contain(t => t.Name == "New" && t != existingTag);
    }

    [Fact]
    public async Task InsertAsync_WhenExistingGenresFound_ShouldReplaceGenresWithExistingOnes()
    {
        // Arrange
        GenreEntity existingGenre = new("Existing");
        _mockContext.Set<GenreEntity>().Add(existingGenre);
        await _mockContext.SaveChangesAsync();

        BookEntity bookModel = _bookModelFixture.CreateBookModel();
        bookModel.Genres = [new("Existing"), new("New")];

        // Act
        ErrorOr<Created> result = await _sut.InsertAsync(bookModel, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(Result.Created);

        EntityEntry<BookEntity>? addedBook = _mockContext.ChangeTracker.Entries<BookEntity>()
            .FirstOrDefault(e => e.State == EntityState.Added && e.Entity.Id == bookModel.Id);
        addedBook.Should().NotBeNull();
        BookEntity addedBookEntity = addedBook!.Entity;
        addedBookEntity.Genres.Should().HaveCount(2);
        addedBookEntity.Genres.Should().Contain(g => g.Name == "Existing" && g == existingGenre);
        addedBookEntity.Genres.Should().Contain(g => g.Name == "New" && g != existingGenre);
    }

    [Fact]
    public async Task GetAllAsync_WhenCalled_ShouldReturnAllBooks()
    {
        // Arrange
        List<BookEntity> books =
        [
            _bookModelFixture.CreateBookModel(),
            _bookModelFixture.CreateBookModel(),
            _bookModelFixture.CreateBookModel()
        ];
        _mockContext.Books.AddRange(books);
        await _mockContext.SaveChangesAsync();

        // Act
        ErrorOr<IEnumerable<BookEntity>> result = await _sut.GetAllAsync(CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(3);
        result.Value.Should().BeEquivalentTo(books);
    }

    [Fact]
    public async Task GetAllAsync_WhenNoBooksExist_ShouldReturnEmptyList()
    {
        // Act
        ErrorOr<IEnumerable<BookEntity>> result = await _sut.GetAllAsync(CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().NotBeNull();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllAsync_WhenCalled_ShouldIncludeRelatedEntities()
    {
        // Arrange
        BookEntity book = _bookModelFixture.CreateBookModel();
        book.Tags = [new TagEntity("Tag1"), new TagEntity("Tag2")];
        book.Genres = [new GenreEntity("Genre1"), new GenreEntity("Genre2")];
        book.ISBNs = [new IsbnEntity("1234567890", IsbnFormat.Isbn10), new IsbnEntity("1234567890123", IsbnFormat.Isbn13)];
        _mockContext.Books.Add(book);
        await _mockContext.SaveChangesAsync();

        // Act
        ErrorOr<IEnumerable<BookEntity>> result = await _sut.GetAllAsync(CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(1);
        BookEntity retrievedBook = result.Value.First();
        retrievedBook.Tags.Should().HaveCount(2);
        retrievedBook.Genres.Should().HaveCount(2);
        retrievedBook.ISBNs.Should().HaveCount(2);
    }
}
