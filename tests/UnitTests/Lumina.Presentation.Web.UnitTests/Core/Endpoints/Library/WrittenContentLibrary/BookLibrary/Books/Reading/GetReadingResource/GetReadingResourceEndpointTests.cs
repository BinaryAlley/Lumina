#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.DTO.FileSystemManagement;
using Lumina.Presentation.Web.Common.Requests.Library.WrittenContentLibrary.BookLibrary.Books.Reading;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.Reading.GetReadingResource;
using Lumina.Presentation.Web.Fixtures.Common.DTO.FileSystemManagement;
using Lumina.Presentation.Web.Fixtures.Common.Requests.Library.WrittenContentLibrary.BookLibrary.Books.Reading;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.UnitTests.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.Reading.GetReadingResource;

/// <summary>
/// Contains unit tests for the <see cref="GetReadingResourceEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetReadingResourceEndpointTests
{
    private readonly IApiHttpClient _mockApiHttpClient;
    private readonly GetReadingResourceEndpoint _sut;
    private readonly GetBookReadingResourceRequestFixture _requestFixture = new();
    private readonly BlobDataDtoFixture _blobDataDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetReadingResourceEndpointTests"/> class.
    /// </summary>
    public GetReadingResourceEndpointTests()
    {
        _mockApiHttpClient = Substitute.For<IApiHttpClient>();
        _sut = Factory.Create<GetReadingResourceEndpoint>(_mockApiHttpClient);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSuccessful_ShouldReturnBytesResultWithResourceData()
    {
        // Arrange
        GetBookReadingResourceRequest request = _requestFixture.Create();
        BlobDataDto blob = _blobDataDtoFixture.Create(contentType: "image/png");
        _mockApiHttpClient.GetBlobAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(blob);

        // Act
        IResult result = await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        FileContentHttpResult bytesResult = Assert.IsType<FileContentHttpResult>(result);
        Assert.Equal(blob.Data, bytesResult.FileContents);
        Assert.Equal(blob.ContentType, bytesResult.ContentType);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldRequestReadingResourceFromApi()
    {
        // Arrange
        GetBookReadingResourceRequest request = _requestFixture.Create();
        BlobDataDto blob = _blobDataDtoFixture.Create();
        _mockApiHttpClient.GetBlobAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(blob);
        string expectedEndpoint = ApiRoutes.Books.GET_BOOK_READING_RESOURCE
            .Replace("{bookId}", request.BookId.ToString())
            .Replace("{resourceKey}", Uri.EscapeDataString(request.ResourceKey));

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).GetBlobAsync(expectedEndpoint, Arg.Any<CancellationToken>());
    }
}
