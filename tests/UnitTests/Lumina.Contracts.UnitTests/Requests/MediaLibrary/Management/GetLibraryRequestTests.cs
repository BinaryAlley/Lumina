#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Fixtures.Core.Requests.MediaLibrary.Management;
using Lumina.Contracts.Requests.MediaLibrary.Management;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
#endregion

namespace Lumina.Contracts.UnitTests.Requests.MediaLibrary.Management;

/// <summary>
/// Contains unit tests for the <see cref="GetLibraryRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetLibraryRequestTests
{
    private readonly GetLibraryRequestFixture _getLibraryRequestFixture = new();

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public void Create_WhenCalled_ShouldReturnValidGetLibraryRequest()
    {
        // Act
        GetLibraryRequest sut = _getLibraryRequestFixture.Create();

        // Assert
        Assert.NotNull(sut);
        Assert.NotEqual(Guid.Empty, sut.Id);
    }

    [Fact]
    public void Equality_WhenTwoInstancesHaveSameValues_ShouldBeEqual()
    {
        // Arrange
        GetLibraryRequest first = _getLibraryRequestFixture.Create();
        GetLibraryRequest second = first with { };

        // Act
        bool areEqual = first == second;

        // Assert
        Assert.True(areEqual);
    }

    [Fact]
    public void RoundTrip_WhenSerializingGetLibraryRequest_ShouldPreserveValues()
    {
        // Arrange
        GetLibraryRequest expected = _getLibraryRequestFixture.Create();

        // Act
        string json = JsonSerializer.Serialize(expected, _jsonOptions);
        GetLibraryRequest? actual = JsonSerializer.Deserialize<GetLibraryRequest>(json, _jsonOptions);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(expected, actual);
    }
}
