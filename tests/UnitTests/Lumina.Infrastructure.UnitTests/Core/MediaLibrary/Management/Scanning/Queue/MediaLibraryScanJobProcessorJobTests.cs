#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Cancellation;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Jobs;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.UserManagementBoundedContext.UserAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.UserManagementBoundedContext.UserAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Infrastructure.Core.MediaLibrary.Management.Scanning.Queue;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Infrastructure.UnitTests.Core.MediaLibrary.Management.Scanning.Queue;

/// <summary>
/// Contains unit tests for the <see cref="MediaLibraryScanJobProcessorJob"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MediaLibraryScanJobProcessorJobTests
{
    private readonly IMediaLibrariesScanCancellationTracker _mockCancellationTracker;
    private readonly ScanIdFixture _scanIdFixture = new();
    private readonly UserIdFixture _userIdFixture = new();
    private readonly LibraryIdFixture _libraryIdFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="MediaLibraryScanJobProcessorJobTests"/> class.
    /// </summary>
    public MediaLibraryScanJobProcessorJobTests()
    {
        _mockCancellationTracker = Substitute.For<IMediaLibrariesScanCancellationTracker>();
    }

    [Fact]
    public async Task StartAsync_WhenJobIsEnqueued_ShouldExecuteTheJobWithLinkedCancellationToken()
    {
        // Arrange
        MediaLibrariesScanQueue queue = new();
        MediaLibraryScanJobProcessorJob sut = new(queue, _mockCancellationTracker);
        RecordingScanJob job = CreateRecordingScanJob();
        _mockCancellationTracker.GetTokenForScan(Arg.Any<MediaLibraryScanCompositeId>())
            .Returns(CancellationToken.None);

        await sut.StartAsync(CancellationToken.None);

        // Act
        queue.Writer.TryWrite(job);
        await job.ExecutedTask.WaitAsync(TimeSpan.FromSeconds(5));

        // Assert
        Assert.True(job.WasExecuted);
        Assert.False(job.ReceivedCancellationToken.IsCancellationRequested);
        _mockCancellationTracker.Received(1).GetTokenForScan(
            Arg.Is<MediaLibraryScanCompositeId>(compositeId => compositeId.ScanId == job.ScanId && compositeId.UserId == job.UserId));

        await sut.StopAsync(CancellationToken.None);
    }

    [Fact]
    public async Task StartAsync_WhenScanWasAlreadyCancelled_ShouldExecuteJobWithAlreadyCancelledLinkedToken()
    {
        // Arrange
        MediaLibrariesScanQueue queue = new();
        MediaLibraryScanJobProcessorJob sut = new(queue, _mockCancellationTracker);
        RecordingScanJob job = CreateRecordingScanJob();
        using CancellationTokenSource scanCancellationTokenSource = new();
        scanCancellationTokenSource.Cancel();
        _mockCancellationTracker.GetTokenForScan(Arg.Any<MediaLibraryScanCompositeId>())
            .Returns(scanCancellationTokenSource.Token);

        await sut.StartAsync(CancellationToken.None);

        // Act
        queue.Writer.TryWrite(job);
        await job.ExecutedTask.WaitAsync(TimeSpan.FromSeconds(5));

        // Assert
        Assert.True(job.WasExecuted);
        Assert.True(job.ReceivedCancellationToken.IsCancellationRequested);

        await sut.StopAsync(CancellationToken.None);
    }

    /// <summary>
    /// Creates a recording scan job with a random scan and user identity.
    /// </summary>
    /// <returns>The created recording scan job.</returns>
    private RecordingScanJob CreateRecordingScanJob()
    {
        return new RecordingScanJob
        {
            ScanId = _scanIdFixture.Create(),
            UserId = _userIdFixture.Create(),
            LibraryId = _libraryIdFixture.Create()
        };
    }

    /// <summary>
    /// Test double for <see cref="IMediaLibraryScanJob"/> that records the token it was executed with.
    /// </summary>
    private sealed class RecordingScanJob : IMediaLibraryScanJob
    {
        private readonly TaskCompletionSource<bool> _executed = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public required ScanId ScanId { get; set; }

        public required UserId UserId { get; set; }

        public required LibraryId LibraryId { get; set; }

        public List<IMediaLibraryScanJob> Children { get; } = [];

        public List<IMediaLibraryScanJob> Parents { get; } = [];

        public LibraryScanJobStatus Status { get; private set; }

        public Task ExecutedTask => _executed.Task;

        public bool WasExecuted { get; private set; }

        public CancellationToken ReceivedCancellationToken { get; private set; }

        public void AddChild(IMediaLibraryScanJob job)
        {
        }

        public void AddParent(IMediaLibraryScanJob job)
        {
        }

        public Task ExecuteAsync<TInput>(Guid id, TInput input, CancellationToken cancellationToken)
        {
            WasExecuted = true;
            ReceivedCancellationToken = cancellationToken;
            _executed.TrySetResult(true);
            return Task.CompletedTask;
        }
    }
}
