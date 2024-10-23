#region ========================================================================= USING =====================================================================================
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using FluentAssertions;
using Lumina.Application.Common.Mapping.FileSystemManagement.Paths;
using Lumina.Application.Core.FileSystemManagement.Paths.Queries.CheckPathExists;
using Lumina.Contracts.Requests.FileSystemManagement.Path;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.FileSystemManagement.Paths;

/// <summary>
/// Contains unit tests for the <see cref="CheckPathExistsQueryMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class CheckPathExistsQueryMappingTests
{
    private readonly IFixture _fixture;

    /// <summary>
    /// Initializes a new instance of the <see cref="CheckPathExistsQueryMappingTests"/> class.
    /// </summary>
    public CheckPathExistsQueryMappingTests()
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
        result.Should().NotBeNull();
        result.Path.Should().Be(request.Path);
        result.IncludeHiddenElements.Should().Be(request.IncludeHiddenElements);
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
        result.Should().NotBeNull();
        result.Path.Should().Be(path);
        result.IncludeHiddenElements.Should().Be(includeHiddenElements);
    }

    [Fact]
    public void ToQuery_WhenMappingMultipleRequests_ShouldMapAllCorrectly()
    {
        // Arrange
        List<CheckPathExistsRequest> requests = _fixture.CreateMany<CheckPathExistsRequest>().ToList();

        // Act
        List<CheckPathExistsQuery> results = requests.Select(r => r.ToQuery()).ToList();

        // Assert
        results.Should().NotBeNull();
        results.Should().HaveCount(requests.Count);
        for (int i = 0; i < requests.Count; i++)
        {
            results[i].Path.Should().Be(requests[i].Path);
            results[i].IncludeHiddenElements.Should().Be(requests[i].IncludeHiddenElements);
        }
    }
}
