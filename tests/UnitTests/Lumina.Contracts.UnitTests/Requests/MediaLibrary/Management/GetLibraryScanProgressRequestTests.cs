#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Fixtures.Core.Requests.MediaLibrary.Management;
using Lumina.Contracts.Requests.MediaLibrary.Management;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
#endregion

namespace Lumina.Contracts.UnitTests.Requests.MediaLibrary.Management;

/// <summary>
/// Contains unit tests for the <see cref="GetLibraryScanProgressRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetLibraryScanProgressRequestTests
{
    private readonly GetLibraryScanProgressRequestFixture _getLibraryScanProgressRequestFixture = new();

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public void Create_WhenCalled_ShouldReturnValidGetLibraryScanProgressRequest()
    {
        // Act
        GetLibraryScanProgressRequest sut = _getLibraryScanProgressRequestFixture.Create();

        // Assert
        Assert.NotNull(sut);
        Assert.NotEqual(Guid.Empty, sut.LibraryId);
        Assert.NotEqual(Guid.Empty, sut.ScanId);
    }

    [Fact]
    public void Equality_WhenTwoInstancesHaveSameValues_ShouldBeEqual()
    {
        // Arrange
        GetLibraryScanProgressRequest first = _getLibraryScanProgressRequestFixture.Create();
        GetLibraryScanProgressRequest second = first with { };

        // Act
        bool areEqual = first == second;

        // Assert
        Assert.True(areEqual);
    }

    [Fact]
    public void RoundTrip_WhenSerializingGetLibraryScanProgressRequest_ShouldPreserveValues()
    {
        // Arrange
        GetLibraryScanProgressRequest expected = _getLibraryScanProgressRequestFixture.Create();

        // Act
        string json = JsonSerializer.Serialize(expected, _jsonOptions);
        GetLibraryScanProgressRequest? actual = JsonSerializer.Deserialize<GetLibraryScanProgressRequest>(json, _jsonOptions);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(expected, actual);
    }
}
