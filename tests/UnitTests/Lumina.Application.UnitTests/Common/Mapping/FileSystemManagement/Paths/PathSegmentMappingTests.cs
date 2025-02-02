#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.Mapping.FileSystemManagement.Paths;
using Lumina.Contracts.Responses.FileSystemManagement.Path;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.ValueObjects;
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
    [Fact]
    public void ToResponse_WhenMappingPathSegment_ShouldMapCorrectly()
    {
        // Arrange
        ErrorOr<PathSegment> createResult = PathSegment.Create("TestSegment", true, false);
        Assert.False(createResult.IsError);
        PathSegment pathSegment = createResult.Value;

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
        ErrorOr<PathSegment> createResult = PathSegment.Create(name, isDirectory, isDrive);
        Assert.False(createResult.IsError);
        PathSegment pathSegment = createResult.Value;

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
            PathSegment.Create("C:", false, true).Value,
            PathSegment.Create("Users", true, false).Value,
            PathSegment.Create("Documents", true, false).Value,
            PathSegment.Create("file.txt", false, false).Value
        ];

        // Act
        IEnumerable<PathSegmentResponse> results = pathSegments.ToResponses();

        // Assert
        Assert.NotNull(results);
        Assert.Equal(pathSegments.Count, results.Count());
        List<PathSegmentResponse> resultList = results.ToList();
        for (int i = 0; i < pathSegments.Count; i++)
            Assert.Equal(pathSegments[i].Name, resultList[i].Path);
    }
}
