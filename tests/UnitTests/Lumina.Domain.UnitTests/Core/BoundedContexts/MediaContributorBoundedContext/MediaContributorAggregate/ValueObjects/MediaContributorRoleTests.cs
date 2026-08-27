#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.MediaContributorBoundedContext.MediaContributorAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.MediaContributorBoundedContext.MediaContributorAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.MediaContributors;
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
    public void Create_WhenCalledWithDisplayNameAndCategory_ShouldCreateRole()
    {
        // Act
        Result<MediaContributorRole> result = MediaContributorRole.Create("Author", MediaContributorRoleCategory.Author);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal("Author", result.Value.DisplayName);
        Assert.Equal(MediaContributorRoleCategory.Author, result.Value.Category);
    }

    [Fact]
    public void Create_WhenCalledWithEmptyDisplayName_ShouldReturnError()
    {
        // Act
        Result<MediaContributorRole> result = MediaContributorRole.Create(" ", MediaContributorRoleCategory.Author);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.MediaContributors.MediaContributorRoleNameCannotBeEmpty, result.FirstError);
    }

    [Fact]
    public void Equals_WithSameValues_ShouldReturnTrue()
    {
        // Arrange
        MediaContributorRole firstRole = _mediaContributorRoleFixture.Create(name: "Author", category: MediaContributorRoleCategory.Author);
        MediaContributorRole secondRole = _mediaContributorRoleFixture.Create(name: "Author", category: MediaContributorRoleCategory.Author);

        // Act
        bool result = firstRole.Equals(secondRole);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Equals_WithDifferentDisplayName_ShouldReturnFalse()
    {
        // Arrange
        MediaContributorRole firstRole = _mediaContributorRoleFixture.Create(name: "Author", category: MediaContributorRoleCategory.Author);
        MediaContributorRole secondRole = _mediaContributorRoleFixture.Create(name: "Writer", category: MediaContributorRoleCategory.Author);

        // Act
        bool result = firstRole.Equals(secondRole);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Equals_WithDifferentCategory_ShouldReturnFalse()
    {
        // Arrange
        MediaContributorRole firstRole = _mediaContributorRoleFixture.Create(name: "Author", category: MediaContributorRoleCategory.Author);
        MediaContributorRole secondRole = _mediaContributorRoleFixture.Create(name: "Author", category: MediaContributorRoleCategory.Translator);

        // Act
        bool result = firstRole.Equals(secondRole);

        // Assert
        Assert.False(result);
    }
}
