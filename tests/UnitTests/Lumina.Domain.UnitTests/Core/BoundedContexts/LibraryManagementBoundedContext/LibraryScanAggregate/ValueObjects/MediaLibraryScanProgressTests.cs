#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.UserManagementBoundedContext.UserAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.UserManagementBoundedContext.UserAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.UnitTests.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;

/// <summary>
/// Contains unit tests for the <see cref="MediaLibraryScanProgress"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MediaLibraryScanProgressTests
{
    private readonly ScanIdFixture _scanIdFixture = new();
    private readonly UserIdFixture _userIdFixture = new();
    private readonly LibraryIdFixture _libraryIdFixture = new();
    private readonly MediaLibraryScanJobProgressFixture _mediaLibraryScanJobProgressFixture = new();
    private readonly MediaLibraryScanProgressFixture _mediaLibraryScanProgressFixture = new();

    [Fact]
    public void Create_WhenCalledWithValidValues_ShouldCreateProgressWithExpectedValues()
    {
        // Arrange
        ScanId scanId = _scanIdFixture.Create();
        UserId userId = _userIdFixture.Create();
        LibraryId libraryId = _libraryIdFixture.Create();
        Optional<MediaLibraryScanJobProgress> currentJobProgress = Optional<MediaLibraryScanJobProgress>.Some(_mediaLibraryScanJobProgressFixture.Create());

        // Act
        Result<MediaLibraryScanProgress> result = MediaLibraryScanProgress.Create(scanId, userId, libraryId, completedJobs: 2, totalJobs: 5, LibraryScanJobStatus.Running, currentJobProgress);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(scanId, result.Value.ScanId);
        Assert.Equal(userId, result.Value.UserId);
        Assert.Equal(libraryId, result.Value.LibraryId);
        Assert.Equal(2, result.Value.CompletedJobs);
        Assert.Equal(5, result.Value.TotalJobs);
        Assert.Equal(LibraryScanJobStatus.Running, result.Value.Status);
        Assert.True(result.Value.CurrentJobProgress.HasValue);
        Assert.Equal(40, result.Value.OverallProgressPercentage);
    }

    [Fact]
    public void Create_WhenCalledWithDefaults_ShouldUsePendingStatusAndNoCurrentJobProgress()
    {
        // Act
        Result<MediaLibraryScanProgress> result = MediaLibraryScanProgress.Create(
            _scanIdFixture.Create(),
            _userIdFixture.Create(),
            _libraryIdFixture.Create(),
            completedJobs: 0,
            totalJobs: 1,
            LibraryScanJobStatus.Pending,
            Optional<MediaLibraryScanJobProgress>.None());

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(LibraryScanJobStatus.Pending, result.Value.Status);
        Assert.False(result.Value.CurrentJobProgress.HasValue);
    }

    [Theory]
    [InlineData(0)] // total jobs must be positive
    [InlineData(-1)] // total jobs must be positive
    public void Create_WhenTotalJobsIsNotPositive_ShouldReturnError(int totalJobs)
    {
        // Act
        Result<MediaLibraryScanProgress> result = MediaLibraryScanProgress.Create(
            _scanIdFixture.Create(),
            _userIdFixture.Create(),
            _libraryIdFixture.Create(),
            completedJobs: 0,
            totalJobs: totalJobs,
            LibraryScanJobStatus.Pending,
            Optional<MediaLibraryScanJobProgress>.None());

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.LibraryScanning.TotalScanJobsCountMustBePositive, result.FirstError);
    }

    [Fact]
    public void Create_WhenCompletedJobsIsNegative_ShouldReturnError()
    {
        // Act
        Result<MediaLibraryScanProgress> result = MediaLibraryScanProgress.Create(
            _scanIdFixture.Create(),
            _userIdFixture.Create(),
            _libraryIdFixture.Create(),
            completedJobs: -1,
            totalJobs: 5,
            LibraryScanJobStatus.Pending,
            Optional<MediaLibraryScanJobProgress>.None());

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.LibraryScanning.CompletedScanJobsCountMustBePositive, result.FirstError);
    }

    [Fact]
    public void Create_WhenCompletedJobsExceedsTotalJobs_ShouldReturnError()
    {
        // Act
        Result<MediaLibraryScanProgress> result = MediaLibraryScanProgress.Create(
            _scanIdFixture.Create(),
            _userIdFixture.Create(),
            _libraryIdFixture.Create(),
            completedJobs: 6,
            totalJobs: 5,
            LibraryScanJobStatus.Pending,
            Optional<MediaLibraryScanJobProgress>.None());

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.LibraryScanning.CompletedScanJobsCountCantExceedTotalScanJobsCount, result.FirstError);
    }

    [Fact]
    public void OverallProgressPercentage_WhenNoJobsCompleted_ShouldReturnZero()
    {
        // Arrange
        MediaLibraryScanProgress scanProgress = _mediaLibraryScanProgressFixture.Create(completedJobs: 0, totalJobs: 5);

        // Act
        decimal result = scanProgress.OverallProgressPercentage;

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void Equals_WithSameValues_ShouldReturnTrue()
    {
        // Arrange
        ScanId scanId = _scanIdFixture.Create();
        UserId userId = _userIdFixture.Create();
        LibraryId libraryId = _libraryIdFixture.Create();

        // Act
        MediaLibraryScanProgress firstResult = _mediaLibraryScanProgressFixture.Create(
            scanId: scanId,
            userId: userId,
            libraryId: libraryId,
            completedJobs: 2,
            totalJobs: 5,
            status: LibraryScanJobStatus.Running,
            currentJobProgress: Optional<MediaLibraryScanJobProgress>.None());
        MediaLibraryScanProgress secondResult = _mediaLibraryScanProgressFixture.Create(
            scanId: scanId,
            userId: userId,
            libraryId: libraryId,
            completedJobs: 2,
            totalJobs: 5,
            status: LibraryScanJobStatus.Running,
            currentJobProgress: Optional<MediaLibraryScanJobProgress>.None());

        // Assert
        Assert.Equal(firstResult, secondResult);
    }

    [Fact]
    public void Equals_WithDifferentCompletedJobs_ShouldReturnFalse()
    {
        // Arrange
        ScanId scanId = _scanIdFixture.Create();
        UserId userId = _userIdFixture.Create();
        LibraryId libraryId = _libraryIdFixture.Create();

        // Act
        MediaLibraryScanProgress firstResult = _mediaLibraryScanProgressFixture.Create(
            scanId: scanId,
            userId: userId,
            libraryId: libraryId,
            completedJobs: 2,
            totalJobs: 5,
            status: LibraryScanJobStatus.Running,
            currentJobProgress: Optional<MediaLibraryScanJobProgress>.None());
        MediaLibraryScanProgress secondResult = _mediaLibraryScanProgressFixture.Create(
            scanId: scanId,
            userId: userId,
            libraryId: libraryId,
            completedJobs: 3,
            totalJobs: 5,
            status: LibraryScanJobStatus.Running,
            currentJobProgress: Optional<MediaLibraryScanJobProgress>.None());

        // Assert
        Assert.NotEqual(firstResult, secondResult);
    }

    [Fact]
    public void ToString_WhenCalled_ShouldReturnFormattedProgressString()
    {
        // Arrange
        MediaLibraryScanJobProgress currentJobProgress = _mediaLibraryScanJobProgressFixture.Create(completedItems: 3, totalItems: 10, currentOperation: "Hashing files");
        MediaLibraryScanProgress scanProgress = _mediaLibraryScanProgressFixture.Create(
            completedJobs: 2,
            totalJobs: 5,
            status: LibraryScanJobStatus.Running,
            currentJobProgress: Optional<MediaLibraryScanJobProgress>.Some(currentJobProgress));

        // Act
        string result = scanProgress.ToString();

        // Assert
        Assert.Equal("CompletedJobs: 2; TotalJobs: 5; CurrentJobProgress: CompletedItems: 3; TotalItems: 10; CurrentOperation: Hashing files", result);
    }
}
