#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.BoundedContexts.VideoLibraryBoundedContext.TvShowLibraryAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.VideoLibraryBoundedContext.TvShowLibraryAggregate.ValueObjects;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.UnitTests.Core.BoundedContexts.VideoLibraryBoundedContext.TvShowLibraryAggregate.ValueObjects;

/// <summary>
/// Contains unit tests for the <see cref="EpisodeId"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class EpisodeIdTests
{
    private readonly EpisodeIdFixture _episodeIdFixture = new();

    [Fact]
    public void CreateUnique_WhenCalled_ShouldReturnIdWithNonEmptyValue()
    {
        // Act
        EpisodeId episodeId = EpisodeId.CreateUnique();

        // Assert
        Assert.NotNull(episodeId);
        Assert.NotEqual(default, episodeId.Value);
    }

    [Fact]
    public void Create_WhenCalledWithValue_ShouldReturnIdWithThatValue()
    {
        // Arrange
        Guid value = Guid.NewGuid();

        // Act
        EpisodeId episodeId = EpisodeId.Create(value);

        // Assert
        Assert.Equal(value, episodeId.Value);
    }

    [Fact]
    public void Equals_WithSameValue_ShouldReturnTrue()
    {
        // Arrange
        Guid value = Guid.NewGuid();
        EpisodeId firstId = _episodeIdFixture.Create(value);
        EpisodeId secondId = _episodeIdFixture.Create(value);

        // Act
        bool result = firstId.Equals(secondId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Equals_WithDifferentValue_ShouldReturnFalse()
    {
        // Arrange
        EpisodeId firstId = _episodeIdFixture.Create();
        EpisodeId secondId = _episodeIdFixture.Create();

        // Act
        bool result = firstId.Equals(secondId);

        // Assert
        Assert.False(result);
    }
}
