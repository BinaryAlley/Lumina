#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.DTO.WrittenContentLibrary.BookLibrary;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.UpdateBookCover;
using Lumina.Presentation.Web.Fixtures.Common.DTO.WrittenContentLibrary.BookLibrary;
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
    private readonly UpdateBookCoverDtoFixture _updateBookCoverDtoFixture = new();
    private static readonly Guid s_libraryId = Guid.NewGuid();

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
        _mockApiHttpClient.PutMultipartAsync<UpdateBookCoverDto>(Arg.Any<string>(), Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(_updateBookCoverDtoFixture.Create(coverPath: "/media/books/cover.jpg"));
        ConfigureRequest(bookId, [1, 2, 3], "cover.jpg");

        // Act
        await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).PutMultipartAsync<UpdateBookCoverDto>(
            Arg.Is<string>(endpoint => endpoint == ApiRoutes.Books.UPDATE_BOOK_COVER.Replace("{libraryId}", s_libraryId.ToString()).Replace("{bookId}", bookId.ToString())),
            Arg.Any<Stream>(),
            Arg.Is<string>(fileName => fileName == "cover.jpg"),
            Arg.Is<string>(fieldName => fieldName == "cover"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenRouteIdIsNotParseable_ShouldUploadCoverWithEmptyBookId()
    {
        // Arrange
        _mockApiHttpClient.PutMultipartAsync<UpdateBookCoverDto>(Arg.Any<string>(), Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(_updateBookCoverDtoFixture.Create(coverPath: "/media/books/cover.jpg"));
        ConfigureRequest("not-a-guid", [1, 2, 3], "cover.jpg");

        // Act
        await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).PutMultipartAsync<UpdateBookCoverDto>(
            Arg.Is<string>(endpoint => endpoint == ApiRoutes.Books.UPDATE_BOOK_COVER.Replace("{libraryId}", s_libraryId.ToString()).Replace("{bookId}", Guid.Empty.ToString())),
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
        _mockApiHttpClient.PutMultipartAsync<UpdateBookCoverDto>(Arg.Any<string>(), Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(_updateBookCoverDtoFixture.Create(coverPath: expectedPath));
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
    public async Task ExecuteAsync_WhenFormHasNoFile_ShouldForwardRequestWithoutFileToApi()
    {
        // Arrange
        Guid bookId = Guid.NewGuid();
        string expectedPath = "/media/books/cover.jpg";
        _mockApiHttpClient.PutMultipartAsync<UpdateBookCoverDto>(Arg.Any<string>(), Arg.Any<Stream?>(), Arg.Any<string?>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(_updateBookCoverDtoFixture.Create(coverPath: expectedPath));
        ConfigureRequestWithoutFiles(bookId);

        // Act
        IResult result = await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);
        string body = await JsonResultTestHelper.GetResponseBodyAsync(result);

        // Assert
        await _mockApiHttpClient.Received(1).PutMultipartAsync<UpdateBookCoverDto>(
            Arg.Is<string>(endpoint => endpoint == ApiRoutes.Books.UPDATE_BOOK_COVER.Replace("{libraryId}", s_libraryId.ToString()).Replace("{bookId}", bookId.ToString())),
            Arg.Is<Stream?>(stream => stream == null),
            Arg.Is<string?>(fileName => fileName == null),
            Arg.Is<string>(fieldName => fieldName == "cover"),
            Arg.Any<CancellationToken>());
        using JsonDocument jsonDocument = JsonDocument.Parse(body);
        Assert.True(jsonDocument.RootElement.GetProperty("success").GetBoolean());
        Assert.Equal(expectedPath, jsonDocument.RootElement.GetProperty("data").GetString());
    }

    [Fact]
    public async Task ExecuteAsync_WhenRequestIsNotMultipart_ShouldForwardRequestWithoutFileToApi()
    {
        // Arrange
        Guid bookId = Guid.NewGuid();
        string expectedPath = "/media/books/cover.jpg";
        _mockApiHttpClient.PutMultipartAsync<UpdateBookCoverDto>(Arg.Any<string>(), Arg.Any<Stream?>(), Arg.Any<string?>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(_updateBookCoverDtoFixture.Create(coverPath: expectedPath));
        _sut.HttpContext.Request.RouteValues["bookId"] = bookId.ToString();
        _sut.HttpContext.Request.QueryString = new QueryString($"?libraryId={s_libraryId}");
        _sut.HttpContext.Request.ContentType = "application/json";

        // Act
        IResult result = await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);
        string body = await JsonResultTestHelper.GetResponseBodyAsync(result);

        // Assert
        await _mockApiHttpClient.Received(1).PutMultipartAsync<UpdateBookCoverDto>(
            Arg.Is<string>(endpoint => endpoint == ApiRoutes.Books.UPDATE_BOOK_COVER.Replace("{libraryId}", s_libraryId.ToString()).Replace("{bookId}", bookId.ToString())),
            Arg.Is<Stream?>(stream => stream == null),
            Arg.Is<string?>(fileName => fileName == null),
            Arg.Is<string>(fieldName => fieldName == "cover"),
            Arg.Any<CancellationToken>());
        using JsonDocument jsonDocument = JsonDocument.Parse(body);
        Assert.True(jsonDocument.RootElement.GetProperty("success").GetBoolean());
        Assert.Equal(expectedPath, jsonDocument.RootElement.GetProperty("data").GetString());
    }

    private void ConfigureRequest(Guid bookId, byte[] content, string fileName)
    {
        ConfigureRequest(bookId.ToString(), content, fileName);
    }

    private void ConfigureRequest(string bookId, byte[] content, string fileName)
    {
        _sut.HttpContext.Request.RouteValues["bookId"] = bookId;
        _sut.HttpContext.Request.QueryString = new QueryString($"?libraryId={s_libraryId}");
        MemoryStream coverStream = new(content);
        IFormFile formFile = new FormFile(coverStream, 0, content.Length, "cover", fileName);
        FormFileCollection files = [formFile];
        IFormCollection form = new FormCollection([], files);
        _sut.HttpContext.Request.ContentType = "multipart/form-data; boundary=----test";
        _sut.HttpContext.Features.Set<IFormFeature>(new FormFeature(form));
    }

    private void ConfigureRequestWithoutFiles(Guid bookId)
    {
        _sut.HttpContext.Request.RouteValues["bookId"] = bookId.ToString();
        _sut.HttpContext.Request.QueryString = new QueryString($"?libraryId={s_libraryId}");
        IFormCollection form = new FormCollection([]);
        _sut.HttpContext.Request.ContentType = "multipart/form-data; boundary=----test";
        _sut.HttpContext.Features.Set<IFormFeature>(new FormFeature(form));
    }
}
