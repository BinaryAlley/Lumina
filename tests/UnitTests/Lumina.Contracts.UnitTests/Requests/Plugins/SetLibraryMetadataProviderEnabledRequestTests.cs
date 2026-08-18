#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Requests.Plugins;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
#endregion

namespace Lumina.Contracts.UnitTests.Requests.Plugins;

/// <summary>
/// Contains unit tests for the <see cref="SetLibraryMetadataProviderEnabledRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class SetLibraryMetadataProviderEnabledRequestTests
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public void RoundTrip_WhenSerializingSetLibraryMetadataProviderEnabledRequest_ShouldPreserveValues()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid pluginId = Guid.NewGuid();
        SetLibraryMetadataProviderEnabledRequest expected = new(libraryId, pluginId, true);

        // Act
        string json = JsonSerializer.Serialize(expected, _jsonOptions);
        SetLibraryMetadataProviderEnabledRequest? actual = JsonSerializer.Deserialize<SetLibraryMetadataProviderEnabledRequest>(json, _jsonOptions);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Deconstruct_WhenCalled_ShouldReturnAllProperties()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid pluginId = Guid.NewGuid();
        SetLibraryMetadataProviderEnabledRequest sut = new(libraryId, pluginId, false);

        // Act
        (Guid sutLibraryId, Guid sutPluginId, bool isEnabled) = sut;

        // Assert
        Assert.Equal(sut.LibraryId, sutLibraryId);
        Assert.Equal(sut.PluginId, sutPluginId);
        Assert.Equal(sut.IsEnabled, isEnabled);
    }
}
