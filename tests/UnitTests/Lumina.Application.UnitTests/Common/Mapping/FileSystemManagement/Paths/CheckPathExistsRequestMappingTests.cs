#region ========================================================================= USING =====================================================================================
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using Lumina.Application.Common.Mapping.FileSystemManagement.Paths;
using Lumina.Application.Core.FileSystemManagement.Paths.Queries.CheckPathExists;
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
    private readonly IFixture _fixture;

    /// <summary>
    /// Initializes a new instance of the <see cref="CheckPathExistsRequestMappingTests"/> class.
    /// </summary>
    public CheckPathExistsRequestMappingTests()
    {
        _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
    }

    [Fact]
    public void ToQuery_WhenMappingCheckPathExistsRequest_ShouldMapCorrectly()
    {
        // Arrange
        CheckPathExistsRequest request = _fixture.Create<CheckPathExistsRequest>();

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
        List<CheckPathExistsRequest> requests = _fixture.CreateMany<CheckPathExistsRequest>().ToList();

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
