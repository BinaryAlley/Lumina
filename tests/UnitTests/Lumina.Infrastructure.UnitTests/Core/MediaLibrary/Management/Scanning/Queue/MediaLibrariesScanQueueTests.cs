#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Lumina.Infrastructure.Core.MediaLibrary.Management.Scanning.Queue;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Infrastructure.UnitTests.Core.MediaLibrary.Management.Scanning.Queue;

/// <summary>
/// Contains unit tests for the <see cref="MediaLibrariesScanQueue"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MediaLibrariesScanQueueTests
{
    private readonly MediaLibrariesScanQueue _sut = new();
    private readonly MediaLibraryScanCompositeIdFixture _mediaLibraryScanCompositeIdFixture = new();

    [Fact]
    public async Task Writer_WhenScanIsWritten_ShouldAllowReadingItFromReader()
    {
        // Arrange
        MediaLibraryScanCompositeId scanId = _mediaLibraryScanCompositeIdFixture.Create();

        // Act
        bool writeResult = _sut.Writer.TryWrite(scanId);
        MediaLibraryScanCompositeId readScanId = await _sut.Reader.ReadAsync(CancellationToken.None);

        // Assert
        Assert.True(writeResult);
        Assert.Equal(scanId, readScanId);
    }

    [Fact]
    public async Task Writer_WhenMultipleScansAreWritten_ShouldReadThemInFifoOrder()
    {
        // Arrange
        MediaLibraryScanCompositeId firstScanId = _mediaLibraryScanCompositeIdFixture.Create();
        MediaLibraryScanCompositeId secondScanId = _mediaLibraryScanCompositeIdFixture.Create();
        MediaLibraryScanCompositeId thirdScanId = _mediaLibraryScanCompositeIdFixture.Create();

        // Act
        _sut.Writer.TryWrite(firstScanId);
        _sut.Writer.TryWrite(secondScanId);
        _sut.Writer.TryWrite(thirdScanId);
        MediaLibraryScanCompositeId firstReadScanId = await _sut.Reader.ReadAsync(CancellationToken.None);
        MediaLibraryScanCompositeId secondReadScanId = await _sut.Reader.ReadAsync(CancellationToken.None);
        MediaLibraryScanCompositeId thirdReadScanId = await _sut.Reader.ReadAsync(CancellationToken.None);

        // Assert
        // The scan processor relies on the queue order: scans must run in the order in which they were queued.
        Assert.Equal(firstScanId, firstReadScanId);
        Assert.Equal(secondScanId, secondReadScanId);
        Assert.Equal(thirdScanId, thirdReadScanId);
    }

    [Fact]
    public async Task Reader_WhenQueueIsEmpty_ShouldNotCompleteUntilAScanIsWritten()
    {
        // Arrange
        MediaLibraryScanCompositeId scanId = _mediaLibraryScanCompositeIdFixture.Create();

        // Act
        Task<MediaLibraryScanCompositeId> readTask = _sut.Reader.ReadAsync(CancellationToken.None).AsTask();
        _sut.Writer.TryWrite(scanId);

        // Assert
        MediaLibraryScanCompositeId readScanId = await readTask;
        Assert.Equal(scanId, readScanId);
    }
}
