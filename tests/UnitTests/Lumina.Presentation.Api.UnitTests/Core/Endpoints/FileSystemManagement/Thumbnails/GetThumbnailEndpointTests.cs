#region ========================================================================= USING =====================================================================================
using ErrorOr;
using FastEndpoints;
using Lumina.Application.Core.FileSystemManagement.Thumbnails.Queries.GetThumbnail;
using Lumina.Contracts.Requests.FileSystemManagement.Thumbnails;
using Lumina.Contracts.Responses.FileSystemManagement.Thumbnails;
using Lumina.Presentation.Api.Common.Utilities;
using Lumina.Presentation.Api.Core.Endpoints.FileSystemManagement.Thumbnails.GetThumbnail;
using Lumina.Presentation.Api.UnitTests.Core.Endpoints.FileSystemManagement.Fixtures;
using Mediator;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.UnitTests.Core.Endpoints.FileSystemManagement.Thumbnails;

/// <summary>
/// Contains unit tests for the <see cref="GetThumbnailEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetThumbnailEndpointTests
{
    private readonly ISender _mockSender;
    private readonly GetThumbnailEndpoint _sut;
    private readonly GetThumbnailRequestFixture _getThumbnailRequestFixture;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetThumbnailEndpointTests"/> class.
    /// </summary>
    public GetThumbnailEndpointTests()
    {
        _mockSender = Substitute.For<ISender>();
        _sut = Factory.Create<GetThumbnailEndpoint>(_mockSender);
        _getThumbnailRequestFixture = new GetThumbnailRequestFixture();
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldReturnFileResultWithThumbnailResponse()
    {
        // Arrange
        GetThumbnailRequest request = _getThumbnailRequestFixture.Create("/path/to/file.jpg", 80);
        CancellationToken cancellationToken = CancellationToken.None;
        ThumbnailResponse expectedResponse = ThumbnailResponseFixture.CreateThumbnailResponse();
        _mockSender.Send(Arg.Any<GetThumbnailQuery>(), Arg.Any<CancellationToken>())
            .Returns(ErrorOrFactory.From(expectedResponse));

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        FileContentHttpResult fileResult = Assert.IsType<FileContentHttpResult>(result);
        Assert.Equal(expectedResponse.Bytes, fileResult.FileContents.ToArray());
        Assert.Equal(MimeTypes.GetMimeType(expectedResponse.Type), fileResult.ContentType);
    }

    [Fact]
    public async Task ExecuteAsync_WhenMediatorReturnsError_ShouldReturnProblemResult()
    {
        // Arrange
        GetThumbnailRequest request = _getThumbnailRequestFixture.Create("/path/to/nonexistent/file.jpg", 80);
        CancellationToken cancellationToken = CancellationToken.None;
        Error expectedError = Error.NotFound("Thumbnail.NotFound", "The requested thumbnail was not found.");
        _mockSender.Send(Arg.Any<GetThumbnailQuery>(), Arg.Any<CancellationToken>())
            .Returns(expectedError);

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        ProblemHttpResult problemDetails = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status404NotFound, problemDetails.StatusCode);
        Assert.Equal("application/problem+json", problemDetails.ContentType);
        Assert.IsType<Microsoft.AspNetCore.Mvc.ProblemDetails>(problemDetails.ProblemDetails);

        Assert.Equal("Thumbnail.NotFound", problemDetails.ProblemDetails.Title);
        Assert.Equal("The requested thumbnail was not found.", problemDetails.ProblemDetails.Detail);
        Assert.Equal(StatusCodes.Status404NotFound, problemDetails.ProblemDetails.Status);
        Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.5.5", problemDetails.ProblemDetails.Type);
        Assert.NotNull(problemDetails.ProblemDetails.Extensions["traceId"]);
    }

    [Fact]
    public async Task ExecuteAsync_WhenMediatorReturnsValidationError_ShouldReturnValidationProblemResult()
    {
        // Arrange
        GetThumbnailRequest request = _getThumbnailRequestFixture.Create("/path/to/nonexistent/file.jpg", 80);
        CancellationToken cancellationToken = CancellationToken.None;
        Error expectedError = Error.Validation("Path.Invalid", "The provided path is invalid.");
        _mockSender.Send(Arg.Any<GetThumbnailQuery>(), Arg.Any<CancellationToken>())
            .Returns(expectedError);

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        ProblemHttpResult problemDetails = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, problemDetails.StatusCode);
        Assert.Equal("application/problem+json", problemDetails.ContentType);
        HttpValidationProblemDetails validationProblemDetails = Assert.IsType<HttpValidationProblemDetails>(problemDetails.ProblemDetails);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, validationProblemDetails.Status);
        Assert.Equal("General.Validation", validationProblemDetails.Title);
        Assert.Equal("OneOrMoreValidationErrorsOccurred", validationProblemDetails.Detail);
        Assert.Equal("https://tools.ietf.org/html/rfc4918#section-11.2", validationProblemDetails.Type);
        Assert.Single(validationProblemDetails.Errors);
        Assert.Equal(new[] { "The provided path is invalid." }, validationProblemDetails.Errors["Path.Invalid"]);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldSendGetThumbnailQueryToSender()
    {
        // Arrange
        GetThumbnailRequest request = _getThumbnailRequestFixture.Create("/path/to/file.jpg", 80);
        CancellationToken cancellationToken = CancellationToken.None;
        _mockSender.Send(Arg.Any<GetThumbnailQuery>(), Arg.Any<CancellationToken>())
            .Returns(ErrorOrFactory.From(ThumbnailResponseFixture.CreateThumbnailResponse()));

        // Act
        await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        await _mockSender.Received(1).Send(Arg.Is<GetThumbnailQuery>(q => q.Path == request.Path && q.Quality == request.Quality), Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationRequested_ShouldCancelOperation()
    {
        // Arrange
        GetThumbnailRequest request = _getThumbnailRequestFixture.Create("/path/to/file.jpg", 80);
        CancellationTokenSource cts = new();
        TaskCompletionSource<bool> operationStarted = new();
        TaskCompletionSource<bool> cancellationRequested = new();

        _mockSender.Send(Arg.Any<GetThumbnailQuery>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => new ValueTask<ErrorOr<ThumbnailResponse>>(Task.Run(async () =>
            {
                operationStarted.SetResult(true);
                await cancellationRequested.Task;
                callInfo.Arg<CancellationToken>().ThrowIfCancellationRequested();
                return ErrorOrFactory.From(ThumbnailResponseFixture.CreateThumbnailResponse());
            }, callInfo.Arg<CancellationToken>())));

        // Act
        Task<IResult> operationTask = _sut.ExecuteAsync(request, cts.Token);
        await operationStarted.Task;
        cts.Cancel();
        cancellationRequested.SetResult(true);

        // Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => operationTask);
    }
}
