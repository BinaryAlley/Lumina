#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.BoundedContexts.MediaContributorBoundedContext.MediaContributorAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.MediaContributorBoundedContext.MediaContributorAggregate.ValueObjects;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.UnitTests.Core.BoundedContexts.MediaContributorBoundedContext.MediaContributorAggregate.ValueObjects;

/// <summary>
/// Contains unit tests for the <see cref="MediaContributorRole"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MediaContributorRoleTests
{
    private readonly MediaContributorRoleFixture _mediaContributorRoleFixture = new();

    [Fact]
    public void Create_WhenCalledWithNameAndCategory_ShouldCreateRole()
    {
        // Act
        MediaContributorRole role = MediaContributorRole.Create("Actor", "Crew");

        // Assert
        Assert.Equal("Actor", role.Name);
        Assert.Equal("Crew", role.Category);
    }

    [Fact]
    public void Equals_WithSameValues_ShouldReturnTrue()
    {
        // Arrange
        MediaContributorRole firstRole = _mediaContributorRoleFixture.Create(name: "Actor", category: "Crew");
        MediaContributorRole secondRole = _mediaContributorRoleFixture.Create(name: "Actor", category: "Crew");

        // Act
        bool result = firstRole.Equals(secondRole);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Equals_WithDifferentName_ShouldReturnFalse()
    {
        // Arrange
        MediaContributorRole firstRole = _mediaContributorRoleFixture.Create(name: "Actor", category: "Crew");
        MediaContributorRole secondRole = _mediaContributorRoleFixture.Create(name: "Director", category: "Crew");

        // Act
        bool result = firstRole.Equals(secondRole);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Equals_WithDifferentCategory_ShouldReturnFalse()
    {
        // Arrange
        MediaContributorRole firstRole = _mediaContributorRoleFixture.Create(name: "Actor", category: "Crew");
        MediaContributorRole secondRole = _mediaContributorRoleFixture.Create(name: "Actor", category: "Voice");

        // Act
        bool result = firstRole.Equals(secondRole);

        // Assert
        Assert.False(result);
    }
}
