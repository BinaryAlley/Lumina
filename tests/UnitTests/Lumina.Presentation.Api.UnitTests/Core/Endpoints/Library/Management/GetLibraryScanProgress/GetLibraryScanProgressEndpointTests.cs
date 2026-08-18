#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Application.Common.CQRS;
using Lumina.Application.Core.MediaLibrary.Management.Queries.GetLibraryScanProgress;
using Lumina.Contracts.Fixtures.Core.Requests.MediaLibrary.Management;
using Lumina.Contracts.Fixtures.Core.Responses.MediaLibrary.Management;
using Lumina.Contracts.Requests.MediaLibrary.Management;
using Lumina.Contracts.Responses.MediaLibrary.Management;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Core.Endpoints.Library.Management.GetLibraryScanProgress;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.UnitTests.Core.Endpoints.Library.Management.GetLibraryScanProgress;

/// <summary>
/// Contains unit tests for the <see cref="GetLibraryScanProgressEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetLibraryScanProgressEndpointTests
{
    private readonly IQueryHandler<GetLibraryScanProgressQuery, Result<MediaLibraryScanProgressResponse>> _mockHandler;
    private readonly GetLibraryScanProgressEndpoint _sut;
    private readonly GetLibraryScanProgressRequestFixture _getLibraryScanProgressRequestFixture = new();
    private readonly MediaLibraryScanProgressResponseFixture _mediaLibraryScanProgressResponseFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetLibraryScanProgressEndpointTests"/> class.
    /// </summary>
    public GetLibraryScanProgressEndpointTests()
    {
        _mockHandler = Substitute.For<IQueryHandler<GetLibraryScanProgressQuery, Result<MediaLibraryScanProgressResponse>>>();
        _sut = Factory.Create<GetLibraryScanProgressEndpoint>(_mockHandler);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSuccessful_ShouldReturnOkResultWithMediaLibraryScanProgressResponse()
    {
        // Arrange
        GetLibraryScanProgressRequest request = _getLibraryScanProgressRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        MediaLibraryScanProgressResponse expectedResponse = _mediaLibraryScanProgressResponseFixture.Create(scanId: request.ScanId, libraryId: request.LibraryId, totalJobs: 5, completedJobs: 2, currentJobProgress: null, status: "Running", overallProgressPercentage: 40M);
        _mockHandler.HandleAsync(Arg.Any<GetLibraryScanProgressQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(expectedResponse));

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        Ok<MediaLibraryScanProgressResponse> okResult = Assert.IsType<Ok<MediaLibraryScanProgressResponse>>(result);
        Assert.Equal(expectedResponse, okResult.Value);
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsError_ShouldReturnProblemResult()
    {
        // Arrange
        GetLibraryScanProgressRequest request = _getLibraryScanProgressRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        Error expectedError = Error.NotFound("LibraryScan.NotFound", "The requested library scan was not found.");
        _mockHandler.HandleAsync(Arg.Any<GetLibraryScanProgressQuery>(), Arg.Any<CancellationToken>())
            .Returns(expectedError);

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        ProblemHttpResult problemDetails = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status404NotFound, problemDetails.StatusCode);
        Assert.Equal("application/problem+json", problemDetails.ContentType);
        Assert.IsType<Microsoft.AspNetCore.Mvc.ProblemDetails>(problemDetails.ProblemDetails);

        Assert.Equal("LibraryScan.NotFound", problemDetails.ProblemDetails.Title);
        Assert.Equal("The requested library scan was not found.", problemDetails.ProblemDetails.Detail);
        Assert.Equal(StatusCodes.Status404NotFound, problemDetails.ProblemDetails.Status);
        Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.5.5", problemDetails.ProblemDetails.Type);
        Assert.NotNull(problemDetails.ProblemDetails.Extensions["traceId"]);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldSendGetLibraryScanProgressQueryToSender()
    {
        // Arrange
        GetLibraryScanProgressRequest request = _getLibraryScanProgressRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<GetLibraryScanProgressQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_mediaLibraryScanProgressResponseFixture.Create(scanId: request.ScanId, libraryId: request.LibraryId, totalJobs: 5, completedJobs: 2, currentJobProgress: null, status: "Running", overallProgressPercentage: 40M)));

        // Act
        await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<GetLibraryScanProgressQuery>(query =>
                query.LibraryId == request.LibraryId &&
                query.ScanId == request.ScanId),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationRequested_ShouldCancelOperation()
    {
        // Arrange
        GetLibraryScanProgressRequest request = _getLibraryScanProgressRequestFixture.Create();
        CancellationTokenSource cts = new();
        TaskCompletionSource<bool> operationStarted = new();
        TaskCompletionSource<bool> cancellationRequested = new();

        _mockHandler.HandleAsync(Arg.Any<GetLibraryScanProgressQuery>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.Run(async () =>
            {
                operationStarted.SetResult(true);
                await cancellationRequested.Task;
                callInfo.Arg<CancellationToken>().ThrowIfCancellationRequested();
                return Result.From(_mediaLibraryScanProgressResponseFixture.Create(scanId: request.ScanId, libraryId: request.LibraryId, totalJobs: 5, completedJobs: 2, currentJobProgress: null, status: "Running", overallProgressPercentage: 40M));
            }, callInfo.Arg<CancellationToken>()));

        // Act
        Task<IResult> operationTask = _sut.ExecuteAsync(request, cts.Token);
        await operationStarted.Task;
        cts.Cancel();
        cancellationRequested.SetResult(true);

        // Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => operationTask);
    }
}
