#region ========================================================================= USING =====================================================================================
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using FluentAssertions;
using Lumina.Application.Common.Mapping.FileSystemManagement.Paths;
using Lumina.Application.Core.FileSystemManagement.Paths.Queries.GetPathParent;
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
    private readonly IFixture _fixture;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetPathParentRequestMappingTests"/> class.
    /// </summary>
    public GetPathParentRequestMappingTests()
    {
        _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
    }

    [Fact]
    public void ToQuery_WhenMappingGetPathParentRequest_ShouldMapCorrectly()
    {
        // Arrange
        GetPathParentRequest request = _fixture.Create<GetPathParentRequest>();

        // Act
        GetPathParentQuery result = request.ToQuery();

        // Assert
        result.Should().NotBeNull();
        result.Path.Should().Be(request.Path);
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
        result.Should().NotBeNull();
        result.Path.Should().Be(path);
    }

    [Fact]
    public void ToQuery_WhenMappingMultipleRequests_ShouldMapAllCorrectly()
    {
        // Arrange
        List<GetPathParentRequest> requests = _fixture.CreateMany<GetPathParentRequest>().ToList();

        // Act
        List<GetPathParentQuery> results = requests.Select(r => r.ToQuery()).ToList();

        // Assert
        results.Should().NotBeNull();
        results.Should().HaveCount(requests.Count);
        for (int i = 0; i < requests.Count; i++)
        {
            results[i].Path.Should().Be(requests[i].Path);
        }
    }
}
