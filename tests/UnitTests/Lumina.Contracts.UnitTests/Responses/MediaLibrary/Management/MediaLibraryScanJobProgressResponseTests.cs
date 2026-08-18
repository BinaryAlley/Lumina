#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Fixtures.Core.Responses.MediaLibrary.Management;
using Lumina.Contracts.Responses.MediaLibrary.Management;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
#endregion

namespace Lumina.Contracts.UnitTests.Responses.MediaLibrary.Management;

/// <summary>
/// Contains unit tests for the <see cref="MediaLibraryScanJobProgressResponse"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class MediaLibraryScanJobProgressResponseTests
{
    private readonly MediaLibraryScanJobProgressResponseFixture _mediaLibraryScanJobProgressResponseFixture = new();
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public void RoundTrip_WhenSerializingMediaLibraryScanJobProgressResponse_ShouldPreserveValues()
    {
        // Arrange
        MediaLibraryScanJobProgressResponse expected = _mediaLibraryScanJobProgressResponseFixture.Create();

        // Act
        string json = JsonSerializer.Serialize(expected, _jsonOptions);
        MediaLibraryScanJobProgressResponse? actual = JsonSerializer.Deserialize<MediaLibraryScanJobProgressResponse>(json, _jsonOptions);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Deconstruct_WhenCalled_ShouldReturnAllProperties()
    {
        // Arrange
        MediaLibraryScanJobProgressResponse sut = _mediaLibraryScanJobProgressResponseFixture.Create();

        // Act
        (int completedItems, int totalItems, string currentOperation, decimal progressPercentage) = sut;

        // Assert
        Assert.Equal(sut.CompletedItems, completedItems);
        Assert.Equal(sut.TotalItems, totalItems);
        Assert.Equal(sut.CurrentOperation, currentOperation);
        Assert.Equal(sut.ProgressPercentage, progressPercentage);
    }
}
