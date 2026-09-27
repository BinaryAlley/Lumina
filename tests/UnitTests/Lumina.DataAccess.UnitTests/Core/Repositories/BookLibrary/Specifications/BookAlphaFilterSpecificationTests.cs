#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Common.DTO.Filtering;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.DataAccess.Core.Repositories.BookLibrary.Specifications;
using Lumina.DataAccess.Core.UoW;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.DataAccess.UnitTests.Core.Repositories.BookLibrary.Specifications;

/// <summary>
/// Contains unit tests for the <see cref="BookAlphaFilterSpecification"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class BookAlphaFilterSpecificationTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly LuminaDbContext _context;
    private readonly BookEntityFixture _bookEntityFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="BookAlphaFilterSpecificationTests"/> class.
    /// </summary>
    public BookAlphaFilterSpecificationTests()
    {
        // the specification translates to the SQLite glob() function, so it can only be exercised against a real SQLite database
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();
        _context = new LuminaDbContext(new DbContextOptionsBuilder<LuminaDbContext>().UseSqlite(_connection).Options);
        _context.Database.EnsureCreated();
    }

    [Fact]
    public void ToExpression_WhenAlphaKeyIsNull_ShouldMatchEveryBook()
    {
        // Arrange
        List<BookEntity> books = SeedBooks(("Alpha", null), ("Beta", null), ("1984", null));
        BookAlphaFilterSpecification specification = new(null, false);

        // Act
        List<BookEntity> result = [.. _context.Books.Where(specification.ToExpression())];

        // Assert
        Assert.Equal(books.Count, result.Count);
    }

    [Fact]
    public void ToExpression_WhenAlphaKeyIsALowercaseLetter_ShouldMatchOnlyBooksWhoseTitleStartsWithIt()
    {
        // Arrange
        SeedBooks(("Fellowship", null), ("Hobbit", null), ("The Two Towers", null));
        BookAlphaFilterSpecification specification = new("f", false);

        // Act
        List<string> result = [.. _context.Books.Where(specification.ToExpression()).Select(book => book.Title)];

        // Assert
        Assert.Equal(["Fellowship"], result);
    }

    [Fact]
    public void ToExpression_WhenAlphaKeyIsAnUppercaseLetter_ShouldMatchBooksWhoseTitleStartsWithItsLowercase()
    {
        // Arrange
        SeedBooks(("Fellowship", null), ("Hobbit", null));
        BookAlphaFilterSpecification specification = new("F", false);

        // Act
        List<string> result = [.. _context.Books.Where(specification.ToExpression()).Select(book => book.Title)];

        // Assert
        Assert.Equal(["Fellowship"], result);
    }

    [Fact]
    public void ToExpression_WhenNoTitleStartsWithTheAlphaKey_ShouldNotMatchAnyBook()
    {
        // Arrange
        SeedBooks(("Fellowship", null), ("Hobbit", null));
        BookAlphaFilterSpecification specification = new("z", false);

        // Act
        List<BookEntity> result = [.. _context.Books.Where(specification.ToExpression())];

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void ToExpression_WhenAlphaKeyIsTheNumberKey_ShouldMatchOnlyBooksWhoseTitleStartsWithADigit()
    {
        // Arrange
        SeedBooks(("1984", null), ("Alpha", null), ("Beta", null));
        BookAlphaFilterSpecification specification = new(LibraryItemAlphaKeys.NUMBER, false);

        // Act
        List<string> result = [.. _context.Books.Where(specification.ToExpression()).Select(book => book.Title)];

        // Assert
        Assert.Equal(["1984"], result);
    }

    [Fact]
    public void ToExpression_WhenAlphaKeyIsTheSymbolKey_ShouldMatchOnlyBooksWhoseTitleStartsWithASymbol()
    {
        // Arrange
        SeedBooks(("!Important", null), ("1984", null), ("Alpha", null));
        BookAlphaFilterSpecification specification = new(LibraryItemAlphaKeys.SYMBOL, false);

        // Act
        List<string> result = [.. _context.Books.Where(specification.ToExpression()).Select(book => book.Title)];

        // Assert
        Assert.Equal(["!Important"], result);
    }

    [Fact]
    public void ToExpression_WhenTitleIsEmpty_ShouldMatchOnTheOriginalTitle()
    {
        // Arrange
        BookEntity book = _bookEntityFixture.Create(title: string.Empty, originalTitle: "Rendezvous", includeMetadata: false);
        _context.Books.Add(book);
        _context.SaveChanges();
        BookAlphaFilterSpecification specification = new("r", false);

        // Act
        List<string> result = [.. _context.Books.Where(specification.ToExpression()).Select(entity => entity.OriginalTitle!)];

        // Assert
        Assert.Equal(["Rendezvous"], result);
    }

    [Fact]
    public void ToExpression_WhenIgnoringThePrefixAndTitleStartsWithThe_ShouldMatchOnTheStrippedTitle()
    {
        // Arrange
        SeedBooks(("The Art of War", null), ("Beneath the Surface", null));
        BookAlphaFilterSpecification specification = new("a", true);

        // Act
        List<string> result = [.. _context.Books.Where(specification.ToExpression()).Select(book => book.Title)];

        // Assert
        Assert.Equal(["The Art of War"], result);
    }

    [Fact]
    public void ToExpression_WhenNotIgnoringThePrefixAndTitleStartsWithThe_ShouldNotMatchOnTheStrippedTitle()
    {
        // Arrange
        SeedBooks(("The Art of War", null), ("Beneath the Surface", null));
        BookAlphaFilterSpecification specification = new("a", false);

        // Act
        List<BookEntity> result = [.. _context.Books.Where(specification.ToExpression())];

        // Assert
        Assert.Empty(result);
    }

    /// <summary>
    /// Seeds books with the provided titles.
    /// </summary>
    /// <param name="books">The title and original title pairs of the books to seed.</param>
    /// <returns>The seeded <see cref="BookEntity"/> instances.</returns>
    private List<BookEntity> SeedBooks(params (string Title, string? OriginalTitle)[] books)
    {
        List<BookEntity> entities = [];
        foreach ((string title, string? originalTitle) in books)
            entities.Add(_bookEntityFixture.Create(title: title, originalTitle: originalTitle, includeMetadata: false));
        _context.Books.AddRange(entities);
        _context.SaveChanges();
        return entities;
    }

    /// <summary>
    /// Disposes the database context and its underlying connection.
    /// </summary>
    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }
}
