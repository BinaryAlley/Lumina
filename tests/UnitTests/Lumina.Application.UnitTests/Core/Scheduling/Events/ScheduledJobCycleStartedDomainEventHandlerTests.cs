#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Scheduling;
using Lumina.Application.Common.DataAccess.Repositories.Scheduling;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Scheduling;
using Lumina.Application.Core.Scheduling.Events;
using Lumina.Application.Core.Scheduling.Notifications;
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
/// Contains unit tests for the <see cref="ScheduledJobCycleStartedDomainEventHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ScheduledJobCycleStartedDomainEventHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly IScheduledJobNotifier _mockScheduledJobNotifier;
    private readonly IScheduledJobScheduler _mockScheduledJobScheduler;
    private readonly IScheduledJobRepository _mockScheduledJobRepository;
    private readonly ScheduledJobCycleStartedDomainEventHandler _sut;
    private readonly ScheduledJobCycleStartedDomainEventFixture _scheduledJobCycleStartedDomainEventFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ScheduledJobCycleStartedDomainEventHandlerTests"/> class.
    /// </summary>
    public ScheduledJobCycleStartedDomainEventHandlerTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockScheduledJobNotifier = Substitute.For<IScheduledJobNotifier>();
        _mockScheduledJobScheduler = Substitute.For<IScheduledJobScheduler>();
        _mockScheduledJobRepository = Substitute.For<IScheduledJobRepository>();

        _mockUnitOfWork.ScheduledJobRepository.Returns(_mockScheduledJobRepository);
        _mockScheduledJobRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(Result.From<IEnumerable<ScheduledJobEntity>>([]));

        _sut = new ScheduledJobCycleStartedDomainEventHandler(_mockScheduledJobNotifier, _mockScheduledJobScheduler, _mockUnitOfWork);
    }

    [Fact]
    public async Task HandleAsync_WhenScheduledJobsAreLoaded_ShouldStartTheCycleInTheSchedulerAndNotify()
    {
        // Arrange
        ScheduledJobCycleStartedDomainEvent domainEvent = _scheduledJobCycleStartedDomainEventFixture.Create();

        // Act
        await _sut.HandleAsync(domainEvent, CancellationToken.None);

        // Assert
        await _mockScheduledJobScheduler.Received(1).StartCycleAsync(domainEvent.ScheduledJobId, Arg.Any<CancellationToken>());
        await _mockScheduledJobNotifier.Received(1).SendScheduledJobsAsync(Arg.Any<IReadOnlyList<ScheduledJobResponse>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenGetAllScheduledJobsFails_ShouldThrowEventualConsistencyException()
    {
        // Arrange
        ScheduledJobCycleStartedDomainEvent domainEvent = _scheduledJobCycleStartedDomainEventFixture.Create();
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
