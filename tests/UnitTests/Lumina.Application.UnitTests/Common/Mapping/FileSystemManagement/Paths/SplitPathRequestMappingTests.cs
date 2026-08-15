#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.FileSystemManagement.Paths;
using Lumina.Application.Core.FileSystemManagement.Paths.Commands.SplitPath;
using Lumina.Contracts.Fixtures.Core.Requests.FileSystemManagement.Path;
using Lumina.Contracts.Requests.FileSystemManagement.Path;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.FileSystemManagement.Paths;

/// <summary>
/// Contains unit tests for the <see cref="SplitPathRequestMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class SplitPathRequestMappingTests
{
    private readonly SplitPathRequestFixture _splitPathRequestFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="SplitPathRequestMappingTests"/> class.
    /// </summary>
    public SplitPathRequestMappingTests()
    {
    }

    [Fact]
    public void ToCommand_WhenMappingSplitPathRequest_ShouldMapCorrectly()
    {
        // Arrange
        SplitPathRequest request = _splitPathRequestFixture.Create();

        // Act
        SplitPathCommand result = request.ToCommand();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.Path, result.Path);
    }

    [Theory]
    [InlineData("/home/user/documents")]
    [InlineData("C:\\Users\\Documents")]
    [InlineData("/var/www/html")]
    [InlineData("D:\\Projects\\MyProject\\file.txt")]
    public void ToCommand_WhenMappingWithDifferentPaths_ShouldMapCorrectly(string path)
    {
        // Arrange
        SplitPathRequest request = new(path);

        // Act
        SplitPathCommand result = request.ToCommand();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(path, result.Path);
    }

    [Fact]
    public void ToCommand_WhenMappingMultipleRequests_ShouldMapAllCorrectly()
    {
        // Arrange
        List<SplitPathRequest> requests = _splitPathRequestFixture.CreateMany();

        // Act
        List<SplitPathCommand> results = [.. requests.Select(r => r.ToCommand())];

        // Assert
        Assert.NotNull(results);
        Assert.Equal(requests.Count, results.Count);
        for (int i = 0; i < requests.Count; i++)
            Assert.Equal(requests[i].Path, results[i].Path);
    }
}
