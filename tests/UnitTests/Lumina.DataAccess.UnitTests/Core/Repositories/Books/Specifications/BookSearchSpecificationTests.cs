#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.DataAccess.Core.Repositories.Books.Specifications;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.DataAccess.UnitTests.Core.Repositories.Books.Specifications;

/// <summary>
/// Contains unit tests for the <see cref="BookSearchSpecification"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class BookSearchSpecificationTests
{
    private readonly BookEntityFixture _bookEntityFixture = new();

    [Fact]
    public void ToExpression_WhenTitleContainsSearchTerm_ShouldMatchBook()
    {
        // Arrange
        BookEntity book = _bookEntityFixture.Create();
        book.Title = "The Fellowship of the Ring";
        book.OriginalTitle = null;
        BookSearchSpecification specification = new("Fellowship");

        // Act
        bool result = specification.IsSatisfiedBy(book);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ToExpression_WhenOriginalTitleContainsSearchTerm_ShouldMatchBook()
    {
        // Arrange
        BookEntity book = _bookEntityFixture.Create();
        book.Title = "A Random Title";
        book.OriginalTitle = "Lord of the Rings: The Fellowship";
        BookSearchSpecification specification = new("Fellowship");

        // Act
        bool result = specification.IsSatisfiedBy(book);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ToExpression_WhenOriginalTitleIsNull_ShouldMatchOnlyOnTitle()
    {
        // Arrange
        BookEntity book = _bookEntityFixture.Create();
        book.Title = "The Fellowship of the Ring";
        book.OriginalTitle = null;
        BookSearchSpecification specification = new("Ring");

        // Act
        bool result = specification.IsSatisfiedBy(book);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ToExpression_WhenNeitherTitleNorOriginalTitleContainsSearchTerm_ShouldNotMatchBook()
    {
        // Arrange
        BookEntity book = _bookEntityFixture.Create();
        book.Title = "The Fellowship of the Ring";
        book.OriginalTitle = "The Two Towers";
        BookSearchSpecification specification = new("Hobbit");

        // Act
        bool result = specification.IsSatisfiedBy(book);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ToExpression_WhenSearchTermIsNotPresent_ShouldFilterOutNonMatchingBooks()
    {
        // Arrange
        List<BookEntity> books = _bookEntityFixture.CreateMany(3);
        books[0].Title = "The Fellowship of the Ring";
        books[1].Title = "The Two Towers";
        books[2].Title = "The Return of the King";
        BookSearchSpecification specification = new("Fellowship");

        // Act
        IEnumerable<BookEntity> matchingBooks = books.AsQueryable().Where(specification.ToExpression());

        // Assert
        BookEntity matchedBook = Assert.Single(matchingBooks);
        Assert.Equal(books[0].Title, matchedBook.Title);
    }

    [Fact]
    public void ToExpression_WhenTitleMatchesInDifferentCase_ShouldNotMatchCaseInsensitively()
    {
        // Arrange
        BookEntity book = _bookEntityFixture.Create();
        book.Title = "The Fellowship of the Ring";
        book.OriginalTitle = null;
        BookSearchSpecification specification = new("fellowship");

        // Act
        bool result = specification.IsSatisfiedBy(book);

        // Assert
        Assert.False(result);
    }
}
