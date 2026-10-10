#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Repositories.MediaLibrary;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.SchedulingBoundedContext.ScheduledJobAggregate;
using Lumina.Domain.Fixtures.Core.BoundedContexts.SchedulingBoundedContext.ScheduledJobAggregate;
using Lumina.Domain.SharedKernel.Common.Enums.Scheduling;
using Lumina.Infrastructure.Core.Scheduling.Execution.TaskExecutors;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Infrastructure.UnitTests.Core.Scheduling.Execution.TaskExecutors;

/// <summary>
/// Contains unit tests for the <see cref="TechnicalDataCleanupTaskExecutor"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class TechnicalDataCleanupTaskExecutorTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly ILibraryScanRepository _mockLibraryScanRepository;
    private readonly TechnicalDataCleanupTaskExecutor _sut;
    private readonly ScheduledJobFixture _scheduledJobFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="TechnicalDataCleanupTaskExecutorTests"/> class.
    /// </summary>
    public TechnicalDataCleanupTaskExecutorTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockLibraryScanRepository = Substitute.For<ILibraryScanRepository>();
        _mockUnitOfWork.LibraryScanRepository.Returns(_mockLibraryScanRepository);
        ILogger<TechnicalDataCleanupTaskExecutor> logger = Substitute.For<ILogger<TechnicalDataCleanupTaskExecutor>>();
        _sut = new TechnicalDataCleanupTaskExecutor(logger, _mockUnitOfWork);
    }

    [Fact]
    public async Task ExecutePayloadAsync_WhenCalled_ShouldFailTheInterruptedScans()
    {
        // Arrange
        ScheduledJob scheduledJob = _scheduledJobFixture.Create(taskType: ScheduledTaskType.TechnicalDataCleanup);
        _mockLibraryScanRepository.FailInterruptedScansAsync(Arg.Any<CancellationToken>()).Returns(Result.Updated);

        // Act
        Result<Success> result = await _sut.ExecutePayloadAsync(scheduledJob, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        await _mockLibraryScanRepository.Received(1).FailInterruptedScansAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecutePayloadAsync_WhenFailingTheInterruptedScansFails_ShouldReturnTheError()
    {
        // Arrange
        ScheduledJob scheduledJob = _scheduledJobFixture.Create(taskType: ScheduledTaskType.TechnicalDataCleanup);
        Error expectedError = Error.Failure("Database.Error", "Failed to fail the interrupted scans");
        _mockLibraryScanRepository.FailInterruptedScansAsync(Arg.Any<CancellationToken>()).Returns(expectedError);

        // Act
        Result<Success> result = await _sut.ExecutePayloadAsync(scheduledJob, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(expectedError, result.FirstError);
    }
}
