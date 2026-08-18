#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Responses.MediaLibrary.Management;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Progress;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Infrastructure.Core.MediaLibrary.Management.Scanning.Progress;
using Microsoft.AspNetCore.SignalR;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Infrastructure.UnitTests.Core.MediaLibrary.Management.Scanning.Progress;

/// <summary>
/// Contains unit tests for the <see cref="DebouncedMediaLibraryScanProgressNotifier"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class DebouncedMediaLibraryScanProgressNotifierTests
{
    private readonly IHubContext<MediaLibraryScanProgressHub> _mockHubContext;
    private readonly IHubClients _mockHubClients;
    private readonly IClientProxy _mockClientProxy;
    private readonly IMediaLibrariesScanProgressTracker _mockProgressTracker;
    private readonly DebouncedMediaLibraryScanProgressNotifier _sut;
    private readonly MediaLibraryScanCompositeIdFixture _mediaLibraryScanCompositeIdFixture = new();
    private readonly MediaLibraryScanProgressFixture _mediaLibraryScanProgressFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="DebouncedMediaLibraryScanProgressNotifierTests"/> class.
    /// </summary>
    public DebouncedMediaLibraryScanProgressNotifierTests()
    {
        _mockHubContext = Substitute.For<IHubContext<MediaLibraryScanProgressHub>>();
        _mockHubClients = Substitute.For<IHubClients>();
        _mockClientProxy = Substitute.For<IClientProxy>();
        _mockHubContext.Clients.Returns(_mockHubClients);
        _mockHubClients.Group(Arg.Any<string>()).Returns(_mockClientProxy);
        _mockClientProxy.SendCoreAsync(Arg.Any<string>(), Arg.Any<object?[]>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        _mockProgressTracker = Substitute.For<IMediaLibrariesScanProgressTracker>();
        _sut = new DebouncedMediaLibraryScanProgressNotifier(_mockHubContext, _mockProgressTracker);
    }

    [Fact]
    public async Task SendLibraryProgressUpdateEventAsync_WhenTokenIsCancelled_ShouldNotPublishOrQueryProgress()
    {
        // Arrange
        MediaLibraryScanCompositeId compositeId = _mediaLibraryScanCompositeIdFixture.Create();
        using CancellationTokenSource cancellationTokenSource = new();
        cancellationTokenSource.Cancel();

        // Act
        await _sut.SendLibraryProgressUpdateEventAsync(compositeId, cancellationTokenSource.Token);

        // Assert
        _mockProgressTracker.DidNotReceive().GetScanProgress(Arg.Any<MediaLibraryScanCompositeId>());
        _mockHubClients.DidNotReceive().Group(Arg.Any<string>());
    }

    [Fact]
    public async Task SendLibraryProgressUpdateEventAsync_WhenProgressExists_ShouldSendProgressUpdateToGroup()
    {
        // Arrange
        MediaLibraryScanCompositeId compositeId = _mediaLibraryScanCompositeIdFixture.Create();
        MediaLibraryScanProgress progress = _mediaLibraryScanProgressFixture.Create(
            scanId: compositeId.ScanId,
            userId: compositeId.UserId,
            completedJobs: 1,
            totalJobs: 2,
            status: LibraryScanJobStatus.Running);
        _mockProgressTracker.GetScanProgress(compositeId).Returns(Result.From(progress));

        // Act
        await _sut.SendLibraryProgressUpdateEventAsync(compositeId, CancellationToken.None);

        // Assert
        _mockHubClients.Received(1).Group(compositeId.ToString());
        await _mockClientProxy.Received(1).SendCoreAsync(
            "libraryScanProgressUpdateEvent",
            Arg.Is<object?[]>(args => args.Length == 1
                && args[0] != null
                && ((MediaLibraryScanProgressResponse)args[0]!).Status == LibraryScanJobStatus.Running.ToString()),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SendLibraryProgressUpdateEventAsync_WhenProgressIsMissing_ShouldNotSendUpdate()
    {
        // Arrange
        MediaLibraryScanCompositeId compositeId = _mediaLibraryScanCompositeIdFixture.Create();
        _mockProgressTracker.GetScanProgress(compositeId)
            .Returns(Error.Failure("Tracking.Error", "The scan progress is missing"));

        // Act
        await _sut.SendLibraryProgressUpdateEventAsync(compositeId, CancellationToken.None);

        // Assert
        await _mockClientProxy.DidNotReceive().SendCoreAsync(Arg.Any<string>(), Arg.Any<object?[]>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SendLibraryScanFinishedEventAsync_WhenProgressExists_ShouldSendFinishedEventWithCompletedStatus()
    {
        // Arrange
        MediaLibraryScanCompositeId compositeId = _mediaLibraryScanCompositeIdFixture.Create();
        MediaLibraryScanProgress progress = _mediaLibraryScanProgressFixture.Create(
            scanId: compositeId.ScanId,
            userId: compositeId.UserId,
            completedJobs: 1,
            totalJobs: 2,
            status: LibraryScanJobStatus.Running);
        _mockProgressTracker.RemoveScanProgress(compositeId).Returns(Result.From(progress));

        // Act
        await _sut.SendLibraryScanFinishedEventAsync(compositeId, CancellationToken.None);

        // Assert
        await _mockClientProxy.Received(1).SendCoreAsync(
            "libraryScanFinishedEvent",
            Arg.Is<object?[]>(args => args.Length == 1
                && args[0] != null
                && ((MediaLibraryScanProgressResponse)args[0]!).Status == LibraryScanJobStatus.Completed.ToString()),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SendLibraryScanFinishedEventAsync_WhenProgressIsMissing_ShouldNotSendFinishedEvent()
    {
        // Arrange
        MediaLibraryScanCompositeId compositeId = _mediaLibraryScanCompositeIdFixture.Create();
        _mockProgressTracker.RemoveScanProgress(compositeId)
            .Returns(Error.Failure("Tracking.Error", "The scan progress is missing"));

        // Act
        await _sut.SendLibraryScanFinishedEventAsync(compositeId, CancellationToken.None);

        // Assert
        await _mockClientProxy.DidNotReceive().SendCoreAsync(Arg.Any<string>(), Arg.Any<object?[]>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SendLibraryScanFailedEventAsync_WhenProgressExists_ShouldSendFailedEventWithFailedStatus()
    {
        // Arrange
        MediaLibraryScanCompositeId compositeId = _mediaLibraryScanCompositeIdFixture.Create();
        MediaLibraryScanProgress progress = _mediaLibraryScanProgressFixture.Create(
            scanId: compositeId.ScanId,
            userId: compositeId.UserId,
            completedJobs: 1,
            totalJobs: 2,
            status: LibraryScanJobStatus.Running);
        _mockProgressTracker.RemoveScanProgress(compositeId).Returns(Result.From(progress));

        // Act
        await _sut.SendLibraryScanFailedEventAsync(compositeId, CancellationToken.None);

        // Assert
        await _mockClientProxy.Received(1).SendCoreAsync(
            "libraryScanFailedEvent",
            Arg.Is<object?[]>(args => args.Length == 1
                && args[0] != null
                && ((MediaLibraryScanProgressResponse)args[0]!).Status == LibraryScanJobStatus.Failed.ToString()),
            Arg.Any<CancellationToken>());
    }
}
