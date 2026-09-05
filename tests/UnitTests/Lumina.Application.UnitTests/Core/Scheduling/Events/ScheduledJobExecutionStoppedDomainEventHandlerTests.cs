#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Scheduling;
using Lumina.Application.Common.DataAccess.Repositories.Scheduling;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Core.Scheduling.Events;
using Lumina.Application.Core.Scheduling.Notifications;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Scheduling;
using Lumina.Contracts.Responses.Scheduling;
using Lumina.Domain.Common.Exceptions;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.SchedulingBoundedContext.ScheduledJobAggregate.Events;
using Lumina.Domain.Fixtures.Core.BoundedContexts.SchedulingBoundedContext.ScheduledJobAggregate.Events;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.UnitTests.Core.Scheduling.Events;

/// <summary>
/// Contains unit tests for the <see cref="ScheduledJobExecutionStoppedDomainEventHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ScheduledJobExecutionStoppedDomainEventHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly IScheduledJobNotifier _mockScheduledJobNotifier;
    private readonly IScheduledJobRepository _mockScheduledJobRepository;
    private readonly IScheduledJobExecutionRepository _mockScheduledJobExecutionRepository;
    private readonly ScheduledJobExecutionStoppedDomainEventHandler _sut;
    private readonly ScheduledJobExecutionEntityFixture _scheduledJobExecutionEntityFixture = new();
    private readonly ScheduledJobExecutionStoppedDomainEventFixture _scheduledJobExecutionStoppedDomainEventFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ScheduledJobExecutionStoppedDomainEventHandlerTests"/> class.
    /// </summary>
    public ScheduledJobExecutionStoppedDomainEventHandlerTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockScheduledJobNotifier = Substitute.For<IScheduledJobNotifier>();
        _mockScheduledJobRepository = Substitute.For<IScheduledJobRepository>();
        _mockScheduledJobExecutionRepository = Substitute.For<IScheduledJobExecutionRepository>();

        _mockUnitOfWork.ScheduledJobRepository.Returns(_mockScheduledJobRepository);
        _mockUnitOfWork.ScheduledJobExecutionRepository.Returns(_mockScheduledJobExecutionRepository);
        _mockScheduledJobExecutionRepository.UpdateAsync(Arg.Any<ScheduledJobExecutionEntity>(), Arg.Any<CancellationToken>()).Returns(Result.Updated);
        _mockScheduledJobRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(Result.From<IEnumerable<ScheduledJobEntity>>([]));

        _sut = new ScheduledJobExecutionStoppedDomainEventHandler(_mockScheduledJobNotifier, _mockUnitOfWork);
    }

    [Fact]
    public async Task HandleAsync_WhenOpenExecutionExists_ShouldCloseItAndNotifyScheduledJobs()
    {
        // Arrange
        DateTime occurredOnUtc = DateTime.UtcNow;
        ScheduledJobExecutionStoppedDomainEvent domainEvent = _scheduledJobExecutionStoppedDomainEventFixture.Create(occurredOnUtc: occurredOnUtc);
        ScheduledJobExecutionEntity openExecution = _scheduledJobExecutionEntityFixture.Create(
            scheduledJobId: domainEvent.ScheduledJobId.Value,
            completedOnUtc: null);
        _mockScheduledJobExecutionRepository.GetOpenByScheduledJobIdAsync(domainEvent.ScheduledJobId.Value, Arg.Any<CancellationToken>())
            .Returns(Result.From<ScheduledJobExecutionEntity?>(openExecution));

        // Act
        await _sut.HandleAsync(domainEvent, CancellationToken.None);

        // Assert
        await _mockScheduledJobExecutionRepository.Received(1).UpdateAsync(
            Arg.Is<ScheduledJobExecutionEntity>(updatedExecution => updatedExecution.CompletedOnUtc == occurredOnUtc),
            Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _mockScheduledJobNotifier.Received(1).SendScheduledJobsAsync(Arg.Any<IReadOnlyList<ScheduledJobResponse>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenNoOpenExecutionExists_ShouldOnlyNotifyScheduledJobs()
    {
        // Arrange
        ScheduledJobExecutionStoppedDomainEvent domainEvent = _scheduledJobExecutionStoppedDomainEventFixture.Create();
        _mockScheduledJobExecutionRepository.GetOpenByScheduledJobIdAsync(domainEvent.ScheduledJobId.Value, Arg.Any<CancellationToken>())
            .Returns(Result.From<ScheduledJobExecutionEntity?>(null));

        // Act
        await _sut.HandleAsync(domainEvent, CancellationToken.None);

        // Assert
        await _mockScheduledJobExecutionRepository.DidNotReceive().UpdateAsync(Arg.Any<ScheduledJobExecutionEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        await _mockScheduledJobNotifier.Received(1).SendScheduledJobsAsync(Arg.Any<IReadOnlyList<ScheduledJobResponse>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenGetOpenExecutionFails_ShouldThrowEventualConsistencyException()
    {
        // Arrange
        ScheduledJobExecutionStoppedDomainEvent domainEvent = _scheduledJobExecutionStoppedDomainEventFixture.Create();
        Error error = Error.Failure("Database.Error", "Failed to get the open execution");
        _mockScheduledJobExecutionRepository.GetOpenByScheduledJobIdAsync(domainEvent.ScheduledJobId.Value, Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        EventualConsistencyException exception = await Assert.ThrowsAsync<EventualConsistencyException>(
            () => _sut.HandleAsync(domainEvent, CancellationToken.None).AsTask());

        // Assert
        Assert.Equal(error, exception.EventualConsistencyError);
        await _mockScheduledJobNotifier.DidNotReceive().SendScheduledJobsAsync(Arg.Any<IReadOnlyList<ScheduledJobResponse>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenUpdateExecutionFails_ShouldThrowEventualConsistencyException()
    {
        // Arrange
        ScheduledJobExecutionStoppedDomainEvent domainEvent = _scheduledJobExecutionStoppedDomainEventFixture.Create();
        ScheduledJobExecutionEntity openExecution = _scheduledJobExecutionEntityFixture.Create(
            scheduledJobId: domainEvent.ScheduledJobId.Value,
            completedOnUtc: null);
        _mockScheduledJobExecutionRepository.GetOpenByScheduledJobIdAsync(domainEvent.ScheduledJobId.Value, Arg.Any<CancellationToken>())
            .Returns(Result.From<ScheduledJobExecutionEntity?>(openExecution));
        Error error = Error.Failure("Database.Error", "Failed to update the execution");
        _mockScheduledJobExecutionRepository.UpdateAsync(Arg.Any<ScheduledJobExecutionEntity>(), Arg.Any<CancellationToken>()).Returns(error);

        // Act
        EventualConsistencyException exception = await Assert.ThrowsAsync<EventualConsistencyException>(
            () => _sut.HandleAsync(domainEvent, CancellationToken.None).AsTask());

        // Assert
        Assert.Equal(error, exception.EventualConsistencyError);
        await _mockScheduledJobNotifier.DidNotReceive().SendScheduledJobsAsync(Arg.Any<IReadOnlyList<ScheduledJobResponse>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenGetAllScheduledJobsFails_ShouldThrowEventualConsistencyException()
    {
        // Arrange
        ScheduledJobExecutionStoppedDomainEvent domainEvent = _scheduledJobExecutionStoppedDomainEventFixture.Create();
        _mockScheduledJobExecutionRepository.GetOpenByScheduledJobIdAsync(domainEvent.ScheduledJobId.Value, Arg.Any<CancellationToken>())
            .Returns(Result.From<ScheduledJobExecutionEntity?>(null));
        Error error = Error.Failure("Database.Error", "Failed to get the scheduled jobs");
        _mockScheduledJobRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(error);

        // Act
        EventualConsistencyException exception = await Assert.ThrowsAsync<EventualConsistencyException>(
            () => _sut.HandleAsync(domainEvent, CancellationToken.None).AsTask());

        // Assert
        Assert.Equal(error, exception.EventualConsistencyError);
        await _mockScheduledJobNotifier.DidNotReceive().SendScheduledJobsAsync(Arg.Any<IReadOnlyList<ScheduledJobResponse>>(), Arg.Any<CancellationToken>());
    }
}
