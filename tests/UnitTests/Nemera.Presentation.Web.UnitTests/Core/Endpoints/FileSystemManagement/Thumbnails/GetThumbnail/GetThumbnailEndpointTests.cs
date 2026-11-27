#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.Requests.FileSystemManagement.Thumbnails;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.FileSystemManagement.Thumbnails.GetThumbnail;
using Lumina.Presentation.Web.Fixtures.Common.DTO.FileSystemManagement;
using Lumina.Presentation.Web.Fixtures.Common.Requests.FileSystemManagement.Thumbnails;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.UnitTests.Core.Endpoints.FileSystemManagement.Thumbnails.GetThumbnail;

/// <summary>
/// Contains unit tests for the <see cref="GetThumbnailEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetThumbnailEndpointTests
{
    private readonly IApiHttpClient _mockApiHttpClient;
    private readonly GetThumbnailEndpoint _sut;
    private readonly GetThumbnailRequestFixture _getThumbnailRequestFixture = new();
    private readonly BlobDataDtoFixture _blobDataDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetThumbnailEndpointTests"/> class.
    /// </summary>
    public GetThumbnailEndpointTests()
    {
        _mockApiHttpClient = Substitute.For<IApiHttpClient>();
        _sut = Factory.Create<GetThumbnailEndpoint>(_mockApiHttpClient);
    }

    [Fact]
    public async Task ExecuteAsync_WhenApiReturnsThumbnail_ShouldReturnFileWithThumbnailBytes()
    {
        // Arrange
        GetThumbnailRequest request = _getThumbnailRequestFixture.Create(path: "/media/photo.png", quality: 70);
        byte[] expectedBytes = [0x89, 0x50, 0x4E, 0x47];
        _mockApiHttpClient.GetBlobAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(_blobDataDtoFixture.Create(data: expectedBytes, contentType: "image/png"));

        // Act
        IResult result = await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        string expectedEndpoint = $"{ApiRoutes.Thumbnails.GET_THUMBNAIL}?path={Uri.EscapeDataString(request.Path!)}&quality={request.Quality}";
        await _mockApiHttpClient.Received(1).GetBlobAsync(expectedEndpoint, Arg.Any<CancellationToken>());
        FileContentHttpResult fileResult = Assert.IsType<FileContentHttpResult>(result);
        Assert.Equal(expectedBytes, fileResult.FileContents);
        Assert.Equal("image/png", fileResult.ContentType);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldRequestThumbnailAtTheSpecifiedQuality()
    {
        // Arrange
        GetThumbnailRequest request = _getThumbnailRequestFixture.Create(path: "/media/photo.png", quality: 30);
        _mockApiHttpClient.GetBlobAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(_blobDataDtoFixture.Create());

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        string expectedEndpoint = $"{ApiRoutes.Thumbnails.GET_THUMBNAIL}?path={Uri.EscapeDataString(request.Path!)}&quality=30";
        await _mockApiHttpClient.Received(1).GetBlobAsync(expectedEndpoint, Arg.Any<CancellationToken>());
    }
}
