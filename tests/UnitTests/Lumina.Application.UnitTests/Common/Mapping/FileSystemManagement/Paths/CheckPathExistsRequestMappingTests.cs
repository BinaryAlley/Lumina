#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.FileSystemManagement.Paths;
using Lumina.Application.Core.FileSystemManagement.Paths.Queries.CheckPathExists;
using Lumina.Contracts.Fixtures.Core.Requests.FileSystemManagement.Path;
using Lumina.Contracts.Requests.FileSystemManagement.Path;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.FileSystemManagement.Paths;

/// <summary>
/// Contains unit tests for the <see cref="CheckPathExistsRequestMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class CheckPathExistsRequestMappingTests
{
    private readonly CheckPathExistsRequestFixture _checkPathExistsRequestFixture = new();

    [Fact]
    public void ToQuery_WhenMappingCheckPathExistsRequest_ShouldMapCorrectly()
    {
        // Arrange
        CheckPathExistsRequest request = _checkPathExistsRequestFixture.Create();

        // Act
        CheckPathExistsQuery result = request.ToQuery();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.Path, result.Path);
        Assert.Equal(request.IncludeHiddenElements, result.IncludeHiddenElements);
    }

    [Theory]
    [InlineData("/home/user/documents", true)]
    [InlineData("C:\\Users\\Documents", false)]
    [InlineData("/var/www/html", true)]
    public void ToQuery_WhenMappingWithDifferentPathsAndHiddenElementsFlag_ShouldMapCorrectly(string path, bool includeHiddenElements)
    {
        // Arrange
        CheckPathExistsRequest request = new(path, includeHiddenElements);

        // Act
        CheckPathExistsQuery result = request.ToQuery();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(path, result.Path);
        Assert.Equal(includeHiddenElements, result.IncludeHiddenElements);
    }

    [Fact]
    public void ToQuery_WhenMappingMultipleRequests_ShouldMapAllCorrectly()
    {
        // Arrange
        List<CheckPathExistsRequest> requests = _checkPathExistsRequestFixture.CreateMany();

        // Act
        List<CheckPathExistsQuery> results = requests.Select(r => r.ToQuery()).ToList();

        // Assert
        Assert.NotNull(results);
        Assert.Equal(requests.Count, results.Count);
        for (int i = 0; i < requests.Count; i++)
        {
            Assert.Equal(requests[i].Path, results[i].Path);
            Assert.Equal(requests[i].IncludeHiddenElements, results[i].IncludeHiddenElements);
        }
    }
}
