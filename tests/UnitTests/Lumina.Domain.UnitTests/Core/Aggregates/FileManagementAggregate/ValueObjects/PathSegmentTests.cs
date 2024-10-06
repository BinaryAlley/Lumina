#region ========================================================================= USING =====================================================================================
using ErrorOr;
using FluentAssertions;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Core.Aggregates.FileManagement.FileManagementAggregate.ValueObjects;
using Lumina.Domain.UnitTests.Core.Aggregates.FileManagementAggregate.ValueObjects.Fixtures;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.UnitTests.Core.Aggregates.FileManagementAggregate.ValueObjects;

/// <summary>
/// Contains unit tests for the <see cref="PathSegment"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class PathSegmentTests
{
    #region ================================================================== FIELD MEMBERS ================================================================================
    private readonly PathSegmentFixture _pathSegmentFixture;
    #endregion

    #region ====================================================================== CTOR =====================================================================================
    /// <summary>
    /// Initializes a new instance of the <see cref="PathSegmentTests"/> class.
    /// </summary>
    public PathSegmentTests()
    {
        _pathSegmentFixture = new PathSegmentFixture();
    }
    #endregion

    #region ===================================================================== METHODS ===================================================================================
    [Fact]
    public void Create_WithValidInput_ShouldReturnPathSegment()
    {
        // Arrange
        string name = "testFolder";
        bool isDirectory = true;
        bool isDrive = false;

        // Act
        ErrorOr<PathSegment> result = PathSegment.Create(name, isDirectory, isDrive);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Name.Should().Be(name);
        result.Value.IsDirectory.Should().Be(isDirectory);
        result.Value.IsDrive.Should().Be(isDrive);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidName_ShouldReturnError(string invalidName)
    {
        // Act
        ErrorOr<PathSegment> result = PathSegment.Create(invalidName, true, false);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.FileManagement.NameCannotBeEmpty);
    }

    [Fact]
    public void Equals_WithSameProperties_ShouldReturnTrue()
    {
        // Arrange
        PathSegment segment1 = _pathSegmentFixture.CreatePathSegment("folder", true, false);
        PathSegment segment2 = _pathSegmentFixture.CreatePathSegment("folder", true, false);

        // Act
        bool result = segment1.Equals(segment2);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentProperties_ShouldReturnFalse()
    {
        // Arrange
        PathSegment segment1 = _pathSegmentFixture.CreatePathSegment("folder1", true, false);
        PathSegment segment2 = _pathSegmentFixture.CreatePathSegment("folder2", true, false);

        // Act
        bool result = segment1.Equals(segment2);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void GetHashCode_WithSameProperties_ShouldReturnSameHashCode()
    {
        // Arrange
        PathSegment segment1 = _pathSegmentFixture.CreatePathSegment("folder", true, false);
        PathSegment segment2 = _pathSegmentFixture.CreatePathSegment("folder", true, false);

        // Act
        int hashCode1 = segment1.GetHashCode();
        int hashCode2 = segment2.GetHashCode();

        // Assert
        hashCode1.Should().Be(hashCode2);
    }

    [Fact]
    public void GetHashCode_WithDifferentProperties_ShouldReturnDifferentHashCode()
    {
        // Arrange
        PathSegment segment1 = _pathSegmentFixture.CreatePathSegment("folder1", true, false);
        PathSegment segment2 = _pathSegmentFixture.CreatePathSegment("folder2", true, false);

        // Act
        int hashCode1 = segment1.GetHashCode();
        int hashCode2 = segment2.GetHashCode();

        // Assert
        hashCode1.Should().NotBe(hashCode2);
    }

    [Fact]
    public void EqualityOperator_WithSameProperties_ShouldReturnTrue()
    {
        // Arrange
        PathSegment segment1 = _pathSegmentFixture.CreatePathSegment("folder", true, false);
        PathSegment segment2 = _pathSegmentFixture.CreatePathSegment("folder", true, false);

        // Act
        bool result = segment1 == segment2;

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void InequalityOperator_WithDifferentProperties_ShouldReturnTrue()
    {
        // Arrange
        PathSegment segment1 = _pathSegmentFixture.CreatePathSegment("folder1", true, false);
        PathSegment segment2 = _pathSegmentFixture.CreatePathSegment("folder2", true, false);

        // Act
        bool result = segment1 != segment2;

        // Assert
        result.Should().BeTrue();
    }
    #endregion
}