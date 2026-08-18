#region ========================================================================= USING =====================================================================================
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
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public void RoundTrip_WhenSerializingGetLibraryRequest_ShouldPreserveValues()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        GetLibraryRequest expected = new(id);

        // Act
        string json = JsonSerializer.Serialize(expected, _jsonOptions);
        GetLibraryRequest? actual = JsonSerializer.Deserialize<GetLibraryRequest>(json, _jsonOptions);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Equality_WhenTwoInstancesHaveSameValues_ShouldBeEqual()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        GetLibraryRequest first = new(id);
        GetLibraryRequest second = new(id);

        // Act
        bool areEqual = first == second;

        // Assert
        Assert.True(areEqual);
    }
}
