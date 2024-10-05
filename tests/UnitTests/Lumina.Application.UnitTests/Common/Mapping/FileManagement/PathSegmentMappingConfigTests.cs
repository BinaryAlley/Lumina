#region ========================================================================= USING =====================================================================================
using ErrorOr;
using FluentAssertions;
using Lumina.Application.Common.Mapping.FileManagement;
using Lumina.Contracts.Responses.FileManagement;
using Lumina.Domain.Core.Aggregates.FileManagement.FileManagementAggregate.ValueObjects;
using Mapster;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.FileManagement;

/// <summary>
/// Contains unit tests for the <see cref="PathSegmentMappingConfig"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class PathSegmentMappingConfigTests
{
    #region ================================================================== FIELD MEMBERS ================================================================================
    private readonly TypeAdapterConfig _config;
    private readonly PathSegmentMappingConfig _pathSegmentMappingConfig;
    #endregion

    #region ====================================================================== CTOR =====================================================================================
    public PathSegmentMappingConfigTests()
    {
        _config = new TypeAdapterConfig();
        _pathSegmentMappingConfig = new PathSegmentMappingConfig();
        _pathSegmentMappingConfig.Register(_config);
    }
    #endregion

    #region ===================================================================== METHODS ===================================================================================
    [Fact]
    public void Register_WhenMappingPathSegmentToPathSegmentResponse_ShouldMapCorrectly()
    {
        // Arrange
        ErrorOr<PathSegment> createResult = PathSegment.Create("TestSegment", true, false);
        createResult.IsError.Should().BeFalse();
        PathSegment pathSegment = createResult.Value;

        // Act
        PathSegmentResponse result = pathSegment.Adapt<PathSegmentResponse>(_config);

        // Assert
        result.Should().NotBeNull();
        result.Path.Should().Be(pathSegment.Name);
    }

    [Theory]
    [InlineData("Folder", true, false)]
    [InlineData("File.txt", false, false)]
    [InlineData("C:", false, true)]
    public void Register_WhenMappingDifferentPathSegmentTypes_ShouldMapCorrectly(string name, bool isDirectory, bool isDrive)
    {
        // Arrange
        ErrorOr<PathSegment> createResult = PathSegment.Create(name, isDirectory, isDrive);
        createResult.IsError.Should().BeFalse();
        PathSegment pathSegment = createResult.Value;

        // Act
        PathSegmentResponse result = pathSegment.Adapt<PathSegmentResponse>(_config);

        // Assert
        result.Should().NotBeNull();
        result.Path.Should().Be(pathSegment.Name);
    }

    [Fact]
    public void Register_WhenMappingMultiplePathSegments_ShouldMapAllCorrectly()
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
        List<PathSegmentResponse> results = pathSegments.Adapt<List<PathSegmentResponse>>(_config);

        // Assert
        results.Should().NotBeNull();
        results.Should().HaveCount(pathSegments.Count);
        for (int i = 0; i < pathSegments.Count; i++)
        {
            results[i].Path.Should().Be(pathSegments[i].Name);
        }
    }
    #endregion
}
