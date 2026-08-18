#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.MediaLibrary.Management.Events;
using Lumina.Application.Core.MediaLibrary.Management.Progress;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Events;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Progress;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.UnitTests.Core.MediaLibrary.Management.Events;

/// <summary>
/// Contains unit tests for the <see cref="LibraryScanJobProgressChangedDomainEventHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class LibraryScanJobProgressChangedDomainEventHandlerTests
{
    private readonly IMediaLibrariesScanProgressTracker _mockMediaLibrariesScanProgressTracker;
    private readonly IMediaLibraryScanProgressNotifier _mockDebouncedLibraryScanProgressNotifier;
    private readonly LibraryScanJobProgressChangedDomainEventHandler _sut;
    private readonly LibraryIdFixture _libraryIdFixture = new();
    private readonly MediaLibraryScanCompositeIdFixture _mediaLibraryScanCompositeIdFixture = new();
    private readonly MediaLibraryScanJobProgressFixture _mediaLibraryScanJobProgressFixture = new();

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
        LibraryId libraryId = _libraryIdFixture.Create();
        MediaLibraryScanCompositeId compositeId = _mediaLibraryScanCompositeIdFixture.Create();
        MediaLibraryScanJobProgress progress = _mediaLibraryScanJobProgressFixture.Create(completedItems: 5, totalItems: 10, currentOperation: "Hashing");
        LibraryScanJobProgressChangedDomainEvent domainEvent = new(Guid.NewGuid(), libraryId, compositeId, progress, DateTime.UtcNow);
        _mockMediaLibrariesScanProgressTracker.UpdateScanJobProgress(libraryId, compositeId, progress).Returns(Result.Updated);

        // Act
        await _sut.HandleAsync(domainEvent, CancellationToken.None);

        // Assert
        _mockMediaLibrariesScanProgressTracker.Received(1).UpdateScanJobProgress(libraryId, compositeId, progress);
        await _mockDebouncedLibraryScanProgressNotifier.Received(1).SendLibraryProgressUpdateEventAsync(compositeId, CancellationToken.None);
    }
}
