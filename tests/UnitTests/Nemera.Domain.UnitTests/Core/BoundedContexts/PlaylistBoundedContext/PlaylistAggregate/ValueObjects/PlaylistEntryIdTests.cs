#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.BoundedContexts.PlaylistBoundedContext.PlaylistAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.PlaylistBoundedContext.PlaylistAggregate.ValueObjects;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.UnitTests.Core.BoundedContexts.PlaylistBoundedContext.PlaylistAggregate.ValueObjects;

/// <summary>
/// Contains unit tests for the <see cref="PlaylistEntryId"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class PlaylistEntryIdTests
{
    private readonly PlaylistEntryIdFixture _playlistEntryIdFixture = new();

    [Fact]
    public void CreateUnique_WhenCalled_ShouldReturnIdWithNonEmptyValue()
    {
        // Act
        PlaylistEntryId playlistEntryId = PlaylistEntryId.CreateUnique();

        // Assert
        Assert.NotNull(playlistEntryId);
        Assert.NotEqual(Guid.Empty, playlistEntryId.Value);
    }

    [Fact]
    public void CreateUnique_WhenCalledTwice_ShouldGenerateDistinctValues()
    {
        // Act
        PlaylistEntryId firstId = PlaylistEntryId.CreateUnique();
        PlaylistEntryId secondId = PlaylistEntryId.CreateUnique();

        // Assert
        Assert.NotEqual(firstId, secondId);
    }

    [Fact]
    public void Create_WhenCalledWithValue_ShouldReturnIdWithThatValue()
    {
        // Arrange
        Guid value = Guid.NewGuid();

        // Act
        PlaylistEntryId playlistEntryId = PlaylistEntryId.Create(value);

        // Assert
        Assert.Equal(value, playlistEntryId.Value);
    }

    [Fact]
    public void Equals_WithSameValue_ShouldReturnTrue()
    {
        // Arrange
        Guid value = Guid.NewGuid();
        PlaylistEntryId firstId = _playlistEntryIdFixture.Create(value);
        PlaylistEntryId secondId = _playlistEntryIdFixture.Create(value);

        // Act
        bool result = firstId.Equals(secondId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Equals_WithDifferentValue_ShouldReturnFalse()
    {
        // Arrange
        PlaylistEntryId firstId = _playlistEntryIdFixture.Create();
        PlaylistEntryId secondId = _playlistEntryIdFixture.Create();

        // Act
        bool result = firstId.Equals(secondId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GetEqualityComponents_WhenCalled_ShouldReturnOnlyTheValue()
    {
        // Arrange
        PlaylistEntryId playlistEntryId = _playlistEntryIdFixture.Create();

        // Act
        List<object?> components = [.. playlistEntryId.GetEqualityComponents()];

        // Assert
        object? component = Assert.Single(components);
        Assert.Equal(playlistEntryId.Value, component);
    }
}
