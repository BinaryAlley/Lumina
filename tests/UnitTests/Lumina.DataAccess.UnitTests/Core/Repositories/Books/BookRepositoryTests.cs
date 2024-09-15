#region ========================================================================= USING =====================================================================================
using EntityFrameworkCore.Testing.NSubstitute;
using ErrorOr;
using FluentAssertions;
using Lumina.Contracts.Enums.BookLibrary;
using Lumina.Contracts.Models.Common;
using Lumina.Contracts.Models.WrittenContentLibrary.BookLibrary;
using Lumina.DataAccess.Core.Repositories.Books;
using Lumina.DataAccess.Core.UoW;
using Lumina.DataAccess.UnitTests.Core.Repositories.Books.Fixtures;
using Lumina.Domain.Common.Errors;
using Microsoft.EntityFrameworkCore;
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
    #region ================================================================== FIELD MEMBERS ================================================================================
    private readonly LuminaDbContext _mockContext;
    private readonly BookRepository _sut;
    private readonly BookModelFixture _bookModelFixture;
    #endregion

    #region ====================================================================== CTOR =====================================================================================
    /// <summary>
    /// Initializes a new instance of the <see cref="BookRepositoryTests"/> class.
    /// </summary>
    public BookRepositoryTests()
    {
        _mockContext = Create.MockedDbContextFor<LuminaDbContext>();
        _sut = new BookRepository(_mockContext);
        _bookModelFixture = new BookModelFixture();
    }
    #endregion

    #region ===================================================================== METHODS ===================================================================================
    [Fact]
    public async Task InsertAsync_WhenBookDoesNotExist_ShouldAddBookToContextAndReturnCreated()
    {
        // Arrange
        var bookModel = _bookModelFixture.CreateBookModel();

        // Act
        var result = await _sut.InsertAsync(bookModel, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(Result.Created);

        // Check if the book was added to the context's ChangeTracker
        var addedBook = _mockContext.ChangeTracker.Entries<BookModel>()
            .FirstOrDefault(e => e.State == EntityState.Added && e.Entity.Id == bookModel.Id);
        addedBook.Should().NotBeNull();
    }

    [Fact]
    public async Task InsertAsync_WhenBookAlreadyExists_ShouldReturnError()
    {
        // Arrange
        var bookModel = _bookModelFixture.CreateBookModel();

        _mockContext.Books.Add(bookModel);
        await _mockContext.SaveChangesAsync();

        // Act
        var result = await _sut.InsertAsync(bookModel, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.WrittenContent.BookAlreadyExists);
        _mockContext.ChangeTracker.Entries<BookModel>().Should().HaveCount(1); // Only the existing book should be in the context}
    }

    [Fact]
    public async Task InsertAsync_WhenExistingTagsFound_ShouldReplaceTagsWithExistingOnes()
    {
        // Arrange
        var existingTag = new TagModel("Existing");
        _mockContext.Set<TagModel>().Add(existingTag);
        await _mockContext.SaveChangesAsync();

        var bookModel = _bookModelFixture.CreateBookModel();
        bookModel.Tags = [new("Existing"), new("New")];

        // Act
        var result = await _sut.InsertAsync(bookModel, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(Result.Created);

        var addedBook = _mockContext.ChangeTracker.Entries<BookModel>()
            .FirstOrDefault(e => e.State == EntityState.Added && e.Entity.Id == bookModel.Id);
        addedBook.Should().NotBeNull();
        var addedBookEntity = addedBook!.Entity;
        addedBookEntity.Tags.Should().HaveCount(2);
        addedBookEntity.Tags.Should().Contain(t => t.Name == "Existing" && t == existingTag);
        addedBookEntity.Tags.Should().Contain(t => t.Name == "New" && t != existingTag);
    }

    [Fact]
    public async Task InsertAsync_WhenExistingGenresFound_ShouldReplaceGenresWithExistingOnes()
    {
        // Arrange
        var existingGenre = new GenreModel("Existing");
        _mockContext.Set<GenreModel>().Add(existingGenre);
        await _mockContext.SaveChangesAsync();

        var bookModel = _bookModelFixture.CreateBookModel();
        bookModel.Genres = [new("Existing"), new("New")];

        // Act
        var result = await _sut.InsertAsync(bookModel, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(Result.Created);

        var addedBook = _mockContext.ChangeTracker.Entries<BookModel>()
            .FirstOrDefault(e => e.State == EntityState.Added && e.Entity.Id == bookModel.Id);
        addedBook.Should().NotBeNull();
        var addedBookEntity = addedBook!.Entity;
        addedBookEntity.Genres.Should().HaveCount(2);
        addedBookEntity.Genres.Should().Contain(g => g.Name == "Existing" && g == existingGenre);
        addedBookEntity.Genres.Should().Contain(g => g.Name == "New" && g != existingGenre);
    }

    [Fact]
    public async Task GetAllAsync_WhenCalled_ShouldReturnAllBooks()
    {
        // Arrange
        var books = new List<BookModel>
        {
            _bookModelFixture.CreateBookModel(),
            _bookModelFixture.CreateBookModel(),
            _bookModelFixture.CreateBookModel()
        };
        _mockContext.Books.AddRange(books);
        await _mockContext.SaveChangesAsync();

        // Act
        var result = await _sut.GetAllAsync(CancellationToken.None);

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
        var result = await _sut.GetAllAsync(CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().NotBeNull();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllAsync_WhenCalled_ShouldIncludeRelatedEntities()
    {
        // Arrange
        var book = _bookModelFixture.CreateBookModel();
        book.Tags = [new TagModel("Tag1"), new TagModel("Tag2")];
        book.Genres = [new GenreModel("Genre1"), new GenreModel("Genre2")];
        book.ISBNs = [new IsbnModel("1234567890", IsbnFormat.Isbn10), new IsbnModel("1234567890123", IsbnFormat.Isbn13)];
        _mockContext.Books.Add(book);
        await _mockContext.SaveChangesAsync();

        // Act
        var result = await _sut.GetAllAsync(CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(1);
        var retrievedBook = result.Value.First();
        retrievedBook.Tags.Should().HaveCount(2);
        retrievedBook.Genres.Should().HaveCount(2);
        retrievedBook.ISBNs.Should().HaveCount(2);
    }
    #endregion
}