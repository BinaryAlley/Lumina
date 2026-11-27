#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.Mapping.Plugins;
using Nemera.Application.Core.Plugins.Queries.GetLibraryArtworkProviders;
using Nemera.Contracts.Fixtures.Core.Requests.Plugins;
using Nemera.Contracts.Requests.Plugins;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Nemera.Application.UnitTests.Common.Mapping.Plugins;

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
