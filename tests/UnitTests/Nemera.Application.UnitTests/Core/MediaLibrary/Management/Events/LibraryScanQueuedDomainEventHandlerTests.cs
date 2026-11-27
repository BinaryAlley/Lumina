#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.MediaLibrary.Management.Events;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Events;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Cancellation;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Progress;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Queue;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Events;
using NSubstitute;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.UnitTests.Core.MediaLibrary.Management.Events;

/// <summary>
/// Contains unit tests for the <see cref="LibraryScanQueuedDomainEventHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class LibraryScanQueuedDomainEventHandlerTests
{
    private readonly IMediaLibrariesScanQueue _mockScanQueue;
    private readonly IMediaLibrariesScanCancellationTracker _mockCancellationTracker;
    private readonly IMediaLibrariesScanProgressTracker _mockProgressTracker;
    private readonly LibraryScanQueuedDomainEventHandler _sut;
    private readonly LibraryScanQueuedDomainEventFixture _libraryScanQueuedDomainEventFixture = new();
    private readonly Channel<MediaLibraryScanCompositeId> _channel;

    /// <summary>
    /// Initializes a new instance of the <see cref="LibraryScanQueuedDomainEventHandlerTests"/> class.
    /// </summary>
    public LibraryScanQueuedDomainEventHandlerTests()
    {
        _channel = Channel.CreateUnbounded<MediaLibraryScanCompositeId>();
        _mockScanQueue = Substitute.For<IMediaLibrariesScanQueue>();
        _mockScanQueue.Writer.Returns(_channel.Writer);
        _mockScanQueue.Reader.Returns(_channel.Reader);
        _mockCancellationTracker = Substitute.For<IMediaLibrariesScanCancellationTracker>();
        _mockProgressTracker = Substitute.For<IMediaLibrariesScanProgressTracker>();

        // Default stubs: the queued scan progress is initialized successfully.
        _mockProgressTracker.InitializeQueuedScanProgress(Arg.Any<LibraryId>(), Arg.Any<MediaLibraryScanCompositeId>())
            .Returns(Result.Created);

        _sut = new LibraryScanQueuedDomainEventHandler(_mockScanQueue, _mockCancellationTracker, _mockProgressTracker);
    }

    [Fact]
    public async Task HandleAsync_WhenCalled_ShouldRegisterTheScanForCancellation()
    {
        // Arrange
        LibraryScanQueuedDomainEvent domainEvent = _libraryScanQueuedDomainEventFixture.Create();

        // Act
        await _sut.HandleAsync(domainEvent, CancellationToken.None);

        // Assert
        _mockCancellationTracker.Received(1).RegisterScan(
            Arg.Is<MediaLibraryScanCompositeId>(compositeId => compositeId.ScanId == domainEvent.ScanId && compositeId.UserId == domainEvent.UserId));
    }

    [Fact]
    public async Task HandleAsync_WhenCalled_ShouldInitializeTheQueuedScanProgress()
    {
        // Arrange
        LibraryScanQueuedDomainEvent domainEvent = _libraryScanQueuedDomainEventFixture.Create();

        // Act
        await _sut.HandleAsync(domainEvent, CancellationToken.None);

        // Assert
        _mockProgressTracker.Received(1).InitializeQueuedScanProgress(
            domainEvent.LibraryId,
            Arg.Is<MediaLibraryScanCompositeId>(compositeId => compositeId.ScanId == domainEvent.ScanId && compositeId.UserId == domainEvent.UserId));
    }

    [Fact]
    public async Task HandleAsync_WhenCalled_ShouldWriteTheCompositeScanIdToTheQueue()
    {
        // Arrange
        LibraryScanQueuedDomainEvent domainEvent = _libraryScanQueuedDomainEventFixture.Create();

        // Act
        await _sut.HandleAsync(domainEvent, CancellationToken.None);

        // Assert
        Assert.True(_channel.Reader.TryRead(out MediaLibraryScanCompositeId? enqueuedScanId));
        Assert.Equal(domainEvent.ScanId, enqueuedScanId!.ScanId);
        Assert.Equal(domainEvent.UserId, enqueuedScanId.UserId);
        Assert.False(_channel.Reader.TryRead(out _));
    }

    [Fact]
    public async Task HandleAsync_WhenCalled_ShouldNotStartProcessingTheScan()
    {
        // Arrange
        LibraryScanQueuedDomainEvent domainEvent = _libraryScanQueuedDomainEventFixture.Create();

        // Act
        await _sut.HandleAsync(domainEvent, CancellationToken.None);

        // Assert
        // Queuing a scan only registers it and reports it as waiting: the scan processor prepares and starts it later, one scan at a time.
        _mockProgressTracker.DidNotReceive().InitializeScanProgress(Arg.Any<LibraryId>(), Arg.Any<MediaLibraryScanCompositeId>(), Arg.Any<int>());
        _mockProgressTracker.DidNotReceive().UpdateScanProgress(Arg.Any<LibraryId>(), Arg.Any<MediaLibraryScanCompositeId>());
        _mockProgressTracker.DidNotReceive().UpdateScanJobProgress(Arg.Any<LibraryId>(), Arg.Any<MediaLibraryScanCompositeId>(), Arg.Any<MediaLibraryScanJobProgress>());
    }
}
