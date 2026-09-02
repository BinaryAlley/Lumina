#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Application.Common.CQRS;
using Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading.Queries.GetReadingResource;
using Lumina.Contracts.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary.Reading;
using Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary.Reading;
using Lumina.Contracts.Fixtures.Core.Requests.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading;
using Lumina.Contracts.Requests.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.Reading.GetReadingResource;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.UnitTests.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.Reading.GetReadingResource;

/// <summary>
/// Contains unit tests for the <see cref="GetReadingResourceEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetReadingResourceEndpointTests
{
    private readonly IQueryHandler<GetReadingResourceQuery, Result<ReadingResourceDataDto>> _mockHandler;
    private readonly GetReadingResourceEndpoint _sut;
    private readonly GetReadingResourceRequestFixture _getReadingResourceRequestFixture = new();
    private readonly ReadingResourceDataDtoFixture _readingResourceDataDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetReadingResourceEndpointTests"/> class.
    /// </summary>
    public GetReadingResourceEndpointTests()
    {
        _mockHandler = Substitute.For<IQueryHandler<GetReadingResourceQuery, Result<ReadingResourceDataDto>>>();
        _sut = Factory.Create<GetReadingResourceEndpoint>(_mockHandler);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSuccessful_ShouldReturnBytesResultWithResourceData()
    {
        // Arrange
        GetReadingResourceRequest request = _getReadingResourceRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        ReadingResourceDataDto expectedResponse = _readingResourceDataDtoFixture.Create();
        _mockHandler.HandleAsync(Arg.Any<GetReadingResourceQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(expectedResponse));

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        FileContentHttpResult bytesResult = Assert.IsType<FileContentHttpResult>(result);
        Assert.Equal(expectedResponse.Data, bytesResult.FileContents);
        Assert.Equal(expectedResponse.MimeType, bytesResult.ContentType);
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsError_ShouldReturnProblemResult()
    {
        // Arrange
        GetReadingResourceRequest request = _getReadingResourceRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        Error expectedError = Error.NotFound("Reading.ResourceNotFound", "ResourceNotFound");
        _mockHandler.HandleAsync(Arg.Any<GetReadingResourceQuery>(), Arg.Any<CancellationToken>())
            .Returns(expectedError);

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        ProblemHttpResult problemResult = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status404NotFound, problemResult.StatusCode);
        Assert.Equal("application/problem+json", problemResult.ContentType);
        Assert.Equal("Reading.ResourceNotFound", problemResult.ProblemDetails.Title);
        Assert.Equal("ResourceNotFound", problemResult.ProblemDetails.Detail);
    }

    [Theory]
    [InlineData("image/png", "image/png")] // a non-svg image is inert content
    [InlineData("image/svg+xml", "application/octet-stream")] // an svg can be executed, so it is served as an opaque download
    [InlineData("audio/mpeg", "audio/mpeg")] // audio is inert content
    [InlineData("video/mp4", "video/mp4")] // video is inert content
    [InlineData("font/woff2", "font/woff2")] // fonts are inert content
    [InlineData("application/font-woff", "application/font-woff")] // legacy woff font media type
    [InlineData("application/font-woff2", "application/font-woff2")] // legacy woff2 font media type
    [InlineData("application/vnd.ms-opentype", "application/vnd.ms-opentype")] // opentype font media type
    [InlineData("application/x-font-ttf", "application/x-font-ttf")] // ttf font media type
    [InlineData("application/x-font-opentype", "application/x-font-opentype")] // otf font media type
    [InlineData("text/css", "text/css")] // stylesheets are inert content
    [InlineData("text/html", "application/octet-stream")] // an html document can be executed, so it is served as an opaque download
    public async Task ExecuteAsync_WhenServingAResource_ShouldUseTheSafeContentType(string mimeType, string expectedContentType)
    {
        // Arrange
        GetReadingResourceRequest request = _getReadingResourceRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        ReadingResourceDataDto response = new(Guid.NewGuid().ToByteArray(), mimeType);
        _mockHandler.HandleAsync(Arg.Any<GetReadingResourceQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(response));

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        FileContentHttpResult bytesResult = Assert.IsType<FileContentHttpResult>(result);
        Assert.Equal(response.Data, bytesResult.FileContents);
        Assert.Equal(expectedContentType, bytesResult.ContentType);
        Assert.Equal("nosniff", _sut.HttpContext.Response.Headers["X-Content-Type-Options"]);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldSendGetReadingResourceQueryToSender()
    {
        // Arrange
        GetReadingResourceRequest request = _getReadingResourceRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        ReadingResourceDataDto response = _readingResourceDataDtoFixture.Create();
        _mockHandler.HandleAsync(Arg.Any<GetReadingResourceQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(response));

        // Act
        await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<GetReadingResourceQuery>(query => query.BookId == request.BookId && query.ResourceKey == request.ResourceKey),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationRequested_ShouldCancelOperation()
    {
        // Arrange
        GetReadingResourceRequest request = _getReadingResourceRequestFixture.Create();
        CancellationTokenSource cts = new();
        TaskCompletionSource<bool> operationStarted = new();
        TaskCompletionSource<bool> cancellationRequested = new();

        _mockHandler.HandleAsync(Arg.Any<GetReadingResourceQuery>(), Arg.Any<CancellationToken>())
            .Returns(info => Task.Run(async () =>
            {
                operationStarted.SetResult(true);
                await cancellationRequested.Task;
                info.Arg<CancellationToken>().ThrowIfCancellationRequested();
                return Result.From(_readingResourceDataDtoFixture.Create());
            }, info.Arg<CancellationToken>()));

        // Act
        Task<IResult> operationTask = _sut.ExecuteAsync(request, cts.Token);
        await operationStarted.Task;
        cts.Cancel();
        cancellationRequested.SetResult(true);

        // Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => operationTask);
    }
}
