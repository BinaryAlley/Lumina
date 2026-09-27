#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.UnitTests.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;

/// <summary>
/// Contains unit tests for the <see cref="TrackId"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class TrackIdTests
{
    private readonly TrackIdFixture _trackIdFixture = new();

    [Fact]
    public void CreateUnique_WhenCalled_ShouldReturnIdWithNonEmptyValue()
    {
        // Act
        TrackId trackId = TrackId.CreateUnique();

        // Assert
        Assert.NotNull(trackId);
        Assert.NotEqual(default, trackId.Value);
    }

    [Fact]
    public void Create_WhenCalledWithValue_ShouldReturnIdWithThatValue()
    {
        // Arrange
        Guid value = Guid.NewGuid();

        // Act
        TrackId trackId = TrackId.Create(value);

        // Assert
        Assert.Equal(value, trackId.Value);
    }

    [Fact]
    public void Equals_WithSameValue_ShouldReturnTrue()
    {
        // Arrange
        Guid value = Guid.NewGuid();
        TrackId firstId = _trackIdFixture.Create(value);
        TrackId secondId = _trackIdFixture.Create(value);

        // Act
        bool result = firstId.Equals(secondId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Equals_WithDifferentValue_ShouldReturnFalse()
    {
        // Arrange
        TrackId firstId = _trackIdFixture.Create();
        TrackId secondId = _trackIdFixture.Create();

        // Act
        bool result = firstId.Equals(secondId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ToString_WhenCalled_ShouldReturnTheStringRepresentationOfTheValue()
    {
        // Arrange
        Guid value = Guid.NewGuid();
        TrackId trackId = _trackIdFixture.Create(value);

        // Act
        string? result = trackId.ToString();

        // Assert
        Assert.Equal(value.ToString(), result);
    }
}
