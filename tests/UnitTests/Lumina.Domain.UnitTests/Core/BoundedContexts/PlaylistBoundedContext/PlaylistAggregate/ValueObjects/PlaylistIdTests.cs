#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.BoundedContexts.PlaylistBoundedContext.PlaylistAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.PlaylistBoundedContext.PlaylistAggregate.ValueObjects;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.UnitTests.Core.BoundedContexts.PlaylistBoundedContext.PlaylistAggregate.ValueObjects;

/// <summary>
/// Contains unit tests for the <see cref="PlaylistId"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class PlaylistIdTests
{
    private readonly PlaylistIdFixture _playlistIdFixture = new();

    [Fact]
    public void CreateUnique_WhenCalled_ShouldReturnIdWithNonEmptyValue()
    {
        // Act
        PlaylistId playlistId = PlaylistId.CreateUnique();

        // Assert
        Assert.NotNull(playlistId);
        Assert.NotEqual(Guid.Empty, playlistId.Value);
    }

    [Fact]
    public void CreateUnique_WhenCalledTwice_ShouldGenerateDistinctValues()
    {
        // Act
        PlaylistId firstId = PlaylistId.CreateUnique();
        PlaylistId secondId = PlaylistId.CreateUnique();

        // Assert
        Assert.NotEqual(firstId, secondId);
    }

    [Fact]
    public void Create_WhenCalledWithValue_ShouldReturnIdWithThatValue()
    {
        // Arrange
        Guid value = Guid.NewGuid();

        // Act
        PlaylistId playlistId = PlaylistId.Create(value);

        // Assert
        Assert.Equal(value, playlistId.Value);
    }

    [Fact]
    public void Equals_WithSameValue_ShouldReturnTrue()
    {
        // Arrange
        Guid value = Guid.NewGuid();
        PlaylistId firstId = _playlistIdFixture.Create(value);
        PlaylistId secondId = _playlistIdFixture.Create(value);

        // Act
        bool result = firstId.Equals(secondId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Equals_WithDifferentValue_ShouldReturnFalse()
    {
        // Arrange
        PlaylistId firstId = _playlistIdFixture.Create();
        PlaylistId secondId = _playlistIdFixture.Create();

        // Act
        bool result = firstId.Equals(secondId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GetEqualityComponents_WhenCalled_ShouldReturnOnlyTheValue()
    {
        // Arrange
        PlaylistId playlistId = _playlistIdFixture.Create();

        // Act
        List<object?> components = [.. playlistId.GetEqualityComponents()];

        // Assert
        object? component = Assert.Single(components);
        Assert.Equal(playlistId.Value, component);
    }
}
