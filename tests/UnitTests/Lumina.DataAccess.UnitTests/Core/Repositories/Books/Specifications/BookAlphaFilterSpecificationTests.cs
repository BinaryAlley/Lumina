#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Common.DTO.Filtering;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.DataAccess.Core.Repositories.Books.Specifications;
using Lumina.DataAccess.Core.UoW;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
#endregion

namespace Lumina.DataAccess.UnitTests.Core.Repositories.Books.Specifications;

/// <summary>
/// Contains unit tests for the <see cref="BookAlphaFilterSpecification"/> class.
/// </summary>
/// <remarks>
/// The specification builds an expression that uses the SQLite GLOB function, which can only be evaluated by the Entity Framework SQLite provider,
/// so the tests exercise the expression against a real SQLite database instead of evaluating it in memory.
/// </remarks>
[ExcludeFromCodeCoverage]
public class BookAlphaFilterSpecificationTests : IDisposable
{
    private readonly string _connectionString;
    private readonly SqliteConnection _anchorConnection;
    private readonly LuminaDbContext _context;
    private readonly BookEntityFixture _bookEntityFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="BookAlphaFilterSpecificationTests"/> class.
    /// </summary>
    public BookAlphaFilterSpecificationTests()
    {
        _connectionString = $"Data Source=luminadataccess-alphaspec-tests-{Guid.NewGuid()};Mode=Memory;Cache=Shared";
        _anchorConnection = new SqliteConnection(_connectionString);
        _anchorConnection.Open();
        _context = new LuminaDbContext(new DbContextOptionsBuilder<LuminaDbContext>().UseSqlite(_connectionString).Options);
        _context.Database.EnsureCreated();
    }

    [Fact]
    public async Task ToExpression_WhenFilterAlphaKeyIsNull_ShouldMatchAllBooks()
    {
        // Arrange
        List<BookEntity> books = await SeedBooksAsync(
            _bookEntityFixture.Create(title: "The Fellowship of the Ring", includeMetadata: false),
            _bookEntityFixture.Create(title: "1984", includeMetadata: false),
            _bookEntityFixture.Create(title: "@home", includeMetadata: false));
        BookAlphaFilterSpecification specification = new(null, false);

        // Act
        List<BookEntity> matchingBooks = await _context.Books.Where(specification.ToExpression()).ToListAsync();

        // Assert
        Assert.Equal(3, matchingBooks.Count);
    }

    [Fact]
    public async Task ToExpression_WhenFilterAlphaKeyIsALetter_ShouldMatchTitlesStartingWithThatLetter()
    {
        // Arrange
        List<BookEntity> books = await SeedBooksAsync(
            _bookEntityFixture.Create(title: "The Fellowship of the Ring", includeMetadata: false),
            _bookEntityFixture.Create(title: "1984", includeMetadata: false),
            _bookEntityFixture.Create(title: "Zoo", includeMetadata: false));
        BookAlphaFilterSpecification specification = new("T", false);

        // Act
        List<BookEntity> matchingBooks = await _context.Books.Where(specification.ToExpression()).ToListAsync();

        // Assert
        BookEntity matchedBook = Assert.Single(matchingBooks);
        Assert.Equal(books[0].Id, matchedBook.Id);
    }

    [Fact]
    public async Task ToExpression_WhenFilterAlphaKeyIsALowercaseLetter_ShouldMatchTitlesStartingWithThatLetter()
    {
        // Arrange
        await SeedBooksAsync(
            _bookEntityFixture.Create(title: "Zoo", includeMetadata: false),
            _bookEntityFixture.Create(title: "moby dick", includeMetadata: false));
        BookAlphaFilterSpecification specification = new("Z", false);

        // Act
        List<BookEntity> matchingBooks = await _context.Books.Where(specification.ToExpression()).ToListAsync();

        // Assert
        BookEntity matchedBook = Assert.Single(matchingBooks);
        Assert.Equal("Zoo", matchedBook.Title);
    }

    [Fact]
    public async Task ToExpression_WhenFilterAlphaKeyIsNumber_ShouldMatchTitlesStartingWithADigit()
    {
        // Arrange
        List<BookEntity> books = await SeedBooksAsync(
            _bookEntityFixture.Create(title: "1984", includeMetadata: false),
            _bookEntityFixture.Create(title: "The Fellowship of the Ring", includeMetadata: false));
        BookAlphaFilterSpecification specification = new(LibraryItemAlphaKeys.NUMBER, false);

        // Act
        List<BookEntity> matchingBooks = await _context.Books.Where(specification.ToExpression()).ToListAsync();

        // Assert
        BookEntity matchedBook = Assert.Single(matchingBooks);
        Assert.Equal(books[0].Id, matchedBook.Id);
    }

    [Fact]
    public async Task ToExpression_WhenFilterAlphaKeyIsSymbol_ShouldMatchTitlesStartingWithANonAlphanumericCharacter()
    {
        // Arrange
        List<BookEntity> books = await SeedBooksAsync(
            _bookEntityFixture.Create(title: "@home", includeMetadata: false),
            _bookEntityFixture.Create(title: "1984", includeMetadata: false),
            _bookEntityFixture.Create(title: "Zoo", includeMetadata: false));
        BookAlphaFilterSpecification specification = new(LibraryItemAlphaKeys.SYMBOL, false);

        // Act
        List<BookEntity> matchingBooks = await _context.Books.Where(specification.ToExpression()).ToListAsync();

        // Assert
        BookEntity matchedBook = Assert.Single(matchingBooks);
        Assert.Equal(books[0].Id, matchedBook.Id);
    }

    [Fact]
    public async Task ToExpression_WhenIgnoreThePrefixIsTrue_ShouldStripTheLeadingThePrefix()
    {
        // Arrange
        List<BookEntity> books = await SeedBooksAsync(
            _bookEntityFixture.Create(title: "The Fellowship of the Ring", includeMetadata: false),
            _bookEntityFixture.Create(title: "To Kill a Mockingbird", includeMetadata: false));
        BookAlphaFilterSpecification specification = new("T", true);

        // Act
        List<BookEntity> matchingBooks = await _context.Books.Where(specification.ToExpression()).ToListAsync();

        // Assert
        BookEntity matchedBook = Assert.Single(matchingBooks);
        Assert.Equal(books[1].Id, matchedBook.Id);
    }

    [Fact]
    public async Task ToExpression_WhenIgnoreThePrefixIsFalse_ShouldNotStripTheLeadingThePrefix()
    {
        // Arrange
        await SeedBooksAsync(
            _bookEntityFixture.Create(title: "The Fellowship of the Ring", includeMetadata: false),
            _bookEntityFixture.Create(title: "To Kill a Mockingbird", includeMetadata: false));
        BookAlphaFilterSpecification specification = new("T", false);

        // Act
        List<BookEntity> matchingBooks = await _context.Books.Where(specification.ToExpression()).ToListAsync();

        // Assert
        Assert.Equal(2, matchingBooks.Count);
    }

    [Fact]
    public async Task ToExpression_WhenTitleIsEmpty_ShouldFallBackToOriginalTitle()
    {
        // Arrange
        await SeedBooksAsync(_bookEntityFixture.Create(title: string.Empty, originalTitle: "The Real Title", includeMetadata: false));
        BookAlphaFilterSpecification specification = new("T", false);

        // Act
        List<BookEntity> matchingBooks = await _context.Books.Where(specification.ToExpression()).ToListAsync();

        // Assert
        BookEntity matchedBook = Assert.Single(matchingBooks);
        Assert.Equal("The Real Title", matchedBook.OriginalTitle);
    }

    /// <summary>
    /// Persists the provided books to the database.
    /// </summary>
    /// <param name="books">The books to persist.</param>
    /// <returns>The persisted books.</returns>
    private async Task<List<BookEntity>> SeedBooksAsync(params BookEntity[] books)
    {
        _context.Books.AddRange(books);
        await _context.SaveChangesAsync();
        return [.. books];
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _context.Dispose();
        _anchorConnection.Dispose();
    }
}
