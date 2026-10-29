#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Core.UsersManagement.Settings.Commands.UpdatePlaybackState;
using Lumina.Contracts.Fixtures.Core.Requests.UsersManagement.Settings;
using Lumina.Contracts.Requests.UsersManagement.Settings;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Core.Endpoints.UsersManagement.Settings.UpdatePlaybackState;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.UnitTests.Core.Endpoints.UsersManagement.Settings.UpdatePlaybackState;

/// <summary>
/// Contains unit tests for the <see cref="UpdatePlaybackStateEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdatePlaybackStateEndpointTests
{
    private readonly ICommandHandler<UpdatePlaybackStateCommand, Result<Updated>> _mockHandler;
    private readonly UpdatePlaybackStateEndpoint _sut;
    private readonly UpdatePlaybackStateRequestFixture _updatePlaybackStateRequestFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdatePlaybackStateEndpointTests"/> class.
    /// </summary>
    public UpdatePlaybackStateEndpointTests()
    {
        _mockHandler = Substitute.For<ICommandHandler<UpdatePlaybackStateCommand, Result<Updated>>>();
        _sut = FastEndpoints.Factory.Create<UpdatePlaybackStateEndpoint>(_mockHandler);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSuccessful_ShouldReturnOkResultWithUpdatedMarker()
    {
        // Arrange
        UpdatePlaybackStateRequest request = _updatePlaybackStateRequestFixture.Create(volume: 0.35, isMuted: true);
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<UpdatePlaybackStateCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Updated);

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        Ok<Updated> okResult = Assert.IsType<Ok<Updated>>(result);
        Assert.Equal(Result.Updated, okResult.Value);
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsValidationErrors_ShouldReturnValidationProblemResult()
    {
        // Arrange
        UpdatePlaybackStateRequest request = _updatePlaybackStateRequestFixture.Create(volume: 1.5);
        CancellationToken cancellationToken = CancellationToken.None;
        Error expectedError = Error.Validation("UserSettings.VolumeMustBeBetweenZeroAndOne", "Volume must be between zero and one.");
        _mockHandler.HandleAsync(Arg.Any<UpdatePlaybackStateCommand>(), Arg.Any<CancellationToken>())
            .Returns(expectedError);

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        ProblemHttpResult problemResult = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, problemResult.StatusCode);
        Assert.Equal("application/problem+json", problemResult.ContentType);
        HttpValidationProblemDetails validationProblemDetails = Assert.IsType<HttpValidationProblemDetails>(problemResult.ProblemDetails);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, validationProblemDetails.Status);
        Assert.Equal("General.Validation", validationProblemDetails.Title);
        Assert.Equal(new[] { "Volume must be between zero and one." }, validationProblemDetails.Errors["UserSettings.VolumeMustBeBetweenZeroAndOne"]);
        Assert.NotNull(validationProblemDetails.Extensions["traceId"]);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldSendUpdatePlaybackStateCommandToHandler()
    {
        // Arrange
        UpdatePlaybackStateRequest request = _updatePlaybackStateRequestFixture.Create(volume: 0.25, isMuted: false);
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<UpdatePlaybackStateCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Updated);

        // Act
        await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<UpdatePlaybackStateCommand>(command =>
                command.Volume == 0.25 &&
                command.IsMuted == false),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenRequestHasNullValues_ShouldForwardTheNullsToTheHandler()
    {
        // Arrange
        UpdatePlaybackStateRequest request = _updatePlaybackStateRequestFixture.Create(includeVolume: false, includeIsMuted: false);
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<UpdatePlaybackStateCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Updated);

        // Act
        await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<UpdatePlaybackStateCommand>(command =>
                command.Volume == null &&
                command.IsMuted == null),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationRequested_ShouldCancelOperation()
    {
        // Arrange
        CancellationTokenSource cancellationTokenSource = new();
        TaskCompletionSource<bool> operationStarted = new();
        TaskCompletionSource<bool> cancellationRequested = new();

        _mockHandler.HandleAsync(Arg.Any<UpdatePlaybackStateCommand>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.Run(async () =>
            {
                operationStarted.SetResult(true);
                await cancellationRequested.Task;
                callInfo.Arg<CancellationToken>().ThrowIfCancellationRequested();
                return Result.From(Result.Updated);
            }, callInfo.Arg<CancellationToken>()));

        // Act
        Task<IResult> operationTask = _sut.ExecuteAsync(_updatePlaybackStateRequestFixture.Create(), cancellationTokenSource.Token);
        await operationStarted.Task;
        cancellationTokenSource.Cancel();
        cancellationRequested.SetResult(true);

        // Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => operationTask);
    }
}
