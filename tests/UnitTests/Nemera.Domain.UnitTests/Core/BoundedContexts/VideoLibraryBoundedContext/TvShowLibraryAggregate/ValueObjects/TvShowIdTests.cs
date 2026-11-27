#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.BoundedContexts.VideoLibraryBoundedContext.TvShowLibraryAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.VideoLibraryBoundedContext.TvShowLibraryAggregate.ValueObjects;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.UnitTests.Core.BoundedContexts.VideoLibraryBoundedContext.TvShowLibraryAggregate.ValueObjects;

/// <summary>
/// Contains unit tests for the <see cref="TvShowId"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class TvShowIdTests
{
    private readonly TvShowIdFixture _tvShowIdFixture = new();

    [Fact]
    public void CreateUnique_WhenCalled_ShouldReturnIdWithNonEmptyValue()
    {
        // Act
        TvShowId tvShowId = TvShowId.CreateUnique();

        // Assert
        Assert.NotNull(tvShowId);
        Assert.NotEqual(default, tvShowId.Value);
    }

    [Fact]
    public void Create_WhenCalledWithValue_ShouldReturnIdWithThatValue()
    {
        // Arrange
        Guid value = Guid.NewGuid();

        // Act
        TvShowId tvShowId = TvShowId.Create(value);

        // Assert
        Assert.Equal(value, tvShowId.Value);
    }

    [Fact]
    public void Equals_WithSameValue_ShouldReturnTrue()
    {
        // Arrange
        Guid value = Guid.NewGuid();
        TvShowId firstId = _tvShowIdFixture.Create(value);
        TvShowId secondId = _tvShowIdFixture.Create(value);

        // Act
        bool result = firstId.Equals(secondId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Equals_WithDifferentValue_ShouldReturnFalse()
    {
        // Arrange
        TvShowId firstId = _tvShowIdFixture.Create();
        TvShowId secondId = _tvShowIdFixture.Create();

        // Act
        bool result = firstId.Equals(secondId);

        // Assert
        Assert.False(result);
    }
}
