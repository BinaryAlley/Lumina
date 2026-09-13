#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Queries.GetBooks;
using Lumina.Contracts.Fixtures.Core.Requests.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Contracts.Requests.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;

/// <summary>
/// Contains unit tests for the <see cref="GetBooksRequestMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetBooksRequestMappingTests
{
    private readonly GetBooksRequestFixture _getBooksRequestFixture = new();

    [Fact]
    public void ToQuery_WhenMappingGetBooksRequest_ShouldMapCorrectly()
    {
        // Arrange
        GetBooksRequest request = _getBooksRequestFixture.Create();
        string libraryId = Guid.NewGuid().ToString();

        // Act
        GetBooksQuery result = request.ToQuery(libraryId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(libraryId, result.LibraryId);
        Assert.Equal(request.SearchTerm, result.SearchTerm);
        Assert.Equal(request.SortBy, result.SortBy);
        Assert.Equal(request.SortOrder, result.SortOrder);
        Assert.NotNull(result.PaginationData);
        Assert.Equal(request.CurrentPage, result.PaginationData!.CurrentPage);
        Assert.Equal(request.PerPage, result.PaginationData.PerPage);
    }

    [Fact]
    public void ToQuery_WhenLibraryIdIsNotParseable_ShouldMapItAsIs()
    {
        // Arrange
        GetBooksRequest request = _getBooksRequestFixture.Create();
        string libraryId = "not-a-library-guid";

        // Act
        GetBooksQuery result = request.ToQuery(libraryId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(libraryId, result.LibraryId);
    }

    [Fact]
    public void ToQuery_WhenLibraryIdIsNull_ShouldMapNullLibraryId()
    {
        // Arrange
        GetBooksRequest request = _getBooksRequestFixture.Create();

        // Act
        GetBooksQuery result = request.ToQuery(null);

        // Assert
        Assert.NotNull(result);
        Assert.Null(result.LibraryId);
    }

    [Fact]
    public void ToQuery_WhenNoPaginationProvided_ShouldNotBuildPaginationData()
    {
        // Arrange
        GetBooksRequest request = _getBooksRequestFixture.Create(includeCurrentPage: false, includePerPage: false);
        string libraryId = Guid.NewGuid().ToString();

        // Act
        GetBooksQuery result = request.ToQuery(libraryId);

        // Assert
        Assert.NotNull(result);
        Assert.Null(result.PaginationData);
        Assert.Equal(libraryId, result.LibraryId);
    }

    [Fact]
    public void ToQuery_WhenOnlyPerPageProvided_ShouldDefaultCurrentPageToOne()
    {
        // Arrange
        GetBooksRequest request = _getBooksRequestFixture.Create(perPage: 25, includeCurrentPage: false);
        string libraryId = Guid.NewGuid().ToString();

        // Act
        GetBooksQuery result = request.ToQuery(libraryId);

        // Assert
        Assert.NotNull(result.PaginationData);
        Assert.Equal(1, result.PaginationData!.CurrentPage);
        Assert.Equal(25, result.PaginationData.PerPage);
    }

    [Fact]
    public void ToQuery_WhenOnlyCurrentPageProvided_ShouldDefaultPerPageToTwoHundred()
    {
        // Arrange
        GetBooksRequest request = _getBooksRequestFixture.Create(currentPage: 3, includePerPage: false);
        string libraryId = Guid.NewGuid().ToString();

        // Act
        GetBooksQuery result = request.ToQuery(libraryId);

        // Assert
        Assert.NotNull(result.PaginationData);
        Assert.Equal(3, result.PaginationData!.CurrentPage);
        Assert.Equal(200, result.PaginationData.PerPage);
    }
}
