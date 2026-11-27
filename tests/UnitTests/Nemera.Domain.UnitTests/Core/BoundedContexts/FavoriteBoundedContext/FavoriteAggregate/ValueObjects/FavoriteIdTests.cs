#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.BoundedContexts.FavoriteBoundedContext.FavoriteAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.FavoriteBoundedContext.FavoriteAggregate.ValueObjects;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.UnitTests.Core.BoundedContexts.FavoriteBoundedContext.FavoriteAggregate.ValueObjects;

/// <summary>
/// Contains unit tests for the <see cref="FavoriteId"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class FavoriteIdTests
{
    private readonly FavoriteIdFixture _favoriteIdFixture = new();

    [Fact]
    public void CreateUnique_WhenCalled_ShouldReturnIdWithNonEmptyValue()
    {
        // Act
        FavoriteId favoriteId = FavoriteId.CreateUnique();

        // Assert
        Assert.NotNull(favoriteId);
        Assert.NotEqual(Guid.Empty, favoriteId.Value);
    }

    [Fact]
    public void Create_WhenCalledWithValue_ShouldReturnIdWithThatValue()
    {
        // Arrange
        Guid value = Guid.NewGuid();

        // Act
        FavoriteId favoriteId = FavoriteId.Create(value);

        // Assert
        Assert.Equal(value, favoriteId.Value);
    }

    [Fact]
    public void Equals_WithSameValue_ShouldReturnTrue()
    {
        // Arrange
        Guid value = Guid.NewGuid();
        FavoriteId firstId = _favoriteIdFixture.Create(value);
        FavoriteId secondId = _favoriteIdFixture.Create(value);

        // Act
        bool result = firstId.Equals(secondId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Equals_WithDifferentValue_ShouldReturnFalse()
    {
        // Arrange
        FavoriteId firstId = _favoriteIdFixture.Create();
        FavoriteId secondId = _favoriteIdFixture.Create();

        // Act
        bool result = firstId.Equals(secondId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GetEqualityComponents_WhenCalled_ShouldReturnOnlyTheValue()
    {
        // Arrange
        FavoriteId favoriteId = _favoriteIdFixture.Create();

        // Act
        List<object?> components = [.. favoriteId.GetEqualityComponents()];

        // Assert
        object? component = Assert.Single(components);
        Assert.Equal(favoriteId.Value, component);
    }
}
