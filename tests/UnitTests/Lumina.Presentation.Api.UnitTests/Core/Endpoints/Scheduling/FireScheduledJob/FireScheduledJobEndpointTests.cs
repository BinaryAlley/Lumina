#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Domain.Common.Errors;
using Lumina.Application.Core.Scheduling.Commands.FireScheduledJob;
using Lumina.Contracts.Fixtures.Core.Requests.Scheduling;
using Lumina.Contracts.Fixtures.Core.Responses.Scheduling;
using Lumina.Contracts.Requests.Scheduling;
using Lumina.Contracts.Responses.Scheduling;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.Scheduling;
using Lumina.Presentation.Api.Core.Endpoints.Scheduling.FireScheduledJob;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
#endregion

namespace Lumina.Presentation.Api.UnitTests.Core.Endpoints.Scheduling.FireScheduledJob;

/// <summary>
/// Contains unit tests for the <see cref="FireScheduledJobEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class FireScheduledJobEndpointTests
{
    private readonly Application.Common.CQRS.ICommandHandler<FireScheduledJobCommand, Result<ScheduledJobResponse>> _mockHandler;
    private readonly FireScheduledJobEndpoint _sut;
    private readonly FireScheduledJobRequestFixture _fireScheduledJobRequestFixture = new();
    private readonly ScheduledJobResponseFixture _scheduledJobResponseFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="FireScheduledJobEndpointTests"/> class.
    /// </summary>
    public FireScheduledJobEndpointTests()
    {
        _mockHandler = Substitute.For<Application.Common.CQRS.ICommandHandler<FireScheduledJobCommand, Result<ScheduledJobResponse>>>();
        _sut = Factory.Create<FireScheduledJobEndpoint>(_mockHandler);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSuccessful_ShouldReturnOkResultWithScheduledJobResponse()
    {
        // Arrange
        FireScheduledJobRequest request = _fireScheduledJobRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        ScheduledJobResponse expectedResponse = _scheduledJobResponseFixture.Create(
            id: request.ScheduledJobId,
            name: "Scan job",
            taskType: ScheduledTaskType.ScanMediaLibraries,
            scheduleType: ScheduleType.WithIntervalInMinutes,
            intervalMinutes: 60,
            status: ScheduledJobStatus.Active);
        _mockHandler.HandleAsync(Arg.Any<FireScheduledJobCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(expectedResponse));

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        Ok<ScheduledJobResponse> okResult = Assert.IsType<Ok<ScheduledJobResponse>>(result);
        Assert.Equal(expectedResponse, okResult.Value);
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsError_ShouldReturnProblemResult()
    {
        // Arrange
        FireScheduledJobRequest request = _fireScheduledJobRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        Error expectedError = Errors.Scheduling.ScheduledJobNotFound;
        _mockHandler.HandleAsync(Arg.Any<FireScheduledJobCommand>(), Arg.Any<CancellationToken>())
            .Returns(expectedError);

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        ProblemHttpResult problemResult = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status404NotFound, problemResult.StatusCode);
        Assert.Equal("application/problem+json", problemResult.ContentType);
        Assert.NotNull(problemResult.ProblemDetails.Extensions["traceId"]);
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsNotAuthorized_ShouldReturnForbiddenProblemResult()
    {
        // Arrange
        FireScheduledJobRequest request = _fireScheduledJobRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<FireScheduledJobCommand>(), Arg.Any<CancellationToken>())
            .Returns(ApplicationErrors.Authorization.NotAuthorized);

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        ProblemHttpResult problemResult = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status403Forbidden, problemResult.StatusCode);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldSendMappedCommandToHandler()
    {
        // Arrange
        FireScheduledJobRequest request = _fireScheduledJobRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        ScheduledJobResponse response = _scheduledJobResponseFixture.Create(
            id: request.ScheduledJobId,
            name: "Scan job",
            taskType: ScheduledTaskType.ScanMediaLibraries,
            scheduleType: ScheduleType.WithIntervalInMinutes,
            intervalMinutes: 60,
            status: ScheduledJobStatus.Active);
        _mockHandler.HandleAsync(Arg.Any<FireScheduledJobCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(response));

        // Act
        await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<FireScheduledJobCommand>(command => command.ScheduledJobId == request.ScheduledJobId),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationRequested_ShouldCancelOperation()
    {
        // Arrange
        FireScheduledJobRequest request = _fireScheduledJobRequestFixture.Create();
        CancellationTokenSource cts = new();
        TaskCompletionSource<bool> operationStarted = new();
        TaskCompletionSource<bool> cancellationRequested = new();
        ScheduledJobResponse response = _scheduledJobResponseFixture.Create(
            id: request.ScheduledJobId,
            name: "Scan job",
            taskType: ScheduledTaskType.ScanMediaLibraries,
            scheduleType: ScheduleType.WithIntervalInMinutes,
            intervalMinutes: 60,
            status: ScheduledJobStatus.Active);

        _mockHandler.HandleAsync(Arg.Any<FireScheduledJobCommand>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.Run(async () =>
            {
                operationStarted.SetResult(true);
                await cancellationRequested.Task;
                callInfo.Arg<CancellationToken>().ThrowIfCancellationRequested();
                return Result.From(response);
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
