#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Scheduling;
using Lumina.Application.Common.DataAccess.Repositories.Scheduling;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Core.Scheduling.Events;
using Lumina.Application.Core.Scheduling.Notifications;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Scheduling;
using Lumina.Contracts.Responses.Scheduling;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Exceptions;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.SchedulingBoundedContext.ScheduledJobAggregate.Events;
using Lumina.Domain.Fixtures.Core.BoundedContexts.SchedulingBoundedContext.ScheduledJobAggregate.Events;
using Lumina.Domain.SharedKernel.Common.Enums.Scheduling;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.UnitTests.Core.Scheduling.Events;

/// <summary>
/// Contains unit tests for the <see cref="ScheduledJobExecutionStartedDomainEventHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ScheduledJobExecutionStartedDomainEventHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly IScheduledJobNotifier _mockScheduledJobNotifier;
    private readonly IScheduledJobRepository _mockScheduledJobRepository;
    private readonly IScheduledJobExecutionRepository _mockScheduledJobExecutionRepository;
    private readonly ScheduledJobExecutionStartedDomainEventHandler _sut;
    private readonly ScheduledJobEntityFixture _scheduledJobEntityFixture = new();
    private readonly ScheduledJobExecutionEntityFixture _scheduledJobExecutionEntityFixture = new();
    private readonly ScheduledJobExecutionStartedDomainEventFixture _scheduledJobExecutionStartedDomainEventFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ScheduledJobExecutionStartedDomainEventHandlerTests"/> class.
    /// </summary>
    public ScheduledJobExecutionStartedDomainEventHandlerTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockScheduledJobNotifier = Substitute.For<IScheduledJobNotifier>();
        _mockScheduledJobRepository = Substitute.For<IScheduledJobRepository>();
        _mockScheduledJobExecutionRepository = Substitute.For<IScheduledJobExecutionRepository>();

        _mockUnitOfWork.ScheduledJobRepository.Returns(_mockScheduledJobRepository);
        _mockUnitOfWork.ScheduledJobExecutionRepository.Returns(_mockScheduledJobExecutionRepository);
        _mockScheduledJobRepository.UpdateAsync(Arg.Any<ScheduledJobEntity>(), Arg.Any<CancellationToken>()).Returns(Result.Updated);
        _mockScheduledJobExecutionRepository.InsertAsync(Arg.Any<ScheduledJobExecutionEntity>(), Arg.Any<CancellationToken>()).Returns(Result.Created);
        _mockScheduledJobRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(Result.From<IEnumerable<ScheduledJobEntity>>([]));

        _sut = new ScheduledJobExecutionStartedDomainEventHandler(_mockScheduledJobNotifier, _mockUnitOfWork);
    }

    [Fact]
    public async Task HandleAsync_WhenScheduledJobExists_ShouldUpdateItsStatusAndInsertTheExecution()
    {
        // Arrange
        DateTime startedOnUtc = DateTime.UtcNow.AddMinutes(-1);
        ScheduledJobExecutionStartedDomainEvent domainEvent = _scheduledJobExecutionStartedDomainEventFixture.Create(startedOnUtc: startedOnUtc);
        ScheduledJobEntity scheduledJob = _scheduledJobEntityFixture.Create(
            id: domainEvent.ScheduledJobId.Value,
            taskType: domainEvent.TaskType,
            scheduleType: ScheduleType.WithIntervalInMinutes,
            status: ScheduledJobStatus.Active);
        _mockScheduledJobRepository.GetByIdAsync(domainEvent.ScheduledJobId.Value, Arg.Any<CancellationToken>())
            .Returns(Result.From<ScheduledJobEntity?>(scheduledJob));

        // Act
        await _sut.HandleAsync(domainEvent, CancellationToken.None);

        // Assert
        await _mockScheduledJobRepository.Received(1).UpdateAsync(
            Arg.Is<ScheduledJobEntity>(updatedScheduledJob =>
                updatedScheduledJob.Status == ScheduledJobStatus.Running &&
                updatedScheduledJob.LastStartedOnUtc == startedOnUtc),
            Arg.Any<CancellationToken>());
        await _mockScheduledJobExecutionRepository.Received(1).InsertAsync(
            Arg.Is<ScheduledJobExecutionEntity>(execution =>
                execution.Id == domainEvent.RunId &&
                execution.ScheduledJobId == scheduledJob.Id &&
                execution.TaskType == domainEvent.TaskType &&
                execution.IsCycleRun == domainEvent.IsCycleRun &&
                execution.StartedOnUtc == startedOnUtc &&
                execution.CompletedOnUtc == null),
            Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _mockScheduledJobNotifier.Received(1).SendScheduledJobsAsync(Arg.Any<IReadOnlyList<ScheduledJobResponse>>(), Arg.Any<CancellationToken>());
        await _mockScheduledJobNotifier.Received(1).SendScheduledJobExecutionStartedAsync(Arg.Any<ScheduledJobExecutionResponse>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenGetByIdReturnsError_ShouldThrowEventualConsistencyException()
    {
        // Arrange
        ScheduledJobExecutionStartedDomainEvent domainEvent = _scheduledJobExecutionStartedDomainEventFixture.Create();
        Error error = Error.Failure("Database.Error", "Failed to get the scheduled job");
        _mockScheduledJobRepository.GetByIdAsync(domainEvent.ScheduledJobId.Value, Arg.Any<CancellationToken>()).Returns(error);

        // Act
        EventualConsistencyException exception = await Assert.ThrowsAsync<EventualConsistencyException>(
            () => _sut.HandleAsync(domainEvent, CancellationToken.None).AsTask());

        // Assert
        Assert.Equal(error, exception.EventualConsistencyError);
        await _mockScheduledJobExecutionRepository.DidNotReceive().InsertAsync(Arg.Any<ScheduledJobExecutionEntity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenScheduledJobDoesNotExist_ShouldThrowEventualConsistencyException()
    {
        // Arrange
        ScheduledJobExecutionStartedDomainEvent domainEvent = _scheduledJobExecutionStartedDomainEventFixture.Create();
        _mockScheduledJobRepository.GetByIdAsync(domainEvent.ScheduledJobId.Value, Arg.Any<CancellationToken>())
            .Returns(Result.From<ScheduledJobEntity?>(null));

        // Act
        EventualConsistencyException exception = await Assert.ThrowsAsync<EventualConsistencyException>(
            () => _sut.HandleAsync(domainEvent, CancellationToken.None).AsTask());

        // Assert
        Assert.Equal(Errors.Scheduling.ScheduledJobNotFound, exception.EventualConsistencyError);
    }

    [Fact]
    public async Task HandleAsync_WhenUpdateFails_ShouldThrowEventualConsistencyException()
    {
        // Arrange
        ScheduledJobExecutionStartedDomainEvent domainEvent = _scheduledJobExecutionStartedDomainEventFixture.Create();
        ScheduledJobEntity scheduledJob = _scheduledJobEntityFixture.Create(
            id: domainEvent.ScheduledJobId.Value,
            status: ScheduledJobStatus.Active);
        _mockScheduledJobRepository.GetByIdAsync(domainEvent.ScheduledJobId.Value, Arg.Any<CancellationToken>())
            .Returns(Result.From<ScheduledJobEntity?>(scheduledJob));
        Error error = Error.Failure("Database.Error", "Failed to update the scheduled job");
        _mockScheduledJobRepository.UpdateAsync(Arg.Any<ScheduledJobEntity>(), Arg.Any<CancellationToken>()).Returns(error);

        // Act
        EventualConsistencyException exception = await Assert.ThrowsAsync<EventualConsistencyException>(
            () => _sut.HandleAsync(domainEvent, CancellationToken.None).AsTask());

        // Assert
        Assert.Equal(error, exception.EventualConsistencyError);
        await _mockScheduledJobExecutionRepository.DidNotReceive().InsertAsync(Arg.Any<ScheduledJobExecutionEntity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenInsertExecutionFails_ShouldThrowEventualConsistencyException()
    {
        // Arrange
        ScheduledJobExecutionStartedDomainEvent domainEvent = _scheduledJobExecutionStartedDomainEventFixture.Create();
        ScheduledJobEntity scheduledJob = _scheduledJobEntityFixture.Create(id: domainEvent.ScheduledJobId.Value);
        _mockScheduledJobRepository.GetByIdAsync(domainEvent.ScheduledJobId.Value, Arg.Any<CancellationToken>())
            .Returns(Result.From<ScheduledJobEntity?>(scheduledJob));
        Error error = Error.Failure("Database.Error", "Failed to insert the execution");
        _mockScheduledJobExecutionRepository.InsertAsync(Arg.Any<ScheduledJobExecutionEntity>(), Arg.Any<CancellationToken>()).Returns(error);

        // Act
        EventualConsistencyException exception = await Assert.ThrowsAsync<EventualConsistencyException>(
            () => _sut.HandleAsync(domainEvent, CancellationToken.None).AsTask());

        // Assert
        Assert.Equal(error, exception.EventualConsistencyError);
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenGetAllScheduledJobsFails_ShouldThrowEventualConsistencyException()
    {
        // Arrange
        ScheduledJobExecutionStartedDomainEvent domainEvent = _scheduledJobExecutionStartedDomainEventFixture.Create();
        ScheduledJobEntity scheduledJob = _scheduledJobEntityFixture.Create(id: domainEvent.ScheduledJobId.Value);
        _mockScheduledJobRepository.GetByIdAsync(domainEvent.ScheduledJobId.Value, Arg.Any<CancellationToken>())
            .Returns(Result.From<ScheduledJobEntity?>(scheduledJob));
        Error error = Error.Failure("Database.Error", "Failed to get the scheduled jobs");
        _mockScheduledJobRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(error);

        // Act
        EventualConsistencyException exception = await Assert.ThrowsAsync<EventualConsistencyException>(
            () => _sut.HandleAsync(domainEvent, CancellationToken.None).AsTask());

        // Assert
        Assert.Equal(error, exception.EventualConsistencyError);
        await _mockScheduledJobNotifier.DidNotReceive().SendScheduledJobExecutionStartedAsync(Arg.Any<ScheduledJobExecutionResponse>(), Arg.Any<CancellationToken>());
    }
}
