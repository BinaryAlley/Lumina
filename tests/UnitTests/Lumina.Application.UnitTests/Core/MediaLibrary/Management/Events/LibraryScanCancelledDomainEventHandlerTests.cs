#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.Repositories.MediaLibrary;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Core.MediaLibrary.Management.Events;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Domain.Common.Exceptions;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Events;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Cancellation;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Progress;
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
    private readonly IMediaLibraryScanningService _mockMediaLibraryScanningService;
    private readonly IMediaLibrariesScanProgressTracker _mockMediaLibrariesScanProgressTracker;
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
        _mockLibraryScanRepository = Substitute.For<ILibraryScanRepository>();
        _mockLibraryScanStagingResultsRepository = Substitute.For<ILibraryScanStagingResultsRepository>();
        _mockUnitOfWork.LibraryScanRepository.Returns(_mockLibraryScanRepository);
        _mockUnitOfWork.LibraryScanStagingResultsRepository.Returns(_mockLibraryScanStagingResultsRepository);
        _mockMediaLibraryScanningService = Substitute.For<IMediaLibraryScanningService>();
        _mockMediaLibrariesScanProgressTracker = Substitute.For<IMediaLibrariesScanProgressTracker>();
        _mockMediaLibrariesScanCancellationTracker = Substitute.For<IMediaLibrariesScanCancellationTracker>();

        // default stubs: the cancellation service and the staging results clearing succeed
        _mockMediaLibraryScanningService.CancelScan(Arg.Any<LibraryScan>()).Returns(Result.Success);
        _mockLibraryScanStagingResultsRepository.ClearForScanAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success);

        _sut = new LibraryScanCancelledDomainEventHandler(_mockMediaLibraryScanningService, _mockMediaLibrariesScanCancellationTracker, _mockMediaLibrariesScanProgressTracker, _mockUnitOfWork);
    }

    [Fact]
    public async Task HandleAsync_WhenScanExists_ShouldCancelScanAndCleanupResources()
    {
        // Arrange
        LibraryScanCancelledDomainEvent domainEvent = _libraryScanCancelledDomainEventFixture.Create();
        LibraryScanEntity scan = _libraryScanEntityFixture.Create(
            id: domainEvent.ScanId.Value,
            libraryId: domainEvent.LibraryId.Value);
        _mockLibraryScanRepository.GetByIdAsync(scan.Id, Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryScanEntity?>(scan));

        // Act
        await _sut.HandleAsync(domainEvent, CancellationToken.None);

        // Assert
        _mockMediaLibraryScanningService.Received(1).CancelScan(Arg.Is<LibraryScan>(libraryScan => libraryScan.Id.Value == scan.Id));
        _mockMediaLibrariesScanCancellationTracker.Received(1).RemoveScan(Arg.Any<MediaLibraryScanCompositeId>());
        _mockMediaLibrariesScanProgressTracker.Received(1).RemoveScanProgress(Arg.Any<MediaLibraryScanCompositeId>());
        await _mockLibraryScanStagingResultsRepository.Received(1).ClearForScanAsync(scan.Id, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenGetByIdFails_ShouldThrowEventualConsistencyException()
    {
        // Arrange
        LibraryScanCancelledDomainEvent domainEvent = _libraryScanCancelledDomainEventFixture.Create();
        Error error = Error.Failure(description: "Failed to get library scan");
        _mockLibraryScanRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        EventualConsistencyException exception = await Assert.ThrowsAsync<EventualConsistencyException>(
            () => _sut.HandleAsync(domainEvent, CancellationToken.None).AsTask());

        // Assert
        Assert.Equal(error, exception.EventualConsistencyError);
        await _mockLibraryScanStagingResultsRepository.DidNotReceive().ClearForScanAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenScanDoesNotExist_ShouldThrowEventualConsistencyExceptionWithNotFoundError()
    {
        // Arrange
        LibraryScanCancelledDomainEvent domainEvent = _libraryScanCancelledDomainEventFixture.Create();
        _mockLibraryScanRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Result<LibraryScanEntity?>.Success(null));

        // Act
        EventualConsistencyException exception = await Assert.ThrowsAsync<EventualConsistencyException>(
            () => _sut.HandleAsync(domainEvent, CancellationToken.None).AsTask());

        // Assert
        Assert.Equal(DomainErrors.LibraryScanning.LibraryScanNotFound, exception.EventualConsistencyError);
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
        _mockLibraryScanRepository.GetByIdAsync(scan.Id, Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryScanEntity?>(scan));
        Error error = Error.Failure(description: "Failed to cancel scan");
        _mockMediaLibraryScanningService.CancelScan(Arg.Any<LibraryScan>()).Returns(error);

        // Act
        EventualConsistencyException exception = await Assert.ThrowsAsync<EventualConsistencyException>(
            () => _sut.HandleAsync(domainEvent, CancellationToken.None).AsTask());

        // Assert
        Assert.Equal(error, exception.EventualConsistencyError);
        _mockMediaLibrariesScanCancellationTracker.DidNotReceive().RemoveScan(Arg.Any<MediaLibraryScanCompositeId>());
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
        _mockLibraryScanRepository.GetByIdAsync(scan.Id, Arg.Any<CancellationToken>())
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
}
