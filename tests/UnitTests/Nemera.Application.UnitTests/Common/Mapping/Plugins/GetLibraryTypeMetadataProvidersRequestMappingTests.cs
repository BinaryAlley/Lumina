#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.Mapping.Plugins;
using Nemera.Application.Core.Plugins.Queries.GetLibraryTypeMetadataProviders;
using Nemera.Contracts.Fixtures.Core.Requests.Plugins;
using Nemera.Contracts.Requests.Plugins;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Nemera.Application.UnitTests.Common.Mapping.Plugins;

/// <summary>
/// Contains unit tests for the <see cref="GetLibraryTypeMetadataProvidersRequestMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetLibraryTypeMetadataProvidersRequestMappingTests
{
    private readonly GetLibraryTypeMetadataProvidersRequestFixture _getLibraryTypeMetadataProvidersRequestFixture = new();

    [Fact]
    public void ToQuery_WhenMappingValidRequest_ShouldMapCorrectly()
    {
        // Arrange
        GetLibraryTypeMetadataProvidersRequest request = _getLibraryTypeMetadataProvidersRequestFixture.Create(libraryType: "Music");

        // Act
        GetLibraryTypeMetadataProvidersQuery result = request.ToQuery();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.LibraryType, result.LibraryType);
    }
}
