#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.Mapping.Plugins;
using Nemera.Application.Core.Plugins.Queries.GetLibraryMetadataProviders;
using Nemera.Contracts.Fixtures.Core.Requests.Plugins;
using Nemera.Contracts.Requests.Plugins;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Nemera.Application.UnitTests.Common.Mapping.Plugins;

/// <summary>
/// Contains unit tests for the <see cref="GetLibraryMetadataProvidersRequestMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetLibraryMetadataProvidersRequestMappingTests
{
    private readonly GetLibraryMetadataProvidersRequestFixture _getLibraryMetadataProvidersRequestFixture = new();

    [Fact]
    public void ToQuery_WhenMappingValidRequest_ShouldMapCorrectly()
    {
        // Arrange
        GetLibraryMetadataProvidersRequest request = _getLibraryMetadataProvidersRequestFixture.Create(Guid.NewGuid());

        // Act
        GetLibraryMetadataProvidersQuery result = request.ToQuery();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.LibraryId, result.LibraryId);
    }
}
