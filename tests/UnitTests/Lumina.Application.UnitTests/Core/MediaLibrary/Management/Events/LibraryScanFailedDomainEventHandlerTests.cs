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
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Cancellation;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Progress;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.UserManagementBoundedContext.UserAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
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
/// Contains unit tests for the <see cref="LibraryScanFailedDomainEventHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class LibraryScanFailedDomainEventHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly ILibraryScanRepository _mockLibraryScanRepository;
    private readonly ILibraryScanStagingResultsRepository _mockLibraryScanStagingResultsRepository;
    private readonly IMediaLibrariesScanProgressTracker _mockMediaLibrariesScanProgressTracker;
    private readonly IMediaLibrariesScanCancellationTracker _mockMediaLibrariesScanCancellationTracker;
    private readonly IMediaLibraryScanProgressNotifier _mockDebouncedLibraryScanProgressNotifier;
    private readonly LibraryScanFailedDomainEventHandler _sut;
    private readonly LibraryScanEntityFixture _libraryScanEntityFixture = new();
    private readonly ScanIdFixture _scanIdFixture = new();
    private readonly UserIdFixture _userIdFixture = new();
    private readonly LibraryIdFixture _libraryIdFixture = new();
    private readonly MediaLibraryScanCompositeIdFixture _mediaLibraryScanCompositeIdFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="LibraryScanFailedDomainEventHandlerTests"/> class.
    /// </summary>
    public LibraryScanFailedDomainEventHandlerTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockLibraryScanRepository = Substitute.For<ILibraryScanRepository>();
        _mockLibraryScanStagingResultsRepository = Substitute.For<ILibraryScanStagingResultsRepository>();
        _mockUnitOfWork.LibraryScanRepository.Returns(_mockLibraryScanRepository);
        _mockUnitOfWork.LibraryScanStagingResultsRepository.Returns(_mockLibraryScanStagingResultsRepository);
        _mockMediaLibrariesScanProgressTracker = Substitute.For<IMediaLibrariesScanProgressTracker>();
        _mockMediaLibrariesScanCancellationTracker = Substitute.For<IMediaLibrariesScanCancellationTracker>();
        _mockDebouncedLibraryScanProgressNotifier = Substitute.For<IMediaLibraryScanProgressNotifier>();

        // default stubs: the library scan update and the staging results clearing succeed
        _mockLibraryScanRepository.UpdateAsync(Arg.Any<LibraryScanEntity>(), Arg.Any<CancellationToken>())
            .Returns(Result.Updated);
        _mockLibraryScanStagingResultsRepository.ClearForScanAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success);

        _sut = new LibraryScanFailedDomainEventHandler(_mockDebouncedLibraryScanProgressNotifier, _mockMediaLibrariesScanCancellationTracker, _mockMediaLibrariesScanProgressTracker, _mockUnitOfWork);
    }

    [Fact]
    public async Task HandleAsync_WhenScanIsRunning_ShouldFailScanAndCleanupResources()
    {
        // Arrange
        LibraryScanEntity scan = _libraryScanEntityFixture.Create();
        MediaLibraryScanCompositeId compositeId = _mediaLibraryScanCompositeIdFixture.Create(
            scanId: _scanIdFixture.Create(value: scan.Id),
            userId: _userIdFixture.Create(value: scan.UserId));
        LibraryScanFailedDomainEvent domainEvent = new(Guid.NewGuid(), _libraryIdFixture.Create(value: scan.LibraryId), compositeId, DateTime.UtcNow);
        _mockLibraryScanRepository.GetByIdAsync(scan.Id, Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryScanEntity?>(scan));

        // Act
        await _sut.HandleAsync(domainEvent, CancellationToken.None);

        // Assert
        await _mockLibraryScanRepository.Received(1).UpdateAsync(Arg.Any<LibraryScanEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        _mockMediaLibrariesScanCancellationTracker.Received(1).RemoveScan(compositeId);
        _mockMediaLibrariesScanProgressTracker.Received(1).RemoveScanProgress(compositeId);
        await _mockLibraryScanStagingResultsRepository.Received(1).ClearForScanAsync(scan.Id, Arg.Any<CancellationToken>());
        await _mockDebouncedLibraryScanProgressNotifier.Received(1).SendLibraryScanFailedEventAsync(compositeId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenScanIsNotRunning_ShouldSkipPersistenceButStillCleanupResources()
    {
        // Arrange
        LibraryScanEntity scan = _libraryScanEntityFixture.Create(status: LibraryScanJobStatus.Failed);
        MediaLibraryScanCompositeId compositeId = _mediaLibraryScanCompositeIdFixture.Create(
            scanId: _scanIdFixture.Create(value: scan.Id),
            userId: _userIdFixture.Create(value: scan.UserId));
        LibraryScanFailedDomainEvent domainEvent = new(Guid.NewGuid(), _libraryIdFixture.Create(value: scan.LibraryId), compositeId, DateTime.UtcNow);
        _mockLibraryScanRepository.GetByIdAsync(scan.Id, Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryScanEntity?>(scan));

        // Act
        await _sut.HandleAsync(domainEvent, CancellationToken.None);

        // Assert
        await _mockLibraryScanRepository.DidNotReceive().UpdateAsync(Arg.Any<LibraryScanEntity>(), Arg.Any<CancellationToken>());
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        _mockMediaLibrariesScanCancellationTracker.Received(1).RemoveScan(compositeId);
        _mockMediaLibrariesScanProgressTracker.Received(1).RemoveScanProgress(compositeId);
        await _mockLibraryScanStagingResultsRepository.Received(1).ClearForScanAsync(scan.Id, Arg.Any<CancellationToken>());
        await _mockDebouncedLibraryScanProgressNotifier.Received(1).SendLibraryScanFailedEventAsync(compositeId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenGetByIdFails_ShouldThrowEventualConsistencyException()
    {
        // Arrange
        MediaLibraryScanCompositeId compositeId = _mediaLibraryScanCompositeIdFixture.Create();
        LibraryScanFailedDomainEvent domainEvent = new(Guid.NewGuid(), _libraryIdFixture.Create(), compositeId, DateTime.UtcNow);
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
        MediaLibraryScanCompositeId compositeId = _mediaLibraryScanCompositeIdFixture.Create();
        LibraryScanFailedDomainEvent domainEvent = new(Guid.NewGuid(), _libraryIdFixture.Create(), compositeId, DateTime.UtcNow);
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
    public async Task HandleAsync_WhenUpdateFails_ShouldThrowEventualConsistencyException()
    {
        // Arrange
        LibraryScanEntity scan = _libraryScanEntityFixture.Create();
        MediaLibraryScanCompositeId compositeId = _mediaLibraryScanCompositeIdFixture.Create(
            scanId: _scanIdFixture.Create(value: scan.Id),
            userId: _userIdFixture.Create(value: scan.UserId));
        LibraryScanFailedDomainEvent domainEvent = new(Guid.NewGuid(), _libraryIdFixture.Create(value: scan.LibraryId), compositeId, DateTime.UtcNow);
        _mockLibraryScanRepository.GetByIdAsync(scan.Id, Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryScanEntity?>(scan));
        Error error = Error.Failure(description: "Failed to update library scan");
        _mockLibraryScanRepository.UpdateAsync(Arg.Any<LibraryScanEntity>(), Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        EventualConsistencyException exception = await Assert.ThrowsAsync<EventualConsistencyException>(
            () => _sut.HandleAsync(domainEvent, CancellationToken.None).AsTask());

        // Assert
        Assert.Equal(error, exception.EventualConsistencyError);
        await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenClearForScanFails_ShouldThrowEventualConsistencyException()
    {
        // Arrange
        LibraryScanEntity scan = _libraryScanEntityFixture.Create();
        MediaLibraryScanCompositeId compositeId = _mediaLibraryScanCompositeIdFixture.Create(
            scanId: _scanIdFixture.Create(value: scan.Id),
            userId: _userIdFixture.Create(value: scan.UserId));
        LibraryScanFailedDomainEvent domainEvent = new(Guid.NewGuid(), _libraryIdFixture.Create(value: scan.LibraryId), compositeId, DateTime.UtcNow);
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
        await _mockDebouncedLibraryScanProgressNotifier.DidNotReceive().SendLibraryScanFailedEventAsync(Arg.Any<MediaLibraryScanCompositeId>(), Arg.Any<CancellationToken>());
    }
}
