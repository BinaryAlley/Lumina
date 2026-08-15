#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Application.Common.CQRS;
using Lumina.Application.Core.FileSystemManagement.Directories.Queries.GetDirectories;
using Lumina.Contracts.Fixtures.Core.Requests.FileSystemManagement.Directories;
using Lumina.Contracts.Fixtures.Core.Responses.FileSystemManagement.Directories;
using Lumina.Contracts.Requests.FileSystemManagement.Directories;
using Lumina.Contracts.Responses.FileSystemManagement.Directories;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Core.Endpoints.FileSystemManagement.Directories.GetDirectories;
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

namespace Lumina.Presentation.Api.UnitTests.Core.Endpoints.FileSystemManagement.Directories;

/// <summary>
/// Contains unit tests for the <see cref="GetDirectoriesEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetDirectoriesEndpointTests
{
    private readonly IQueryHandler<GetDirectoriesQuery, Result<IEnumerable<DirectoryResponse>>> _mockHandler;
    private readonly GetDirectoriesEndpoint _sut;
    private readonly GetDirectoriesRequestFixture _getDirectoriesRequestFixture;
    private readonly DirectoryResponseFixture _directoryResponseFixture;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetDirectoriesEndpointTests"/> class.
    /// </summary>
    public GetDirectoriesEndpointTests()
    {
        _mockHandler = Substitute.For<IQueryHandler<GetDirectoriesQuery, Result<IEnumerable<DirectoryResponse>>>>();
        _getDirectoriesRequestFixture = new GetDirectoriesRequestFixture();
        _directoryResponseFixture = new DirectoryResponseFixture();
        _sut = Factory.Create<GetDirectoriesEndpoint>(_mockHandler);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldReturnOkResultWithDirectoryResponses()
    {
        // Arrange
        GetDirectoriesRequest request = _getDirectoriesRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        List<DirectoryResponse> expectedResponses = [.. _directoryResponseFixture.CreateMany(3)];
        _mockHandler.HandleAsync(Arg.Any<GetDirectoriesQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(expectedResponses.AsEnumerable()));

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        Ok<IEnumerable<DirectoryResponse>> okResult = Assert.IsType<Ok<IEnumerable<DirectoryResponse>>>(result);
        Assert.Equal(expectedResponses, okResult.Value);
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsError_ShouldReturnProblemResult()
    {
        // Arrange
        GetDirectoriesRequest request = _getDirectoriesRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        Error expectedError = Error.NotFound("Directory.NotFound", "The requested directory was not found.");
        _mockHandler.HandleAsync(Arg.Any<GetDirectoriesQuery>(), Arg.Any<CancellationToken>())
            .Returns(expectedError);

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        ProblemHttpResult problemDetails = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status404NotFound, problemDetails.StatusCode);
        Assert.Equal("application/problem+json", problemDetails.ContentType);
        Assert.IsType<Microsoft.AspNetCore.Mvc.ProblemDetails>(problemDetails.ProblemDetails);

        Assert.Equal("Directory.NotFound", problemDetails.ProblemDetails.Title);
        Assert.Equal("The requested directory was not found.", problemDetails.ProblemDetails.Detail);
        Assert.Equal(StatusCodes.Status404NotFound, problemDetails.ProblemDetails.Status);
        Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.5.5", problemDetails.ProblemDetails.Type);
        Assert.NotNull(problemDetails.ProblemDetails.Extensions["traceId"]);
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsValidationError_ShouldReturnValidationProblemResult()
    {
        // Arrange
        GetDirectoriesRequest request = _getDirectoriesRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        Error expectedError = Error.Validation("Path.Invalid", "The provided path is invalid.");
        _mockHandler.HandleAsync(Arg.Any<GetDirectoriesQuery>(), Arg.Any<CancellationToken>())
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
    public async Task ExecuteAsync_WhenCalled_ShouldSendGetDirectoriesQueryToHandler()
    {
        // Arrange
        GetDirectoriesRequest request = _getDirectoriesRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<GetDirectoriesQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(Enumerable.Empty<DirectoryResponse>()));

        // Act
        await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<GetDirectoriesQuery>(q => q.Path == request.Path && q.IncludeHiddenElements == request.IncludeHiddenElements),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationRequested_ShouldCancelOperation()
    {
        // Arrange
        GetDirectoriesRequest request = _getDirectoriesRequestFixture.Create();
        CancellationTokenSource cts = new();
        TaskCompletionSource<bool> operationStarted = new();
        TaskCompletionSource<bool> cancellationRequested = new();

        _mockHandler.HandleAsync(Arg.Any<GetDirectoriesQuery>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.Run(async () =>
            {
                operationStarted.SetResult(true);
                await cancellationRequested.Task;
                callInfo.Arg<CancellationToken>().ThrowIfCancellationRequested();
                return Result.From(_directoryResponseFixture.CreateMany(3).AsEnumerable());
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
