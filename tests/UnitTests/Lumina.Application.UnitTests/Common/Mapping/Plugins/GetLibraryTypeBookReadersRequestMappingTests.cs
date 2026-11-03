#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.Plugins;
using Lumina.Application.Core.Plugins.Queries.GetLibraryTypeBookReaders;
using Lumina.Contracts.Fixtures.Core.Requests.Plugins;
using Lumina.Contracts.Requests.Plugins;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.Plugins;

/// <summary>
/// Contains unit tests for the <see cref="GetLibraryTypeBookReadersRequestMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetLibraryTypeBookReadersRequestMappingTests
{
    private readonly GetLibraryTypeBookReadersRequestFixture _getLibraryTypeBookReadersRequestFixture = new();

    [Fact]
    public void ToQuery_WhenMappingValidRequest_ShouldMapCorrectly()
    {
        // Arrange
        GetLibraryTypeBookReadersRequest request = _getLibraryTypeBookReadersRequestFixture.Create(libraryType: "Music");

        // Act
        GetLibraryTypeBookReadersQuery result = request.ToQuery();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.LibraryType, result.LibraryType);
    }
}
