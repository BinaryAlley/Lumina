#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.DTO.WrittenContentLibrary.BookLibrary;
using Lumina.Presentation.Web.Common.Requests.Library.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.SaveBook;
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

namespace Lumina.Presentation.Web.UnitTests.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.SaveBook;

/// <summary>
/// Contains unit tests for the <see cref="SaveBookEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class SaveBookEndpointTests
{
    private readonly IApiHttpClient _mockApiHttpClient;
    private readonly SaveBookEndpoint _sut;
    private readonly UpdateBookRequestFixture _updateBookRequestFixture = new();
    private readonly BookDetailsDtoFixture _bookDetailsDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="SaveBookEndpointTests"/> class.
    /// </summary>
    public SaveBookEndpointTests()
    {
        _mockApiHttpClient = Substitute.For<IApiHttpClient>();
        _sut = Factory.Create<SaveBookEndpoint>(_mockApiHttpClient);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalledWithMatchingRouteId_ShouldForwardUpdateBookRequestToApi()
    {
        // Arrange
        string routeId = Guid.NewGuid().ToString();
        UpdateBookRequest request = _updateBookRequestFixture.Create();
        BookDetailsDto expectedBook = _bookDetailsDtoFixture.Create();
        _mockApiHttpClient.PutAsync<BookDetailsDto, UpdateBookRequest>(Arg.Any<string>(), Arg.Any<UpdateBookRequest>(), Arg.Any<CancellationToken>())
            .Returns(expectedBook);
        ConfigureRequest(routeId);

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).PutAsync<BookDetailsDto, UpdateBookRequest>(
            Arg.Is<string>(endpoint => endpoint == ApiRoutes.Books.UPDATE_BOOK.Replace("{id}", routeId)),
            Arg.Is<UpdateBookRequest>(forwardedRequest => forwardedRequest == request && forwardedRequest.Id == routeId),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenRouteIdDiffersFromBodyId_ShouldForwardTheRouteIdToApi()
    {
        // Arrange
        string routeId = Guid.NewGuid().ToString();
        string bodyId = Guid.NewGuid().ToString();
        UpdateBookRequest request = _updateBookRequestFixture.Create();
        request.Id = bodyId;
        BookDetailsDto expectedBook = _bookDetailsDtoFixture.Create();
        _mockApiHttpClient.PutAsync<BookDetailsDto, UpdateBookRequest>(Arg.Any<string>(), Arg.Any<UpdateBookRequest>(), Arg.Any<CancellationToken>())
            .Returns(expectedBook);
        ConfigureRequest(routeId);

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).PutAsync<BookDetailsDto, UpdateBookRequest>(
            Arg.Is<string>(endpoint => endpoint == ApiRoutes.Books.UPDATE_BOOK.Replace("{id}", routeId)),
            Arg.Is<UpdateBookRequest>(forwardedRequest => forwardedRequest == request && forwardedRequest.Id == routeId && forwardedRequest.Id != bodyId),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenRouteIdIsNotParseable_ShouldForwardGuidEmptyToApi()
    {
        // Arrange
        const string ROUTE_ID = "not-a-guid";
        string expectedId = Guid.Empty.ToString();
        UpdateBookRequest request = _updateBookRequestFixture.Create();
        BookDetailsDto expectedBook = _bookDetailsDtoFixture.Create();
        _mockApiHttpClient.PutAsync<BookDetailsDto, UpdateBookRequest>(Arg.Any<string>(), Arg.Any<UpdateBookRequest>(), Arg.Any<CancellationToken>())
            .Returns(expectedBook);
        ConfigureRequest(ROUTE_ID);

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).PutAsync<BookDetailsDto, UpdateBookRequest>(
            Arg.Is<string>(endpoint => endpoint == ApiRoutes.Books.UPDATE_BOOK.Replace("{id}", expectedId)),
            Arg.Is<UpdateBookRequest>(forwardedRequest => forwardedRequest == request && forwardedRequest.Id == expectedId),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenRouteIdIsMissing_ShouldForwardGuidEmptyToApi()
    {
        // Arrange
        string expectedId = Guid.Empty.ToString();
        UpdateBookRequest request = _updateBookRequestFixture.Create();
        BookDetailsDto expectedBook = _bookDetailsDtoFixture.Create();
        _mockApiHttpClient.PutAsync<BookDetailsDto, UpdateBookRequest>(Arg.Any<string>(), Arg.Any<UpdateBookRequest>(), Arg.Any<CancellationToken>())
            .Returns(expectedBook);
        ConfigureRequestWithoutId();

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).PutAsync<BookDetailsDto, UpdateBookRequest>(
            Arg.Is<string>(endpoint => endpoint == ApiRoutes.Books.UPDATE_BOOK.Replace("{id}", expectedId)),
            Arg.Is<UpdateBookRequest>(forwardedRequest => forwardedRequest == request && forwardedRequest.Id == expectedId),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenApiReturnsUpdatedBook_ShouldReturnSuccessJsonWithBook()
    {
        // Arrange
        string routeId = Guid.NewGuid().ToString();
        UpdateBookRequest request = _updateBookRequestFixture.Create();
        BookDetailsDto expectedBook = _bookDetailsDtoFixture.Create();
        _mockApiHttpClient.PutAsync<BookDetailsDto, UpdateBookRequest>(Arg.Any<string>(), Arg.Any<UpdateBookRequest>(), Arg.Any<CancellationToken>())
            .Returns(expectedBook);
        ConfigureRequest(routeId);

        // Act
        IResult result = await _sut.ExecuteAsync(request, CancellationToken.None);
        string body = await JsonResultTestHelper.GetResponseBodyAsync(result);

        // Assert
        using JsonDocument jsonDocument = JsonDocument.Parse(body);
        Assert.True(jsonDocument.RootElement.GetProperty("success").GetBoolean());
        Assert.Equal(expectedBook.Id, jsonDocument.RootElement.GetProperty("data").GetProperty("id").GetGuid());
    }

    /// <summary>
    /// Sets the <c>id</c> route value the endpoint reads the book Id from.
    /// </summary>
    /// <param name="routeId">The raw route value of the book Id.</param>
    private void ConfigureRequest(string routeId)
    {
        _sut.HttpContext.Request.RouteValues["id"] = routeId;
    }

    /// <summary>
    /// Leaves the <c>id</c> route value unset.
    /// </summary>
    private void ConfigureRequestWithoutId()
    {
        _sut.HttpContext.Request.RouteValues.Remove("id");
    }
}
