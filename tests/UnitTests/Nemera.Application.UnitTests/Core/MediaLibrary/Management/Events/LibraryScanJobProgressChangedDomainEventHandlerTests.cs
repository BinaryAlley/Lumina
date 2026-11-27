#region ========================================================================= USING =====================================================================================
using Nemera.Application.Core.MediaLibrary.Management.Events;
using Nemera.Application.Core.MediaLibrary.Management.Progress;
using Nemera.Domain.Common.Primitives;
using Nemera.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using Nemera.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Events;
using Nemera.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Progress;
using Nemera.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Nemera.Domain.Fixtures.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Events;
using Nemera.Domain.Fixtures.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Nemera.Application.UnitTests.Core.MediaLibrary.Management.Events;

/// <summary>
/// Contains unit tests for the <see cref="LibraryScanJobProgressChangedDomainEventHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class LibraryScanJobProgressChangedDomainEventHandlerTests
{
    private readonly IMediaLibrariesScanProgressTracker _mockMediaLibrariesScanProgressTracker;
    private readonly IMediaLibraryScanProgressNotifier _mockDebouncedLibraryScanProgressNotifier;
    private readonly LibraryScanJobProgressChangedDomainEventHandler _sut;
    private readonly MediaLibraryScanJobProgressFixture _mediaLibraryScanJobProgressFixture = new();
    private readonly LibraryScanJobProgressChangedDomainEventFixture _libraryScanJobProgressChangedDomainEventFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="LibraryScanJobProgressChangedDomainEventHandlerTests"/> class.
    /// </summary>
    public LibraryScanJobProgressChangedDomainEventHandlerTests()
    {
        _mockMediaLibrariesScanProgressTracker = Substitute.For<IMediaLibrariesScanProgressTracker>();
        _mockDebouncedLibraryScanProgressNotifier = Substitute.For<IMediaLibraryScanProgressNotifier>();

        _sut = new LibraryScanJobProgressChangedDomainEventHandler(_mockMediaLibrariesScanProgressTracker, _mockDebouncedLibraryScanProgressNotifier);
    }

    [Fact]
    public async Task HandleAsync_WhenScanJobProgressChanges_ShouldUpdateTrackerAndNotifySignalRClients()
    {
        // Arrange
        MediaLibraryScanJobProgress progress = _mediaLibraryScanJobProgressFixture.Create(completedItems: 5, totalItems: 10, currentOperation: "Hashing");
        LibraryScanJobProgressChangedDomainEvent domainEvent = _libraryScanJobProgressChangedDomainEventFixture.Create(progress: progress);
        LibraryId libraryId = domainEvent.LibraryId;
        MediaLibraryScanCompositeId compositeId = domainEvent.MediaLibraryScanCompositeId;
        _mockMediaLibrariesScanProgressTracker.UpdateScanJobProgress(libraryId, compositeId, progress).Returns(Result.Updated);

        // Act
        await _sut.HandleAsync(domainEvent, CancellationToken.None);

        // Assert
        _mockMediaLibrariesScanProgressTracker.Received(1).UpdateScanJobProgress(libraryId, compositeId, progress);
        await _mockDebouncedLibraryScanProgressNotifier.Received(1).SendLibraryProgressUpdateEventAsync(compositeId, CancellationToken.None);
    }
}
