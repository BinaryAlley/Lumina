#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Fixtures.Core.Requests.Plugins;
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
    private readonly SetLibraryMetadataProviderEnabledRequestFixture _setLibraryMetadataProviderEnabledRequestFixture = new();

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public void Create_WhenCalled_ShouldReturnValidSetLibraryMetadataProviderEnabledRequest()
    {
        // Act
        SetLibraryMetadataProviderEnabledRequest sut = _setLibraryMetadataProviderEnabledRequestFixture.Create();

        // Assert
        Assert.NotNull(sut);
        Assert.NotEqual(Guid.Empty, sut.LibraryId);
        Assert.NotEqual(Guid.Empty, sut.PluginId);
    }

    [Fact]
    public void Deconstruct_WhenCalled_ShouldReturnAllProperties()
    {
        // Arrange
        SetLibraryMetadataProviderEnabledRequest sut = _setLibraryMetadataProviderEnabledRequestFixture.Create();

        // Act
        (Guid sutLibraryId, Guid sutPluginId, bool isEnabled) = sut;

        // Assert
        Assert.Equal(sut.LibraryId, sutLibraryId);
        Assert.Equal(sut.PluginId, sutPluginId);
        Assert.Equal(sut.IsEnabled, isEnabled);
    }

    [Fact]
    public void RoundTrip_WhenSerializingSetLibraryMetadataProviderEnabledRequest_ShouldPreserveValues()
    {
        // Arrange
        SetLibraryMetadataProviderEnabledRequest expected = _setLibraryMetadataProviderEnabledRequestFixture.Create();

        // Act
        string json = JsonSerializer.Serialize(expected, _jsonOptions);
        SetLibraryMetadataProviderEnabledRequest? actual = JsonSerializer.Deserialize<SetLibraryMetadataProviderEnabledRequest>(json, _jsonOptions);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(expected, actual);
    }
}
