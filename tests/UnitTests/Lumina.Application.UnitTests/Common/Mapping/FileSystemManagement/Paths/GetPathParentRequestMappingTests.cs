#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.FileSystemManagement.Paths;
using Lumina.Application.Core.FileSystemManagement.Paths.Queries.GetPathParent;
using Lumina.Contracts.Fixtures.Core.Requests.FileSystemManagement.Path;
using Lumina.Contracts.Requests.FileSystemManagement.Path;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.FileSystemManagement.Paths;

/// <summary>
/// Contains unit tests for the <see cref="GetPathParentRequestMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetPathParentRequestMappingTests
{
    private readonly GetPathParentRequestFixture _getPathParentRequestFixture = new();

    [Fact]
    public void ToQuery_WhenMappingGetPathParentRequest_ShouldMapCorrectly()
    {
        // Arrange
        GetPathParentRequest request = _getPathParentRequestFixture.Create();

        // Act
        GetPathParentQuery result = request.ToQuery();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.Path, result.Path);
    }

    [Theory]
    [InlineData("/home/user/documents")]
    [InlineData("C:\\Users\\Documents")]
    [InlineData("/var/www/html")]
    public void ToQuery_WhenMappingWithDifferentPaths_ShouldMapCorrectly(string path)
    {
        // Arrange
        GetPathParentRequest request = new(path);

        // Act
        GetPathParentQuery result = request.ToQuery();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(path, result.Path);
    }

    [Fact]
    public void ToQuery_WhenMappingMultipleRequests_ShouldMapAllCorrectly()
    {
        // Arrange
        List<GetPathParentRequest> requests = _getPathParentRequestFixture.CreateMany();

        // Act
        List<GetPathParentQuery> results = [.. requests.Select(r => r.ToQuery())];

        // Assert
        Assert.NotNull(results);
        Assert.Equal(requests.Count, results.Count);
        for (int i = 0; i < requests.Count; i++)
        {
            Assert.Equal(requests[i].Path, results[i].Path);
        }
    }
}
