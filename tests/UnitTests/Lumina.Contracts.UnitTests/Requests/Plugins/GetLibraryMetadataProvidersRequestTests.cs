#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Requests.Plugins;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
#endregion

namespace Lumina.Contracts.UnitTests.Requests.Plugins;

/// <summary>
/// Contains unit tests for the <see cref="GetLibraryMetadataProvidersRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetLibraryMetadataProvidersRequestTests
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public void RoundTrip_WhenSerializingGetLibraryMetadataProvidersRequest_ShouldPreserveValues()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        GetLibraryMetadataProvidersRequest expected = new(libraryId);

        // Act
        string json = JsonSerializer.Serialize(expected, _jsonOptions);
        GetLibraryMetadataProvidersRequest? actual = JsonSerializer.Deserialize<GetLibraryMetadataProvidersRequest>(json, _jsonOptions);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Equality_WhenTwoInstancesHaveSameValues_ShouldBeEqual()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        GetLibraryMetadataProvidersRequest first = new(libraryId);
        GetLibraryMetadataProvidersRequest second = new(libraryId);

        // Act
        bool areEqual = first == second;

        // Assert
        Assert.True(areEqual);
    }
}
