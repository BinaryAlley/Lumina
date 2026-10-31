#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Events;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Events;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.UserManagementBoundedContext.UserAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate;
using Lumina.Domain.Fixtures.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.UserManagementBoundedContext.UserAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Domain.UnitTests.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate;

/// <summary>
/// Contains unit tests for the <see cref="LibraryScan"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class LibraryScanTests
{
    private readonly LibraryScanFixture _libraryScanFixture = new();
    private readonly ScanIdFixture _scanIdFixture = new();
    private readonly LibraryIdFixture _libraryIdFixture = new();
    private readonly UserIdFixture _userIdFixture = new();

    [Fact]
    public void Create_WhenCalledWithValidData_ShouldCreateScanWithPendingStatus()
    {
        // Arrange
        LibraryId libraryId = _libraryIdFixture.Create();
        UserId userId = _userIdFixture.Create();

        // Act
        Result<LibraryScan> result = LibraryScan.Create(libraryId, userId, []);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(libraryId, result.Value.LibraryId);
        Assert.Equal(userId, result.Value.UserId);
        Assert.Equal(LibraryScanJobStatus.Pending, result.Value.Status);
        Assert.NotEqual(default, result.Value.Id.Value);
    }

    [Fact]
    public void Create_WhenCalledWithPreExistingIdAndStatus_ShouldCreateScanWithThoseValues()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        LibraryId libraryId = _libraryIdFixture.Create();
        UserId userId = _userIdFixture.Create();

        // Act
        Result<LibraryScan> result = LibraryScan.Create(_scanIdFixture.Create(id), libraryId, userId, LibraryScanJobStatus.Running, []);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(id, result.Value.Id.Value);
        Assert.Equal(libraryId, result.Value.LibraryId);
        Assert.Equal(userId, result.Value.UserId);
        Assert.Equal(LibraryScanJobStatus.Running, result.Value.Status);
    }

    [Fact]
    public void Create_WhenCalledTwice_ShouldGenerateDistinctIds()
    {
        // Act
        Result<LibraryScan> firstResult = LibraryScan.Create(_libraryIdFixture.Create(), _userIdFixture.Create(), []);
        Result<LibraryScan> secondResult = LibraryScan.Create(_libraryIdFixture.Create(), _userIdFixture.Create(), []);

        // Assert
        Assert.False(firstResult.IsFailure);
        Assert.False(secondResult.IsFailure);
        Assert.NotEqual(firstResult.Value.Id.Value, secondResult.Value.Id.Value);
    }

    [Fact]
    public void QueueScan_WhenNoActivePastScanExists_ShouldSucceedAndRaiseQueuedEvent()
    {
        // Arrange
        LibraryScan scan = _libraryScanFixture.Create();

        // Act
        Result<Success> result = scan.QueueScan();

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Success, result.Value);
        Assert.Equal(LibraryScanJobStatus.Pending, scan.Status);
        LibraryScanQueuedDomainEvent queuedEvent = Assert.IsType<LibraryScanQueuedDomainEvent>(Assert.Single(scan.GetDomainEvents()));
        Assert.Equal(scan.Id, queuedEvent.ScanId);
        Assert.Equal(scan.UserId, queuedEvent.UserId);
        Assert.Equal(scan.LibraryId, queuedEvent.LibraryId);
    }

    [Fact]
    public void QueueScan_WhenPastScanIsRunning_ShouldReturnError()
    {
        // Arrange
        LibraryScan pastScan = _libraryScanFixture.Create(status: LibraryScanJobStatus.Running);
        LibraryScan scan = _libraryScanFixture.Create(pastScans: [pastScan]);

        // Act
        Result<Success> result = scan.QueueScan();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.LibraryScanning.LibraryAlreadyBeingScanned, result.FirstError);
        Assert.Empty(scan.GetDomainEvents());
    }

    [Fact]
    public void QueueScan_WhenPastScanIsPending_ShouldReturnError()
    {
        // Arrange
        LibraryScan pastScan = _libraryScanFixture.Create(status: LibraryScanJobStatus.Pending);
        LibraryScan scan = _libraryScanFixture.Create(pastScans: [pastScan]);

        // Act
        Result<Success> result = scan.QueueScan();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.LibraryScanning.LibraryAlreadyBeingScanned, result.FirstError);
        Assert.Empty(scan.GetDomainEvents());
    }

    [Fact]
    public void StartScan_WhenPending_ShouldTransitionToRunningAndRaiseStartedEvent()
    {
        // Arrange
        LibraryScan scan = _libraryScanFixture.Create();

        // Act
        Result<Success> result = scan.StartScan();

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Success, result.Value);
        Assert.Equal(LibraryScanJobStatus.Running, scan.Status);
        LibraryScanStartedDomainEvent startedEvent = Assert.IsType<LibraryScanStartedDomainEvent>(Assert.Single(scan.GetDomainEvents()));
        Assert.Equal(scan, startedEvent.LibraryScan);
    }

    [Theory]
    [InlineData(LibraryScanJobStatus.Running)] // Cannot start an already running scan.
    [InlineData(LibraryScanJobStatus.Completed)] // Cannot start a completed scan.
    [InlineData(LibraryScanJobStatus.Canceled)] // Cannot start a canceled scan.
    [InlineData(LibraryScanJobStatus.Failed)] // Cannot start a failed scan.
    public void StartScan_WhenNotPending_ShouldReturnError(LibraryScanJobStatus status)
    {
        // Arrange
        LibraryScan scan = _libraryScanFixture.Create(status: status);

        // Act
        Result<Success> result = scan.StartScan();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.LibraryScanning.CanOnlyStartPendingScans, result.FirstError);
        Assert.Equal(status, scan.Status);
        Assert.Empty(scan.GetDomainEvents());
    }

    [Fact]
    public void StartScan_WhenPastScanIsActive_ShouldReturnError()
    {
        // Arrange
        LibraryScan pastScan = _libraryScanFixture.Create(status: LibraryScanJobStatus.Running);
        LibraryScan scan = _libraryScanFixture.Create(pastScans: [pastScan]);

        // Act
        Result<Success> result = scan.StartScan();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.LibraryScanning.LibraryAlreadyBeingScanned, result.FirstError);
        Assert.Empty(scan.GetDomainEvents());
    }

    [Fact]
    public void FinishScan_WhenRunning_ShouldTransitionToCompleted()
    {
        // Arrange
        LibraryScan scan = _libraryScanFixture.Create(status: LibraryScanJobStatus.Running);

        // Act
        Result<Success> result = scan.FinishScan();

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Success, result.Value);
        Assert.Equal(LibraryScanJobStatus.Completed, scan.Status);
    }

    [Theory]
    [InlineData(LibraryScanJobStatus.Pending)] // Cannot complete a pending scan.
    [InlineData(LibraryScanJobStatus.Completed)] // Cannot complete an already completed scan.
    [InlineData(LibraryScanJobStatus.Canceled)] // Cannot complete a canceled scan.
    [InlineData(LibraryScanJobStatus.Failed)] // Cannot complete a failed scan.
    public void FinishScan_WhenNotRunning_ShouldReturnError(LibraryScanJobStatus status)
    {
        // Arrange
        LibraryScan scan = _libraryScanFixture.Create(status: status);

        // Act
        Result<Success> result = scan.FinishScan();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.LibraryScanning.CanOnlyCompleteRunningScans, result.FirstError);
        Assert.Equal(status, scan.Status);
    }

    [Theory]
    [InlineData(LibraryScanJobStatus.Pending)] // A scan waiting in the scan queue can be canceled too.
    [InlineData(LibraryScanJobStatus.Running)] // A scan that is already executing can be canceled.
    public void CancelScan_WhenPendingOrRunning_ShouldTransitionToCanceledAndRaiseCancelledEvent(LibraryScanJobStatus status)
    {
        // Arrange
        LibraryScan scan = _libraryScanFixture.Create(status: status);

        // Act
        Result<Success> result = scan.CancelScan();

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Success, result.Value);
        Assert.Equal(LibraryScanJobStatus.Canceled, scan.Status);
        LibraryScanCancelledDomainEvent cancelledEvent = Assert.IsType<LibraryScanCancelledDomainEvent>(Assert.Single(scan.GetDomainEvents()));
        Assert.Equal(scan.Id, cancelledEvent.ScanId);
        Assert.Equal(scan.LibraryId, cancelledEvent.LibraryId);
    }

    [Theory]
    [InlineData(LibraryScanJobStatus.Completed)] // Cannot cancel a completed scan.
    [InlineData(LibraryScanJobStatus.Canceled)] // Cannot cancel an already canceled scan.
    [InlineData(LibraryScanJobStatus.Failed)] // Cannot cancel a failed scan.
    public void CancelScan_WhenInATerminalState_ShouldReturnError(LibraryScanJobStatus status)
    {
        // Arrange
        LibraryScan scan = _libraryScanFixture.Create(status: status);

        // Act
        Result<Success> result = scan.CancelScan();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.LibraryScanning.CanOnlyCancelPendingOrRunningScans, result.FirstError);
        Assert.Equal(status, scan.Status);
        Assert.Empty(scan.GetDomainEvents());
    }

    [Theory]
    [InlineData(LibraryScanJobStatus.Pending)] // A scan that has no scanner yet fails before it starts executing.
    [InlineData(LibraryScanJobStatus.Running)] // A scan that is executing can fail too.
    public void FailScan_WhenPendingOrRunning_ShouldTransitionToFailed(LibraryScanJobStatus status)
    {
        // Arrange
        LibraryScan scan = _libraryScanFixture.Create(status: status);

        // Act
        Result<Success> result = scan.FailScan();

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Success, result.Value);
        Assert.Equal(LibraryScanJobStatus.Failed, scan.Status);
    }

    [Theory]
    [InlineData(LibraryScanJobStatus.Completed)] // Cannot fail a completed scan.
    [InlineData(LibraryScanJobStatus.Canceled)] // Cannot fail a canceled scan.
    [InlineData(LibraryScanJobStatus.Failed)] // Cannot fail an already failed scan.
    public void FailScan_WhenInATerminalState_ShouldReturnError(LibraryScanJobStatus status)
    {
        // Arrange
        LibraryScan scan = _libraryScanFixture.Create(status: status);

        // Act
        Result<Success> result = scan.FailScan();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.LibraryScanning.CanOnlyFailPendingOrRunningScans, result.FirstError);
        Assert.Equal(status, scan.Status);
    }
}
