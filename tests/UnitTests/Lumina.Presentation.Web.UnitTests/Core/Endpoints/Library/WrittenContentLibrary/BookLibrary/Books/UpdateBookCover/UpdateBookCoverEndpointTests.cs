#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.UpdateBookCover;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.UnitTests.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.UpdateBookCover;

/// <summary>
/// Contains unit tests for the <see cref="UpdateBookCoverEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateBookCoverEndpointTests
{
    private readonly IApiHttpClient _mockApiHttpClient;
    private readonly UpdateBookCoverEndpoint _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateBookCoverEndpointTests"/> class.
    /// </summary>
    public UpdateBookCoverEndpointTests()
    {
        _mockApiHttpClient = Substitute.For<IApiHttpClient>();
        _sut = Factory.Create<UpdateBookCoverEndpoint>(_mockApiHttpClient);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalledWithFile_ShouldUploadCoverToApi()
    {
        // Arrange
        Guid bookId = Guid.NewGuid();
        _mockApiHttpClient.PutMultipartAsync<string>(Arg.Any<string>(), Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("/media/books/cover.jpg");
        ConfigureRequest(bookId, [1, 2, 3], "cover.jpg");

        // Act
        await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).PutMultipartAsync<string>(
            Arg.Is<string>(endpoint => endpoint == ApiRoutes.Books.UPDATE_BOOK_COVER.Replace("{id}", bookId.ToString())),
            Arg.Any<Stream>(),
            Arg.Is<string>(fileName => fileName == "cover.jpg"),
            Arg.Is<string>(fieldName => fieldName == "cover"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenRouteIdIsNotParseable_ShouldUploadCoverWithEmptyBookId()
    {
        // Arrange
        _mockApiHttpClient.PutMultipartAsync<string>(Arg.Any<string>(), Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("/media/books/cover.jpg");
        ConfigureRequest("not-a-guid", [1, 2, 3], "cover.jpg");

        // Act
        await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).PutMultipartAsync<string>(
            Arg.Is<string>(endpoint => endpoint == ApiRoutes.Books.UPDATE_BOOK_COVER.Replace("{id}", Guid.Empty.ToString())),
            Arg.Any<Stream>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenApiReturnsCoverPath_ShouldReturnSuccessJsonWithPath()
    {
        // Arrange
        Guid bookId = Guid.NewGuid();
        string expectedPath = "/media/books/cover.jpg";
        _mockApiHttpClient.PutMultipartAsync<string>(Arg.Any<string>(), Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(expectedPath);
        ConfigureRequest(bookId, [1, 2, 3], "cover.jpg");

        // Act
        IResult result = await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);
        string body = await JsonResultTestHelper.GetResponseBodyAsync(result);

        // Assert
        using JsonDocument jsonDocument = JsonDocument.Parse(body);
        Assert.True(jsonDocument.RootElement.GetProperty("success").GetBoolean());
        Assert.Equal(expectedPath, jsonDocument.RootElement.GetProperty("data").GetString());
    }

    [Fact]
    public async Task ExecuteAsync_WhenFormHasNoFile_ShouldReturnBadRequestProblem()
    {
        // Arrange
        Guid bookId = Guid.NewGuid();
        ConfigureRequestWithoutFiles(bookId);

        // Act
        IResult result = await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);

        // Assert
        ProblemHttpResult problemDetails = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, problemDetails.StatusCode);
        Assert.Equal("The uploaded cover image is missing.", problemDetails.ProblemDetails.Detail);
        await _mockApiHttpClient.DidNotReceive().PutMultipartAsync<string>(Arg.Any<string>(), Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    private void ConfigureRequest(Guid bookId, byte[] content, string fileName)
    {
        ConfigureRequest(bookId.ToString(), content, fileName);
    }

    private void ConfigureRequest(string bookId, byte[] content, string fileName)
    {
        _sut.HttpContext.Request.RouteValues["id"] = bookId;
        MemoryStream coverStream = new(content);
        IFormFile formFile = new FormFile(coverStream, 0, content.Length, "cover", fileName);
        FormFileCollection files = [formFile];
        IFormCollection form = new FormCollection([], files);
        _sut.HttpContext.Request.ContentType = "multipart/form-data; boundary=----test";
        _sut.HttpContext.Features.Set<IFormFeature>(new FormFeature(form));
    }

    private void ConfigureRequestWithoutFiles(Guid bookId)
    {
        _sut.HttpContext.Request.RouteValues["id"] = bookId.ToString();
        IFormCollection form = new FormCollection([]);
        _sut.HttpContext.Request.ContentType = "multipart/form-data; boundary=----test";
        _sut.HttpContext.Features.Set<IFormFeature>(new FormFeature(form));
    }
}
