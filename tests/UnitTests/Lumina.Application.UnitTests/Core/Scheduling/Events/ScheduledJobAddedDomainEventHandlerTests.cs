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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.UnitTests.Core.Scheduling.Events;

/// <summary>
/// Contains unit tests for the <see cref="ScheduledJobAddedDomainEventHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ScheduledJobAddedDomainEventHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly IScheduledJobNotifier _mockScheduledJobNotifier;
    private readonly IScheduledJobRepository _mockScheduledJobRepository;
    private readonly ScheduledJobAddedDomainEventHandler _sut;
    private readonly ScheduledJobEntityFixture _scheduledJobEntityFixture = new();
    private readonly ScheduledJobAddedDomainEventFixture _scheduledJobAddedDomainEventFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ScheduledJobAddedDomainEventHandlerTests"/> class.
    /// </summary>
    public ScheduledJobAddedDomainEventHandlerTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockScheduledJobNotifier = Substitute.For<IScheduledJobNotifier>();
        _mockScheduledJobRepository = Substitute.For<IScheduledJobRepository>();

        _mockUnitOfWork.ScheduledJobRepository.Returns(_mockScheduledJobRepository);
        _mockScheduledJobRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(Result.From<IEnumerable<ScheduledJobEntity>>([]));

        _sut = new ScheduledJobAddedDomainEventHandler(_mockScheduledJobNotifier, _mockUnitOfWork);
    }

    [Fact]
    public async Task HandleAsync_WhenScheduledJobsAreLoaded_ShouldNotifyTheCurrentScheduledJobs()
    {
        // Arrange
        ScheduledJobAddedDomainEvent domainEvent = _scheduledJobAddedDomainEventFixture.Create();
        ScheduledJobEntity scheduledJob1 = _scheduledJobEntityFixture.Create(name: "Job 1");
        ScheduledJobEntity scheduledJob2 = _scheduledJobEntityFixture.Create(name: "Job 2");
        _mockScheduledJobRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(Result.From<IEnumerable<ScheduledJobEntity>>([scheduledJob1, scheduledJob2]));

        // Act
        await _sut.HandleAsync(domainEvent, CancellationToken.None);

        // Assert
        await _mockScheduledJobNotifier.Received(1).SendScheduledJobsAsync(
            Arg.Is<IReadOnlyList<ScheduledJobResponse>>(scheduledJobs => scheduledJobs.Count == 2),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenGetAllScheduledJobsFails_ShouldThrowEventualConsistencyException()
    {
        // Arrange
        ScheduledJobAddedDomainEvent domainEvent = _scheduledJobAddedDomainEventFixture.Create();
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
