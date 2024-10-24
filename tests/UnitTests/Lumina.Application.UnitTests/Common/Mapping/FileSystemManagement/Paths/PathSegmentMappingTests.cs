#region ========================================================================= USING =====================================================================================
using ErrorOr;
using FluentAssertions;
using Lumina.Application.Common.Mapping.FileSystemManagement.Paths;
using Lumina.Contracts.Responses.FileSystemManagement.Path;
using Lumina.Domain.Core.Aggregates.FileSystemManagement.FileSystemManagementAggregate.ValueObjects;
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
        createResult.IsError.Should().BeFalse();
        PathSegment pathSegment = createResult.Value;

        // Act
        PathSegmentResponse result = pathSegment.ToResponse();

        // Assert
        result.Should().NotBeNull();
        result.Path.Should().Be(pathSegment.Name);
    }

    [Theory]
    [InlineData("Folder", true, false)]
    [InlineData("File.txt", false, false)]
    [InlineData("C:", false, true)]
    public void ToResponse_WhenMappingDifferentPathSegmentTypes_ShouldMapCorrectly(string name, bool isDirectory, bool isDrive)
    {
        // Arrange
        ErrorOr<PathSegment> createResult = PathSegment.Create(name, isDirectory, isDrive);
        createResult.IsError.Should().BeFalse();
        PathSegment pathSegment = createResult.Value;

        // Act
        PathSegmentResponse result = pathSegment.ToResponse();

        // Assert
        result.Should().NotBeNull();
        result.Path.Should().Be(pathSegment.Name);
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
        results.Should().NotBeNull();
        results.Should().HaveCount(pathSegments.Count);
        List<PathSegmentResponse> resultList = results.ToList();
        for (int i = 0; i < pathSegments.Count; i++)
            resultList[i].Path.Should().Be(pathSegments[i].Name);
    }
}
