#region ========================================================================= USING =====================================================================================
using DomainErrors = Lumina.Domain.Common.Errors.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate;
using Lumina.Domain.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate.Entities;
using Lumina.Domain.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate;
using Lumina.Domain.Fixtures.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate.ValueObjects;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.UnitTests.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate.Entities;

/// <summary>
/// Contains unit tests for the <see cref="BookSeries"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class BookSeriesTests
{
    private readonly BookFixture _bookFixture = new();
    private readonly BookSeriesIdFixture _bookSeriesIdFixture = new();
    private readonly WrittenContentMetadataFixture _writtenContentMetadataFixture = new();
    private readonly BookSeriesFixture _bookSeriesFixture = new();

    [Fact]
    public void Create_WhenCalledWithValidData_ShouldCreateSeries()
    {
        // Act
        Result<BookSeries> result = BookSeries.Create(_writtenContentMetadataFixture.Create(), isComplete: false, []);

        // Assert
        Assert.False(result.IsFailure);
        Assert.False(result.Value.IsComplete);
        Assert.Empty(result.Value.Books);
        Assert.NotEqual(default, result.Value.Id.Value);
    }

    [Fact]
    public void Create_WhenCalledWithPreExistingId_ShouldCreateSeriesWithThatId()
    {
        // Arrange
        Guid id = Guid.NewGuid();

        // Act
        Result<BookSeries> result = BookSeries.Create(_bookSeriesIdFixture.Create(id), _writtenContentMetadataFixture.Create(), isComplete: true, []);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(id, result.Value.Id.Value);
        Assert.True(result.Value.IsComplete);
    }

    [Fact]
    public void AddBook_WhenBookIsNotInSeries_ShouldAddBookAndReturnCreated()
    {
        // Arrange
        BookSeries series = _bookSeriesFixture.Create();
        Book book = _bookFixture.Create();

        // Act
        Result<Created> result = series.AddBook(book);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Created, result.Value);
        Assert.Single(series.Books);
        Assert.Contains(book, series.Books);
    }

    [Fact]
    public void AddBook_WhenBookIsAlreadyInSeries_ShouldReturnError()
    {
        // Arrange
        BookSeries series = _bookSeriesFixture.Create();
        Book book = _bookFixture.Create();
        series.AddBook(book);

        // Act
        Result<Created> result = series.AddBook(book);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.WrittenContent.TheBookIsAlreadyInTheSeries, result.FirstError);
        Assert.Single(series.Books);
    }

    [Fact]
    public void RemoveBook_WhenBookIsInSeries_ShouldRemoveBookAndReturnDeleted()
    {
        // Arrange
        BookSeries series = _bookSeriesFixture.Create();
        Book book = _bookFixture.Create();
        series.AddBook(book);

        // Act
        Result<Deleted> result = series.RemoveBook(book);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Deleted, result.Value);
        Assert.Empty(series.Books);
    }

    [Fact]
    public void RemoveBook_WhenBookIsNotInSeries_ShouldReturnError()
    {
        // Arrange
        BookSeries series = _bookSeriesFixture.Create();
        Book book = _bookFixture.Create();

        // Act
        Result<Deleted> result = series.RemoveBook(book);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.WrittenContent.TheBookIsNotInTheSeries, result.FirstError);
    }
}
