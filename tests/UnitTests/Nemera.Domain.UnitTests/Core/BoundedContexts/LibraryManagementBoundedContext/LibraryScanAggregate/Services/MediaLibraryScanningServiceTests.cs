#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Events;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Events;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Cancellation;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Jobs;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Progress;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Scanners;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.UserManagementBoundedContext.UserAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate;
using Lumina.Domain.Fixtures.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Jobs;
using Lumina.Domain.Fixtures.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.UserManagementBoundedContext.UserAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using NSubstitute;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Domain.UnitTests.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services;

/// <summary>
/// Contains unit tests for the <see cref="MediaLibraryScanningService"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MediaLibraryScanningServiceTests
{
    private readonly IMediaLibraryScannerFactory _mockScannerFactory;
    private readonly IMediaLibrariesScanCancellationTracker _mockCancellationTracker;
    private readonly IMediaLibrariesScanProgressTracker _mockProgressTracker;
    private readonly IDomainEventPublisher _mockDomainEventPublisher;
    private readonly MediaLibraryScanningService _sut;
    private readonly LibraryScanFixture _libraryScanFixture = new();
    private readonly ScanIdFixture _scanIdFixture = new();
    private readonly UserIdFixture _userIdFixture = new();
    private readonly LibraryIdFixture _libraryIdFixture = new();
    private readonly MediaLibraryScanDiscoveryJobFixture _mediaLibraryScanDiscoveryJobFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="MediaLibraryScanningServiceTests"/> class.
    /// </summary>
    public MediaLibraryScanningServiceTests()
    {
        _mockScannerFactory = Substitute.For<IMediaLibraryScannerFactory>();
        _mockCancellationTracker = Substitute.For<IMediaLibrariesScanCancellationTracker>();
        _mockProgressTracker = Substitute.For<IMediaLibrariesScanProgressTracker>();
        _mockProgressTracker.InitializeScanProgress(Arg.Any<LibraryId>(), Arg.Any<MediaLibraryScanCompositeId>(), Arg.Any<int>())
            .Returns(Result.Created);
        _mockDomainEventPublisher = Substitute.For<IDomainEventPublisher>();
        _sut = new MediaLibraryScanningService(_mockScannerFactory, _mockCancellationTracker, _mockProgressTracker, _mockDomainEventPublisher);
    }

    [Fact]
    public async Task StartScanAsync_WhenScanStartsSuccessfully_ShouldReturnTheDiscoveryRootJob()
    {
        // Arrange
        LibraryScan scan = _libraryScanFixture.Create();
        FakeMediaLibraryScanDiscoveryJob rootJob = _mediaLibraryScanDiscoveryJobFixture.Create();
        RegisterScanner(LibraryType.Book, rootJob);

        // Act
        Result<IMediaLibraryScanDiscoveryJob> result = await _sut.StartScanAsync(scan, LibraryType.Book, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Same(rootJob, result.Value);
        Assert.Equal(LibraryScanJobStatus.Running, scan.Status);
    }

    [Fact]
    public async Task StartScanAsync_WhenScanStartsSuccessfully_ShouldPublishTheStartedEvent()
    {
        // Arrange
        LibraryScan scan = _libraryScanFixture.Create();
        RegisterScanner(LibraryType.Book, _mediaLibraryScanDiscoveryJobFixture.Create());

        // Act
        await _sut.StartScanAsync(scan, LibraryType.Book, CancellationToken.None);

        // Assert
        await _mockDomainEventPublisher.Received(1).PublishAsync(
            Arg.Is<IDomainEvent>(domainEvent => domainEvent.GetType() == typeof(LibraryScanStartedDomainEvent)
                && ((LibraryScanStartedDomainEvent)domainEvent).LibraryScan == scan),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task StartScanAsync_WhenScanStartsSuccessfully_ShouldRegisterTheScanForCancellation()
    {
        // Arrange
        LibraryScan scan = _libraryScanFixture.Create();
        RegisterScanner(LibraryType.Book, _mediaLibraryScanDiscoveryJobFixture.Create());

        // Act
        await _sut.StartScanAsync(scan, LibraryType.Book, CancellationToken.None);

        // Assert
        _mockCancellationTracker.Received(1).RegisterScan(
            Arg.Is<MediaLibraryScanCompositeId>(compositeId => compositeId.ScanId == scan.Id && compositeId.UserId == scan.UserId));
    }

    [Fact]
    public async Task StartScanAsync_WhenScanStartsSuccessfully_ShouldInitializeProgressWithTheRealJobCount()
    {
        // Arrange
        LibraryScan scan = _libraryScanFixture.Create();
        FakeMediaLibraryScanDiscoveryJob leafJob = _mediaLibraryScanDiscoveryJobFixture.Create();
        FakeMediaLibraryScanDiscoveryJob middleJob = _mediaLibraryScanDiscoveryJobFixture.Create(children: [leafJob]);
        FakeMediaLibraryScanDiscoveryJob rootJob = _mediaLibraryScanDiscoveryJobFixture.Create(children: [middleJob]);
        RegisterScanner(LibraryType.Book, rootJob);

        // Act
        await _sut.StartScanAsync(scan, LibraryType.Book, CancellationToken.None);

        // Assert
        _mockProgressTracker.Received(1).InitializeScanProgress(
            scan.LibraryId,
            Arg.Is<MediaLibraryScanCompositeId>(compositeId => compositeId.ScanId == scan.Id && compositeId.UserId == scan.UserId),
            totalJobs: 3);
    }

    [Fact]
    public async Task StartScanAsync_WhenScanStartsSuccessfully_ShouldSetScanPropertiesOnTheWholeJobChain()
    {
        // Arrange
        LibraryScan scan = _libraryScanFixture.Create();
        FakeMediaLibraryScanDiscoveryJob leafJob = _mediaLibraryScanDiscoveryJobFixture.Create();
        FakeMediaLibraryScanDiscoveryJob middleJob = _mediaLibraryScanDiscoveryJobFixture.Create(children: [leafJob]);
        FakeMediaLibraryScanDiscoveryJob rootJob = _mediaLibraryScanDiscoveryJobFixture.Create(children: [middleJob]);
        RegisterScanner(LibraryType.Book, rootJob);

        // Act
        await _sut.StartScanAsync(scan, LibraryType.Book, CancellationToken.None);

        // Assert
        AssertScanProperties(rootJob, scan);
        AssertScanProperties(middleJob, scan);
        AssertScanProperties(leafJob, scan);
    }

    [Fact]
    public async Task StartScanAsync_WhenScannerIsUnavailable_ShouldReturnErrorAndChangeNothing()
    {
        // Arrange
        LibraryScan scan = _libraryScanFixture.Create();
        _mockScannerFactory.CreateLibraryScanner(LibraryType.Movie).Returns(Errors.LibraryScanning.NoScannerForLibraryType);

        // Act
        Result<IMediaLibraryScanDiscoveryJob> result = await _sut.StartScanAsync(scan, LibraryType.Movie, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.LibraryScanning.NoScannerForLibraryType, result.FirstError);
        Assert.Equal(LibraryScanJobStatus.Pending, scan.Status);
        Assert.Empty(scan.GetDomainEvents());
        _mockCancellationTracker.DidNotReceive().RegisterScan(Arg.Any<MediaLibraryScanCompositeId>());
        _mockProgressTracker.DidNotReceive().InitializeScanProgress(Arg.Any<LibraryId>(), Arg.Any<MediaLibraryScanCompositeId>(), Arg.Any<int>());
        await _mockDomainEventPublisher.DidNotReceive().PublishAsync(Arg.Any<IDomainEvent>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task StartScanAsync_WhenScanIsNotPending_ShouldReturnErrorWithoutRegisteringOrPublishing()
    {
        // Arrange
        LibraryScan scan = _libraryScanFixture.Create(status: LibraryScanJobStatus.Running);
        RegisterScanner(LibraryType.Book, _mediaLibraryScanDiscoveryJobFixture.Create());

        // Act
        Result<IMediaLibraryScanDiscoveryJob> result = await _sut.StartScanAsync(scan, LibraryType.Book, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.LibraryScanning.CanOnlyStartPendingScans, result.FirstError);
        Assert.Equal(LibraryScanJobStatus.Running, scan.Status);
        Assert.Empty(scan.GetDomainEvents());
        _mockCancellationTracker.DidNotReceive().RegisterScan(Arg.Any<MediaLibraryScanCompositeId>());
        _mockProgressTracker.DidNotReceive().InitializeScanProgress(Arg.Any<LibraryId>(), Arg.Any<MediaLibraryScanCompositeId>(), Arg.Any<int>());
        await _mockDomainEventPublisher.DidNotReceive().PublishAsync(Arg.Any<IDomainEvent>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task StartScanAsync_WhenProgressInitializationFails_ShouldReturnErrorWithoutPublishingEvents()
    {
        // Arrange
        LibraryScan scan = _libraryScanFixture.Create();
        RegisterScanner(LibraryType.Book, _mediaLibraryScanDiscoveryJobFixture.Create());
        Error error = Error.Failure(description: "Failed to initialize the media library scan progress");
        _mockProgressTracker.InitializeScanProgress(Arg.Any<LibraryId>(), Arg.Any<MediaLibraryScanCompositeId>(), Arg.Any<int>())
            .Returns(error);

        // Act
        Result<IMediaLibraryScanDiscoveryJob> result = await _sut.StartScanAsync(scan, LibraryType.Book, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
        await _mockDomainEventPublisher.DidNotReceive().PublishAsync(Arg.Any<IDomainEvent>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task StartScanAsync_WhenJobGraphHasSharedChild_ShouldCountOnlyUniqueJobs()
    {
        // Arrange
        LibraryScan scan = _libraryScanFixture.Create();
        IMediaLibraryScanJob sharedChild = CreateJob(children: []);
        IMediaLibraryScanJob firstChild = CreateJob(children: [sharedChild]);
        IMediaLibraryScanJob secondChild = CreateJob(children: [sharedChild]);
        FakeMediaLibraryScanDiscoveryJob rootJob = _mediaLibraryScanDiscoveryJobFixture.Create(children: [firstChild, secondChild]);
        RegisterScanner(LibraryType.Book, rootJob);

        // Act
        await _sut.StartScanAsync(scan, LibraryType.Book, CancellationToken.None);

        // Assert
        _mockProgressTracker.Received(1).InitializeScanProgress(scan.LibraryId, Arg.Any<MediaLibraryScanCompositeId>(), totalJobs: 4);
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
    /// Registers a mocked media library scanner that returns <paramref name="rootJob"/> with the mocked scanner factory.
    /// </summary>
    /// <param name="libraryType">The media library type that the scanner is registered for.</param>
    /// <param name="rootJob">The file system discovery job returned by the scanner.</param>
    /// <returns>The created mocked scanner.</returns>
    private IMediaTypeScanner RegisterScanner(LibraryType libraryType, IMediaLibraryScanDiscoveryJob rootJob)
    {
        IMediaTypeScanner scanner = Substitute.For<IMediaTypeScanner>();
        scanner.CreateScanJobsForLibrary(Arg.Any<LibraryId>()).Returns(rootJob);
        _mockScannerFactory.CreateLibraryScanner(libraryType).Returns(Result.From(scanner));
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
    /// Asserts that the scan identity of the scan was propagated to <paramref name="job"/>.
    /// </summary>
    /// <param name="job">The job whose scan properties are asserted.</param>
    /// <param name="scan">The media library scan that owns the job.</param>
    private static void AssertScanProperties(IMediaLibraryScanJob job, LibraryScan scan)
    {
        Assert.Equal(scan.Id, job.ScanId);
        Assert.Equal(scan.UserId, job.UserId);
    }
}
