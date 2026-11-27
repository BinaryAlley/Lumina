#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.Mapping.Plugins;
using Nemera.Application.Core.Plugins.Queries.GetLibraryTypeArtworkProviders;
using Nemera.Contracts.Fixtures.Core.Requests.Plugins;
using Nemera.Contracts.Requests.Plugins;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Nemera.Application.UnitTests.Common.Mapping.Plugins;

/// <summary>
/// Contains unit tests for the <see cref="GetLibraryTypeArtworkProvidersRequestMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetLibraryTypeArtworkProvidersRequestMappingTests
{
    private readonly GetLibraryTypeArtworkProvidersRequestFixture _getLibraryTypeArtworkProvidersRequestFixture = new();

    [Fact]
    public void ToQuery_WhenMappingValidRequest_ShouldMapCorrectly()
    {
        // Arrange
        GetLibraryTypeArtworkProvidersRequest request = _getLibraryTypeArtworkProvidersRequestFixture.Create(libraryType: "Music");

        // Act
        GetLibraryTypeArtworkProvidersQuery result = request.ToQuery();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.LibraryType, result.LibraryType);
    }
}
