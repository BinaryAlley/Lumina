#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Progress;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.UnitTests.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Progress;

/// <summary>
/// Contains unit tests for the <see cref="MediaLibrariesScanProgressTracker"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MediaLibrariesScanProgressTrackerTests
{
    private readonly MediaLibrariesScanProgressTracker _sut = new();
    private readonly LibraryIdFixture _libraryIdFixture = new();
    private readonly MediaLibraryScanCompositeIdFixture _mediaLibraryScanCompositeIdFixture = new();
    private readonly MediaLibraryScanJobProgressFixture _mediaLibraryScanJobProgressFixture = new();

    [Fact]
    public void InitializeScanProgress_WhenCalled_ShouldCreateProgressWithZeroCompletedJobs()
    {
        // Arrange
        LibraryId libraryId = _libraryIdFixture.Create();
        MediaLibraryScanCompositeId compositeId = _mediaLibraryScanCompositeIdFixture.Create();

        // Act
        Result<Created> result = _sut.InitializeScanProgress(libraryId, compositeId, totalJobs: 5);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Created, result.Value);
        Result<MediaLibraryScanProgress> progressResult = _sut.GetScanProgress(compositeId);
        Assert.False(progressResult.IsFailure);
        Assert.Equal(0, progressResult.Value.CompletedJobs);
        Assert.Equal(5, progressResult.Value.TotalJobs);
        Assert.Equal(LibraryScanJobStatus.Pending, progressResult.Value.Status);
        Assert.True(progressResult.Value.CurrentJobProgress.HasValue);
        Assert.Equal("Initializing", progressResult.Value.CurrentJobProgress.Value.CurrentOperation);
    }

    [Fact]
    public void InitializeScanProgress_WhenTotalJobsIsNotPositive_ShouldReturnError()
    {
        // Arrange
        LibraryId libraryId = _libraryIdFixture.Create();
        MediaLibraryScanCompositeId compositeId = _mediaLibraryScanCompositeIdFixture.Create();

        // Act
        Result<Created> result = _sut.InitializeScanProgress(libraryId, compositeId, totalJobs: 0);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.LibraryScanning.TotalScanJobsCountMustBePositive, result.FirstError);
    }

    [Fact]
    public void UpdateScanProgress_WhenScanExists_ShouldIncrementCompletedJobs()
    {
        // Arrange
        LibraryId libraryId = _libraryIdFixture.Create();
        MediaLibraryScanCompositeId compositeId = _mediaLibraryScanCompositeIdFixture.Create();
        _sut.InitializeScanProgress(libraryId, compositeId, totalJobs: 2);

        // Act
        Result<Updated> result = _sut.UpdateScanProgress(libraryId, compositeId);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Updated, result.Value);
        Result<MediaLibraryScanProgress> progressResult = _sut.GetScanProgress(compositeId);
        Assert.False(progressResult.IsFailure);
        Assert.Equal(1, progressResult.Value.CompletedJobs);
        Assert.Equal(LibraryScanJobStatus.Running, progressResult.Value.Status);
    }

    [Fact]
    public void UpdateScanProgress_WhenLastJobIsCompleted_ShouldSetStatusCompleted()
    {
        // Arrange
        LibraryId libraryId = _libraryIdFixture.Create();
        MediaLibraryScanCompositeId compositeId = _mediaLibraryScanCompositeIdFixture.Create();
        _sut.InitializeScanProgress(libraryId, compositeId, totalJobs: 1);

        // Act
        Result<Updated> result = _sut.UpdateScanProgress(libraryId, compositeId);

        // Assert
        Assert.False(result.IsFailure);
        Result<MediaLibraryScanProgress> progressResult = _sut.GetScanProgress(compositeId);
        Assert.False(progressResult.IsFailure);
        Assert.Equal(1, progressResult.Value.CompletedJobs);
        Assert.Equal(LibraryScanJobStatus.Completed, progressResult.Value.Status);
        Assert.Equal(100, progressResult.Value.OverallProgressPercentage);
    }

    [Fact]
    public void UpdateScanProgress_WhenScanIsAlreadyCompleted_ShouldReturnError()
    {
        // Arrange
        LibraryId libraryId = _libraryIdFixture.Create();
        MediaLibraryScanCompositeId compositeId = _mediaLibraryScanCompositeIdFixture.Create();
        _sut.InitializeScanProgress(libraryId, compositeId, totalJobs: 1);
        _sut.UpdateScanProgress(libraryId, compositeId);

        // Act
        Result<Updated> result = _sut.UpdateScanProgress(libraryId, compositeId);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.LibraryScanning.CompletedScanJobsCountCantExceedTotalScanJobsCount, result.FirstError);
    }

    [Fact]
    public void UpdateScanJobProgress_WhenScanExists_ShouldUpdateCurrentJobProgress()
    {
        // Arrange
        LibraryId libraryId = _libraryIdFixture.Create();
        MediaLibraryScanCompositeId compositeId = _mediaLibraryScanCompositeIdFixture.Create();
        _sut.InitializeScanProgress(libraryId, compositeId, totalJobs: 2);
        MediaLibraryScanJobProgress jobProgress = _mediaLibraryScanJobProgressFixture.Create(completedItems: 3, totalItems: 10, currentOperation: "Hashing files");

        // Act
        Result<Updated> result = _sut.UpdateScanJobProgress(libraryId, compositeId, jobProgress);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Updated, result.Value);
        Result<MediaLibraryScanProgress> progressResult = _sut.GetScanProgress(compositeId);
        Assert.False(progressResult.IsFailure);
        Assert.True(progressResult.Value.CurrentJobProgress.HasValue);
        Assert.Equal(3, progressResult.Value.CurrentJobProgress.Value.CompletedItems);
        Assert.Equal("Hashing files", progressResult.Value.CurrentJobProgress.Value.CurrentOperation);
    }

    [Fact]
    public void UpdateScanJobProgress_WhenScanDoesNotExist_ShouldReturnNotFoundError()
    {
        // Arrange
        LibraryId libraryId = _libraryIdFixture.Create();
        MediaLibraryScanCompositeId compositeId = _mediaLibraryScanCompositeIdFixture.Create();
        MediaLibraryScanJobProgress jobProgress = _mediaLibraryScanJobProgressFixture.Create(completedItems: 1, totalItems: 10, currentOperation: "Hashing files");

        // Act
        Result<Updated> result = _sut.UpdateScanJobProgress(libraryId, compositeId, jobProgress);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.LibraryScanning.LibraryScanNotFound, result.FirstError);
    }

    [Fact]
    public void GetScanProgress_WhenScanExists_ShouldReturnProgress()
    {
        // Arrange
        LibraryId libraryId = _libraryIdFixture.Create();
        MediaLibraryScanCompositeId compositeId = _mediaLibraryScanCompositeIdFixture.Create();
        _sut.InitializeScanProgress(libraryId, compositeId, totalJobs: 3);

        // Act
        Result<MediaLibraryScanProgress> result = _sut.GetScanProgress(compositeId);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(compositeId.ScanId, result.Value.ScanId);
        Assert.Equal(compositeId.UserId, result.Value.UserId);
        Assert.Equal(libraryId, result.Value.LibraryId);
    }

    [Fact]
    public void GetScanProgress_WhenScanDoesNotExist_ShouldReturnNotFoundError()
    {
        // Act
        Result<MediaLibraryScanProgress> result = _sut.GetScanProgress(_mediaLibraryScanCompositeIdFixture.Create());

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.LibraryScanning.LibraryScanNotFound, result.FirstError);
    }

    [Fact]
    public void RemoveScanProgress_WhenScanExists_ShouldRemoveAndReturnProgress()
    {
        // Arrange
        LibraryId libraryId = _libraryIdFixture.Create();
        MediaLibraryScanCompositeId compositeId = _mediaLibraryScanCompositeIdFixture.Create();
        _sut.InitializeScanProgress(libraryId, compositeId, totalJobs: 3);

        // Act
        Result<MediaLibraryScanProgress> result = _sut.RemoveScanProgress(compositeId);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(compositeId.ScanId, result.Value.ScanId);
        Assert.True(_sut.GetScanProgress(compositeId).IsFailure);
    }

    [Fact]
    public void RemoveScanProgress_WhenScanDoesNotExist_ShouldReturnNotFoundError()
    {
        // Act
        Result<MediaLibraryScanProgress> result = _sut.RemoveScanProgress(_mediaLibraryScanCompositeIdFixture.Create());

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.LibraryScanning.LibraryScanNotFound, result.FirstError);
    }
}
