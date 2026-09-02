#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Queries.GetBooks;
using Lumina.Contracts.Fixtures.Core.Requests.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Contracts.Requests.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
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

        // Act
        GetBooksQuery result = request.ToQuery();

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
        GetBooksRequest request = _getBooksRequestFixture.Create();
        request = request with { CurrentPage = null, PerPage = null };

        // Act
        GetBooksQuery result = request.ToQuery();

        // Assert
        Assert.NotNull(result);
        Assert.Null(result.PaginationData);
        Assert.Equal(request.LibraryId, result.Filter.LibraryId);
    }

    [Fact]
    public void ToQuery_WhenOnlyPerPageProvided_ShouldDefaultCurrentPageToOne()
    {
        // Arrange
        GetBooksRequest request = _getBooksRequestFixture.Create();
        request = request with { CurrentPage = null, PerPage = 25 };

        // Act
        GetBooksQuery result = request.ToQuery();

        // Assert
        Assert.NotNull(result.PaginationData);
        Assert.Equal(1, result.PaginationData!.CurrentPage);
        Assert.Equal(25, result.PaginationData.PerPage);
    }

    [Fact]
    public void ToQuery_WhenOnlyCurrentPageProvided_ShouldDefaultPerPageToTwoHundred()
    {
        // Arrange
        GetBooksRequest request = _getBooksRequestFixture.Create();
        request = request with { CurrentPage = 3, PerPage = null };

        // Act
        GetBooksQuery result = request.ToQuery();

        // Assert
        Assert.NotNull(result.PaginationData);
        Assert.Equal(3, result.PaginationData!.CurrentPage);
        Assert.Equal(200, result.PaginationData.PerPage);
    }
}
