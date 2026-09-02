#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.Plugins;
using Lumina.Application.Core.Plugins.Commands.SetLibraryBookReaderEnabled;
using Lumina.Contracts.Fixtures.Core.Requests.Plugins;
using Lumina.Contracts.Requests.Plugins;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.Plugins;

/// <summary>
/// Contains unit tests for the <see cref="SetLibraryBookReaderEnabledRequestMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class SetLibraryBookReaderEnabledRequestMappingTests
{
    private readonly SetLibraryBookReaderEnabledRequestFixture _requestFixture = new();

    [Fact]
    public void ToCommand_WhenMappingValidRequest_ShouldMapCorrectly()
    {
        // Arrange
        SetLibraryBookReaderEnabledRequest request = _requestFixture.Create();

        // Act
        SetLibraryBookReaderEnabledCommand result = request.ToCommand();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.LibraryId, result.LibraryId);
        Assert.Equal(request.PluginId, result.PluginId);
        Assert.Equal(request.IsEnabled, result.IsEnabled);
    }
}
