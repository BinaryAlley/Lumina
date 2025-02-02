#region ========================================================================= USING =====================================================================================
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using Lumina.Application.Common.Mapping.FileSystemManagement.Paths;
using Lumina.Application.Core.FileSystemManagement.Paths.Queries.ValidatePath;
using Lumina.Contracts.Requests.FileSystemManagement.Path;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.FileSystemManagement.Paths;

/// <summary>
/// Contains unit tests for the <see cref="ValidatePathRequestMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ValidatePathRequestMappingTests
{
    private readonly IFixture _fixture;

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidatePathRequestMappingTests"/> class.
    /// </summary>
    public ValidatePathRequestMappingTests()
    {
        _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
    }

    [Fact]
    public void ToQuery_WhenMappingValidatePathRequest_ShouldMapCorrectly()
    {
        // Arrange
        ValidatePathRequest request = _fixture.Create<ValidatePathRequest>();

        // Act
        ValidatePathQuery result = request.ToQuery();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.Path, result.Path);
    }

    [Theory]
    [InlineData("/home/user/documents")]
    [InlineData("C:\\Users\\Documents")]
    [InlineData("/var/www/html")]
    [InlineData("D:\\Projects\\MyProject\\file.txt")]
    [InlineData("\\\\NetworkShare\\Folder")]
    [InlineData("relative/path/to/file.txt")]
    public void ToQuery_WhenMappingWithDifferentPaths_ShouldMapCorrectly(string path)
    {
        // Arrange
        ValidatePathRequest request = new(path);

        // Act
        ValidatePathQuery result = request.ToQuery();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(path, result.Path);
    }

    [Fact]
    public void ToQuery_WhenMappingMultipleRequests_ShouldMapAllCorrectly()
    {
        // Arrange
        List<ValidatePathRequest> requests = _fixture.CreateMany<ValidatePathRequest>().ToList();

        // Act
        List<ValidatePathQuery> results = requests.Select(r => r.ToQuery()).ToList();

        // Assert
        Assert.NotNull(results);
        Assert.Equal(requests.Count, results.Count);
        for (int i = 0; i < requests.Count; i++)
            Assert.Equal(requests[i].Path, results[i].Path);
    }
}
