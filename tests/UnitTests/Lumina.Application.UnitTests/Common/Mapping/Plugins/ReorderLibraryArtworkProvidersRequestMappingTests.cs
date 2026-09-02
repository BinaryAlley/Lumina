#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.Plugins;
using Lumina.Application.Core.Plugins.Commands.ReorderLibraryArtworkProviders;
using Lumina.Contracts.Fixtures.Core.Requests.Plugins;
using Lumina.Contracts.Requests.Plugins;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.Plugins;

/// <summary>
/// Contains unit tests for the <see cref="ReorderLibraryArtworkProvidersRequestMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ReorderLibraryArtworkProvidersRequestMappingTests
{
    private readonly ReorderLibraryArtworkProvidersRequestFixture _reorderLibraryArtworkProvidersRequestFixture = new();

    [Fact]
    public void ToCommand_WhenMappingValidRequest_ShouldMapCorrectly()
    {
        // Arrange
        ReorderLibraryArtworkProvidersRequest request = _reorderLibraryArtworkProvidersRequestFixture.Create();

        // Act
        ReorderLibraryArtworkProvidersCommand result = request.ToCommand();

        // Assert
        Assert.Equal(request.LibraryId, result.LibraryId);
        Assert.Equal(request.PluginIds, result.PluginIds);
    }
}
