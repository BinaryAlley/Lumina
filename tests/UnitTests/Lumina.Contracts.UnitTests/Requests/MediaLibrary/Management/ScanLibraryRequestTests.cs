#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Fixtures.Core.Requests.MediaLibrary.Management;
using Lumina.Contracts.Requests.MediaLibrary.Management;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
#endregion

namespace Lumina.Contracts.UnitTests.Requests.MediaLibrary.Management;

/// <summary>
/// Contains unit tests for the <see cref="ScanLibraryRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class ScanLibraryRequestTests
{
    private readonly ScanLibraryRequestFixture _scanLibraryRequestFixture = new();

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public void Create_WhenCalled_ShouldReturnValidScanLibraryRequest()
    {
        // Act
        ScanLibraryRequest sut = _scanLibraryRequestFixture.Create();

        // Assert
        Assert.NotNull(sut);
        Assert.NotEqual(Guid.Empty, sut.Id);
    }

    [Fact]
    public void Equality_WhenTwoInstancesHaveSameValues_ShouldBeEqual()
    {
        // Arrange
        ScanLibraryRequest first = _scanLibraryRequestFixture.Create();
        ScanLibraryRequest second = first with { };

        // Act
        bool areEqual = first == second;

        // Assert
        Assert.True(areEqual);
    }

    [Fact]
    public void RoundTrip_WhenSerializingScanLibraryRequest_ShouldPreserveValues()
    {
        // Arrange
        ScanLibraryRequest expected = _scanLibraryRequestFixture.Create();

        // Act
        string json = JsonSerializer.Serialize(expected, _jsonOptions);
        ScanLibraryRequest? actual = JsonSerializer.Deserialize<ScanLibraryRequest>(json, _jsonOptions);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(expected, actual);
    }
}
