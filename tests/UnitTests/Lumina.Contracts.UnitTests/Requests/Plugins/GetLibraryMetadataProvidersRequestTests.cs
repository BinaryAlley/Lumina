#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Fixtures.Core.Requests.Plugins;
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
    private readonly GetLibraryMetadataProvidersRequestFixture _getLibraryMetadataProvidersRequestFixture = new();

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public void Create_WhenCalled_ShouldReturnValidGetLibraryMetadataProvidersRequest()
    {
        // Act
        GetLibraryMetadataProvidersRequest sut = _getLibraryMetadataProvidersRequestFixture.Create();

        // Assert
        Assert.NotNull(sut);
        Assert.NotEqual(Guid.Empty, sut.LibraryId);
    }

    [Fact]
    public void Equality_WhenTwoInstancesHaveSameValues_ShouldBeEqual()
    {
        // Arrange
        GetLibraryMetadataProvidersRequest first = _getLibraryMetadataProvidersRequestFixture.Create();
        GetLibraryMetadataProvidersRequest second = first with { };

        // Act
        bool areEqual = first == second;

        // Assert
        Assert.True(areEqual);
    }

    [Fact]
    public void RoundTrip_WhenSerializingGetLibraryMetadataProvidersRequest_ShouldPreserveValues()
    {
        // Arrange
        GetLibraryMetadataProvidersRequest expected = _getLibraryMetadataProvidersRequestFixture.Create();

        // Act
        string json = JsonSerializer.Serialize(expected, _jsonOptions);
        GetLibraryMetadataProvidersRequest? actual = JsonSerializer.Deserialize<GetLibraryMetadataProvidersRequest>(json, _jsonOptions);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(expected, actual);
    }
}
