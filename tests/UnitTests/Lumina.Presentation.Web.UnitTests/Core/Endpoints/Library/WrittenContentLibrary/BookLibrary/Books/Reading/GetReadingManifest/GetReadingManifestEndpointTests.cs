#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.DTO.Reading;
using Lumina.Presentation.Web.Common.Exceptions;
using Lumina.Presentation.Web.Common.Requests.Library.WrittenContentLibrary.BookLibrary.Books.Reading;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.Reading.GetReadingManifest;
using Lumina.Presentation.Web.Fixtures.Common.DTO.Common;
using Lumina.Presentation.Web.Fixtures.Common.DTO.Reading;
using Lumina.Presentation.Web.Fixtures.Common.Requests.Library.WrittenContentLibrary.BookLibrary.Books.Reading;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.UnitTests.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.Reading.GetReadingManifest;

/// <summary>
/// Contains unit tests for the <see cref="GetReadingManifestEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetReadingManifestEndpointTests
{
    private readonly IApiHttpClient _mockApiHttpClient;
    private readonly GetReadingManifestEndpoint _sut;
    private readonly GetBookReadingManifestRequestFixture _requestFixture = new();
    private readonly ReadingManifestDtoFixture _readingManifestDtoFixture = new();
    private readonly ProblemDetailsDtoFixture _problemDetailsDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetReadingManifestEndpointTests"/> class.
    /// </summary>
    public GetReadingManifestEndpointTests()
    {
        _mockApiHttpClient = Substitute.For<IApiHttpClient>();
        _sut = Factory.Create<GetReadingManifestEndpoint>(_mockApiHttpClient);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSuccessful_ShouldReturnSuccessJsonWithManifest()
    {
        // Arrange
        GetBookReadingManifestRequest request = _requestFixture.Create();
        ReadingManifestDto manifest = _readingManifestDtoFixture.Create();
        _mockApiHttpClient.GetAsync<ReadingManifestDto>(ApiRoutes.Books.GET_BOOK_READING_MANIFEST.Replace("{bookId}", request.BookId.ToString()), Arg.Any<CancellationToken>())
            .Returns(manifest);

        // Act
        IResult result = await _sut.ExecuteAsync(request, CancellationToken.None);
        string body = await JsonResultTestHelper.GetResponseBodyAsync(result);

        // Assert
        using JsonDocument jsonDocument = JsonDocument.Parse(body);
        Assert.True(jsonDocument.RootElement.GetProperty("success").GetBoolean());
        Assert.Equal(manifest.Title, jsonDocument.RootElement.GetProperty("data").GetProperty("title").GetString());
    }

    [Fact]
    public async Task ExecuteAsync_WhenApiReturnsNoReaderAvailable_ShouldReturnDistinctErrorCodeJson()
    {
        // Arrange
        GetBookReadingManifestRequest request = _requestFixture.Create();
        _mockApiHttpClient.GetAsync<ReadingManifestDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns<Task<ReadingManifestDto>>(_ => throw new ApiException(_problemDetailsDtoFixture.Create(detail: "NoReaderAvailable"), HttpStatusCode.NotFound));

        // Act
        IResult result = await _sut.ExecuteAsync(request, CancellationToken.None);
        string body = await JsonResultTestHelper.GetResponseBodyAsync(result);

        // Assert
        using JsonDocument jsonDocument = JsonDocument.Parse(body);
        Assert.False(jsonDocument.RootElement.GetProperty("success").GetBoolean());
        Assert.Equal("NoReaderAvailable", jsonDocument.RootElement.GetProperty("errorCode").GetString());
    }

    [Fact]
    public async Task ExecuteAsync_WhenApiReturnsReaderDisabled_ShouldReturnDistinctErrorCodeJson()
    {
        // Arrange
        GetBookReadingManifestRequest request = _requestFixture.Create();
        _mockApiHttpClient.GetAsync<ReadingManifestDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns<Task<ReadingManifestDto>>(_ => throw new ApiException(_problemDetailsDtoFixture.Create(detail: "ReaderDisabled"), HttpStatusCode.NotFound));

        // Act
        IResult result = await _sut.ExecuteAsync(request, CancellationToken.None);
        string body = await JsonResultTestHelper.GetResponseBodyAsync(result);

        // Assert
        using JsonDocument jsonDocument = JsonDocument.Parse(body);
        Assert.False(jsonDocument.RootElement.GetProperty("success").GetBoolean());
        Assert.Equal("ReaderDisabled", jsonDocument.RootElement.GetProperty("errorCode").GetString());
    }

    [Fact]
    public async Task ExecuteAsync_WhenApiReturnsUnrelatedError_ShouldRethrow()
    {
        // Arrange
        GetBookReadingManifestRequest request = _requestFixture.Create();
        _mockApiHttpClient.GetAsync<ReadingManifestDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns<Task<ReadingManifestDto>>(_ => throw new ApiException(_problemDetailsDtoFixture.Create(detail: "BookNotFound"), HttpStatusCode.NotFound));

        // Act
        Func<Task<IResult>> act = () => _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<ApiException>(act);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldRequestReadingManifestFromApi()
    {
        // Arrange
        GetBookReadingManifestRequest request = _requestFixture.Create();
        ReadingManifestDto manifest = _readingManifestDtoFixture.Create();
        _mockApiHttpClient.GetAsync<ReadingManifestDto>(ApiRoutes.Books.GET_BOOK_READING_MANIFEST.Replace("{bookId}", request.BookId.ToString()), Arg.Any<CancellationToken>())
            .Returns(manifest);

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).GetAsync<ReadingManifestDto>(ApiRoutes.Books.GET_BOOK_READING_MANIFEST.Replace("{bookId}", request.BookId.ToString()), Arg.Any<CancellationToken>());
    }
}
