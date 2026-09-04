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
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.UnitTests.Core.Scheduling.Events;

/// <summary>
/// Contains unit tests for the <see cref="ScheduledJobExecutionCompletedDomainEventHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ScheduledJobExecutionCompletedDomainEventHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly IScheduledJobNotifier _mockScheduledJobNotifier;
    private readonly IScheduledJobRepository _mockScheduledJobRepository;
    private readonly IScheduledJobExecutionRepository _mockScheduledJobExecutionRepository;
    private readonly ScheduledJobExecutionCompletedDomainEventHandler _sut;
    private readonly ScheduledJobEntityFixture _scheduledJobEntityFixture = new();
    private readonly ScheduledJobExecutionEntityFixture _scheduledJobExecutionEntityFixture = new();
    private readonly ScheduledJobExecutionCompletedDomainEventFixture _scheduledJobExecutionCompletedDomainEventFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ScheduledJobExecutionCompletedDomainEventHandlerTests"/> class.
    /// </summary>
    public ScheduledJobExecutionCompletedDomainEventHandlerTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockScheduledJobNotifier = Substitute.For<IScheduledJobNotifier>();
        _mockScheduledJobRepository = Substitute.For<IScheduledJobRepository>();
        _mockScheduledJobExecutionRepository = Substitute.For<IScheduledJobExecutionRepository>();

        _mockUnitOfWork.ScheduledJobRepository.Returns(_mockScheduledJobRepository);
        _mockUnitOfWork.ScheduledJobExecutionRepository.Returns(_mockScheduledJobExecutionRepository);
        _mockScheduledJobRepository.UpdateAsync(Arg.Any<ScheduledJobEntity>(), Arg.Any<CancellationToken>()).Returns(Result.Updated);
        _mockScheduledJobExecutionRepository.UpdateAsync(Arg.Any<ScheduledJobExecutionEntity>(), Arg.Any<CancellationToken>()).Returns(Result.Updated);
        _mockScheduledJobRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(Result.From<IEnumerable<ScheduledJobEntity>>([]));

        _sut = new ScheduledJobExecutionCompletedDomainEventHandler(_mockScheduledJobNotifier, _mockUnitOfWork);
    }

    [Theory]
    [InlineData(true, ScheduledJobStatus.Active)] // a cycle run returns the scheduled job to its active status
    [InlineData(false, ScheduledJobStatus.Completed)] // a one time execution completes the scheduled job
    public async Task HandleAsync_WhenScheduledJobAndExecutionExist_ShouldUpdateBothAndNotify(bool isCycleRun, ScheduledJobStatus expectedStatus)
    {
        // Arrange
        DateTime completedOnUtc = DateTime.UtcNow;
        ScheduledJobExecutionCompletedDomainEvent domainEvent = _scheduledJobExecutionCompletedDomainEventFixture.Create(isCycleRun: isCycleRun, completedOnUtc: completedOnUtc);
        ScheduledJobEntity scheduledJob = _scheduledJobEntityFixture.Create(
            id: domainEvent.ScheduledJobId.Value,
            status: ScheduledJobStatus.Running,
            lastStartedOnUtc: completedOnUtc.AddMinutes(-5));
        _mockScheduledJobRepository.GetByIdAsync(domainEvent.ScheduledJobId.Value, Arg.Any<CancellationToken>())
            .Returns(Result.From<ScheduledJobEntity?>(scheduledJob));
        ScheduledJobExecutionEntity execution = _scheduledJobExecutionEntityFixture.Create(
            id: domainEvent.RunId,
            scheduledJobId: scheduledJob.Id,
            isCycleRun: isCycleRun,
            completedOnUtc: null);
        _mockScheduledJobExecutionRepository.GetByIdAsync(domainEvent.RunId, Arg.Any<CancellationToken>())
            .Returns(Result.From<ScheduledJobExecutionEntity?>(execution));

        // Act
        await _sut.HandleAsync(domainEvent, CancellationToken.None);

        // Assert
        await _mockScheduledJobRepository.Received(1).UpdateAsync(
            Arg.Is<ScheduledJobEntity>(updatedScheduledJob =>
                updatedScheduledJob.Status == expectedStatus &&
                updatedScheduledJob.LastCompletedOnUtc == completedOnUtc),
            Arg.Any<CancellationToken>());
        await _mockScheduledJobExecutionRepository.Received(1).UpdateAsync(
            Arg.Is<ScheduledJobExecutionEntity>(updatedExecution => updatedExecution.CompletedOnUtc == completedOnUtc),
            Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _mockScheduledJobNotifier.Received(1).SendScheduledJobsAsync(Arg.Any<IReadOnlyList<ScheduledJobResponse>>(), Arg.Any<CancellationToken>());
        await _mockScheduledJobNotifier.Received(1).SendScheduledJobExecutionCompletedAsync(Arg.Any<ScheduledJobExecutionResponse>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenScheduledJobDoesNotExist_ShouldThrowEventualConsistencyException()
    {
        // Arrange
        ScheduledJobExecutionCompletedDomainEvent domainEvent = _scheduledJobExecutionCompletedDomainEventFixture.Create(isCycleRun: true);
        _mockScheduledJobRepository.GetByIdAsync(domainEvent.ScheduledJobId.Value, Arg.Any<CancellationToken>())
            .Returns(Result.From<ScheduledJobEntity?>(null));

        // Act
        EventualConsistencyException exception = await Assert.ThrowsAsync<EventualConsistencyException>(
            () => _sut.HandleAsync(domainEvent, CancellationToken.None).AsTask());

        // Assert
        Assert.Equal(Errors.Scheduling.ScheduledJobNotFound, exception.EventualConsistencyError);
        await _mockScheduledJobExecutionRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenUpdateScheduledJobFails_ShouldThrowEventualConsistencyException()
    {
        // Arrange
        ScheduledJobExecutionCompletedDomainEvent domainEvent = _scheduledJobExecutionCompletedDomainEventFixture.Create(isCycleRun: true);
        ScheduledJobEntity scheduledJob = _scheduledJobEntityFixture.Create(id: domainEvent.ScheduledJobId.Value, status: ScheduledJobStatus.Running);
        _mockScheduledJobRepository.GetByIdAsync(domainEvent.ScheduledJobId.Value, Arg.Any<CancellationToken>())
            .Returns(Result.From<ScheduledJobEntity?>(scheduledJob));
        Error error = Error.Failure("Database.Error", "Failed to update the scheduled job");
        _mockScheduledJobRepository.UpdateAsync(Arg.Any<ScheduledJobEntity>(), Arg.Any<CancellationToken>()).Returns(error);

        // Act
        EventualConsistencyException exception = await Assert.ThrowsAsync<EventualConsistencyException>(
            () => _sut.HandleAsync(domainEvent, CancellationToken.None).AsTask());

        // Assert
        Assert.Equal(error, exception.EventualConsistencyError);
        await _mockScheduledJobExecutionRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenExecutionDoesNotExist_ShouldThrowEventualConsistencyException()
    {
        // Arrange
        ScheduledJobExecutionCompletedDomainEvent domainEvent = _scheduledJobExecutionCompletedDomainEventFixture.Create(isCycleRun: true);
        ScheduledJobEntity scheduledJob = _scheduledJobEntityFixture.Create(id: domainEvent.ScheduledJobId.Value, status: ScheduledJobStatus.Running);
        _mockScheduledJobRepository.GetByIdAsync(domainEvent.ScheduledJobId.Value, Arg.Any<CancellationToken>())
            .Returns(Result.From<ScheduledJobEntity?>(scheduledJob));
        _mockScheduledJobExecutionRepository.GetByIdAsync(domainEvent.RunId, Arg.Any<CancellationToken>())
            .Returns(Result.From<ScheduledJobExecutionEntity?>(null));

        // Act
        EventualConsistencyException exception = await Assert.ThrowsAsync<EventualConsistencyException>(
            () => _sut.HandleAsync(domainEvent, CancellationToken.None).AsTask());

        // Assert
        Assert.Equal(Errors.Scheduling.ScheduledJobExecutionNotFound, exception.EventualConsistencyError);
        await _mockScheduledJobExecutionRepository.DidNotReceive().UpdateAsync(Arg.Any<ScheduledJobExecutionEntity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenUpdateExecutionFails_ShouldThrowEventualConsistencyException()
    {
        // Arrange
        ScheduledJobExecutionCompletedDomainEvent domainEvent = _scheduledJobExecutionCompletedDomainEventFixture.Create(isCycleRun: true);
        ScheduledJobEntity scheduledJob = _scheduledJobEntityFixture.Create(id: domainEvent.ScheduledJobId.Value, status: ScheduledJobStatus.Running);
        _mockScheduledJobRepository.GetByIdAsync(domainEvent.ScheduledJobId.Value, Arg.Any<CancellationToken>())
            .Returns(Result.From<ScheduledJobEntity?>(scheduledJob));
        ScheduledJobExecutionEntity execution = _scheduledJobExecutionEntityFixture.Create(id: domainEvent.RunId);
        _mockScheduledJobExecutionRepository.GetByIdAsync(domainEvent.RunId, Arg.Any<CancellationToken>())
            .Returns(Result.From<ScheduledJobExecutionEntity?>(execution));
        Error error = Error.Failure("Database.Error", "Failed to update the execution");
        _mockScheduledJobExecutionRepository.UpdateAsync(Arg.Any<ScheduledJobExecutionEntity>(), Arg.Any<CancellationToken>()).Returns(error);

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
        ScheduledJobExecutionCompletedDomainEvent domainEvent = _scheduledJobExecutionCompletedDomainEventFixture.Create(isCycleRun: true);
        ScheduledJobEntity scheduledJob = _scheduledJobEntityFixture.Create(id: domainEvent.ScheduledJobId.Value, status: ScheduledJobStatus.Running);
        _mockScheduledJobRepository.GetByIdAsync(domainEvent.ScheduledJobId.Value, Arg.Any<CancellationToken>())
            .Returns(Result.From<ScheduledJobEntity?>(scheduledJob));
        ScheduledJobExecutionEntity execution = _scheduledJobExecutionEntityFixture.Create(id: domainEvent.RunId);
        _mockScheduledJobExecutionRepository.GetByIdAsync(domainEvent.RunId, Arg.Any<CancellationToken>())
            .Returns(Result.From<ScheduledJobExecutionEntity?>(execution));
        Error error = Error.Failure("Database.Error", "Failed to get the scheduled jobs");
        _mockScheduledJobRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(error);

        // Act
        EventualConsistencyException exception = await Assert.ThrowsAsync<EventualConsistencyException>(
            () => _sut.HandleAsync(domainEvent, CancellationToken.None).AsTask());

        // Assert
        Assert.Equal(error, exception.EventualConsistencyError);
        await _mockScheduledJobNotifier.DidNotReceive().SendScheduledJobExecutionCompletedAsync(Arg.Any<ScheduledJobExecutionResponse>(), Arg.Any<CancellationToken>());
    }
}
