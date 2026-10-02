#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.Plugins;
using Lumina.Application.Core.Plugins.Commands.SetLibraryMetadataProviderEnabled;
using Lumina.Contracts.Fixtures.Core.Requests.Plugins;
using Lumina.Contracts.Requests.Plugins;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.Plugins;

/// <summary>
/// Contains unit tests for the <see cref="SetLibraryMetadataProviderEnabledRequestMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class SetLibraryMetadataProviderEnabledRequestMappingTests
{
    private readonly SetLibraryMetadataProviderEnabledRequestFixture _setLibraryMetadataProviderEnabledRequestFixture = new();

    [Fact]
    public void ToCommand_WhenMappingValidRequest_ShouldMapCorrectly()
    {
        // Arrange
        SetLibraryMetadataProviderEnabledRequest request = _setLibraryMetadataProviderEnabledRequestFixture.Create(Guid.NewGuid(), Guid.NewGuid(), true);

        // Act
        SetLibraryMetadataProviderEnabledCommand result = request.ToCommand();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.LibraryId, result.LibraryId);
        Assert.Equal(request.PluginId, result.PluginId);
        Assert.Equal(request.IsEnabled, result.IsEnabled);
    }
}
