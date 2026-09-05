#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Domain.Common.Errors;
using Lumina.Application.Core.Scheduling.Commands.AddScheduledJob;
using Lumina.Contracts.Fixtures.Core.Requests.Scheduling;
using Lumina.Contracts.Fixtures.Core.Responses.Scheduling;
using Lumina.Contracts.Requests.Scheduling;
using Lumina.Contracts.Responses.Scheduling;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.Scheduling;
using Lumina.Presentation.Api.Core.Endpoints.Scheduling.AddScheduledJob;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.UnitTests.Core.Endpoints.Scheduling.AddScheduledJob;

/// <summary>
/// Contains unit tests for the <see cref="AddScheduledJobEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class AddScheduledJobEndpointTests
{
    private readonly Application.Common.CQRS.ICommandHandler<AddScheduledJobCommand, Result<ScheduledJobResponse>> _mockHandler;
    private readonly AddScheduledJobEndpoint _sut;
    private readonly AddScheduledJobRequestFixture _addScheduledJobRequestFixture = new();
    private readonly ScheduledJobResponseFixture _scheduledJobResponseFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="AddScheduledJobEndpointTests"/> class.
    /// </summary>
    public AddScheduledJobEndpointTests()
    {
        _mockHandler = Substitute.For<Application.Common.CQRS.ICommandHandler<AddScheduledJobCommand, Result<ScheduledJobResponse>>>();
        _sut = Factory.Create<AddScheduledJobEndpoint>(_mockHandler);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSuccessful_ShouldReturnOkResultWithScheduledJobResponse()
    {
        // Arrange
        AddScheduledJobRequest request = _addScheduledJobRequestFixture.Create(name: "Scan job", scheduleType: ScheduleType.WithIntervalInMinutes, intervalMinutes: 60);
        CancellationToken cancellationToken = CancellationToken.None;
        ScheduledJobResponse expectedResponse = _scheduledJobResponseFixture.Create(
            name: request.Name,
            taskType: request.TaskType,
            scheduleType: request.ScheduleType,
            intervalMinutes: request.IntervalMinutes,
            hour: request.Hour,
            minute: request.Minute,
            status: ScheduledJobStatus.Added);
        _mockHandler.HandleAsync(Arg.Any<AddScheduledJobCommand>(), Arg.Any<CancellationToken>())
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
        AddScheduledJobRequest request = _addScheduledJobRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        Error expectedError = Lumina.Domain.Common.Errors.Errors.Scheduling.ScheduledJobNameCannotBeEmpty;
        _mockHandler.HandleAsync(Arg.Any<AddScheduledJobCommand>(), Arg.Any<CancellationToken>())
            .Returns(expectedError);

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        ProblemHttpResult problemResult = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, problemResult.StatusCode);
        Assert.Equal("application/problem+json", problemResult.ContentType);
        Assert.NotNull(problemResult.ProblemDetails.Extensions["traceId"]);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldSendMappedCommandToHandler()
    {
        // Arrange
        AddScheduledJobRequest request = _addScheduledJobRequestFixture.Create(
            name: "Mapped job",
            taskType: ScheduledTaskType.CleanTemporaryFiles,
            scheduleType: ScheduleType.DailyAtHourAndMinute,
            hour: 2,
            minute: 15);
        CancellationToken cancellationToken = CancellationToken.None;
        ScheduledJobResponse response = _scheduledJobResponseFixture.Create(
            name: request.Name,
            taskType: request.TaskType,
            scheduleType: request.ScheduleType,
            hour: request.Hour,
            minute: request.Minute,
            status: ScheduledJobStatus.Added);
        _mockHandler.HandleAsync(Arg.Any<AddScheduledJobCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(response));

        // Act
        await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<AddScheduledJobCommand>(command =>
                command.Name == request.Name &&
                command.TaskType == request.TaskType &&
                command.ScheduleType == request.ScheduleType &&
                command.IntervalMinutes == request.IntervalMinutes &&
                command.Hour == request.Hour &&
                command.Minute == request.Minute),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationRequested_ShouldCancelOperation()
    {
        // Arrange
        AddScheduledJobRequest request = _addScheduledJobRequestFixture.Create();
        CancellationTokenSource cts = new();
        TaskCompletionSource<bool> operationStarted = new();
        TaskCompletionSource<bool> cancellationRequested = new();
        ScheduledJobResponse response = _scheduledJobResponseFixture.Create(
            name: request.Name,
            taskType: request.TaskType,
            scheduleType: request.ScheduleType,
            intervalMinutes: request.IntervalMinutes,
            hour: request.Hour,
            minute: request.Minute,
            status: ScheduledJobStatus.Added);

        _mockHandler.HandleAsync(Arg.Any<AddScheduledJobCommand>(), Arg.Any<CancellationToken>())
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
