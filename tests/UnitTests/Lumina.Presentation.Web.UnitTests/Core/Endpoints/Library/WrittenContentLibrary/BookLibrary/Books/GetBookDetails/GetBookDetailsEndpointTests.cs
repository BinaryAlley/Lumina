#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.DTO.WrittenContentLibrary.BookLibrary;
using Lumina.Presentation.Web.Common.Requests.Library.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.GetBookDetails;
using Lumina.Presentation.Web.Fixtures.Common.DTO.WrittenContentLibrary.BookLibrary;
using Lumina.Presentation.Web.Fixtures.Common.Requests.Library.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.UnitTests.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.GetBookDetails;

/// <summary>
/// Contains unit tests for the <see cref="GetBookDetailsEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetBookDetailsEndpointTests
{
    private readonly IApiHttpClient _mockApiHttpClient;
    private readonly GetBookDetailsEndpoint _sut;
    private readonly GetBookRequestFixture _getBookRequestFixture = new();
    private readonly BookDetailsDtoFixture _bookDetailsDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetBookDetailsEndpointTests"/> class.
    /// </summary>
    public GetBookDetailsEndpointTests()
    {
        _mockApiHttpClient = Substitute.For<IApiHttpClient>();
        _sut = Factory.Create<GetBookDetailsEndpoint>(_mockApiHttpClient);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldRequestBookDetailsFromApi()
    {
        // Arrange
        GetBookRequest request = _getBookRequestFixture.Create();
        BookDetailsDto expectedBook = _bookDetailsDtoFixture.Create(id: Guid.Parse(request.Id));
        _mockApiHttpClient.GetAsync<BookDetailsDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(expectedBook);

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).GetAsync<BookDetailsDto>(
            Arg.Is<string>(endpoint => endpoint == ApiRoutes.Books.GET_BOOK.Replace("{id}", request.Id)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenApiReturnsBookDetails_ShouldReturnSuccessJsonWithBook()
    {
        // Arrange
        GetBookRequest request = _getBookRequestFixture.Create();
        BookDetailsDto expectedBook = _bookDetailsDtoFixture.Create(id: Guid.Parse(request.Id));
        _mockApiHttpClient.GetAsync<BookDetailsDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(expectedBook);

        // Act
        IResult result = await _sut.ExecuteAsync(request, CancellationToken.None);
        string body = await JsonResultTestHelper.GetResponseBodyAsync(result);

        // Assert
        using JsonDocument jsonDocument = JsonDocument.Parse(body);
        Assert.True(jsonDocument.RootElement.GetProperty("success").GetBoolean());
        Assert.Equal(expectedBook.Id, jsonDocument.RootElement.GetProperty("data").GetProperty("id").GetGuid());
    }
}
