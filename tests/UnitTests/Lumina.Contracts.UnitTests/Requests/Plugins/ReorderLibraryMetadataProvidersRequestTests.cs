#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Fixtures.Core.Requests.Plugins;
using Lumina.Contracts.Requests.Plugins;
using System.Collections.Generic;
using System;
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
    private readonly ReorderLibraryMetadataProvidersRequestFixture _reorderLibraryMetadataProvidersRequestFixture = new();

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public void Create_WhenCalled_ShouldReturnValidReorderLibraryMetadataProvidersRequest()
    {
        // Act
        ReorderLibraryMetadataProvidersRequest sut = _reorderLibraryMetadataProvidersRequestFixture.Create();

        // Assert
        Assert.NotNull(sut);
        Assert.NotEqual(Guid.Empty, sut.LibraryId);
        Assert.True(sut.PluginIds.Count > 0);
    }

    [Fact]
    public void Deconstruct_WhenCalled_ShouldReturnAllProperties()
    {
        // Arrange
        ReorderLibraryMetadataProvidersRequest sut = _reorderLibraryMetadataProvidersRequestFixture.Create();

        // Act
        (Guid sutLibraryId, IReadOnlyList<Guid> sutPluginIds) = sut;

        // Assert
        Assert.Equal(sut.LibraryId, sutLibraryId);
        Assert.Equal(sut.PluginIds, sutPluginIds);
    }

    [Fact]
    public void RoundTrip_WhenSerializingReorderLibraryMetadataProvidersRequest_ShouldPreserveValues()
    {
        // Arrange
        ReorderLibraryMetadataProvidersRequest expected = _reorderLibraryMetadataProvidersRequestFixture.Create();

        // Act
        string json = JsonSerializer.Serialize(expected, _jsonOptions);
        ReorderLibraryMetadataProvidersRequest? actual = JsonSerializer.Deserialize<ReorderLibraryMetadataProvidersRequest>(json, _jsonOptions);

        // Assert
        Assert.NotNull(actual);
        Assert.Equivalent(expected, actual);
    }
}
