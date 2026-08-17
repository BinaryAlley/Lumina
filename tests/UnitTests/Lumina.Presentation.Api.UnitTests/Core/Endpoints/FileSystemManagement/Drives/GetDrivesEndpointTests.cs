#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Application.Common.CQRS;
using Lumina.Application.Core.FileSystemManagement.Drives.Queries.GetDrives;
using Lumina.Contracts.Fixtures.Core.Responses.FileSystemManagement.Common;
using Lumina.Contracts.Responses.FileSystemManagement.Common;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Core.Endpoints.FileSystemManagement.Drives.GetDrives;
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

namespace Lumina.Presentation.Api.UnitTests.Core.Endpoints.FileSystemManagement.Drives;

/// <summary>
/// Contains unit tests for the <see cref="GetDrivesEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetDrivesEndpointTests
{
    private readonly IQueryHandler<GetDrivesQuery, Result<IEnumerable<FileSystemTreeNodeResponse>>> _mockHandler;
    private readonly GetDrivesEndpoint _sut;
    private readonly FileSystemTreeNodeResponseFixture _fileSystemTreeNodeResponseFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="DrivesControllerTests"/> class.
    /// </summary>
    public GetDrivesEndpointTests()
    {
        _mockHandler = Substitute.For<IQueryHandler<GetDrivesQuery, Result<IEnumerable<FileSystemTreeNodeResponse>>>>();
        _sut = Factory.Create<GetDrivesEndpoint>(_mockHandler);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldReturnOkResultWithFileSystemTreeNodeResponses()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;
        List<FileSystemTreeNodeResponse> expectedResponses = _fileSystemTreeNodeResponseFixture.CreateMany(3, 1, 1);
        _mockHandler.HandleAsync(Arg.Any<GetDrivesQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(expectedResponses.AsEnumerable()));

        // Act
        IResult result = await _sut.ExecuteAsync(EmptyRequest.Instance, cancellationToken);

        // Assert
        Ok<IEnumerable<FileSystemTreeNodeResponse>> okResult = Assert.IsType<Ok<IEnumerable<FileSystemTreeNodeResponse>>>(result);
        Assert.Equal(expectedResponses, okResult.Value);
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsError_ShouldReturnProblemResult()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;
        Error expectedError = Error.NotFound("Drive.NotFound", "The requested drive was not found.");
        _mockHandler.HandleAsync(Arg.Any<GetDrivesQuery>(), Arg.Any<CancellationToken>())
            .Returns(expectedError);

        // Act
        IResult result = await _sut.ExecuteAsync(EmptyRequest.Instance, cancellationToken);

        // Assert
        ProblemHttpResult problemDetails = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status404NotFound, problemDetails.StatusCode);
        Assert.Equal("application/problem+json", problemDetails.ContentType);
        Assert.IsType<Microsoft.AspNetCore.Mvc.ProblemDetails>(problemDetails.ProblemDetails);

        Assert.Equal("Drive.NotFound", problemDetails.ProblemDetails.Title);
        Assert.Equal("The requested drive was not found.", problemDetails.ProblemDetails.Detail);
        Assert.Equal(StatusCodes.Status404NotFound, problemDetails.ProblemDetails.Status);
        Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.5.5", problemDetails.ProblemDetails.Type);
        Assert.NotNull(problemDetails.ProblemDetails.Extensions["traceId"]);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldSendGetDrivesQueryToHandler()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<GetDrivesQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(Enumerable.Empty<FileSystemTreeNodeResponse>()));

        // Act
        await _sut.ExecuteAsync(EmptyRequest.Instance, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(Arg.Any<GetDrivesQuery>(), Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationRequested_ShouldCancelOperation()
    {
        // Arrange
        CancellationTokenSource cts = new();
        TaskCompletionSource<bool> operationStarted = new();
        TaskCompletionSource<bool> cancellationRequested = new();

        _mockHandler.HandleAsync(Arg.Any<GetDrivesQuery>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.Run(async () =>
            {
                operationStarted.SetResult(true);
                await cancellationRequested.Task;
                callInfo.Arg<CancellationToken>().ThrowIfCancellationRequested();
                return Result.From(_fileSystemTreeNodeResponseFixture.CreateMany(3, 1, 1).AsEnumerable());
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
