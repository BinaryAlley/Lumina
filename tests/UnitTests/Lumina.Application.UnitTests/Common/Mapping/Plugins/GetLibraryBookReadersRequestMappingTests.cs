#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.Plugins;
using Lumina.Application.Core.Plugins.Queries.GetLibraryBookReaders;
using Lumina.Contracts.Fixtures.Core.Requests.Plugins;
using Lumina.Contracts.Requests.Plugins;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.Plugins;

/// <summary>
/// Contains unit tests for the <see cref="GetLibraryBookReadersRequestMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetLibraryBookReadersRequestMappingTests
{
    private readonly GetLibraryBookReadersRequestFixture _requestFixture = new();

    [Fact]
    public void ToQuery_WhenMappingValidRequest_ShouldMapCorrectly()
    {
        // Arrange
        GetLibraryBookReadersRequest request = _requestFixture.Create();

        // Act
        GetLibraryBookReadersQuery result = request.ToQuery();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.LibraryId, result.LibraryId);
    }
}
