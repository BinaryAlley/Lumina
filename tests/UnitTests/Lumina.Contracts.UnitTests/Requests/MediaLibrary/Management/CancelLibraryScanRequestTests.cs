#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Fixtures.Core.Requests.MediaLibrary.Management;
using Lumina.Contracts.Requests.MediaLibrary.Management;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
#endregion

namespace Lumina.Contracts.UnitTests.Requests.MediaLibrary.Management;

/// <summary>
/// Contains unit tests for the <see cref="CancelLibraryScanRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class CancelLibraryScanRequestTests
{
    private readonly CancelLibraryScanRequestFixture _cancelLibraryScanRequestFixture = new();

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public void Create_WhenCalled_ShouldReturnValidCancelLibraryScanRequest()
    {
        // Act
        CancelLibraryScanRequest sut = _cancelLibraryScanRequestFixture.Create();

        // Assert
        Assert.NotNull(sut);
        Assert.NotEqual(Guid.Empty, sut.LibraryId);
        Assert.NotEqual(Guid.Empty, sut.ScanId);
    }

    [Fact]
    public void Equality_WhenTwoInstancesHaveSameValues_ShouldBeEqual()
    {
        // Arrange
        CancelLibraryScanRequest first = _cancelLibraryScanRequestFixture.Create();
        CancelLibraryScanRequest second = first with { };

        // Act
        bool areEqual = first == second;

        // Assert
        Assert.True(areEqual);
    }

    [Fact]
    public void RoundTrip_WhenSerializingCancelLibraryScanRequest_ShouldPreserveValues()
    {
        // Arrange
        CancelLibraryScanRequest expected = _cancelLibraryScanRequestFixture.Create();

        // Act
        string json = JsonSerializer.Serialize(expected, _jsonOptions);
        CancelLibraryScanRequest? actual = JsonSerializer.Deserialize<CancelLibraryScanRequest>(json, _jsonOptions);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(expected, actual);
    }
}
