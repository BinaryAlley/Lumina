#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Application.Common.CQRS;
using Lumina.Application.Core.MediaLibrary.Management.Queries.GetRunningLibraryScans;
using Lumina.Contracts.Fixtures.Core.Responses.MediaLibrary.Management;
using Lumina.Contracts.Responses.MediaLibrary.Management;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Core.Endpoints.Library.Management.GetRunningLibraryScans;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.UnitTests.Core.Endpoints.Library.Management.GetRunningLibraryScans;

/// <summary>
/// Contains unit tests for the <see cref="GetRunningLibraryScansEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetRunningLibraryScansEndpointTests
{
    private readonly IQueryHandler<GetRunningLibraryScansQuery, Result<IEnumerable<MediaLibraryScanProgressResponse>>> _mockHandler;
    private readonly GetRunningLibraryScansEndpoint _sut;
    private readonly MediaLibraryScanProgressResponseFixture _mediaLibraryScanProgressResponseFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetRunningLibraryScansEndpointTests"/> class.
    /// </summary>
    public GetRunningLibraryScansEndpointTests()
    {
        _mockHandler = Substitute.For<IQueryHandler<GetRunningLibraryScansQuery, Result<IEnumerable<MediaLibraryScanProgressResponse>>>>();
        _sut = Factory.Create<GetRunningLibraryScansEndpoint>(_mockHandler);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSuccessful_ShouldReturnOkResultWithMediaLibraryScanProgressResponses()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;
        IEnumerable<MediaLibraryScanProgressResponse> expectedResponses =
        [
            _mediaLibraryScanProgressResponseFixture.Create(),
            _mediaLibraryScanProgressResponseFixture.Create()
        ];
        _mockHandler.HandleAsync(Arg.Any<GetRunningLibraryScansQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(expectedResponses));

        // Act
        IResult result = await _sut.ExecuteAsync(EmptyRequest.Instance, cancellationToken);

        // Assert
        Ok<IEnumerable<MediaLibraryScanProgressResponse>> okResult = Assert.IsType<Ok<IEnumerable<MediaLibraryScanProgressResponse>>>(result);
        Assert.Equal(expectedResponses, okResult.Value);
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsError_ShouldReturnProblemResult()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;
        Error expectedError = Error.Failure("LibraryScan.GetRunning.Failed", "Failed to get the running library scans.");
        _mockHandler.HandleAsync(Arg.Any<GetRunningLibraryScansQuery>(), Arg.Any<CancellationToken>())
            .Returns(expectedError);

        // Act
        IResult result = await _sut.ExecuteAsync(EmptyRequest.Instance, cancellationToken);

        // Assert
        ProblemHttpResult problemDetails = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status403Forbidden, problemDetails.StatusCode);
        Assert.Equal("application/problem+json", problemDetails.ContentType);
        Assert.IsType<Microsoft.AspNetCore.Mvc.ProblemDetails>(problemDetails.ProblemDetails);

        Assert.Equal("LibraryScan.GetRunning.Failed", problemDetails.ProblemDetails.Title);
        Assert.Equal("Failed to get the running library scans.", problemDetails.ProblemDetails.Detail);
        Assert.Equal(StatusCodes.Status403Forbidden, problemDetails.ProblemDetails.Status);
        Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.5.4", problemDetails.ProblemDetails.Type);
        Assert.NotNull(problemDetails.ProblemDetails.Extensions["traceId"]);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldSendGetRunningLibraryScansQueryToHandler()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<GetRunningLibraryScansQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(Array.Empty<MediaLibraryScanProgressResponse>().AsEnumerable()));

        // Act
        await _sut.ExecuteAsync(EmptyRequest.Instance, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(Arg.Any<GetRunningLibraryScansQuery>(), Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationRequested_ShouldCancelOperation()
    {
        // Arrange
        CancellationTokenSource cts = new();
        TaskCompletionSource<bool> operationStarted = new();
        TaskCompletionSource<bool> cancellationRequested = new();

        _mockHandler.HandleAsync(Arg.Any<GetRunningLibraryScansQuery>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.Run(async () =>
            {
                operationStarted.SetResult(true);
                await cancellationRequested.Task;
                callInfo.Arg<CancellationToken>().ThrowIfCancellationRequested();
                return Result.From(Array.Empty<MediaLibraryScanProgressResponse>().AsEnumerable());
            }, callInfo.Arg<CancellationToken>()));

        // Act
        Task<IResult> operationTask = _sut.ExecuteAsync(EmptyRequest.Instance, cts.Token);
        await operationStarted.Task;
        cts.Cancel();
        cancellationRequested.SetResult(true);

        // Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => operationTask);
    }
}
