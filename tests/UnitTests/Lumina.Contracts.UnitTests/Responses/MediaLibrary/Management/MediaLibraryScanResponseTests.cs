#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Responses.MediaLibrary.Management;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
#endregion

namespace Lumina.Contracts.UnitTests.Responses.MediaLibrary.Management;

/// <summary>
/// Contains unit tests for the <see cref="MediaLibraryScanResponse"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class MediaLibraryScanResponseTests
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public void RoundTrip_WhenSerializingMediaLibraryScanResponse_ShouldPreserveValues()
    {
        // Arrange
        Guid scanId = Guid.NewGuid();
        Guid libraryId = Guid.NewGuid();
        MediaLibraryScanResponse expected = new(scanId, libraryId);

        // Act
        string json = JsonSerializer.Serialize(expected, _jsonOptions);
        MediaLibraryScanResponse? actual = JsonSerializer.Deserialize<MediaLibraryScanResponse>(json, _jsonOptions);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Deconstruct_WhenCalled_ShouldReturnAllProperties()
    {
        // Arrange
        Guid scanId = Guid.NewGuid();
        Guid libraryId = Guid.NewGuid();
        MediaLibraryScanResponse sut = new(scanId, libraryId);

        // Act
        (Guid sutScanId, Guid sutLibraryId) = sut;

        // Assert
        Assert.Equal(sut.ScanId, sutScanId);
        Assert.Equal(sut.LibraryId, sutLibraryId);
    }
}
