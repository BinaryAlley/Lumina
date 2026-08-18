#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.UnitTests.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;

/// <summary>
/// Contains unit tests for the <see cref="MediaLibraryScanJobProgress"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MediaLibraryScanJobProgressTests
{
    private readonly MediaLibraryScanJobProgressFixture _mediaLibraryScanJobProgressFixture = new();

    [Fact]
    public void Create_WhenCalledWithValidValues_ShouldCreateJobProgressWithExpectedValues()
    {
        // Act
        Result<MediaLibraryScanJobProgress> result = MediaLibraryScanJobProgress.Create(completedItems: 2, totalItems: 10, "Scanning files");

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(2, result.Value.CompletedItems);
        Assert.Equal(10, result.Value.TotalItems);
        Assert.Equal("Scanning files", result.Value.CurrentOperation);
        Assert.Equal(20, result.Value.ProgressPercentage);
    }

    [Fact]
    public void Create_WhenTotalItemsIsZero_ShouldCreateJobProgressWithZeroPercentage()
    {
        // Act
        Result<MediaLibraryScanJobProgress> result = MediaLibraryScanJobProgress.Create(completedItems: 0, totalItems: 0, "Scanning files");

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(0, result.Value.ProgressPercentage);
    }

    [Fact]
    public void Create_WhenTotalItemsIsNegative_ShouldReturnError()
    {
        // Act
        Result<MediaLibraryScanJobProgress> result = MediaLibraryScanJobProgress.Create(completedItems: 0, totalItems: -1, "Scanning files");

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.LibraryScanning.TotalScanJobItemsCountMustBePositive, result.FirstError);
    }

    [Fact]
    public void Create_WhenCompletedItemsIsNegative_ShouldReturnError()
    {
        // Act
        Result<MediaLibraryScanJobProgress> result = MediaLibraryScanJobProgress.Create(completedItems: -1, totalItems: 10, "Scanning files");

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.LibraryScanning.CompletedScanJobItemsCountMustBePositive, result.FirstError);
    }

    [Fact]
    public void Create_WhenCompletedItemsExceedsTotalItems_ShouldReturnError()
    {
        // Act
        Result<MediaLibraryScanJobProgress> result = MediaLibraryScanJobProgress.Create(completedItems: 11, totalItems: 10, "Scanning files");

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.LibraryScanning.CompletedScanJobItemsCountCantExceedTotalScanJobItemsCount, result.FirstError);
    }

    [Theory]
    [InlineData(null)] // null current operation
    [InlineData("")] // empty current operation
    [InlineData("   ")] // whitespace current operation
    public void Create_WhenCurrentOperationIsEmpty_ShouldReturnError(string? currentOperation)
    {
        // Act
        Result<MediaLibraryScanJobProgress> result = MediaLibraryScanJobProgress.Create(completedItems: 1, totalItems: 10, currentOperation!);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.LibraryScanning.ScanJobCurrentOperationCannotBeEmpty, result.FirstError);
    }

    [Fact]
    public void Equals_WithSameValues_ShouldReturnTrue()
    {
        // Arrange
        MediaLibraryScanJobProgress firstProgress = _mediaLibraryScanJobProgressFixture.Create(completedItems: 2, totalItems: 10, currentOperation: "Scanning files");
        MediaLibraryScanJobProgress secondProgress = _mediaLibraryScanJobProgressFixture.Create(completedItems: 2, totalItems: 10, currentOperation: "Scanning files");

        // Act
        bool result = firstProgress.Equals(secondProgress);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Equals_WithDifferentCurrentOperation_ShouldReturnFalse()
    {
        // Arrange
        MediaLibraryScanJobProgress firstProgress = _mediaLibraryScanJobProgressFixture.Create(completedItems: 2, totalItems: 10, currentOperation: "Scanning files");
        MediaLibraryScanJobProgress secondProgress = _mediaLibraryScanJobProgressFixture.Create(completedItems: 2, totalItems: 10, currentOperation: "Hashing files");

        // Act
        bool result = firstProgress.Equals(secondProgress);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ToString_WhenCalled_ShouldReturnFormattedString()
    {
        // Arrange
        MediaLibraryScanJobProgress jobProgress = _mediaLibraryScanJobProgressFixture.Create(completedItems: 2, totalItems: 10, currentOperation: "Scanning files");

        // Act
        string result = jobProgress.ToString();

        // Assert
        Assert.Equal("CompletedItems: 2; TotalItems: 10; CurrentOperation: Scanning files", result);
    }
}
