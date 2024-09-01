#region ========================================================================= USING =====================================================================================
using EntityFrameworkCore.Testing.NSubstitute;
using ErrorOr;
using FluentAssertions;
using Lumina.Application.Common.Models.Books;
using Lumina.Application.Common.Models.Common;
using Lumina.DataAccess.Core.Repositories.Books;
using Lumina.DataAccess.Core.UoW;
using Lumina.DataAccess.UnitTests.Core.Repositories.Books.Fixtures;
using Lumina.Domain.Common.Enums;
using Lumina.Domain.Common.Errors;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;
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
    private readonly BookDtoFixture _bookDtoFixture;
    #endregion

    #region ====================================================================== CTOR =====================================================================================
    /// <summary>
    /// Initializes a new instance of the <see cref="BookRepositoryTests"/> class.
    /// </summary>
    public BookRepositoryTests()
    {
        _mockContext = Create.MockedDbContextFor<LuminaDbContext>();
        _sut = new BookRepository(_mockContext);
        _bookDtoFixture = new BookDtoFixture();
    }
    #endregion

    #region ===================================================================== METHODS ===================================================================================
    [Fact]
    public async Task InsertAsync_WhenBookDoesNotExist_ShouldAddBookToContextAndReturnCreated()
    {
        // Arrange
        var bookDto = _bookDtoFixture.CreateBookDto();

        // Act
        var result = await _sut.InsertAsync(bookDto, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(Result.Created);

        // Check if the book was added to the context's ChangeTracker
        var addedBook = _mockContext.ChangeTracker.Entries<BookDto>()
            .FirstOrDefault(e => e.State == EntityState.Added && e.Entity.Id == bookDto.Id);
        addedBook.Should().NotBeNull();
    }

    [Fact]
    public async Task InsertAsync_WhenBookAlreadyExists_ShouldReturnError()
    {
        // Arrange
        var bookDto = _bookDtoFixture.CreateBookDto();

        _mockContext.Books.Add(bookDto);
        await _mockContext.SaveChangesAsync();

        // Act
        var result = await _sut.InsertAsync(bookDto, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.WrittenContent.BookAlreadyExists);
        _mockContext.ChangeTracker.Entries<BookDto>().Should().HaveCount(1); // Only the existing book should be in the context}
    }

    [Fact]
    public async Task InsertAsync_WhenExistingTagsFound_ShouldReplaceTagsWithExistingOnes()
    {
        // Arrange
        var existingTag = new TagDto("Existing");
        _mockContext.Set<TagDto>().Add(existingTag);
        await _mockContext.SaveChangesAsync();

        var bookDto = _bookDtoFixture.CreateBookDto();
        bookDto.Tags = [new("Existing"), new("New")];

        // Act
        var result = await _sut.InsertAsync(bookDto, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(Result.Created);

        var addedBook = _mockContext.ChangeTracker.Entries<BookDto>()
            .FirstOrDefault(e => e.State == EntityState.Added && e.Entity.Id == bookDto.Id);
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
        var existingGenre = new GenreDto("Existing");
        _mockContext.Set<GenreDto>().Add(existingGenre);
        await _mockContext.SaveChangesAsync();

        var bookDto = _bookDtoFixture.CreateBookDto();
        bookDto.Genres = [new("Existing"), new("New")];

        // Act
        var result = await _sut.InsertAsync(bookDto, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(Result.Created);

        var addedBook = _mockContext.ChangeTracker.Entries<BookDto>()
            .FirstOrDefault(e => e.State == EntityState.Added && e.Entity.Id == bookDto.Id);
        addedBook.Should().NotBeNull();
        var addedBookEntity = addedBook!.Entity;
        addedBookEntity.Genres.Should().HaveCount(2);
        addedBookEntity.Genres.Should().Contain(g => g.Name == "Existing" && g == existingGenre);
        addedBookEntity.Genres.Should().Contain(g => g.Name == "New" && g != existingGenre);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllBooks()
    {
        // Arrange
        var books = new List<BookDto>
        {
            _bookDtoFixture.CreateBookDto(),
            _bookDtoFixture.CreateBookDto(),
            _bookDtoFixture.CreateBookDto()
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
    public async Task GetAllAsync_ShouldIncludeRelatedEntities()
    {
        // Arrange
        var book = _bookDtoFixture.CreateBookDto();
        book.Tags = [new TagDto("Tag1"), new TagDto("Tag2")];
        book.Genres = [new GenreDto("Genre1"), new GenreDto("Genre2")];
        book.ISBNs = [new IsbnDto("1234567890", IsbnFormat.Isbn10), new IsbnDto("1234567890123", IsbnFormat.Isbn13)];
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