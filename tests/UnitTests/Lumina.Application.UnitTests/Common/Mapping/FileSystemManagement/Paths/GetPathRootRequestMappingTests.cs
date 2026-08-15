#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.FileSystemManagement.Paths;
using Lumina.Application.Core.FileSystemManagement.Paths.Queries.GetPathRoot;
using Lumina.Contracts.Fixtures.Core.Requests.FileSystemManagement.Path;
using Lumina.Contracts.Requests.FileSystemManagement.Path;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.FileSystemManagement.Paths;

/// <summary>
/// Contains unit tests for the <see cref="GetPathRootRequestMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetPathRootRequestMappingTests
{
    private readonly GetPathRootRequestFixture _getPathRootRequestFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetPathRootRequestMappingTests"/> class.
    /// </summary>
    public GetPathRootRequestMappingTests()
    {
    }

    [Fact]
    public void ToQuery_WhenMappingGetPathRootRequest_ShouldMapCorrectly()
    {
        // Arrange
        GetPathRootRequest request = _getPathRootRequestFixture.Create();

        // Act
        GetPathRootQuery result = request.ToQuery();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.Path, result.Path);
    }

    [Theory]
    [InlineData("/home/user/documents")]
    [InlineData("C:\\Users\\Documents")]
    [InlineData("/var/www/html")]
    [InlineData("D:\\Projects\\MyProject")]
    public void ToQuery_WhenMappingWithDifferentPaths_ShouldMapCorrectly(string path)
    {
        // Arrange
        GetPathRootRequest request = new(path);

        // Act
        GetPathRootQuery result = request.ToQuery();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(path, result.Path);
    }

    [Fact]
    public void ToQuery_WhenMappingMultipleRequests_ShouldMapAllCorrectly()
    {
        // Arrange
        List<GetPathRootRequest> requests = _getPathRootRequestFixture.CreateMany();

        // Act
        List<GetPathRootQuery> results = [.. requests.Select(r => r.ToQuery())];

        // Assert
        Assert.NotNull(results);
        Assert.Equal(requests.Count, results.Count);
        for (int i = 0; i < requests.Count; i++)
        {
            Assert.Equal(requests[i].Path, results[i].Path);
        }
    }
}
