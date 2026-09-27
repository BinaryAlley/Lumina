#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.ExternalIdentifiers.MediaContributorBoundedContext.MediaContributorAggregate;
using Lumina.Domain.Fixtures.Core.BoundedContexts.AudioLibraryBoundedContext.ExternalIdentifiers.MediaContributorBoundedContext.MediaContributorAggregate;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.UnitTests.Core.BoundedContexts.AudioLibraryBoundedContext.ExternalIdentifiers.MediaContributorBoundedContext.MediaContributorAggregate;

/// <summary>
/// Contains unit tests for the <see cref="MediaContributorId"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MediaContributorIdTests
{
    private readonly MediaContributorIdFixture _mediaContributorIdFixture = new();

    [Fact]
    public void CreateUnique_WhenCalled_ShouldReturnIdWithNonEmptyValue()
    {
        // Act
        MediaContributorId mediaContributorId = MediaContributorId.CreateUnique();

        // Assert
        Assert.NotEqual(Guid.Empty, mediaContributorId.Value);
    }

    [Fact]
    public void Create_WhenCalledWithValue_ShouldReturnIdWithThatValue()
    {
        // Arrange
        Guid value = Guid.NewGuid();

        // Act
        MediaContributorId mediaContributorId = MediaContributorId.Create(value);

        // Assert
        Assert.Equal(value, mediaContributorId.Value);
    }

    [Fact]
    public void Create_WhenCalledWithEmptyGuid_ShouldReturnIdWithEmptyValue()
    {
        // Act
        MediaContributorId mediaContributorId = MediaContributorId.Create(Guid.Empty);

        // Assert
        Assert.Equal(Guid.Empty, mediaContributorId.Value);
    }

    [Fact]
    public void ToString_WhenCalled_ShouldReturnTheStringRepresentationOfTheValue()
    {
        // Arrange
        Guid value = Guid.NewGuid();
        MediaContributorId mediaContributorId = _mediaContributorIdFixture.Create(value);

        // Act
        string? result = mediaContributorId.ToString();

        // Assert
        Assert.Equal(value.ToString(), result);
    }

    [Fact]
    public void Equals_WithSameValue_ShouldReturnTrue()
    {
        // Arrange
        Guid value = Guid.NewGuid();
        MediaContributorId firstId = _mediaContributorIdFixture.Create(value);
        MediaContributorId secondId = _mediaContributorIdFixture.Create(value);

        // Act
        bool result = firstId.Equals(secondId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Equals_WithDifferentValue_ShouldReturnFalse()
    {
        // Arrange
        MediaContributorId firstId = _mediaContributorIdFixture.Create();
        MediaContributorId secondId = _mediaContributorIdFixture.Create();

        // Act
        bool result = firstId.Equals(secondId);

        // Assert
        Assert.False(result);
    }
}
