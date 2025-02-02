#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Enums.PhotoLibrary;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.ValueObjects;
using Lumina.Domain.UnitTests.Core.Aggregates.FileSystemManagementAggregate.ValueObjects.Fixtures;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion


namespace Lumina.Domain.UnitTests.Core.Aggregates.FileSystemManagementAggregate.ValueObjects;

/// <summary>
/// Contains unit tests for the <see cref="Thumbnail"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ThumbnailTests
{
    private readonly ThumbnailFixture _thumbnailFixture;

    /// <summary>
    /// Initializes a new instance of the <see cref="ThumbnailTests"/> class.
    /// </summary>
    public ThumbnailTests()
    {
        _thumbnailFixture = new ThumbnailFixture();
    }

    [Fact]
    public void Constructor_WhenCalled_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        ImageType type = ImageType.PNG;
        byte[] bytes = [1, 2, 3, 4, 5];

        // Act
        Thumbnail thumbnail = new(type, bytes);

        // Assert
        Assert.Equal(type, thumbnail.Type);
        Assert.Equal(bytes, thumbnail.Bytes);
    }

    [Fact]
    public void Equals_WithSameProperties_ShouldReturnTrue()
    {
        // Arrange
        Thumbnail thumbnail1 = _thumbnailFixture.CreateThumbnail();
        Thumbnail thumbnail2 = new(thumbnail1.Type, thumbnail1.Bytes);

        // Act
        bool result = thumbnail1.Equals(thumbnail2);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Equals_WithDifferentType_ShouldReturnFalse()
    {
        // Arrange
        Thumbnail thumbnail1 = _thumbnailFixture.CreateThumbnail(ImageType.PNG);
        Thumbnail thumbnail2 = _thumbnailFixture.CreateThumbnail(ImageType.JPEG, thumbnail1.Bytes);

        // Act
        bool result = thumbnail1.Equals(thumbnail2);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Equals_WithDifferentBytes_ShouldReturnFalse()
    {
        // Arrange
        Thumbnail thumbnail1 = _thumbnailFixture.CreateThumbnail();
        Thumbnail thumbnail2 = _thumbnailFixture.CreateThumbnail(thumbnail1.Type);

        // Act
        bool result = thumbnail1.Equals(thumbnail2);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GetHashCode_WithSameProperties_ShouldReturnSameHashCode()
    {
        // Arrange
        Thumbnail thumbnail1 = _thumbnailFixture.CreateThumbnail();
        Thumbnail thumbnail2 = new(thumbnail1.Type, thumbnail1.Bytes);

        // Act
        int hashCode1 = thumbnail1.GetHashCode();
        int hashCode2 = thumbnail2.GetHashCode();

        // Assert
        Assert.Equal(hashCode1, hashCode2);
    }

    [Fact]
    public void GetHashCode_WithDifferentProperties_ShouldReturnDifferentHashCode()
    {
        // Arrange
        Thumbnail thumbnail1 = _thumbnailFixture.CreateThumbnail();
        Thumbnail thumbnail2 = _thumbnailFixture.CreateThumbnail();

        // Act
        int hashCode1 = thumbnail1.GetHashCode();
        int hashCode2 = thumbnail2.GetHashCode();

        // Assert
        Assert.NotEqual(hashCode1, hashCode2);
    }

    [Fact]
    public void GetEqualityComponents_ShouldReturnTypeAndBytes()
    {
        // Arrange
        Thumbnail thumbnail = _thumbnailFixture.CreateThumbnail();

        // Act
        List<object> components = thumbnail.GetEqualityComponents().ToList();

        // Assert
        Assert.Equal(2, components.Count);
        Assert.Equal(thumbnail.Type, components[0]);
        Assert.Equal(thumbnail.Bytes, components[1]);
    }
}
