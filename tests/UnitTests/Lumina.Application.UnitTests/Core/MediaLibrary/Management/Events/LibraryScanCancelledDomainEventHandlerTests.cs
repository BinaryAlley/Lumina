#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.Repositories.MediaLibrary;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Core.MediaLibrary.Management.Events;
using Lumina.Application.Core.MediaLibrary.Management.Progress;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Domain.Common.Exceptions;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Events;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Cancellation;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Events;
using DomainErrors = Lumina.Domain.Common.Errors.Errors;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.UnitTests.Core.MediaLibrary.Management.Events;

/// <summary>
/// Contains unit tests for the <see cref="LibraryScanCancelledDomainEventHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class LibraryScanCancelledDomainEventHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly ILibraryScanRepository _mockLibraryScanRepository;
    private readonly ILibraryScanStagingResultsRepository _mockLibraryScanStagingResultsRepository;
    private readonly ILibraryScanSnapshotRepository _mockLibraryScanSnapshotRepository;
    private readonly IMediaLibraryScanningService _mockMediaLibraryScanningService;
    private readonly IMediaLibraryScanProgressNotifier _mockMediaLibraryScanProgressNotifier;
    private readonly IMediaLibrariesScanCancellationTracker _mockMediaLibrariesScanCancellationTracker;
    private readonly LibraryScanCancelledDomainEventHandler _sut;
    private readonly LibraryScanEntityFixture _libraryScanEntityFixture = new();
    private readonly LibraryScanCancelledDomainEventFixture _libraryScanCancelledDomainEventFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="LibraryScanCancelledDomainEventHandlerTests"/> class.
    /// </summary>
    public LibraryScanCancelledDomainEventHandlerTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Result.Success);
        _mockLibraryScanRepository = Substitute.For<ILibraryScanRepository>();
        _mockLibraryScanStagingResultsRepository = Substitute.For<ILibraryScanStagingResultsRepository>();
        _mockLibraryScanSnapshotRepository = Substitute.For<ILibraryScanSnapshotRepository>();
        _mockUnitOfWork.LibraryScanRepository.Returns(_mockLibraryScanRepository);
        _mockUnitOfWork.LibraryScanStagingResultsRepository.Returns(_mockLibraryScanStagingResultsRepository);
        _mockUnitOfWork.LibraryScanSnapshotRepository.Returns(_mockLibraryScanSnapshotRepository);
        _mockMediaLibraryScanningService = Substitute.For<IMediaLibraryScanningService>();
        _mockMediaLibraryScanProgressNotifier = Substitute.For<IMediaLibraryScanProgressNotifier>();
        _mockMediaLibrariesScanCancellationTracker = Substitute.For<IMediaLibrariesScanCancellationTracker>();

        // Default stubs: the cancellation service, the client notification, the snapshot rollback and the staging results clearing succeed.
        _mockMediaLibraryScanningService.CancelScan(Arg.Any<LibraryScan>()).Returns(Result.Success);
        _mockMediaLibraryScanProgressNotifier.SendLibraryScanCancelledEventAsync(Arg.Any<MediaLibraryScanCompositeId>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        _mockLibraryScanSnapshotRepository.RollbackSnapshotForScanAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Result.Deleted);
        _mockLibraryScanStagingResultsRepository.ClearForScanAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success);

        _sut = new LibraryScanCancelledDomainEventHandler(_mockMediaLibraryScanningService, _mockMediaLibrariesScanCancellationTracker, _mockMediaLibraryScanProgressNotifier, _mockUnitOfWork);
    }

    [Fact]
    public async Task HandleAsync_WhenScanExists_ShouldCancelScanAndCleanupResources()
    {
        // Arrange
        LibraryScanCancelledDomainEvent domainEvent = _libraryScanCancelledDomainEventFixture.Create();
        LibraryScanEntity scan = _libraryScanEntityFixture.Create(
            id: domainEvent.ScanId.Value,
            libraryId: domainEvent.LibraryId.Value);
        _mockLibraryScanRepository.GetByIdAsync(scan.Id, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryScanEntity?>(scan));

        // Act
        await _sut.HandleAsync(domainEvent, CancellationToken.None);

        // Assert
        _mockMediaLibraryScanningService.Received(1).CancelScan(Arg.Is<LibraryScan>(libraryScan => libraryScan.Id.Value == scan.Id));
        await _mockMediaLibraryScanProgressNotifier.Received(1).SendLibraryScanCancelledEventAsync(
            Arg.Is<MediaLibraryScanCompositeId>(compositeId => compositeId.ScanId == domainEvent.ScanId && compositeId.UserId.Value == scan.UserId),
            Arg.Any<CancellationToken>());
        _mockMediaLibrariesScanCancellationTracker.Received(1).RemoveScan(
            Arg.Is<MediaLibraryScanCompositeId>(compositeId => compositeId.ScanId == domainEvent.ScanId && compositeId.UserId.Value == scan.UserId));
        await _mockLibraryScanSnapshotRepository.Received(1).RollbackSnapshotForScanAsync(domainEvent.LibraryId.Value, domainEvent.ScanId.Value, CancellationToken.None);
        await _mockLibraryScanStagingResultsRepository.Received(1).ClearForScanAsync(scan.Id, CancellationToken.None);
    }

    [Fact]
    public async Task HandleAsync_WhenScanExists_ShouldNotifyClientsBeforeReleasingTheScanResources()
    {
        // Arrange
        LibraryScanCancelledDomainEvent domainEvent = _libraryScanCancelledDomainEventFixture.Create();
        LibraryScanEntity scan = _libraryScanEntityFixture.Create(
            id: domainEvent.ScanId.Value,
            libraryId: domainEvent.LibraryId.Value);
        _mockLibraryScanRepository.GetByIdAsync(scan.Id, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryScanEntity?>(scan));
        List<string> cleanupSteps = [];
        _mockMediaLibraryScanProgressNotifier
            .When(notifier => notifier.SendLibraryScanCancelledEventAsync(Arg.Any<MediaLibraryScanCompositeId>(), Arg.Any<CancellationToken>()))
            .Do(_ => cleanupSteps.Add("notifyClients"));
        _mockMediaLibrariesScanCancellationTracker
            .When(tracker => tracker.RemoveScan(Arg.Any<MediaLibraryScanCompositeId>()))
            .Do(_ => cleanupSteps.Add("removeCancellationToken"));
        _mockLibraryScanSnapshotRepository
            .When(repository => repository.RollbackSnapshotForScanAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()))
            .Do(_ => cleanupSteps.Add("rollbackSnapshot"));
        _mockLibraryScanStagingResultsRepository
            .When(repository => repository.ClearForScanAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()))
            .Do(_ => cleanupSteps.Add("clearStagingResults"));

        // Act
        await _sut.HandleAsync(domainEvent, CancellationToken.None);

        // Assert
        Assert.Equal(["notifyClients", "removeCancellationToken", "rollbackSnapshot", "clearStagingResults"], cleanupSteps);
    }

    [Fact]
    public async Task HandleAsync_WhenGetByIdFails_ShouldThrowEventualConsistencyException()
    {
        // Arrange
        LibraryScanCancelledDomainEvent domainEvent = _libraryScanCancelledDomainEventFixture.Create();
        Error error = Error.Failure(description: "Failed to get library scan");
        _mockLibraryScanRepository.GetByIdAsync(Arg.Any<Guid>(), cancellationToken: Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        EventualConsistencyException exception = await Assert.ThrowsAsync<EventualConsistencyException>(
            () => _sut.HandleAsync(domainEvent, CancellationToken.None).AsTask());

        // Assert
        Assert.Equal(error, exception.EventualConsistencyError);
        await _mockMediaLibraryScanProgressNotifier.DidNotReceive().SendLibraryScanCancelledEventAsync(Arg.Any<MediaLibraryScanCompositeId>(), Arg.Any<CancellationToken>());
        await _mockLibraryScanSnapshotRepository.DidNotReceive().RollbackSnapshotForScanAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockLibraryScanStagingResultsRepository.DidNotReceive().ClearForScanAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenScanDoesNotExist_ShouldThrowEventualConsistencyExceptionWithNotFoundError()
    {
        // Arrange
        LibraryScanCancelledDomainEvent domainEvent = _libraryScanCancelledDomainEventFixture.Create();
        _mockLibraryScanRepository.GetByIdAsync(Arg.Any<Guid>(), cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result<LibraryScanEntity?>.Success(null));

        // Act
        EventualConsistencyException exception = await Assert.ThrowsAsync<EventualConsistencyException>(
            () => _sut.HandleAsync(domainEvent, CancellationToken.None).AsTask());

        // Assert
        Assert.Equal(DomainErrors.LibraryScanning.LibraryScanNotFound, exception.EventualConsistencyError);
        await _mockMediaLibraryScanProgressNotifier.DidNotReceive().SendLibraryScanCancelledEventAsync(Arg.Any<MediaLibraryScanCompositeId>(), Arg.Any<CancellationToken>());
        await _mockLibraryScanStagingResultsRepository.DidNotReceive().ClearForScanAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenCancelScanFails_ShouldThrowEventualConsistencyException()
    {
        // Arrange
        LibraryScanCancelledDomainEvent domainEvent = _libraryScanCancelledDomainEventFixture.Create();
        LibraryScanEntity scan = _libraryScanEntityFixture.Create(
            id: domainEvent.ScanId.Value,
            libraryId: domainEvent.LibraryId.Value);
        _mockLibraryScanRepository.GetByIdAsync(scan.Id, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryScanEntity?>(scan));
        Error error = Error.Failure(description: "Failed to cancel scan");
        _mockMediaLibraryScanningService.CancelScan(Arg.Any<LibraryScan>()).Returns(error);

        // Act
        EventualConsistencyException exception = await Assert.ThrowsAsync<EventualConsistencyException>(
            () => _sut.HandleAsync(domainEvent, CancellationToken.None).AsTask());

        // Assert
        Assert.Equal(error, exception.EventualConsistencyError);
        await _mockMediaLibraryScanProgressNotifier.DidNotReceive().SendLibraryScanCancelledEventAsync(Arg.Any<MediaLibraryScanCompositeId>(), Arg.Any<CancellationToken>());
        _mockMediaLibrariesScanCancellationTracker.DidNotReceive().RemoveScan(Arg.Any<MediaLibraryScanCompositeId>());
        await _mockLibraryScanSnapshotRepository.DidNotReceive().RollbackSnapshotForScanAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mockLibraryScanStagingResultsRepository.DidNotReceive().ClearForScanAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenClearForScanFails_ShouldThrowEventualConsistencyException()
    {
        // Arrange
        LibraryScanCancelledDomainEvent domainEvent = _libraryScanCancelledDomainEventFixture.Create();
        LibraryScanEntity scan = _libraryScanEntityFixture.Create(
            id: domainEvent.ScanId.Value,
            libraryId: domainEvent.LibraryId.Value);
        _mockLibraryScanRepository.GetByIdAsync(scan.Id, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryScanEntity?>(scan));
        Error error = Error.Failure(description: "Failed to clear staging results");
        _mockLibraryScanStagingResultsRepository.ClearForScanAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        EventualConsistencyException exception = await Assert.ThrowsAsync<EventualConsistencyException>(
            () => _sut.HandleAsync(domainEvent, CancellationToken.None).AsTask());

        // Assert
        Assert.Equal(error, exception.EventualConsistencyError);
    }

    [Fact]
    public async Task HandleAsync_WhenTheScanTokenIsCanceled_ShouldStillClearTheStagingResults()
    {
        // Arrange
        // Cancelling the scan cancels its own token, so the cleanup must not be skipped because of it.
        LibraryScanCancelledDomainEvent domainEvent = _libraryScanCancelledDomainEventFixture.Create();
        LibraryScanEntity scan = _libraryScanEntityFixture.Create(
            id: domainEvent.ScanId.Value,
            libraryId: domainEvent.LibraryId.Value);
        _mockLibraryScanRepository.GetByIdAsync(scan.Id, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryScanEntity?>(scan));
        using CancellationTokenSource canceledTokenSource = new();
        canceledTokenSource.Cancel();

        // Act
        await _sut.HandleAsync(domainEvent, canceledTokenSource.Token);

        // Assert
        await _mockLibraryScanSnapshotRepository.Received(1).RollbackSnapshotForScanAsync(domainEvent.LibraryId.Value, domainEvent.ScanId.Value, CancellationToken.None);
        await _mockLibraryScanStagingResultsRepository.Received(1).ClearForScanAsync(scan.Id, CancellationToken.None);
    }
}
