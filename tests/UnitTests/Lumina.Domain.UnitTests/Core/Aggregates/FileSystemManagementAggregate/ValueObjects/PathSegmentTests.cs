#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.ValueObjects;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.UnitTests.Core.Aggregates.FileSystemManagementAggregate.ValueObjects;

/// <summary>
/// Contains unit tests for the <see cref="PathSegment"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class PathSegmentTests
{
    private readonly PathSegmentFixture _pathSegmentFixture = new();

    [Fact]
    public void Create_WithValidInput_ShouldReturnPathSegment()
    {
        // Arrange
        string name = "testFolder";
        bool isDirectory = true;
        bool isDrive = false;

        // Act
        Result<PathSegment> result = PathSegment.Create(name, isDirectory, isDrive);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(name, result.Value.Name);
        Assert.Equal(isDirectory, result.Value.IsDirectory);
        Assert.Equal(isDrive, result.Value.IsDrive);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidName_ShouldReturnError(string? invalidName)
    {
        // Act
        Result<PathSegment> result = PathSegment.Create(invalidName!, true, false);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.FileSystemManagement.NameCannotBeEmpty, result.FirstError);
    }

    [Fact]
    public void Equals_WithSameProperties_ShouldReturnTrue()
    {
        // Arrange
        PathSegment segment1 = _pathSegmentFixture.Create("folder", true, false);
        PathSegment segment2 = _pathSegmentFixture.Create("folder", true, false);

        // Act
        bool result = segment1.Equals(segment2);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Equals_WithDifferentProperties_ShouldReturnFalse()
    {
        // Arrange
        PathSegment segment1 = _pathSegmentFixture.Create("folder1", true, false);
        PathSegment segment2 = _pathSegmentFixture.Create("folder2", true, false);

        // Act
        bool result = segment1.Equals(segment2);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GetHashCode_WithSameProperties_ShouldReturnSameHashCode()
    {
        // Arrange
        PathSegment segment1 = _pathSegmentFixture.Create("folder", true, false);
        PathSegment segment2 = _pathSegmentFixture.Create("folder", true, false);

        // Act
        int hashCode1 = segment1.GetHashCode();
        int hashCode2 = segment2.GetHashCode();

        // Assert
        Assert.Equal(hashCode1, hashCode2);
    }

    [Fact]
    public void GetHashCode_WithDifferentProperties_ShouldReturnDifferentHashCode()
    {
        // Arrange
        PathSegment segment1 = _pathSegmentFixture.Create("folder1", true, false);
        PathSegment segment2 = _pathSegmentFixture.Create("folder2", true, false);

        // Act
        int hashCode1 = segment1.GetHashCode();
        int hashCode2 = segment2.GetHashCode();

        // Assert
        Assert.NotEqual(hashCode1, hashCode2);
    }

    [Fact]
    public void EqualityOperator_WithSameProperties_ShouldReturnTrue()
    {
        // Arrange
        PathSegment segment1 = _pathSegmentFixture.Create("folder", true, false);
        PathSegment segment2 = _pathSegmentFixture.Create("folder", true, false);

        // Act
        bool result = segment1 == segment2;

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void InequalityOperator_WithDifferentProperties_ShouldReturnTrue()
    {
        // Arrange
        PathSegment segment1 = _pathSegmentFixture.Create("folder1", true, false);
        PathSegment segment2 = _pathSegmentFixture.Create("folder2", true, false);

        // Act
        bool result = segment1 != segment2;

        // Assert
        Assert.True(result);
    }
}
