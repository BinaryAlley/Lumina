#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Repositories.Scheduling;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Time;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.SchedulingBoundedContext.ScheduledJobAggregate;
using Lumina.Domain.Fixtures.Core.BoundedContexts.SchedulingBoundedContext.ScheduledJobAggregate;
using Lumina.Domain.SharedKernel.Common.Enums.Scheduling;
using Lumina.Infrastructure.Core.Scheduling.Execution.TaskExecutors;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Infrastructure.UnitTests.Core.Scheduling.Execution.TaskExecutors;

/// <summary>
/// Contains unit tests for the <see cref="CleanScheduledJobExecutionHistoryTaskExecutor"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class CleanScheduledJobExecutionHistoryTaskExecutorTests
{
    private readonly IDateTimeProvider _mockDateTimeProvider;
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly IScheduledJobExecutionRepository _mockScheduledJobExecutionRepository;
    private readonly CleanScheduledJobExecutionHistoryTaskExecutor _sut;
    private readonly ScheduledJobFixture _scheduledJobFixture = new();
    private readonly DateTime _fixedUtcNow = DateTime.UtcNow;

    /// <summary>
    /// Initializes a new instance of the <see cref="CleanScheduledJobExecutionHistoryTaskExecutorTests"/> class.
    /// </summary>
    public CleanScheduledJobExecutionHistoryTaskExecutorTests()
    {
        _mockDateTimeProvider = Substitute.For<IDateTimeProvider>();
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockScheduledJobExecutionRepository = Substitute.For<IScheduledJobExecutionRepository>();
        _mockDateTimeProvider.UtcNow.Returns(_fixedUtcNow);
        _mockUnitOfWork.ScheduledJobExecutionRepository.Returns(_mockScheduledJobExecutionRepository);
        ILogger<CleanScheduledJobExecutionHistoryTaskExecutor> logger = Substitute.For<ILogger<CleanScheduledJobExecutionHistoryTaskExecutor>>();
        _sut = new CleanScheduledJobExecutionHistoryTaskExecutor(_mockDateTimeProvider, logger, _mockUnitOfWork);
    }

    [Fact]
    public async Task ExecutePayloadAsync_WhenCalled_ShouldDeleteTheExecutionsOlderThanAMonth()
    {
        // Arrange
        ScheduledJob scheduledJob = _scheduledJobFixture.Create(taskType: ScheduledTaskType.CleanScheduledJobExecutionHistory);
        _mockScheduledJobExecutionRepository.DeleteOlderThanAsync(Arg.Any<DateTime>(), Arg.Any<CancellationToken>()).Returns(Result.Success);
        DateTime expectedCutoff = _fixedUtcNow.AddMonths(-1);

        // Act
        Result<Success> result = await _sut.ExecutePayloadAsync(scheduledJob, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        await _mockScheduledJobExecutionRepository.Received(1).DeleteOlderThanAsync(Arg.Is<DateTime>(cutoff => cutoff == expectedCutoff), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecutePayloadAsync_WhenTheDeletionFails_ShouldReturnTheError()
    {
        // Arrange
        ScheduledJob scheduledJob = _scheduledJobFixture.Create(taskType: ScheduledTaskType.CleanScheduledJobExecutionHistory);
        Error expectedError = Error.Failure("Database.Error", "Failed to delete the old executions");
        _mockScheduledJobExecutionRepository.DeleteOlderThanAsync(Arg.Any<DateTime>(), Arg.Any<CancellationToken>()).Returns(expectedError);

        // Act
        Result<Success> result = await _sut.ExecutePayloadAsync(scheduledJob, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(expectedError, result.FirstError);
    }
}
