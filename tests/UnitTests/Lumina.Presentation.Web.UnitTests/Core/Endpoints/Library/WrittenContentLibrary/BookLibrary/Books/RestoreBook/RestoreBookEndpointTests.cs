#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.RestoreBook;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.UnitTests.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.RestoreBook;

/// <summary>
/// Contains unit tests for the <see cref="RestoreBookEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class RestoreBookEndpointTests
{
    private readonly IApiHttpClient _mockApiHttpClient;
    private readonly RestoreBookEndpoint _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="RestoreBookEndpointTests"/> class.
    /// </summary>
    public RestoreBookEndpointTests()
    {
        _mockApiHttpClient = Substitute.For<IApiHttpClient>();
        _sut = Factory.Create<RestoreBookEndpoint>(_mockApiHttpClient);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldRestoreBookViaApiAndReturnSuccessPayload()
    {
        // Arrange
        Guid bookId = Guid.NewGuid();
        Guid libraryId = Guid.NewGuid();
        ConfigureRequest(bookId.ToString(), libraryId);
        _mockApiHttpClient.PostAsync<Web.Common.Requests.Common.EmptyRequest, Web.Common.Requests.Common.EmptyRequest>(
            Arg.Any<string>(), Arg.Any<Web.Common.Requests.Common.EmptyRequest>(), Arg.Any<CancellationToken>())
            .Returns(new Web.Common.Requests.Common.EmptyRequest());

        // Act
        IResult result = await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);
        string body = await JsonResultTestHelper.GetResponseBodyAsync(result);

        // Assert
        await _mockApiHttpClient.Received(1).PostAsync<Web.Common.Requests.Common.EmptyRequest, Web.Common.Requests.Common.EmptyRequest>(
            BuildExpectedEndpoint(libraryId, bookId),
            Arg.Any<Web.Common.Requests.Common.EmptyRequest>(),
            Arg.Any<CancellationToken>());
        using JsonDocument jsonDocument = JsonDocument.Parse(body);
        Assert.True(jsonDocument.RootElement.GetProperty("success").GetBoolean());
        Assert.True(jsonDocument.RootElement.GetProperty("data").GetProperty("isRestored").GetBoolean());
    }

    [Fact]
    public async Task ExecuteAsync_WhenBookIdIsNotParseable_ShouldForwardGuidEmptyBookIdToApi()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        ConfigureRequest("not-a-guid", libraryId);
        _mockApiHttpClient.PostAsync<Web.Common.Requests.Common.EmptyRequest, Web.Common.Requests.Common.EmptyRequest>(
            Arg.Any<string>(), Arg.Any<Web.Common.Requests.Common.EmptyRequest>(), Arg.Any<CancellationToken>())
            .Returns(new Web.Common.Requests.Common.EmptyRequest());

        // Act
        await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).PostAsync<Web.Common.Requests.Common.EmptyRequest, Web.Common.Requests.Common.EmptyRequest>(
            BuildExpectedEndpoint(libraryId, Guid.Empty),
            Arg.Any<Web.Common.Requests.Common.EmptyRequest>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenLibraryQueryIdIsNotParseable_ShouldForwardGuidEmptyLibraryIdToApi()
    {
        // Arrange
        Guid bookId = Guid.NewGuid();
        ConfigureRequest(bookId.ToString(), "not-a-guid");
        _mockApiHttpClient.PostAsync<Web.Common.Requests.Common.EmptyRequest, Web.Common.Requests.Common.EmptyRequest>(
            Arg.Any<string>(), Arg.Any<Web.Common.Requests.Common.EmptyRequest>(), Arg.Any<CancellationToken>())
            .Returns(new Web.Common.Requests.Common.EmptyRequest());

        // Act
        await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).PostAsync<Web.Common.Requests.Common.EmptyRequest, Web.Common.Requests.Common.EmptyRequest>(
            BuildExpectedEndpoint(Guid.Empty, bookId),
            Arg.Any<Web.Common.Requests.Common.EmptyRequest>(),
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Sets the route and query values the endpoint reads the book identity from.
    /// </summary>
    /// <param name="bookId">The raw route value of the book Id.</param>
    /// <param name="libraryId">The raw query value of the library Id.</param>
    private void ConfigureRequest(string bookId, Guid libraryId)
    {
        ConfigureRequest(bookId, libraryId.ToString());
    }

    /// <summary>
    /// Sets the route and query values the endpoint reads the book identity from.
    /// </summary>
    /// <param name="bookId">The raw route value of the book Id.</param>
    /// <param name="libraryId">The raw query value of the library Id.</param>
    private void ConfigureRequest(string bookId, string libraryId)
    {
        _sut.HttpContext.Request.RouteValues["bookId"] = bookId;
        _sut.HttpContext.Request.QueryString = new QueryString($"?libraryId={libraryId}");
    }

    /// <summary>
    /// Builds the upstream endpoint the book restoration is expected to be sent to.
    /// </summary>
    /// <param name="libraryId">The normalized Id of the media library.</param>
    /// <param name="bookId">The normalized Id of the book.</param>
    /// <returns>The expected upstream endpoint.</returns>
    private static string BuildExpectedEndpoint(Guid libraryId, Guid bookId)
    {
        return ApiRoutes.Books.RESTORE_BOOK
            .Replace("{libraryId}", libraryId.ToString())
            .Replace("{bookId}", bookId.ToString());
    }
}
