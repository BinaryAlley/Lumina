#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.FileSystemManagement.Paths;
using Lumina.Contracts.Responses.FileSystemManagement.Path;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.ValueObjects;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.FileSystemManagement.Paths;

/// <summary>
/// Contains unit tests for the <see cref="PathSegmentMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class PathSegmentMappingTests
{
    private readonly PathSegmentFixture _pathSegmentFixture = new();

    [Fact]
    public void ToResponse_WhenMappingPathSegment_ShouldMapCorrectly()
    {
        // Arrange
        PathSegment pathSegment = _pathSegmentFixture.Create(name: "TestSegment", isDirectory: true, isDrive: false);

        // Act
        PathSegmentResponse result = pathSegment.ToResponse();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(pathSegment.Name, result.Path);
    }

    [Theory]
    [InlineData("Folder", true, false)]
    [InlineData("File.txt", false, false)]
    [InlineData("C:", false, true)]
    public void ToResponse_WhenMappingDifferentPathSegmentTypes_ShouldMapCorrectly(string name, bool isDirectory, bool isDrive)
    {
        // Arrange
        PathSegment pathSegment = _pathSegmentFixture.Create(name: name, isDirectory: isDirectory, isDrive: isDrive);

        // Act
        PathSegmentResponse result = pathSegment.ToResponse();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(pathSegment.Name, result.Path);
    }

    [Fact]
    public void ToResponses_WhenMappingMultiplePathSegments_ShouldMapAllCorrectly()
    {
        // Arrange
        List<PathSegment> pathSegments =
        [
            _pathSegmentFixture.Create(name: "C:", isDirectory: false, isDrive: true),
            _pathSegmentFixture.Create(name: "Users", isDirectory: true, isDrive: false),
            _pathSegmentFixture.Create(name: "Documents", isDirectory: true, isDrive: false),
            _pathSegmentFixture.Create(name: "file.txt", isDirectory: false, isDrive: false)
        ];

        // Act
        IEnumerable<PathSegmentResponse> results = pathSegments.ToResponses();

        // Assert
        Assert.NotNull(results);
        Assert.Equal(pathSegments.Count, results.Count());
        List<PathSegmentResponse> resultList = [.. results];
        for (int i = 0; i < pathSegments.Count; i++)
            Assert.Equal(pathSegments[i].Name, resultList[i].Path);
    }
}
