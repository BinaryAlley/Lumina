#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Responses.Plugins;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
#endregion

namespace Lumina.Contracts.UnitTests.Responses.Plugins;

/// <summary>
/// Contains unit tests for the <see cref="LibraryMetadataProviderResponse"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class LibraryMetadataProviderResponseTests
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public void RoundTrip_WhenSerializingProviderResponse_ShouldPreserveValues()
    {
        // Arrange
        Guid pluginId = Guid.NewGuid();
        LibraryMetadataProviderResponse expected = new(pluginId, "OpenLibrary", true, 1);

        // Act
        string json = JsonSerializer.Serialize(expected, _jsonOptions);
        LibraryMetadataProviderResponse? actual = JsonSerializer.Deserialize<LibraryMetadataProviderResponse>(json, _jsonOptions);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Deconstruct_WhenCalled_ShouldReturnAllProperties()
    {
        // Arrange
        Guid pluginId = Guid.NewGuid();
        LibraryMetadataProviderResponse sut = new(pluginId, "OpenLibrary", false, 2);

        // Act
        (Guid sutPluginId, string name, bool isEnabled, int rank) = sut;

        // Assert
        Assert.Equal(sut.PluginId, sutPluginId);
        Assert.Equal(sut.Name, name);
        Assert.Equal(sut.IsEnabled, isEnabled);
        Assert.Equal(sut.Rank, rank);
    }
}
