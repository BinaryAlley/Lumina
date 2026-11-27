#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.DataAccess.Entities.Scheduling;
using Nemera.Application.Common.DataAccess.Repositories.Scheduling;
using Nemera.Application.Common.DataAccess.UoW;
using Nemera.Application.Common.DTO.Filtering;
using Nemera.Application.Common.DTO.Pagination;
using Nemera.Application.Common.Infrastructure.Scheduling;
using Nemera.Application.Core.Scheduling.Events;
using Nemera.Application.Core.Scheduling.Notifications;
using Nemera.Application.Fixtures.Common.DTO.Pagination;
using Nemera.Contracts.Responses.Scheduling;
using Nemera.Domain.Common.Exceptions;
using Nemera.Domain.Common.Primitives;
using Nemera.Domain.Core.BoundedContexts.SchedulingBoundedContext.ScheduledJobAggregate.Events;
using Nemera.Domain.Fixtures.Core.BoundedContexts.SchedulingBoundedContext.ScheduledJobAggregate.Events;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Nemera.Application.UnitTests.Core.Scheduling.Events;

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
    private readonly PaginatedResultDtoFixture<ScheduledJobEntity> _paginatedResultDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ScheduledJobCycleStartedDomainEventHandlerTests"/> class.
    /// </summary>
    public ScheduledJobCycleStartedDomainEventHandlerTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Result.Success);
        _mockScheduledJobNotifier = Substitute.For<IScheduledJobNotifier>();
        _mockScheduledJobScheduler = Substitute.For<IScheduledJobScheduler>();
        _mockScheduledJobRepository = Substitute.For<IScheduledJobRepository>();

        _mockUnitOfWork.ScheduledJobRepository.Returns(_mockScheduledJobRepository);
        _mockScheduledJobRepository.GetAllAsync<BaseFilterDto>(cancellationToken: Arg.Any<CancellationToken>()).Returns(Result.From(_paginatedResultDtoFixture.Create(data: [], currentPage: 1, perPage: 0, count: 0, numberOfPages: 1)));

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
        _mockScheduledJobRepository.GetAllAsync<BaseFilterDto>(cancellationToken: Arg.Any<CancellationToken>()).Returns(error);

        // Act
        EventualConsistencyException exception = await Assert.ThrowsAsync<EventualConsistencyException>(
            () => _sut.HandleAsync(domainEvent, CancellationToken.None).AsTask());

        // Assert
        Assert.Equal(error, exception.EventualConsistencyError);
        await _mockScheduledJobNotifier.DidNotReceive().SendScheduledJobsAsync(Arg.Any<IReadOnlyList<ScheduledJobResponse>>(), Arg.Any<CancellationToken>());
    }
}
