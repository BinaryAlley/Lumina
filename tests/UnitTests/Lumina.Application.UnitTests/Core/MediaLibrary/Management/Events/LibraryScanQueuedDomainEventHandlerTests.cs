#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.Repositories.MediaLibrary;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.DomainEvents;
using Lumina.Application.Core.MediaLibrary.Management.Events;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Domain.Common.Events;
using Lumina.Domain.Common.Exceptions;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Events;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services;
using Lumina.Domain.Fixtures.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Events;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using DomainErrors = Lumina.Domain.Common.Errors.Errors;
#endregion

namespace Lumina.Application.UnitTests.Core.MediaLibrary.Management.Events;

/// <summary>
/// Contains unit tests for the <see cref="LibraryScanQueuedDomainEventHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class LibraryScanQueuedDomainEventHandlerTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly ILibraryScanRepository _mockLibraryScanRepository;
    private readonly IMediaLibraryScanningService _mockMediaLibraryScanningService;
    private readonly IDomainEventsQueue _mockDomainEventsQueue;
    private readonly LibraryScanQueuedDomainEventHandler _sut;
    private readonly LibraryScanEntityFixture _libraryScanEntityFixture = new();
    private readonly LibraryScanQueuedDomainEventFixture _libraryScanQueuedDomainEventFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="LibraryScanQueuedDomainEventHandlerTests"/> class.
    /// </summary>
    public LibraryScanQueuedDomainEventHandlerTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockLibraryScanRepository = Substitute.For<ILibraryScanRepository>();
        _mockUnitOfWork.LibraryScanRepository.Returns(_mockLibraryScanRepository);
        _mockMediaLibraryScanningService = Substitute.For<IMediaLibraryScanningService>();
        _mockDomainEventsQueue = Substitute.For<IDomainEventsQueue>();

        // default stubs: the scanning service starts the scan successfully
        _mockMediaLibraryScanningService.StartScanAsync(Arg.Any<LibraryScan>(), Arg.Any<LibraryType>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success);

        _sut = new LibraryScanQueuedDomainEventHandler(_mockMediaLibraryScanningService, _mockDomainEventsQueue, _mockUnitOfWork);
    }

    [Fact]
    public async Task HandleAsync_WhenScanExists_ShouldStartTheScan()
    {
        // Arrange
        LibraryScanQueuedDomainEvent domainEvent = _libraryScanQueuedDomainEventFixture.Create();
        LibraryScanEntity scan = _libraryScanEntityFixture.Create(
            id: domainEvent.ScanId.Value,
            libraryId: domainEvent.LibraryId.Value);
        _mockLibraryScanRepository.GetByIdAsync(scan.Id, Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryScanEntity?>(scan));

        // Act
        await _sut.HandleAsync(domainEvent, CancellationToken.None);

        // Assert
        await _mockMediaLibraryScanningService.Received(1).StartScanAsync(
            Arg.Is<LibraryScan>(libraryScan => libraryScan.Id.Value == scan.Id),
            scan.Library.LibraryType,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenGetByIdFails_ShouldThrowEventualConsistencyException()
    {
        // Arrange
        LibraryScanQueuedDomainEvent domainEvent = _libraryScanQueuedDomainEventFixture.Create();
        Error error = Error.Failure(description: "Failed to get library scan");
        _mockLibraryScanRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        EventualConsistencyException exception = await Assert.ThrowsAsync<EventualConsistencyException>(
            () => _sut.HandleAsync(domainEvent, CancellationToken.None).AsTask());

        // Assert
        Assert.Equal(error, exception.EventualConsistencyError);
        await _mockMediaLibraryScanningService.DidNotReceive().StartScanAsync(Arg.Any<LibraryScan>(), Arg.Any<LibraryType>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenScanDoesNotExist_ShouldThrowEventualConsistencyExceptionWithNotFoundError()
    {
        // Arrange
        LibraryScanQueuedDomainEvent domainEvent = _libraryScanQueuedDomainEventFixture.Create();
        _mockLibraryScanRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Result<LibraryScanEntity?>.Success(null));

        // Act
        EventualConsistencyException exception = await Assert.ThrowsAsync<EventualConsistencyException>(
            () => _sut.HandleAsync(domainEvent, CancellationToken.None).AsTask());

        // Assert
        Assert.Equal(DomainErrors.LibraryScanning.LibraryScanNotFound, exception.EventualConsistencyError);
        await _mockMediaLibraryScanningService.DidNotReceive().StartScanAsync(Arg.Any<LibraryScan>(), Arg.Any<LibraryType>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenStartScanFails_ShouldThrowEventualConsistencyException()
    {
        // Arrange
        LibraryScanQueuedDomainEvent domainEvent = _libraryScanQueuedDomainEventFixture.Create();
        LibraryScanEntity scan = _libraryScanEntityFixture.Create(
            id: domainEvent.ScanId.Value,
            libraryId: domainEvent.LibraryId.Value);
        _mockLibraryScanRepository.GetByIdAsync(scan.Id, Arg.Any<CancellationToken>())
            .Returns(Result.From<LibraryScanEntity?>(scan));
        Error error = Error.Failure(description: "Failed to start scan");
        _mockMediaLibraryScanningService.StartScanAsync(Arg.Any<LibraryScan>(), Arg.Any<LibraryType>(), Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        EventualConsistencyException exception = await Assert.ThrowsAsync<EventualConsistencyException>(
            () => _sut.HandleAsync(domainEvent, CancellationToken.None).AsTask());

        // Assert
        Assert.Equal(error, exception.EventualConsistencyError);
        _mockDomainEventsQueue.DidNotReceive().Enqueue(Arg.Any<IDomainEvent>());
    }
}
