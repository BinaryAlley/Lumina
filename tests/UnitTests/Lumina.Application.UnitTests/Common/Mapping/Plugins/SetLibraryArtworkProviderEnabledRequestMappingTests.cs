#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.Plugins;
using Lumina.Application.Core.Plugins.Commands.SetLibraryArtworkProviderEnabled;
using Lumina.Contracts.Fixtures.Core.Requests.Plugins;
using Lumina.Contracts.Requests.Plugins;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.Plugins;

/// <summary>
/// Contains unit tests for the <see cref="SetLibraryArtworkProviderEnabledRequestMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class SetLibraryArtworkProviderEnabledRequestMappingTests
{
    private readonly SetLibraryArtworkProviderEnabledRequestFixture _setLibraryArtworkProviderEnabledRequestFixture = new();

    [Fact]
    public void ToCommand_WhenMappingValidRequest_ShouldMapCorrectly()
    {
        // Arrange
        SetLibraryArtworkProviderEnabledRequest request = _setLibraryArtworkProviderEnabledRequestFixture.Create(isEnabled: true);

        // Act
        SetLibraryArtworkProviderEnabledCommand result = request.ToCommand();

        // Assert
        Assert.Equal(request.LibraryId, result.LibraryId);
        Assert.Equal(request.PluginId, result.PluginId);
        Assert.Equal(request.IsEnabled, result.IsEnabled);
    }
}
