#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Requests.Plugins;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
#endregion

namespace Lumina.Contracts.UnitTests.Requests.Plugins;

/// <summary>
/// Contains unit tests for the <see cref="ReorderLibraryMetadataProvidersRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class ReorderLibraryMetadataProvidersRequestTests
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public void RoundTrip_WhenSerializingReorderLibraryMetadataProvidersRequest_ShouldPreserveValues()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        List<Guid> pluginIds = [Guid.NewGuid(), Guid.NewGuid()];
        ReorderLibraryMetadataProvidersRequest expected = new(libraryId, pluginIds);

        // Act
        string json = JsonSerializer.Serialize(expected, _jsonOptions);
        ReorderLibraryMetadataProvidersRequest? actual = JsonSerializer.Deserialize<ReorderLibraryMetadataProvidersRequest>(json, _jsonOptions);
        // Assert
        Assert.NotNull(actual);
        Assert.Equivalent(expected, actual);
    }

    [Fact]
    public void Deconstruct_WhenCalled_ShouldReturnAllProperties()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        List<Guid> pluginIds = [Guid.NewGuid()];
        ReorderLibraryMetadataProvidersRequest sut = new(libraryId, pluginIds);

        // Act
        (Guid sutLibraryId, IReadOnlyList<Guid> sutPluginIds) = sut;

        // Assert
        Assert.Equal(sut.LibraryId, sutLibraryId);
        Assert.Equal(sut.PluginIds, sutPluginIds);
    }
}
