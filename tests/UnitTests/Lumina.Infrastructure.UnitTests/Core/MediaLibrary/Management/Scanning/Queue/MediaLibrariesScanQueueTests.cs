#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Jobs;
using Lumina.Infrastructure.Core.MediaLibrary.Management.Scanning.Queue;
using NSubstitute;
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

    [Fact]
    public async Task Writer_WhenJobIsWritten_ShouldAllowReadingItFromReader()
    {
        // Arrange
        IMediaLibraryScanJob job = Substitute.For<IMediaLibraryScanJob>();

        // Act
        bool writeResult = _sut.Writer.TryWrite(job);
        IMediaLibraryScanJob readJob = await _sut.Reader.ReadAsync(CancellationToken.None);

        // Assert
        Assert.True(writeResult);
        Assert.Same(job, readJob);
    }

    [Fact]
    public async Task Writer_WhenMultipleJobsAreWritten_ShouldReadThemInFifoOrder()
    {
        // Arrange
        IMediaLibraryScanJob firstJob = Substitute.For<IMediaLibraryScanJob>();
        IMediaLibraryScanJob secondJob = Substitute.For<IMediaLibraryScanJob>();

        // Act
        _sut.Writer.TryWrite(firstJob);
        _sut.Writer.TryWrite(secondJob);
        IMediaLibraryScanJob firstReadJob = await _sut.Reader.ReadAsync(CancellationToken.None);
        IMediaLibraryScanJob secondReadJob = await _sut.Reader.ReadAsync(CancellationToken.None);

        // Assert
        Assert.Same(firstJob, firstReadJob);
        Assert.Same(secondJob, secondReadJob);
    }
}
