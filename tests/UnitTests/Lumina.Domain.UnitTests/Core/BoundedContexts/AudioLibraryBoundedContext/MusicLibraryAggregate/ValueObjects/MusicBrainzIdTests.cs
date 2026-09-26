#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.UnitTests.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;

/// <summary>
/// Contains unit tests for the <see cref="MusicBrainzId"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MusicBrainzIdTests
{
    private readonly MusicBrainzIdFixture _musicBrainzIdFixture = new();

    [Fact]
    public void Create_WhenCalledWithValue_ShouldReturnIdWithThatValue()
    {
        // Arrange
        Guid value = Guid.NewGuid();

        // Act
        MusicBrainzId musicBrainzId = MusicBrainzId.Create(value);

        // Assert
        Assert.Equal(value, musicBrainzId.Value);
    }

    [Fact]
    public void Equals_WithSameValue_ShouldReturnTrue()
    {
        // Arrange
        Guid value = Guid.NewGuid();
        MusicBrainzId firstId = _musicBrainzIdFixture.Create(value);
        MusicBrainzId secondId = _musicBrainzIdFixture.Create(value);

        // Act
        bool result = firstId.Equals(secondId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Equals_WithDifferentValue_ShouldReturnFalse()
    {
        // Arrange
        MusicBrainzId firstId = _musicBrainzIdFixture.Create();
        MusicBrainzId secondId = _musicBrainzIdFixture.Create();

        // Act
        bool result = firstId.Equals(secondId);

        // Assert
        Assert.False(result);
    }
}
