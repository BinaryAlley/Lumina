#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Queries.GetBooksLite;
using Lumina.Contracts.Fixtures.Core.Requests.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Contracts.Requests.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;

/// <summary>
/// Contains unit tests for the <see cref="GetBooksLiteRequestMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetBooksLiteRequestMappingTests
{
    private readonly GetBooksLiteRequestFixture _getBooksLiteRequestFixture = new();

    [Fact]
    public void ToQuery_WhenMappingGetBooksLiteRequest_ShouldMapCorrectly()
    {
        // Arrange
        GetBooksLiteRequest request = _getBooksLiteRequestFixture.Create();
        string libraryId = Guid.NewGuid().ToString();

        // Act
        GetBooksLiteQuery result = request.ToQuery(libraryId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(libraryId, result.LibraryId);
        Assert.Equal(request.SearchTerm, result.SearchTerm);
        Assert.Equal(request.FilterAlphaKey, result.FilterAlphaKey);
        Assert.Equal(request.ShouldIgnoreThePrefixForAlphaPicker, result.ShouldIgnoreThePrefixForAlphaPicker);
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
        GetBooksLiteRequest request = _getBooksLiteRequestFixture.Create();
        string libraryId = "not-a-library-guid";

        // Act
        GetBooksLiteQuery result = request.ToQuery(libraryId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(libraryId, result.LibraryId);
    }

    [Fact]
    public void ToQuery_WhenLibraryIdIsNull_ShouldMapNullLibraryId()
    {
        // Arrange
        GetBooksLiteRequest request = _getBooksLiteRequestFixture.Create();

        // Act
        GetBooksLiteQuery result = request.ToQuery(null);

        // Assert
        Assert.NotNull(result);
        Assert.Null(result.LibraryId);
    }

    [Fact]
    public void ToQuery_WhenNoPaginationProvided_ShouldNotBuildPaginationData()
    {
        // Arrange
        GetBooksLiteRequest request = _getBooksLiteRequestFixture.Create(includeCurrentPage: false, includePerPage: false);
        string libraryId = Guid.NewGuid().ToString();

        // Act
        GetBooksLiteQuery result = request.ToQuery(libraryId);

        // Assert
        Assert.NotNull(result);
        Assert.Null(result.PaginationData);
        Assert.Equal(libraryId, result.LibraryId);
    }

    [Fact]
    public void ToQuery_WhenOnlyPerPageProvided_ShouldDefaultCurrentPageToOne()
    {
        // Arrange
        GetBooksLiteRequest request = _getBooksLiteRequestFixture.Create(perPage: 25, includeCurrentPage: false);
        string libraryId = Guid.NewGuid().ToString();

        // Act
        GetBooksLiteQuery result = request.ToQuery(libraryId);

        // Assert
        Assert.NotNull(result.PaginationData);
        Assert.Equal(1, result.PaginationData!.CurrentPage);
        Assert.Equal(25, result.PaginationData.PerPage);
    }

    [Fact]
    public void ToQuery_WhenOnlyCurrentPageProvided_ShouldDefaultPerPageToTwoHundred()
    {
        // Arrange
        GetBooksLiteRequest request = _getBooksLiteRequestFixture.Create(currentPage: 3, includePerPage: false);
        string libraryId = Guid.NewGuid().ToString();

        // Act
        GetBooksLiteQuery result = request.ToQuery(libraryId);

        // Assert
        Assert.NotNull(result.PaginationData);
        Assert.Equal(3, result.PaginationData!.CurrentPage);
        Assert.Equal(200, result.PaginationData.PerPage);
    }
}
