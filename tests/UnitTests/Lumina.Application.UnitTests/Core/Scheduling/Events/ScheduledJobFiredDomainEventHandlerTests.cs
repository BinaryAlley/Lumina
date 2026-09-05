#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Scheduling;
using Lumina.Application.Core.Scheduling.Events;
using Lumina.Domain.Core.BoundedContexts.SchedulingBoundedContext.ScheduledJobAggregate.Events;
using Lumina.Domain.Fixtures.Core.BoundedContexts.SchedulingBoundedContext.ScheduledJobAggregate.Events;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.UnitTests.Core.Scheduling.Events;

/// <summary>
/// Contains unit tests for the <see cref="ScheduledJobFiredDomainEventHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ScheduledJobFiredDomainEventHandlerTests
{
    private readonly IScheduledJobScheduler _mockScheduledJobScheduler;
    private readonly ScheduledJobFiredDomainEventHandler _sut;
    private readonly ScheduledJobFiredDomainEventFixture _scheduledJobFiredDomainEventFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ScheduledJobFiredDomainEventHandlerTests"/> class.
    /// </summary>
    public ScheduledJobFiredDomainEventHandlerTests()
    {
        _mockScheduledJobScheduler = Substitute.For<IScheduledJobScheduler>();
        _sut = new ScheduledJobFiredDomainEventHandler(_mockScheduledJobScheduler);
    }

    [Fact]
    public async Task HandleAsync_WhenFiredEventIsHandled_ShouldRunTheTaskOnceInTheScheduler()
    {
        // Arrange
        ScheduledJobFiredDomainEvent domainEvent = _scheduledJobFiredDomainEventFixture.Create();

        // Act
        await _sut.HandleAsync(domainEvent, CancellationToken.None);

        // Assert
        await _mockScheduledJobScheduler.Received(1).RunOnceAsync(domainEvent.ScheduledJobId, Arg.Any<CancellationToken>());
    }
}
