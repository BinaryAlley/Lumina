#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Common.DTO.Pagination;
using Lumina.Application.Common.Mapping.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Application.Fixtures.Common.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Fixtures.Common.DTO.Pagination;
using Lumina.Contracts.Responses.Common;
using Lumina.Contracts.Responses.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;

/// <summary>
/// Contains unit tests for the <see cref="BookLiteRowMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class BookLiteRowMappingTests
{
    private readonly BookLiteRowFixture _bookLiteRowFixture = new();
    private readonly PaginatedResultDtoFixture<BookLiteRow> _paginatedResultDtoFixture = new();

    [Fact]
    public void ToResponse_WhenMappingBookLiteRow_ShouldMapAllPropertiesCorrectly()
    {
        // Arrange
        BookLiteRow readModel = _bookLiteRowFixture.Create();

        // Act
        BookLiteResponse result = readModel.ToResponse();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(readModel.Id, result.Id);
        Assert.Equal(readModel.Title, result.Title);
        Assert.Equal(readModel.ReleaseYear, result.ReleaseYear);
        Assert.Equal(readModel.CoverPath, result.CoverPath);
    }

    [Fact]
    public void ToResponses_WhenMappingMultipleBookLiteRows_ShouldMapAllCorrectly()
    {
        // Arrange
        List<BookLiteRow> readModels = _bookLiteRowFixture.CreateMany(2);

        // Act
        IReadOnlyList<BookLiteResponse> results = readModels.ToResponses();

        // Assert
        Assert.NotNull(results);
        Assert.Equal(readModels.Count, results.Count);
        Assert.Equal(readModels.Select(readModel => readModel.Id), results.Select(response => response.Id));
    }

    [Fact]
    public void ToResponses_WhenMappingPaginatedBookLiteRows_ShouldMapAllPropertiesCorrectly()
    {
        // Arrange
        List<BookLiteRow> readModels = _bookLiteRowFixture.CreateMany(2);
        PaginatedResultDto<BookLiteRow> paginatedReadModels = _paginatedResultDtoFixture.Create(
            data: readModels,
            currentPage: 1,
            perPage: 10,
            count: 2,
            numberOfPages: 1);

        // Act
        PaginatedResponse<BookLiteResponse> result = paginatedReadModels.ToResponses();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(paginatedReadModels.Data.Count, result.Data.Count);
        Assert.Equal(paginatedReadModels.CurrentPage, result.CurrentPage);
        Assert.Equal(paginatedReadModels.PerPage, result.PerPage);
        Assert.Equal(paginatedReadModels.Count, result.Count);
        Assert.Equal(paginatedReadModels.NumberOfPages, result.NumberOfPages);
    }
}
