#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Fixtures.Core.Responses.MediaLibrary.Management;
using Lumina.Contracts.Responses.MediaLibrary.Management;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
#endregion

namespace Lumina.Contracts.UnitTests.Responses.MediaLibrary.Management;

/// <summary>
/// Contains unit tests for the <see cref="MediaLibraryScanProgressResponse"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class MediaLibraryScanProgressResponseTests
{
    private readonly MediaLibraryScanProgressResponseFixture _mediaLibraryScanProgressResponseFixture = new();
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public void RoundTrip_WhenSerializingMediaLibraryScanProgressResponse_ShouldPreserveValues()
    {
        // Arrange
        MediaLibraryScanProgressResponse expected = _mediaLibraryScanProgressResponseFixture.Create();

        // Act
        string json = JsonSerializer.Serialize(expected, _jsonOptions);
        MediaLibraryScanProgressResponse? actual = JsonSerializer.Deserialize<MediaLibraryScanProgressResponse>(json, _jsonOptions);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void RoundTrip_WhenSerializingWithoutCurrentJobProgress_ShouldPreserveNull()
    {
        // Arrange
        MediaLibraryScanProgressResponse expected = _mediaLibraryScanProgressResponseFixture.Create() with { CurrentJobProgress = null };

        // Act
        string json = JsonSerializer.Serialize(expected, _jsonOptions);
        MediaLibraryScanProgressResponse? actual = JsonSerializer.Deserialize<MediaLibraryScanProgressResponse>(json, _jsonOptions);

        // Assert
        Assert.NotNull(actual);
        Assert.Null(actual.CurrentJobProgress);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Deconstruct_WhenCalled_ShouldReturnAllProperties()
    {
        // Arrange
        MediaLibraryScanProgressResponse sut = _mediaLibraryScanProgressResponseFixture.Create();

        // Act
        (Guid scanId, Guid userId, Guid libraryId, int totalJobs, int completedJobs, MediaLibraryScanJobProgressResponse? currentJobProgress, string status, decimal overallProgressPercentage) = sut;

        // Assert
        Assert.Equal(sut.ScanId, scanId);
        Assert.Equal(sut.UserId, userId);
        Assert.Equal(sut.LibraryId, libraryId);
        Assert.Equal(sut.TotalJobs, totalJobs);
        Assert.Equal(sut.CompletedJobs, completedJobs);
        Assert.Equal(sut.CurrentJobProgress, currentJobProgress);
        Assert.Equal(sut.Status, status);
        Assert.Equal(sut.OverallProgressPercentage, overallProgressPercentage);
    }
}
