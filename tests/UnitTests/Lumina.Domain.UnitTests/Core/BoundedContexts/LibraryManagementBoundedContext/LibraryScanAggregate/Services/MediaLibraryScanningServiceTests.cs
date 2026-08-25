#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Events;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Events;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Cancellation;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Jobs;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Progress;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Queue;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Scanners;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.UserManagementBoundedContext.UserAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate;
using Lumina.Domain.Fixtures.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.UserManagementBoundedContext.UserAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
#endregion

namespace Lumina.Domain.UnitTests.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services;

/// <summary>
/// Contains unit tests for the <see cref="MediaLibraryScanningService"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MediaLibraryScanningServiceTests
{
    private readonly IMediaLibrariesScanQueue _mockScanQueue;
    private readonly IMediaLibraryScannerFactory _mockScannerFactory;
    private readonly IMediaLibrariesScanCancellationTracker _mockCancellationTracker;
    private readonly IMediaLibrariesScanProgressTracker _mockProgressTracker;
    private readonly IDomainEventPublisher _mockDomainEventPublisher;
    private readonly MediaLibraryScanningService _sut;
    private readonly LibraryScanFixture _libraryScanFixture = new();
    private readonly ScanIdFixture _scanIdFixture = new();
    private readonly UserIdFixture _userIdFixture = new();
    private readonly Channel<IMediaLibraryScanJob> _channel;

    /// <summary>
    /// Initializes a new instance of the <see cref="MediaLibraryScanningServiceTests"/> class.
    /// </summary>
    public MediaLibraryScanningServiceTests()
    {
        _channel = Channel.CreateUnbounded<IMediaLibraryScanJob>();
        _mockScanQueue = Substitute.For<IMediaLibrariesScanQueue>();
        _mockScanQueue.Writer.Returns(_channel.Writer);
        _mockScanQueue.Reader.Returns(_channel.Reader);
        _mockScannerFactory = Substitute.For<IMediaLibraryScannerFactory>();
        _mockCancellationTracker = Substitute.For<IMediaLibrariesScanCancellationTracker>();
        _mockCancellationTracker.GetTokenForScan(Arg.Any<MediaLibraryScanCompositeId>()).Returns(CancellationToken.None);
        _mockProgressTracker = Substitute.For<IMediaLibrariesScanProgressTracker>();
        _mockProgressTracker.InitializeScanProgress(Arg.Any<LibraryId>(), Arg.Any<MediaLibraryScanCompositeId>(), Arg.Any<int>()).Returns(Result.Created);
        _mockDomainEventPublisher = Substitute.For<IDomainEventPublisher>();
        _sut = new MediaLibraryScanningService(_mockScanQueue, _mockScannerFactory, _mockCancellationTracker, _mockProgressTracker, _mockDomainEventPublisher);
    }

    [Fact]
    public async Task StartScanAsync_WhenScanStartsSuccessfully_ShouldPublishEventsAndEnqueueJobs()
    {
        // Arrange
        LibraryScan scan = _libraryScanFixture.Create();
        IMediaLibraryScanJob job = CreateJob(children: []);
        IMediaTypeScanner scanner = CreateScanner([job]);

        // Act
        Result<Success> result = await _sut.StartScanAsync(scan, LibraryType.Book, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Success, result.Value);
        Assert.Equal(LibraryScanJobStatus.Running, scan.Status);
        await _mockDomainEventPublisher.Received(1).PublishAsync(Arg.Any<LibraryScanStartedDomainEvent>(), Arg.Any<CancellationToken>());
        _mockCancellationTracker.Received(1).RegisterScan(Arg.Is<MediaLibraryScanCompositeId>(compositeId => compositeId.ScanId == scan.Id && compositeId.UserId == scan.UserId));
        _mockProgressTracker.Received(1).InitializeScanProgress(scan.LibraryId, Arg.Any<MediaLibraryScanCompositeId>(), totalJobs: 1);
        Assert.Equal(scan.Id, job.ScanId);
        Assert.Equal(scan.UserId, job.UserId);
        Assert.True(_channel.Reader.TryRead(out IMediaLibraryScanJob? enqueuedJob));
        Assert.Same(job, enqueuedJob);
        Assert.False(_channel.Reader.TryRead(out _));
    }

    [Fact]
    public async Task StartScanAsync_WhenScanCannotStart_ShouldReturnErrorAndNotEnqueueJobs()
    {
        // Arrange
        LibraryScan scan = _libraryScanFixture.Create(status: LibraryScanJobStatus.Running);

        // Act
        Result<Success> result = await _sut.StartScanAsync(scan, LibraryType.Book, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Lumina.Domain.Common.Errors.Errors.LibraryScanning.CanOnlyStartPendingScans, result.FirstError);
        await _mockDomainEventPublisher.DidNotReceive().PublishAsync(Arg.Any<IDomainEvent>(), Arg.Any<CancellationToken>());
        _mockProgressTracker.DidNotReceive().InitializeScanProgress(Arg.Any<LibraryId>(), Arg.Any<MediaLibraryScanCompositeId>(), Arg.Any<int>());
        Assert.False(_channel.Reader.TryRead(out _));
    }

    [Fact]
    public async Task StartScanAsync_WhenScannerThrowsNotImplementedException_ShouldSucceedWithoutEnqueuingJobs()
    {
        // Arrange
        LibraryScan scan = _libraryScanFixture.Create();
        IMediaTypeScanner scanner = Substitute.For<IMediaTypeScanner>();
        scanner.CreateScanJobsForLibrary(Arg.Any<LibraryId>()).Returns(_ => throw new NotImplementedException());
        _mockScannerFactory.CreateLibraryScanner(LibraryType.Book).Returns(scanner);

        // Act
        Result<Success> result = await _sut.StartScanAsync(scan, LibraryType.Book, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Success, result.Value);
        _mockProgressTracker.DidNotReceive().InitializeScanProgress(Arg.Any<LibraryId>(), Arg.Any<MediaLibraryScanCompositeId>(), Arg.Any<int>());
        Assert.False(_channel.Reader.TryRead(out _));
    }

    [Fact]
    public async Task StartScanAsync_WhenJobGraphHasSharedChild_ShouldCountOnlyUniqueJobs()
    {
        // Arrange
        LibraryScan scan = _libraryScanFixture.Create();
        IMediaLibraryScanJob sharedChild = CreateJob(children: []);
        IMediaLibraryScanJob firstRoot = CreateJob(children: [sharedChild]);
        IMediaLibraryScanJob secondRoot = CreateJob(children: [sharedChild]);
        IMediaTypeScanner scanner = CreateScanner([firstRoot, secondRoot]);

        // Act
        Result<Success> result = await _sut.StartScanAsync(scan, LibraryType.Book, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        _mockProgressTracker.Received(1).InitializeScanProgress(scan.LibraryId, Arg.Any<MediaLibraryScanCompositeId>(), totalJobs: 3);
        Assert.Equal(2, CountEnqueuedJobs());
    }

    [Fact]
    public void CancelScan_WhenCalled_ShouldCancelScanInTracker()
    {
        // Arrange
        LibraryScan scan = _libraryScanFixture.Create();

        // Act
        Result<Success> result = _sut.CancelScan(scan);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Success, result.Value);
        _mockCancellationTracker.Received(1).CancelScan(Arg.Is<MediaLibraryScanCompositeId>(compositeId => compositeId.ScanId == scan.Id && compositeId.UserId == scan.UserId));
    }

    [Fact]
    public void SetScanPropertiesForJobChain_WhenCalled_ShouldSetScanPropertiesRecursively()
    {
        // Arrange
        ScanId scanId = _scanIdFixture.Create();
        UserId userId = _userIdFixture.Create();
        IMediaLibraryScanJob leafJob = CreateJob(children: []);
        IMediaLibraryScanJob middleJob = CreateJob(children: [leafJob]);
        IMediaLibraryScanJob rootJob = CreateJob(children: [middleJob]);

        // Act
        MediaLibraryScanningService.SetScanPropertiesForJobChain(rootJob, scanId, userId);

        // Assert
        Assert.Equal(scanId, rootJob.ScanId);
        Assert.Equal(userId, rootJob.UserId);
        Assert.Equal(scanId, middleJob.ScanId);
        Assert.Equal(userId, middleJob.UserId);
        Assert.Equal(scanId, leafJob.ScanId);
        Assert.Equal(userId, leafJob.UserId);
    }

    /// <summary>
    /// Creates a mocked <see cref="IMediaTypeScanner"/> and registers it with the mocked scanner factory.
    /// </summary>
    /// <param name="jobs">The scan jobs the scanner returns when asked to create scan jobs for a library.</param>
    /// <returns>The created mocked scanner.</returns>
    private IMediaTypeScanner CreateScanner(List<IMediaLibraryScanJob> jobs)
    {
        IMediaTypeScanner scanner = Substitute.For<IMediaTypeScanner>();
        scanner.CreateScanJobsForLibrary(Arg.Any<LibraryId>()).Returns(jobs);
        _mockScannerFactory.CreateLibraryScanner(LibraryType.Book).Returns(scanner);
        return scanner;
    }

    /// <summary>
    /// Creates a mocked scan job with the given children.
    /// </summary>
    /// <param name="children">The child scan jobs of the created job.</param>
    /// <returns>The created mocked scan job.</returns>
    private static IMediaLibraryScanJob CreateJob(List<IMediaLibraryScanJob> children)
    {
        IMediaLibraryScanJob job = Substitute.For<IMediaLibraryScanJob>();
        job.Children.Returns(children);
        return job;
    }

    /// <summary>
    /// Counts and drains the scan jobs currently enqueued on the scan queue channel.
    /// </summary>
    /// <returns>The number of enqueued scan jobs.</returns>
    private int CountEnqueuedJobs()
    {
        int count = 0;
        while (_channel.Reader.TryRead(out _))
            count++;
        return count;
    }
}
