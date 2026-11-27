#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.Mapping.Plugins;
using Nemera.Application.Core.Plugins.Queries.GetLibraryBookReaders;
using Nemera.Contracts.Fixtures.Core.Requests.Plugins;
using Nemera.Contracts.Requests.Plugins;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Nemera.Application.UnitTests.Common.Mapping.Plugins;

/// <summary>
/// Contains unit tests for the <see cref="GetLibraryBookReadersRequestMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetLibraryBookReadersRequestMappingTests
{
    private readonly GetLibraryBookReadersRequestFixture _getLibraryBookReadersRequestFixture = new();

    [Fact]
    public void ToQuery_WhenMappingValidRequest_ShouldMapCorrectly()
    {
        // Arrange
        GetLibraryBookReadersRequest request = _getLibraryBookReadersRequestFixture.Create();

        // Act
        GetLibraryBookReadersQuery result = request.ToQuery();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.LibraryId, result.LibraryId);
    }
}
