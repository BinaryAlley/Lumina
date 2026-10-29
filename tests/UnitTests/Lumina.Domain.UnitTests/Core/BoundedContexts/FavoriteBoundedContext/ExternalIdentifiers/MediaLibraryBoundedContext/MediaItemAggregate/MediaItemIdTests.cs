#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.BoundedContexts.FavoriteBoundedContext.ExternalIdentifiers.MediaLibraryBoundedContext.MediaItemAggregate;
using Lumina.Domain.Fixtures.Core.BoundedContexts.FavoriteBoundedContext.ExternalIdentifiers.MediaLibraryBoundedContext.MediaItemAggregate;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.UnitTests.Core.BoundedContexts.FavoriteBoundedContext.ExternalIdentifiers.MediaLibraryBoundedContext.MediaItemAggregate;

/// <summary>
/// Contains unit tests for the <see cref="MediaItemId"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MediaItemIdTests
{
    private readonly MediaItemIdFixture _mediaItemIdFixture = new();

    [Fact]
    public void CreateUnique_WhenCalled_ShouldReturnIdWithNonEmptyValue()
    {
        // Act
        MediaItemId mediaItemId = MediaItemId.CreateUnique();

        // Assert
        Assert.NotNull(mediaItemId);
        Assert.NotEqual(Guid.Empty, mediaItemId.Value);
    }

    [Fact]
    public void Create_WhenCalledWithValue_ShouldReturnIdWithThatValue()
    {
        // Arrange
        Guid value = Guid.NewGuid();

        // Act
        MediaItemId mediaItemId = MediaItemId.Create(value);

        // Assert
        Assert.Equal(value, mediaItemId.Value);
    }

    [Fact]
    public void Equals_WithSameValue_ShouldReturnTrue()
    {
        // Arrange
        Guid value = Guid.NewGuid();
        MediaItemId firstId = _mediaItemIdFixture.Create(value);
        MediaItemId secondId = _mediaItemIdFixture.Create(value);

        // Act
        bool result = firstId.Equals(secondId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Equals_WithDifferentValue_ShouldReturnFalse()
    {
        // Arrange
        MediaItemId firstId = _mediaItemIdFixture.Create();
        MediaItemId secondId = _mediaItemIdFixture.Create();

        // Act
        bool result = firstId.Equals(secondId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GetEqualityComponents_WhenCalled_ShouldReturnOnlyTheValue()
    {
        // Arrange
        MediaItemId mediaItemId = _mediaItemIdFixture.Create();

        // Act
        List<object?> components = [.. mediaItemId.GetEqualityComponents()];

        // Assert
        object? component = Assert.Single(components);
        Assert.Equal(mediaItemId.Value, component);
    }
}
