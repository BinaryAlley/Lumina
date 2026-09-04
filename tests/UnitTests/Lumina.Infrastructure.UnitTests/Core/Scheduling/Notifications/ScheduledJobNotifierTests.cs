#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Fixtures.Core.Responses.Scheduling;
using Lumina.Contracts.Responses.Scheduling;
using Lumina.Infrastructure.Core.Scheduling.Notifications;
using Microsoft.AspNetCore.SignalR;
using NSubstitute;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Infrastructure.UnitTests.Core.Scheduling.Notifications;

/// <summary>
/// Contains unit tests for the <see cref="ScheduledJobNotifier"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ScheduledJobNotifierTests
{
    private readonly IHubContext<ScheduledJobsHub, IScheduledJobsClient> _mockHubContext;
    private readonly IHubClients<IScheduledJobsClient> _mockHubClients;
    private readonly IScheduledJobsClient _mockClient;
    private readonly ScheduledJobNotifier _sut;
    private readonly ScheduledJobResponseFixture _scheduledJobResponseFixture = new();
    private readonly ScheduledJobExecutionResponseFixture _scheduledJobExecutionResponseFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ScheduledJobNotifierTests"/> class.
    /// </summary>
    public ScheduledJobNotifierTests()
    {
        _mockClient = Substitute.For<IScheduledJobsClient>();
        _mockHubClients = Substitute.For<IHubClients<IScheduledJobsClient>>();
        _mockHubClients.All.Returns(_mockClient);
        _mockHubContext = Substitute.For<IHubContext<ScheduledJobsHub, IScheduledJobsClient>>();
        _mockHubContext.Clients.Returns(_mockHubClients);
        _sut = new ScheduledJobNotifier(_mockHubContext);
    }

    [Fact]
    public async Task SendScheduledJobsAsync_WhenCalled_ShouldSendTheScheduledJobsToAllClients()
    {
        // Arrange
        IReadOnlyList<ScheduledJobResponse> scheduledJobs = _scheduledJobResponseFixture.CreateMany(2);

        // Act
        await _sut.SendScheduledJobsAsync(scheduledJobs, CancellationToken.None);

        // Assert
        await _mockClient.Received(1).ReceiveScheduledJobsAsync(scheduledJobs);
    }

    [Fact]
    public async Task SendScheduledJobExecutionStartedAsync_WhenCalled_ShouldSendTheExecutionToAllClients()
    {
        // Arrange
        ScheduledJobExecutionResponse execution = _scheduledJobExecutionResponseFixture.Create();

        // Act
        await _sut.SendScheduledJobExecutionStartedAsync(execution, CancellationToken.None);

        // Assert
        await _mockClient.Received(1).ScheduledJobExecutionStartedAsync(execution);
    }

    [Fact]
    public async Task SendScheduledJobExecutionCompletedAsync_WhenCalled_ShouldSendTheExecutionToAllClients()
    {
        // Arrange
        ScheduledJobExecutionResponse execution = _scheduledJobExecutionResponseFixture.Create();

        // Act
        await _sut.SendScheduledJobExecutionCompletedAsync(execution, CancellationToken.None);

        // Assert
        await _mockClient.Received(1).ScheduledJobExecutionCompletedAsync(execution);
    }
}
