#region ========================================================================= USING =====================================================================================
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using FluentAssertions;
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
        result.Should().NotBeNull();
        result.Path.Should().Be(request.Path);
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
        result.Should().NotBeNull();
        result.Path.Should().Be(path);
    }

    [Fact]
    public void ToQuery_WhenMappingMultipleRequests_ShouldMapAllCorrectly()
    {
        // Arrange
        List<ValidatePathRequest> requests = _fixture.CreateMany<ValidatePathRequest>().ToList();

        // Act
        List<ValidatePathQuery> results = requests.Select(r => r.ToQuery()).ToList();

        // Assert
        results.Should().NotBeNull();
        results.Should().HaveCount(requests.Count);
        for (int i = 0; i < requests.Count; i++)
            results[i].Path.Should().Be(requests[i].Path);
    }
}
