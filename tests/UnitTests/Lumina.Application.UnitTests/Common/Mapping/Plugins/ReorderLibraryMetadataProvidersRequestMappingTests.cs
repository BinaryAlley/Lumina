#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.Plugins;
using Lumina.Application.Core.Plugins.Commands.ReorderLibraryMetadataProviders;
using Lumina.Contracts.Requests.Plugins;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.Plugins;

/// <summary>
/// Contains unit tests for the <see cref="ReorderLibraryMetadataProvidersRequestMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ReorderLibraryMetadataProvidersRequestMappingTests
{
    [Fact]
    public void ToCommand_WhenMappingValidRequest_ShouldMapCorrectly()
    {
        // Arrange
        ReorderLibraryMetadataProvidersRequest request = new(LibraryId: Guid.NewGuid(), PluginIds: [Guid.NewGuid(), Guid.NewGuid()]);

        // Act
        ReorderLibraryMetadataProvidersCommand result = request.ToCommand();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.LibraryId, result.LibraryId);
        Assert.Equal(request.PluginIds, result.PluginIds);
    }
}
