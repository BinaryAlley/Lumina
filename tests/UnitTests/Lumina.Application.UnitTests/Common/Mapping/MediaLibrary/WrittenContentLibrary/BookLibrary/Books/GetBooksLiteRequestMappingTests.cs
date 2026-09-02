#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Queries.GetBooksLite;
using Lumina.Contracts.Fixtures.Core.Requests.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Contracts.Requests.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
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

        // Act
        GetBooksLiteQuery result = request.ToQuery();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.LibraryId, result.Filter.LibraryId);
        Assert.Equal(request.SearchTerm, result.Filter.SearchTerm);
        Assert.Equal(request.SortBy, result.SortBy);
        Assert.Equal(request.SortOrder, result.SortOrder);
        Assert.NotNull(result.PaginationData);
        Assert.Equal(request.CurrentPage, result.PaginationData!.CurrentPage);
        Assert.Equal(request.PerPage, result.PaginationData.PerPage);
    }

    [Fact]
    public void ToQuery_WhenNoPaginationProvided_ShouldNotBuildPaginationData()
    {
        // Arrange
        GetBooksLiteRequest request = _getBooksLiteRequestFixture.Create();
        request = request with { CurrentPage = null, PerPage = null };

        // Act
        GetBooksLiteQuery result = request.ToQuery();

        // Assert
        Assert.NotNull(result);
        Assert.Null(result.PaginationData);
        Assert.Equal(request.LibraryId, result.Filter.LibraryId);
    }

    [Fact]
    public void ToQuery_WhenOnlyPerPageProvided_ShouldDefaultCurrentPageToOne()
    {
        // Arrange
        GetBooksLiteRequest request = _getBooksLiteRequestFixture.Create();
        request = request with { CurrentPage = null, PerPage = 25 };

        // Act
        GetBooksLiteQuery result = request.ToQuery();

        // Assert
        Assert.NotNull(result.PaginationData);
        Assert.Equal(1, result.PaginationData!.CurrentPage);
        Assert.Equal(25, result.PaginationData.PerPage);
    }

    [Fact]
    public void ToQuery_WhenOnlyCurrentPageProvided_ShouldDefaultPerPageToTwoHundred()
    {
        // Arrange
        GetBooksLiteRequest request = _getBooksLiteRequestFixture.Create();
        request = request with { CurrentPage = 3, PerPage = null };

        // Act
        GetBooksLiteQuery result = request.ToQuery();

        // Assert
        Assert.NotNull(result.PaginationData);
        Assert.Equal(3, result.PaginationData!.CurrentPage);
        Assert.Equal(200, result.PaginationData.PerPage);
    }
}
