#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.Plugins;
using Lumina.Application.Core.Plugins.Queries.GetLibraryArtworkProviders;
using Lumina.Contracts.Fixtures.Core.Requests.Plugins;
using Lumina.Contracts.Requests.Plugins;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.Plugins;

/// <summary>
/// Contains unit tests for the <see cref="GetLibraryArtworkProvidersRequestMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetLibraryArtworkProvidersRequestMappingTests
{
    private readonly GetLibraryArtworkProvidersRequestFixture _getLibraryArtworkProvidersRequestFixture = new();

    [Fact]
    public void ToQuery_WhenMappingValidRequest_ShouldMapCorrectly()
    {
        // Arrange
        GetLibraryArtworkProvidersRequest request = _getLibraryArtworkProvidersRequestFixture.Create();

        // Act
        GetLibraryArtworkProvidersQuery result = request.ToQuery();

        // Assert
        Assert.Equal(request.LibraryId, result.LibraryId);
    }
}
