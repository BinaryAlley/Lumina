#region ========================================================================= USING =====================================================================================
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using Lumina.Application.Common.Mapping.FileSystemManagement.Paths;
using Lumina.Application.Core.FileSystemManagement.Paths.Commands.CombinePath;
using Lumina.Contracts.Requests.FileSystemManagement.Path;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.FileSystemManagement.Paths;

/// <summary>
/// Contains unit tests for the <see cref="CombinePathRequestMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class CombinePathRequestMappingTests
{
    private readonly IFixture _fixture;

    /// <summary>
    /// Initializes a new instance of the <see cref="CombinePathRequestMappingTests"/> class.
    /// </summary>
    public CombinePathRequestMappingTests()
    {
        _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
    }

    [Fact]
    public void ToCommand_WhenMappingCombinePathRequest_ShouldMapCorrectly()
    {
        // Arrange
        CombinePathRequest request = _fixture.Create<CombinePathRequest>();

        // Act
        CombinePathCommand result = request.ToCommand();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.OriginalPath, result.OriginalPath);
        Assert.Equal(request.NewPath, result.NewPath);
    }

    [Theory]
    [InlineData("/home/user", "documents")]
    [InlineData("C:\\Users", "Documents")]
    [InlineData("/var/www", "html")]
    public void ToCommand_WhenMappingWithDifferentPaths_ShouldMapCorrectly(string originalPath, string newPath)
    {
        // Arrange
        CombinePathRequest request = new(originalPath, newPath);

        // Act
        CombinePathCommand result = request.ToCommand();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(originalPath, result.OriginalPath);
        Assert.Equal(newPath, result.NewPath);
    }

    [Fact]
    public void ToCommand_WhenMappingMultipleRequests_ShouldMapAllCorrectly()
    {
        // Arrange
        List<CombinePathRequest> requests = _fixture.CreateMany<CombinePathRequest>().ToList();

        // Act
        List<CombinePathCommand> results = requests.Select(r => r.ToCommand()).ToList();

        // Assert
        Assert.NotNull(results);
        Assert.Equal(requests.Count, results.Count);
        for (int i = 0; i < requests.Count; i++)
        {
            Assert.Equal(requests[i].OriginalPath, results[i].OriginalPath);
            Assert.Equal(requests[i].NewPath, results[i].NewPath);
        }
    }
}
