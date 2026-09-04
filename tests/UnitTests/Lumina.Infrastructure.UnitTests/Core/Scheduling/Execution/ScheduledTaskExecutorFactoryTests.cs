#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Domain.Common.Events;
using Lumina.Domain.SharedKernel.Common.Enums.Scheduling;
using Lumina.Infrastructure.Core.Scheduling.Execution;
using Lumina.Infrastructure.Core.Scheduling.Execution.TaskExecutors;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Infrastructure.UnitTests.Core.Scheduling.Execution;

/// <summary>
/// Contains unit tests for the <see cref="ScheduledTaskExecutorFactory"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ScheduledTaskExecutorFactoryTests
{
    private readonly IServiceProvider _mockServiceProvider;
    private readonly MediaLibraryScanTaskExecutor _mockMediaLibraryScanTaskExecutor;
    private readonly TemporaryFilesCleanupTaskExecutor _mockTemporaryFilesCleanupTaskExecutor;
    private readonly ScheduledTaskExecutorFactory _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="ScheduledTaskExecutorFactoryTests"/> class.
    /// </summary>
    public ScheduledTaskExecutorFactoryTests()
    {
        _mockServiceProvider = Substitute.For<IServiceProvider>();
        _mockMediaLibraryScanTaskExecutor = Substitute.For<MediaLibraryScanTaskExecutor>(
            Substitute.For<IDomainEventPublisher>(),
            Substitute.For<ILogger<MediaLibraryScanTaskExecutor>>(),
            Substitute.For<IUnitOfWork>());
        _mockTemporaryFilesCleanupTaskExecutor = Substitute.For<TemporaryFilesCleanupTaskExecutor>(
            Substitute.For<ILogger<TemporaryFilesCleanupTaskExecutor>>());

        _mockServiceProvider.GetService(typeof(MediaLibraryScanTaskExecutor)).Returns(_mockMediaLibraryScanTaskExecutor);
        _mockServiceProvider.GetService(typeof(TemporaryFilesCleanupTaskExecutor)).Returns(_mockTemporaryFilesCleanupTaskExecutor);

        _sut = new ScheduledTaskExecutorFactory(_mockServiceProvider);
    }

    [Fact]
    public void CreateExecutor_WhenTaskTypeIsScanMediaLibraries_ShouldReturnMediaLibraryScanTaskExecutor()
    {
        // Act
        IScheduledTaskExecutor result = _sut.CreateExecutor(ScheduledTaskType.ScanMediaLibraries);

        // Assert
        Assert.Same(_mockMediaLibraryScanTaskExecutor, result);
    }

    [Fact]
    public void CreateExecutor_WhenTaskTypeIsCleanTemporaryFiles_ShouldReturnTemporaryFilesCleanupTaskExecutor()
    {
        // Act
        IScheduledTaskExecutor result = _sut.CreateExecutor(ScheduledTaskType.CleanTemporaryFiles);

        // Assert
        Assert.Same(_mockTemporaryFilesCleanupTaskExecutor, result);
    }

    [Fact]
    public void CreateExecutor_WhenTaskTypeIsUnsupported_ShouldThrowArgumentException()
    {
        // Act
        ArgumentException exception = Assert.Throws<ArgumentException>(() => _sut.CreateExecutor((ScheduledTaskType)999));

        // Assert
        Assert.Contains("Unsupported task type", exception.Message);
        Assert.Equal("taskType", exception.ParamName);
    }
}
