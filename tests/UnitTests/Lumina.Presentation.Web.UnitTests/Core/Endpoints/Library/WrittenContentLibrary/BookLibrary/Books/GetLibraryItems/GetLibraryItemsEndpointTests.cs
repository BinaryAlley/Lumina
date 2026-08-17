#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.DTO.Libraries;
using Lumina.Presentation.Web.Common.Enums.Common;
using Lumina.Presentation.Web.Common.Requests.Libraries;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.GetLibraryItems;
using Lumina.Presentation.Web.Fixtures.Common.Requests.Libraries;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.UnitTests.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.GetLibraryItems;

/// <summary>
/// Contains unit tests for the <see cref="GetLibraryItemsEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetLibraryItemsEndpointTests
{
    private readonly IApiHttpClient _mockApiHttpClient;
    private readonly GetLibraryItemsEndpoint _sut;
    private readonly GetBooksLiteRequestFixture _getBooksLiteRequestFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetLibraryItemsEndpointTests"/> class.
    /// </summary>
    public GetLibraryItemsEndpointTests()
    {
        _mockApiHttpClient = Substitute.For<IApiHttpClient>();
        _sut = Factory.Create<GetLibraryItemsEndpoint>(_mockApiHttpClient);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldRequestLibraryItemsWithQueryParameters()
    {
        // Arrange
        GetBooksLiteRequest request = _getBooksLiteRequestFixture.Create(
            currentPage: 2,
            perPage: 48,
            searchTerm: "the hobbit",
            filterAlphaKey: "H",
            ignoreThePrefixForAlphaPicker: true,
            sortBy: "title",
            sortOrder: SortOrder.Ascending);
        _mockApiHttpClient.GetAsync<PaginatedBookLiteDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new PaginatedBookLiteDto());

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).GetAsync<PaginatedBookLiteDto>(
            Arg.Is<string>(endpoint =>
                endpoint.StartsWith(ApiRoutes.Books.GET_BOOKS_LITE, StringComparison.OrdinalIgnoreCase) &&
                endpoint.Contains($"libraryId={request.LibraryId}") &&
                endpoint.Contains("currentPage=2") &&
                endpoint.Contains("perPage=48") &&
                endpoint.Contains("searchTerm=the%20hobbit") &&
                endpoint.Contains("filterAlphaKey=H") &&
                endpoint.Contains("ignoreThePrefixForAlphaPicker=True") &&
                endpoint.Contains("sortBy=title") &&
                endpoint.Contains("sortOrder=Ascending")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalledWithOnlyLibraryId_ShouldRequestWithoutOptionalParameters()
    {
        // Arrange
        GetBooksLiteRequest request = _getBooksLiteRequestFixture.Create();
        _mockApiHttpClient.GetAsync<PaginatedBookLiteDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new PaginatedBookLiteDto());

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).GetAsync<PaginatedBookLiteDto>(
            Arg.Is<string>(endpoint =>
                endpoint.StartsWith(ApiRoutes.Books.GET_BOOKS_LITE, StringComparison.OrdinalIgnoreCase) &&
                endpoint.EndsWith($"libraryId={request.LibraryId}&ignoreThePrefixForAlphaPicker=False", StringComparison.OrdinalIgnoreCase) &&
                !endpoint.Contains("currentPage=") &&
                !endpoint.Contains("searchTerm=") &&
                !endpoint.Contains("sortBy=")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenApiReturnsPaginatedItems_ShouldReturnSuccessJsonWithItems()
    {
        // Arrange
        GetBooksLiteRequest request = _getBooksLiteRequestFixture.Create();
        PaginatedBookLiteDto expectedResponse = new()
        {
            CurrentPage = 1,
            PerPage = 48,
            Count = 1,
            NumberOfPages = 1,
            Data = [new BookLiteDto { Id = Guid.NewGuid(), Title = "Test Book" }]
        };
        _mockApiHttpClient.GetAsync<PaginatedBookLiteDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(expectedResponse);

        // Act
        IResult result = await _sut.ExecuteAsync(request, CancellationToken.None);
        string body = await JsonResultTestHelper.GetResponseBodyAsync(result);

        // Assert
        using JsonDocument jsonDocument = JsonDocument.Parse(body);
        Assert.True(jsonDocument.RootElement.GetProperty("success").GetBoolean());
        JsonElement data = jsonDocument.RootElement.GetProperty("data");
        Assert.Equal(1, data.GetProperty("currentPage").GetInt32());
        Assert.Equal(1, data.GetProperty("data").GetArrayLength());
    }
}
