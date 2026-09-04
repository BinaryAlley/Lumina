#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.MediaLibrary.Management.Events;
using Lumina.Application.Core.MediaLibrary.Management.Progress;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Events;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Progress;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Events;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.UnitTests.Core.MediaLibrary.Management.Events;

/// <summary>
/// Contains unit tests for the <see cref="LibraryScanProgressChangedDomainEventHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class LibraryScanProgressChangedDomainEventHandlerTests
{
    private readonly IMediaLibrariesScanProgressTracker _mockMediaLibrariesScanProgressTracker;
    private readonly IMediaLibraryScanProgressNotifier _mockDebouncedLibraryScanProgressNotifier;
    private readonly LibraryScanProgressChangedDomainEventHandler _sut;
    private readonly LibraryScanProgressChangedDomainEventFixture _libraryScanProgressChangedDomainEventFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="LibraryScanProgressChangedDomainEventHandlerTests"/> class.
    /// </summary>
    public LibraryScanProgressChangedDomainEventHandlerTests()
    {
        _mockMediaLibrariesScanProgressTracker = Substitute.For<IMediaLibrariesScanProgressTracker>();
        _mockDebouncedLibraryScanProgressNotifier = Substitute.For<IMediaLibraryScanProgressNotifier>();

        _sut = new LibraryScanProgressChangedDomainEventHandler(_mockMediaLibrariesScanProgressTracker, _mockDebouncedLibraryScanProgressNotifier);
    }

    [Fact]
    public async Task HandleAsync_WhenScanProgressChanges_ShouldUpdateTrackerAndNotifySignalRClients()
    {
        // Arrange
        LibraryScanProgressChangedDomainEvent domainEvent = _libraryScanProgressChangedDomainEventFixture.Create();
        LibraryId libraryId = domainEvent.LibraryId;
        MediaLibraryScanCompositeId compositeId = domainEvent.MediaLibraryScanCompositeId;
        _mockMediaLibrariesScanProgressTracker.UpdateScanProgress(libraryId, compositeId).Returns(Result.Updated);

        // Act
        await _sut.HandleAsync(domainEvent, CancellationToken.None);

        // Assert
        _mockMediaLibrariesScanProgressTracker.Received(1).UpdateScanProgress(libraryId, compositeId);
        await _mockDebouncedLibraryScanProgressNotifier.Received(1).SendLibraryProgressUpdateEventAsync(compositeId, CancellationToken.None);
    }
}
