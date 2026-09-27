#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Scheduling;
using Lumina.Application.Common.DataAccess.Repositories.Scheduling;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.DTO.Filtering;
using Lumina.Application.Common.DTO.Pagination;
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
/// Contains unit tests for the <see cref="ScheduledJobCycleStoppedDomainEventHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ScheduledJobCycleStoppedDomainEventHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly IScheduledJobNotifier _mockScheduledJobNotifier;
    private readonly IScheduledJobRepository _mockScheduledJobRepository;
    private readonly ScheduledJobCycleStoppedDomainEventHandler _sut;
    private readonly ScheduledJobEntityFixture _scheduledJobEntityFixture = new();
    private readonly ScheduledJobCycleStoppedDomainEventFixture _scheduledJobCycleStoppedDomainEventFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ScheduledJobCycleStoppedDomainEventHandlerTests"/> class.
    /// </summary>
    public ScheduledJobCycleStoppedDomainEventHandlerTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Result.Success);
        _mockScheduledJobNotifier = Substitute.For<IScheduledJobNotifier>();
        _mockScheduledJobRepository = Substitute.For<IScheduledJobRepository>();

        _mockUnitOfWork.ScheduledJobRepository.Returns(_mockScheduledJobRepository);
        _mockScheduledJobRepository.GetAllAsync<BaseFilterDto>(cancellationToken: Arg.Any<CancellationToken>()).Returns(Result.From(new PaginatedResultDto<ScheduledJobEntity> { Data = [], CurrentPage = 1, PerPage = 0, Count = 0, NumberOfPages = 1 }));

        _sut = new ScheduledJobCycleStoppedDomainEventHandler(_mockScheduledJobNotifier, _mockUnitOfWork);
    }

    [Fact]
    public async Task HandleAsync_WhenScheduledJobsAreLoaded_ShouldNotifyTheCurrentScheduledJobs()
    {
        // Arrange
        ScheduledJobCycleStoppedDomainEvent domainEvent = _scheduledJobCycleStoppedDomainEventFixture.Create();
        ScheduledJobEntity stoppedJob = _scheduledJobEntityFixture.Create(name: "Stopped job", status: Lumina.Domain.SharedKernel.Common.Enums.Scheduling.ScheduledJobStatus.Added);
        _mockScheduledJobRepository.GetAllAsync<BaseFilterDto>(cancellationToken: Arg.Any<CancellationToken>()).Returns(Result.From(new PaginatedResultDto<ScheduledJobEntity> { Data = [stoppedJob], CurrentPage = 1, PerPage = 1, Count = 1, NumberOfPages = 1 }));

        // Act
        await _sut.HandleAsync(domainEvent, CancellationToken.None);

        // Assert
        await _mockScheduledJobNotifier.Received(1).SendScheduledJobsAsync(
            Arg.Is<IReadOnlyList<ScheduledJobResponse>>(scheduledJobs => scheduledJobs.Count == 1),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenGetAllScheduledJobsFails_ShouldThrowEventualConsistencyException()
    {
        // Arrange
        ScheduledJobCycleStoppedDomainEvent domainEvent = _scheduledJobCycleStoppedDomainEventFixture.Create();
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
