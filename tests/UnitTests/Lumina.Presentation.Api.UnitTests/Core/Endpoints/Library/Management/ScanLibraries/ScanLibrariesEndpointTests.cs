#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Core.MediaLibrary.Management.Commands.ScanLibraries;
using Lumina.Contracts.Fixtures.Core.Responses.MediaLibrary.Management;
using Lumina.Contracts.Responses.MediaLibrary.Management;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Core.Endpoints.Library.Management.ScanLibraries;
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

namespace Lumina.Presentation.Api.UnitTests.Core.Endpoints.Library.Management.ScanLibraries;

/// <summary>
/// Contains unit tests for the <see cref="ScanLibrariesEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ScanLibrariesEndpointTests
{
    private readonly ICommandHandler<ScanLibrariesCommand, Result<IEnumerable<MediaLibraryScanResponse>>> _mockHandler;
    private readonly ScanLibrariesEndpoint _sut;
    private readonly MediaLibraryScanResponseFixture _mediaLibraryScanResponseFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ScanLibrariesEndpointTests"/> class.
    /// </summary>
    public ScanLibrariesEndpointTests()
    {
        _mockHandler = Substitute.For<ICommandHandler<ScanLibrariesCommand, Result<IEnumerable<MediaLibraryScanResponse>>>>();
        _sut = FastEndpoints.Factory.Create<ScanLibrariesEndpoint>(_mockHandler);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSuccessful_ShouldReturnOkResultWithMediaLibraryScanResponses()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;
        IEnumerable<MediaLibraryScanResponse> expectedResponses =
        [
            _mediaLibraryScanResponseFixture.Create(),
            _mediaLibraryScanResponseFixture.Create()
        ];
        _mockHandler.HandleAsync(Arg.Any<ScanLibrariesCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(expectedResponses));

        // Act
        IResult result = await _sut.ExecuteAsync(FastEndpoints.EmptyRequest.Instance, cancellationToken);

        // Assert
        Ok<IEnumerable<MediaLibraryScanResponse>> okResult = Assert.IsType<Ok<IEnumerable<MediaLibraryScanResponse>>>(result);
        Assert.Equal(expectedResponses, okResult.Value);
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsError_ShouldReturnProblemResult()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;
        Error expectedError = Error.Failure("Library.Scan.Failed", "Failed to scan the media libraries.");
        _mockHandler.HandleAsync(Arg.Any<ScanLibrariesCommand>(), Arg.Any<CancellationToken>())
            .Returns(expectedError);

        // Act
        IResult result = await _sut.ExecuteAsync(FastEndpoints.EmptyRequest.Instance, cancellationToken);

        // Assert
        ProblemHttpResult problemDetails = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status403Forbidden, problemDetails.StatusCode);
        Assert.Equal("application/problem+json", problemDetails.ContentType);
        Assert.IsType<Microsoft.AspNetCore.Mvc.ProblemDetails>(problemDetails.ProblemDetails);

        Assert.Equal("Library.Scan.Failed", problemDetails.ProblemDetails.Title);
        Assert.Equal("Failed to scan the media libraries.", problemDetails.ProblemDetails.Detail);
        Assert.Equal(StatusCodes.Status403Forbidden, problemDetails.ProblemDetails.Status);
        Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.5.4", problemDetails.ProblemDetails.Type);
        Assert.NotNull(problemDetails.ProblemDetails.Extensions["traceId"]);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldSendScanLibrariesCommandToHandler()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<ScanLibrariesCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(Array.Empty<MediaLibraryScanResponse>().AsEnumerable()));

        // Act
        await _sut.ExecuteAsync(FastEndpoints.EmptyRequest.Instance, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(Arg.Any<ScanLibrariesCommand>(), Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationRequested_ShouldCancelOperation()
    {
        // Arrange
        CancellationTokenSource cts = new();
        TaskCompletionSource<bool> operationStarted = new();
        TaskCompletionSource<bool> cancellationRequested = new();

        _mockHandler.HandleAsync(Arg.Any<ScanLibrariesCommand>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.Run(async () =>
            {
                operationStarted.SetResult(true);
                await cancellationRequested.Task;
                callInfo.Arg<CancellationToken>().ThrowIfCancellationRequested();
                return Result.From(Array.Empty<MediaLibraryScanResponse>().AsEnumerable());
            }, callInfo.Arg<CancellationToken>()));

        // Act
        Task<IResult> operationTask = _sut.ExecuteAsync(FastEndpoints.EmptyRequest.Instance, cts.Token);
        await operationStarted.Task;
        cts.Cancel();
        cancellationRequested.SetResult(true);

        // Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => operationTask);
    }
}
