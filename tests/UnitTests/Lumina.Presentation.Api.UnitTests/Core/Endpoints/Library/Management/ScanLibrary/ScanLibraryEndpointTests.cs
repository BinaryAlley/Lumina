#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Core.MediaLibrary.Management.Commands.ScanLibrary;
using Lumina.Contracts.Fixtures.Core.Requests.MediaLibrary.Management;
using Lumina.Contracts.Fixtures.Core.Responses.MediaLibrary.Management;
using Lumina.Contracts.Requests.MediaLibrary.Management;
using Lumina.Contracts.Responses.MediaLibrary.Management;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Core.Endpoints.Library.Management.ScanLibrary;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.UnitTests.Core.Endpoints.Library.Management.ScanLibrary;

/// <summary>
/// Contains unit tests for the <see cref="ScanLibraryEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ScanLibraryEndpointTests
{
    private readonly ICommandHandler<ScanLibraryCommand, Result<MediaLibraryScanResponse>> _mockHandler;
    private readonly ScanLibraryEndpoint _sut;
    private readonly ScanLibraryRequestFixture _scanLibraryRequestFixture = new();
    private readonly MediaLibraryScanResponseFixture _mediaLibraryScanResponseFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ScanLibraryEndpointTests"/> class.
    /// </summary>
    public ScanLibraryEndpointTests()
    {
        _mockHandler = Substitute.For<ICommandHandler<ScanLibraryCommand, Result<MediaLibraryScanResponse>>>();
        _sut = FastEndpoints.Factory.Create<ScanLibraryEndpoint>(_mockHandler);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSuccessful_ShouldReturnOkResultWithMediaLibraryScanResponse()
    {
        // Arrange
        ScanLibraryRequest request = _scanLibraryRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        MediaLibraryScanResponse expectedResponse = _mediaLibraryScanResponseFixture.Create(libraryId: request.Id);
        _mockHandler.HandleAsync(Arg.Any<ScanLibraryCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(expectedResponse));

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        Ok<MediaLibraryScanResponse> okResult = Assert.IsType<Ok<MediaLibraryScanResponse>>(result);
        Assert.Equal(expectedResponse, okResult.Value);
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsError_ShouldReturnProblemResult()
    {
        // Arrange
        ScanLibraryRequest request = _scanLibraryRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        Error expectedError = Error.NotFound("Library.NotFound", "The requested library was not found.");
        _mockHandler.HandleAsync(Arg.Any<ScanLibraryCommand>(), Arg.Any<CancellationToken>())
            .Returns(expectedError);

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        ProblemHttpResult problemDetails = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status404NotFound, problemDetails.StatusCode);
        Assert.Equal("application/problem+json", problemDetails.ContentType);
        Assert.IsType<Microsoft.AspNetCore.Mvc.ProblemDetails>(problemDetails.ProblemDetails);

        Assert.Equal("Library.NotFound", problemDetails.ProblemDetails.Title);
        Assert.Equal("The requested library was not found.", problemDetails.ProblemDetails.Detail);
        Assert.Equal(StatusCodes.Status404NotFound, problemDetails.ProblemDetails.Status);
        Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.5.5", problemDetails.ProblemDetails.Type);
        Assert.NotNull(problemDetails.ProblemDetails.Extensions["traceId"]);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldSendScanLibraryCommandToSender()
    {
        // Arrange
        ScanLibraryRequest request = _scanLibraryRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<ScanLibraryCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_mediaLibraryScanResponseFixture.Create(libraryId: request.Id)));

        // Act
        await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<ScanLibraryCommand>(command => command.Id == request.Id),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationRequested_ShouldCancelOperation()
    {
        // Arrange
        ScanLibraryRequest request = _scanLibraryRequestFixture.Create();
        CancellationTokenSource cts = new();
        TaskCompletionSource<bool> operationStarted = new();
        TaskCompletionSource<bool> cancellationRequested = new();

        _mockHandler.HandleAsync(Arg.Any<ScanLibraryCommand>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.Run(async () =>
            {
                operationStarted.SetResult(true);
                await cancellationRequested.Task;
                callInfo.Arg<CancellationToken>().ThrowIfCancellationRequested();
                return Result.From(_mediaLibraryScanResponseFixture.Create(libraryId: request.Id));
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
