#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.UnitTests.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;

/// <summary>
/// Contains unit tests for the <see cref="ArtistId"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ArtistIdTests
{
    private readonly ArtistIdFixture _artistIdFixture = new();

    [Fact]
    public void CreateUnique_WhenCalled_ShouldReturnIdWithNonEmptyValue()
    {
        // Act
        ArtistId artistId = ArtistId.CreateUnique();

        // Assert
        Assert.NotEqual(Guid.Empty, artistId.Value);
    }

    [Fact]
    public void Create_WhenCalledWithValue_ShouldReturnIdWithThatValue()
    {
        // Arrange
        Guid value = Guid.NewGuid();

        // Act
        ArtistId artistId = ArtistId.Create(value);

        // Assert
        Assert.Equal(value, artistId.Value);
    }

    [Fact]
    public void Create_WhenCalledWithEmptyGuid_ShouldReturnIdWithEmptyValue()
    {
        // Act
        ArtistId artistId = ArtistId.Create(Guid.Empty);

        // Assert
        Assert.Equal(Guid.Empty, artistId.Value);
    }

    [Fact]
    public void ToString_WhenCalled_ShouldReturnTheStringRepresentationOfTheValue()
    {
        // Arrange
        Guid value = Guid.NewGuid();
        ArtistId artistId = _artistIdFixture.Create(value);

        // Act
        string? result = artistId.ToString();

        // Assert
        Assert.Equal(value.ToString(), result);
    }

    [Fact]
    public void Equals_WithSameValue_ShouldReturnTrue()
    {
        // Arrange
        Guid value = Guid.NewGuid();
        ArtistId firstId = _artistIdFixture.Create(value);
        ArtistId secondId = _artistIdFixture.Create(value);

        // Act
        bool result = firstId.Equals(secondId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Equals_WithDifferentValue_ShouldReturnFalse()
    {
        // Arrange
        ArtistId firstId = _artistIdFixture.Create();
        ArtistId secondId = _artistIdFixture.Create();

        // Act
        bool result = firstId.Equals(secondId);

        // Assert
        Assert.False(result);
    }
}
