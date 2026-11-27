#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Application.Common.CQRS;
using Lumina.Application.Core.UsersManagement.Settings.Queries.GetPlaybackState;
using Lumina.Contracts.Fixtures.Core.Responses.UsersManagement.Settings;
using Lumina.Contracts.Responses.UsersManagement.Settings;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Core.Endpoints.UsersManagement.Settings.GetPlaybackState;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
#endregion

namespace Lumina.Presentation.Api.UnitTests.Core.Endpoints.UsersManagement.Settings.GetPlaybackState;

/// <summary>
/// Contains unit tests for the <see cref="GetPlaybackStateEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetPlaybackStateEndpointTests
{
    private readonly IQueryHandler<GetPlaybackStateQuery, Result<PlaybackStateResponse>> _mockHandler;
    private readonly GetPlaybackStateEndpoint _sut;
    private readonly PlaybackStateResponseFixture _playbackStateResponseFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetPlaybackStateEndpointTests"/> class.
    /// </summary>
    public GetPlaybackStateEndpointTests()
    {
        _mockHandler = Substitute.For<IQueryHandler<GetPlaybackStateQuery, Result<PlaybackStateResponse>>>();
        _sut = Factory.Create<GetPlaybackStateEndpoint>(_mockHandler);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSuccessful_ShouldReturnOkResultWithPlaybackStateResponse()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;
        PlaybackStateResponse expectedResponse = _playbackStateResponseFixture.Create(volume: 0.35, isMuted: true);
        _mockHandler.HandleAsync(Arg.Any<GetPlaybackStateQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(expectedResponse));

        // Act
        IResult result = await _sut.ExecuteAsync(EmptyRequest.Instance, cancellationToken);

        // Assert
        Ok<PlaybackStateResponse> okResult = Assert.IsType<Ok<PlaybackStateResponse>>(result);
        Assert.Equal(expectedResponse, okResult.Value);
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsFailureError_ShouldReturnProblemResult()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;
        Error expectedError = Error.Failure("General.Failure", "An error has occurred.");
        _mockHandler.HandleAsync(Arg.Any<GetPlaybackStateQuery>(), Arg.Any<CancellationToken>())
            .Returns(expectedError);

        // Act
        IResult result = await _sut.ExecuteAsync(EmptyRequest.Instance, cancellationToken);

        // Assert
        ProblemHttpResult problemResult = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status403Forbidden, problemResult.StatusCode);
        Assert.Equal("application/problem+json", problemResult.ContentType);
        Assert.Equal("General.Failure", problemResult.ProblemDetails.Title);
        Assert.Equal("An error has occurred.", problemResult.ProblemDetails.Detail);
        Assert.NotNull(problemResult.ProblemDetails.Extensions["traceId"]);
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsNotAuthorized_ShouldReturnForbiddenProblemResult()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<GetPlaybackStateQuery>(), Arg.Any<CancellationToken>())
            .Returns(ApplicationErrors.Authorization.NotAuthorized);

        // Act
        IResult result = await _sut.ExecuteAsync(EmptyRequest.Instance, cancellationToken);

        // Assert
        ProblemHttpResult problemResult = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status403Forbidden, problemResult.StatusCode);
        Assert.Equal("General.Unauthorized", problemResult.ProblemDetails.Title);
        Assert.Equal("NotAuthorized", problemResult.ProblemDetails.Detail);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldSendGetPlaybackStateQueryToHandler()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<GetPlaybackStateQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_playbackStateResponseFixture.Create()));

        // Act
        await _sut.ExecuteAsync(EmptyRequest.Instance, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Any<GetPlaybackStateQuery>(),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationRequested_ShouldCancelOperation()
    {
        // Arrange
        CancellationTokenSource cancellationTokenSource = new();
        TaskCompletionSource<bool> operationStarted = new();
        TaskCompletionSource<bool> cancellationRequested = new();

        _mockHandler.HandleAsync(Arg.Any<GetPlaybackStateQuery>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.Run(async () =>
            {
                operationStarted.SetResult(true);
                await cancellationRequested.Task;
                callInfo.Arg<CancellationToken>().ThrowIfCancellationRequested();
                return Result.From(_playbackStateResponseFixture.Create());
            }, callInfo.Arg<CancellationToken>()));

        // Act
        Task<IResult> operationTask = _sut.ExecuteAsync(EmptyRequest.Instance, cancellationTokenSource.Token);
        await operationStarted.Task;
        cancellationTokenSource.Cancel();
        cancellationRequested.SetResult(true);

        // Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => operationTask);
    }
}
