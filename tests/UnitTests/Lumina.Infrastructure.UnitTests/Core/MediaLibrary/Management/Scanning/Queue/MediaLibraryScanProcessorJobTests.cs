#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.Repositories.MediaLibrary;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Domain.Common.Events;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Events;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Cancellation;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Jobs;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Jobs;
using Lumina.Domain.Fixtures.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Infrastructure.Core.MediaLibrary.Management.Scanning.Queue;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Infrastructure.UnitTests.Core.MediaLibrary.Management.Scanning.Queue;

/// <summary>
/// Contains unit tests for the <see cref="MediaLibraryScanProcessorJob"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MediaLibraryScanProcessorJobTests : IDisposable
{
    private static readonly TimeSpan s_testTimeout = TimeSpan.FromSeconds(5);
    private readonly MediaLibrariesScanQueue _queue;
    private readonly IMediaLibrariesScanCancellationTracker _mockCancellationTracker;
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly ILibraryScanRepository _mockLibraryScanRepository;
    private readonly IMediaLibraryScanningService _mockMediaLibraryScanningService;
    private readonly IDomainEventPublisher _mockDomainEventPublisher;
    private readonly ILogger<MediaLibraryScanProcessorJob> _mockLogger;
    private readonly ServiceProvider _serviceProvider;
    private readonly MediaLibraryScanProcessorJob _sut;
    private readonly LibraryScanEntityFixture _libraryScanEntityFixture = new();
    private readonly MediaLibraryScanCompositeIdFixture _mediaLibraryScanCompositeIdFixture = new();
    private readonly MediaLibraryScanDiscoveryJobFixture _mediaLibraryScanDiscoveryJobFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="MediaLibraryScanProcessorJobTests"/> class.
    /// </summary>
    public MediaLibraryScanProcessorJobTests()
    {
        _queue = new MediaLibrariesScanQueue();
        _mockCancellationTracker = Substitute.For<IMediaLibrariesScanCancellationTracker>();
        _mockCancellationTracker.GetTokenForScan(Arg.Any<MediaLibraryScanCompositeId>()).Returns(CancellationToken.None);
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockLibraryScanRepository = Substitute.For<ILibraryScanRepository>();
        _mockUnitOfWork.LibraryScanRepository.Returns(_mockLibraryScanRepository);
        _mockMediaLibraryScanningService = Substitute.For<IMediaLibraryScanningService>();
        _mockDomainEventPublisher = Substitute.For<IDomainEventPublisher>();
        _mockLogger = Substitute.For<ILogger<MediaLibraryScanProcessorJob>>();

        ServiceCollection services = new();
        services.AddSingleton(_mockUnitOfWork);
        services.AddSingleton(_mockMediaLibraryScanningService);
        services.AddSingleton(_mockDomainEventPublisher);
        _serviceProvider = services.BuildServiceProvider();

        _sut = new MediaLibraryScanProcessorJob(
            _queue,
            _mockCancellationTracker,
            _serviceProvider.GetRequiredService<IServiceScopeFactory>(),
            _mockLogger);
    }

    [Fact]
    public async Task ExecuteAsync_WhenScansAreQueued_ShouldProcessThemOneAtATimeInOrder()
    {
        // Arrange
        MediaLibraryScanCompositeId firstScanId = _mediaLibraryScanCompositeIdFixture.Create();
        MediaLibraryScanCompositeId secondScanId = _mediaLibraryScanCompositeIdFixture.Create();
        RegisterScan(firstScanId, LibraryScanJobStatus.Pending);
        RegisterScan(secondScanId, LibraryScanJobStatus.Pending);
        TaskCompletionSource<bool> releaseFirstScan = new(TaskCreationOptions.RunContinuationsAsynchronously);
        List<Guid> startedScanIds = [];
        FakeMediaLibraryScanDiscoveryJob firstRootJob = _mediaLibraryScanDiscoveryJobFixture.Create(onExecute: async _ =>
        {
            startedScanIds.Add(firstScanId.ScanId.Value);
            await releaseFirstScan.Task;
        });
        FakeMediaLibraryScanDiscoveryJob secondRootJob = _mediaLibraryScanDiscoveryJobFixture.Create(onExecute: _ =>
        {
            startedScanIds.Add(secondScanId.ScanId.Value);
            return Task.CompletedTask;
        });
        RegisterRootJob(firstScanId, firstRootJob);
        RegisterRootJob(secondScanId, secondRootJob);
        await _sut.StartAsync(CancellationToken.None);

        // Act
        _queue.Writer.TryWrite(firstScanId);
        _queue.Writer.TryWrite(secondScanId);
        await firstRootJob.ExecutionStarted.WaitAsync(s_testTimeout);

        // Assert
        // The second scan must not start while the job graph of the first one is still executing.
        Assert.Equal([firstScanId.ScanId.Value], startedScanIds);
        Assert.Equal(0, secondRootJob.ExecutionCount);

        releaseFirstScan.SetResult(true);
        await secondRootJob.ExecutionCompleted.WaitAsync(s_testTimeout);
        Assert.Equal([firstScanId.ScanId.Value, secondScanId.ScanId.Value], startedScanIds);
        Assert.Equal(1, firstRootJob.ExecutionCount);
        await _sut.StopAsync(CancellationToken.None);
    }

    [Fact]
    public async Task ExecuteAsync_WhenScanIsNotPending_ShouldSkipItAndProcessTheNextScan()
    {
        // Arrange
        MediaLibraryScanCompositeId completedScanId = _mediaLibraryScanCompositeIdFixture.Create();
        MediaLibraryScanCompositeId pendingScanId = _mediaLibraryScanCompositeIdFixture.Create();
        RegisterScan(completedScanId, LibraryScanJobStatus.Completed);
        RegisterScan(pendingScanId, LibraryScanJobStatus.Pending);
        FakeMediaLibraryScanDiscoveryJob pendingRootJob = _mediaLibraryScanDiscoveryJobFixture.Create();
        RegisterRootJob(pendingScanId, pendingRootJob);
        await _sut.StartAsync(CancellationToken.None);

        // Act
        _queue.Writer.TryWrite(completedScanId);
        _queue.Writer.TryWrite(pendingScanId);
        await pendingRootJob.ExecutionCompleted.WaitAsync(s_testTimeout);

        // Assert
        // A scan that was cancelled or otherwise finished while it was waiting for its turn must not be started.
        await _mockMediaLibraryScanningService.DidNotReceive().StartScanAsync(
            Arg.Is<LibraryScan>(libraryScan => libraryScan.Id.Value == completedScanId.ScanId.Value),
            Arg.Any<LibraryType>(),
            Arg.Any<CancellationToken>());
        await _mockMediaLibraryScanningService.Received(1).StartScanAsync(
            Arg.Is<LibraryScan>(libraryScan => libraryScan.Id.Value == pendingScanId.ScanId.Value),
            Arg.Any<LibraryType>(),
            Arg.Any<CancellationToken>());
        await _mockDomainEventPublisher.DidNotReceive().PublishAsync(Arg.Any<IDomainEvent>(), Arg.Any<CancellationToken>());
        await _sut.StopAsync(CancellationToken.None);
    }

    [Fact]
    public async Task ExecuteAsync_WhenScanTokenIsAlreadyCancelled_ShouldSkipItAndProcessTheNextScan()
    {
        // Arrange
        MediaLibraryScanCompositeId cancelledScanId = _mediaLibraryScanCompositeIdFixture.Create();
        MediaLibraryScanCompositeId pendingScanId = _mediaLibraryScanCompositeIdFixture.Create();
        RegisterScan(cancelledScanId, LibraryScanJobStatus.Pending);
        RegisterScan(pendingScanId, LibraryScanJobStatus.Pending);
        using CancellationTokenSource cancelledTokenSource = new();
        cancelledTokenSource.Cancel();
        _mockCancellationTracker.GetTokenForScan(cancelledScanId).Returns(cancelledTokenSource.Token);
        FakeMediaLibraryScanDiscoveryJob pendingRootJob = _mediaLibraryScanDiscoveryJobFixture.Create();
        RegisterRootJob(pendingScanId, pendingRootJob);
        await _sut.StartAsync(CancellationToken.None);

        // Act
        _queue.Writer.TryWrite(cancelledScanId);
        _queue.Writer.TryWrite(pendingScanId);
        await pendingRootJob.ExecutionCompleted.WaitAsync(s_testTimeout);

        // Assert
        // A scan that was cancelled while it was waiting for its turn is not even loaded from the repository.
        await _mockLibraryScanRepository.DidNotReceive().GetByIdAsync(
            cancelledScanId.ScanId.Value,
            Arg.Any<bool>(),
            false,
            Arg.Any<CancellationToken>());
        await _mockMediaLibraryScanningService.DidNotReceive().StartScanAsync(
            Arg.Is<LibraryScan>(libraryScan => libraryScan.Id.Value == cancelledScanId.ScanId.Value),
            Arg.Any<LibraryType>(),
            Arg.Any<CancellationToken>());
        await _mockMediaLibraryScanningService.Received(1).StartScanAsync(
            Arg.Is<LibraryScan>(libraryScan => libraryScan.Id.Value == pendingScanId.ScanId.Value),
            Arg.Any<LibraryType>(),
            Arg.Any<CancellationToken>());
        await _sut.StopAsync(CancellationToken.None);
    }

    [Fact]
    public async Task ExecuteAsync_WhenPreparingAScanFails_ShouldReportTheFailureForThatScanAndProcessTheNextScan()
    {
        // Arrange
        MediaLibraryScanCompositeId failingScanId = _mediaLibraryScanCompositeIdFixture.Create();
        MediaLibraryScanCompositeId pendingScanId = _mediaLibraryScanCompositeIdFixture.Create();
        RegisterScan(failingScanId, LibraryScanJobStatus.Pending);
        RegisterScan(pendingScanId, LibraryScanJobStatus.Pending);
        Error error = Error.Failure(description: "Failed to prepare the media library scan");
        _mockMediaLibraryScanningService.StartScanAsync(
            Arg.Is<LibraryScan>(libraryScan => libraryScan.Id.Value == failingScanId.ScanId.Value),
            Arg.Any<LibraryType>(),
            Arg.Any<CancellationToken>())
            .Returns(error);
        FakeMediaLibraryScanDiscoveryJob pendingRootJob = _mediaLibraryScanDiscoveryJobFixture.Create();
        RegisterRootJob(pendingScanId, pendingRootJob);
        await _sut.StartAsync(CancellationToken.None);

        // Act
        _queue.Writer.TryWrite(failingScanId);
        _queue.Writer.TryWrite(pendingScanId);
        await pendingRootJob.ExecutionCompleted.WaitAsync(s_testTimeout);

        // Assert
        await _mockDomainEventPublisher.Received(1).PublishAsync(
            Arg.Is<IDomainEvent>(domainEvent => domainEvent.GetType() == typeof(LibraryScanFailedDomainEvent)
                && ((LibraryScanFailedDomainEvent)domainEvent).MediaLibraryScanCompositeId == failingScanId),
            Arg.Any<CancellationToken>());
        await _mockDomainEventPublisher.Received(1).PublishAsync(Arg.Any<IDomainEvent>(), Arg.Any<CancellationToken>());
        Assert.Equal(1, pendingRootJob.ExecutionCount);
        await _sut.StopAsync(CancellationToken.None);
    }

    [Fact]
    public async Task ExecuteAsync_WhenExecutingAScanIsCancelled_ShouldNotStopProcessingTheNextScan()
    {
        // Arrange
        MediaLibraryScanCompositeId cancelledScanId = _mediaLibraryScanCompositeIdFixture.Create();
        MediaLibraryScanCompositeId pendingScanId = _mediaLibraryScanCompositeIdFixture.Create();
        RegisterScan(cancelledScanId, LibraryScanJobStatus.Pending);
        RegisterScan(pendingScanId, LibraryScanJobStatus.Pending);
        FakeMediaLibraryScanDiscoveryJob cancelledRootJob = _mediaLibraryScanDiscoveryJobFixture.Create(
            onExecute: _ => throw new OperationCanceledException());
        FakeMediaLibraryScanDiscoveryJob pendingRootJob = _mediaLibraryScanDiscoveryJobFixture.Create();
        RegisterRootJob(cancelledScanId, cancelledRootJob);
        RegisterRootJob(pendingScanId, pendingRootJob);
        await _sut.StartAsync(CancellationToken.None);

        // Act
        _queue.Writer.TryWrite(cancelledScanId);
        _queue.Writer.TryWrite(pendingScanId);
        await pendingRootJob.ExecutionCompleted.WaitAsync(s_testTimeout);

        // Assert
        Assert.Equal(1, cancelledRootJob.ExecutionCount);
        Assert.Equal(1, pendingRootJob.ExecutionCount);
        // A cancelled scan was already reported by its own cancellation event, so no failure is published for it.
        await _mockDomainEventPublisher.DidNotReceive().PublishAsync(Arg.Any<IDomainEvent>(), Arg.Any<CancellationToken>());
        await _sut.StopAsync(CancellationToken.None);
    }

    [Fact]
    public async Task ExecuteAsync_WhenExecutingAScanFails_ShouldReportTheFailureForThatScanAndProcessTheNextScan()
    {
        // Arrange
        MediaLibraryScanCompositeId failingScanId = _mediaLibraryScanCompositeIdFixture.Create();
        MediaLibraryScanCompositeId pendingScanId = _mediaLibraryScanCompositeIdFixture.Create();
        RegisterScan(failingScanId, LibraryScanJobStatus.Pending);
        RegisterScan(pendingScanId, LibraryScanJobStatus.Pending);
        FakeMediaLibraryScanDiscoveryJob failingRootJob = _mediaLibraryScanDiscoveryJobFixture.Create(
            onExecute: _ => throw new InvalidOperationException("The scan job payload failed"));
        FakeMediaLibraryScanDiscoveryJob pendingRootJob = _mediaLibraryScanDiscoveryJobFixture.Create();
        RegisterRootJob(failingScanId, failingRootJob);
        RegisterRootJob(pendingScanId, pendingRootJob);
        await _sut.StartAsync(CancellationToken.None);

        // Act
        _queue.Writer.TryWrite(failingScanId);
        _queue.Writer.TryWrite(pendingScanId);
        await pendingRootJob.ExecutionCompleted.WaitAsync(s_testTimeout);

        // Assert
        await _mockDomainEventPublisher.Received(1).PublishAsync(
            Arg.Is<IDomainEvent>(domainEvent => domainEvent.GetType() == typeof(LibraryScanFailedDomainEvent)
                && ((LibraryScanFailedDomainEvent)domainEvent).MediaLibraryScanCompositeId == failingScanId),
            Arg.Any<CancellationToken>());
        Assert.Equal(1, pendingRootJob.ExecutionCount);
        await _sut.StopAsync(CancellationToken.None);
    }

    /// <summary>
    /// Registers a media library scan with the given status in the mocked repository.
    /// </summary>
    /// <param name="compositeId">The composite identifier of the scan to register.</param>
    /// <param name="status">The status of the media library scan.</param>
    /// <returns>The created media library scan entity.</returns>
    private LibraryScanEntity RegisterScan(MediaLibraryScanCompositeId compositeId, LibraryScanJobStatus status)
    {
        LibraryScanEntity scan = _libraryScanEntityFixture.Create(
            id: compositeId.ScanId.Value,
            userId: compositeId.UserId.Value,
            status: status);
        _mockLibraryScanRepository
            .GetByIdAsync(scan.Id, Arg.Any<bool>(), false, Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryScanEntity?>(scan));
        return scan;
    }

    /// <summary>
    /// Registers the file system discovery job that the mocked scanning service prepares for a media library scan.
    /// </summary>
    /// <param name="compositeId">The composite identifier of the scan that owns the job.</param>
    /// <param name="rootJob">The root job to return when the scan is prepared.</param>
    private void RegisterRootJob(MediaLibraryScanCompositeId compositeId, IMediaLibraryScanDiscoveryJob rootJob)
    {
        _mockMediaLibraryScanningService.StartScanAsync(
            Arg.Is<LibraryScan>(libraryScan => libraryScan.Id.Value == compositeId.ScanId.Value),
            Arg.Any<LibraryType>(),
            Arg.Any<CancellationToken>())
            .Returns(Result.From(rootJob));
    }

    /// <summary>
    /// Disposes the test dependencies.
    /// </summary>
    public void Dispose()
    {
        _serviceProvider.Dispose();
    }
}
