#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Lumina.Infrastructure.Core.MediaLibrary.Management.Scanning.Cancellation;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
#endregion

namespace Lumina.Infrastructure.UnitTests.Core.MediaLibrary.Management.Scanning.Cancellation;

/// <summary>
/// Contains unit tests for the <see cref="MediaLibrariesScanCancellationTracker"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MediaLibrariesScanCancellationTrackerTests
{
    private readonly MediaLibraryScanCompositeIdFixture _mediaLibraryScanCompositeIdFixture = new();

    [Fact]
    public void GetTokenForScan_WhenScanWasRegistered_ShouldReturnActiveToken()
    {
        // Arrange
        MediaLibrariesScanCancellationTracker sut = new();
        MediaLibraryScanCompositeId compositeId = _mediaLibraryScanCompositeIdFixture.Create();

        // Act
        sut.RegisterScan(compositeId);
        CancellationToken token = sut.GetTokenForScan(compositeId);

        // Assert
        Assert.False(token.IsCancellationRequested);
    }

    [Fact]
    public void GetTokenForScan_WhenScanWasNotRegistered_ShouldReturnNone()
    {
        // Arrange
        MediaLibrariesScanCancellationTracker sut = new();
        MediaLibraryScanCompositeId compositeId = _mediaLibraryScanCompositeIdFixture.Create();

        // Act
        CancellationToken token = sut.GetTokenForScan(compositeId);

        // Assert
        Assert.Equal(CancellationToken.None, token);
    }

    [Fact]
    public void CancelScan_WhenScanWasRegistered_ShouldCancelItsTokenAndRemoveTheScan()
    {
        // Arrange
        MediaLibrariesScanCancellationTracker sut = new();
        MediaLibraryScanCompositeId compositeId = _mediaLibraryScanCompositeIdFixture.Create();
        sut.RegisterScan(compositeId);
        CancellationToken token = sut.GetTokenForScan(compositeId);

        // Act
        sut.CancelScan(compositeId);

        // Assert
        Assert.True(token.IsCancellationRequested);
        Assert.Equal(CancellationToken.None, sut.GetTokenForScan(compositeId));
    }

    [Fact]
    public void RemoveScan_WhenScanWasRegistered_ShouldCancelItsTokenAndRemoveTheScan()
    {
        // Arrange
        MediaLibrariesScanCancellationTracker sut = new();
        MediaLibraryScanCompositeId compositeId = _mediaLibraryScanCompositeIdFixture.Create();
        sut.RegisterScan(compositeId);
        CancellationToken token = sut.GetTokenForScan(compositeId);

        // Act
        sut.RemoveScan(compositeId);

        // Assert
        Assert.True(token.IsCancellationRequested);
        Assert.Equal(CancellationToken.None, sut.GetTokenForScan(compositeId));
    }

    [Fact]
    public void CancelScan_WhenScanWasNotRegistered_ShouldNotThrow()
    {
        // Arrange
        MediaLibrariesScanCancellationTracker sut = new();
        MediaLibraryScanCompositeId compositeId = _mediaLibraryScanCompositeIdFixture.Create();

        // Act
        Action act = () => sut.CancelScan(compositeId);

        // Assert
        act();
    }

    [Fact]
    public void Dispose_ShouldCancelAllRegisteredScans()
    {
        // Arrange
        MediaLibrariesScanCancellationTracker sut = new();
        MediaLibraryScanCompositeId firstCompositeId = _mediaLibraryScanCompositeIdFixture.Create();
        MediaLibraryScanCompositeId secondCompositeId = _mediaLibraryScanCompositeIdFixture.Create();
        sut.RegisterScan(firstCompositeId);
        sut.RegisterScan(secondCompositeId);
        CancellationToken firstToken = sut.GetTokenForScan(firstCompositeId);
        CancellationToken secondToken = sut.GetTokenForScan(secondCompositeId);

        // Act
        sut.Dispose();

        // Assert
        Assert.True(firstToken.IsCancellationRequested);
        Assert.True(secondToken.IsCancellationRequested);
    }
}
