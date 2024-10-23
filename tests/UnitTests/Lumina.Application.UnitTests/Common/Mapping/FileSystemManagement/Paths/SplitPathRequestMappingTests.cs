#region ========================================================================= USING =====================================================================================
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using FluentAssertions;
using Lumina.Application.Common.Mapping.FileSystemManagement.Paths;
using Lumina.Application.Core.FileSystemManagement.Paths.Commands.SplitPath;
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
    private readonly IFixture _fixture;

    /// <summary>
    /// Initializes a new instance of the <see cref="SplitPathRequestMappingTests"/> class.
    /// </summary>
    public SplitPathRequestMappingTests()
    {
        _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
    }

    [Fact]
    public void ToCommand_WhenMappingSplitPathRequest_ShouldMapCorrectly()
    {
        // Arrange
        SplitPathRequest request = _fixture.Create<SplitPathRequest>();

        // Act
        SplitPathCommand result = request.ToCommand();

        // Assert
        result.Should().NotBeNull();
        result.Path.Should().Be(request.Path);
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
        result.Should().NotBeNull();
        result.Path.Should().Be(path);
    }

    [Fact]
    public void ToCommand_WhenMappingMultipleRequests_ShouldMapAllCorrectly()
    {
        // Arrange
        List<SplitPathRequest> requests = _fixture.CreateMany<SplitPathRequest>().ToList();

        // Act
        List<SplitPathCommand> results = requests.Select(r => r.ToCommand()).ToList();

        // Assert
        results.Should().NotBeNull();
        results.Should().HaveCount(requests.Count);
        for (int i = 0; i < requests.Count; i++)
            results[i].Path.Should().Be(requests[i].Path);
    }
}