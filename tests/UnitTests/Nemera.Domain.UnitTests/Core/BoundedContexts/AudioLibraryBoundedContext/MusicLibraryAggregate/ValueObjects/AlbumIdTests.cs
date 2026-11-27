#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.UnitTests.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;

/// <summary>
/// Contains unit tests for the <see cref="AlbumId"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class AlbumIdTests
{
    private readonly AlbumIdFixture _albumIdFixture = new();

    [Fact]
    public void CreateUnique_WhenCalled_ShouldReturnIdWithNonEmptyValue()
    {
        // Act
        AlbumId albumId = AlbumId.CreateUnique();

        // Assert
        Assert.NotEqual(Guid.Empty, albumId.Value);
    }

    [Fact]
    public void Create_WhenCalledWithValue_ShouldReturnIdWithThatValue()
    {
        // Arrange
        Guid value = Guid.NewGuid();

        // Act
        AlbumId albumId = AlbumId.Create(value);

        // Assert
        Assert.Equal(value, albumId.Value);
    }

    [Fact]
    public void Create_WhenCalledWithEmptyGuid_ShouldReturnIdWithEmptyValue()
    {
        // Act
        AlbumId albumId = AlbumId.Create(Guid.Empty);

        // Assert
        Assert.Equal(Guid.Empty, albumId.Value);
    }

    [Fact]
    public void ToString_WhenCalled_ShouldReturnTheStringRepresentationOfTheValue()
    {
        // Arrange
        Guid value = Guid.NewGuid();
        AlbumId albumId = _albumIdFixture.Create(value);

        // Act
        string? result = albumId.ToString();

        // Assert
        Assert.Equal(value.ToString(), result);
    }

    [Fact]
    public void Equals_WithSameValue_ShouldReturnTrue()
    {
        // Arrange
        Guid value = Guid.NewGuid();
        AlbumId firstId = _albumIdFixture.Create(value);
        AlbumId secondId = _albumIdFixture.Create(value);

        // Act
        bool result = firstId.Equals(secondId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Equals_WithDifferentValue_ShouldReturnFalse()
    {
        // Arrange
        AlbumId firstId = _albumIdFixture.Create();
        AlbumId secondId = _albumIdFixture.Create();

        // Act
        bool result = firstId.Equals(secondId);

        // Assert
        Assert.False(result);
    }
}
