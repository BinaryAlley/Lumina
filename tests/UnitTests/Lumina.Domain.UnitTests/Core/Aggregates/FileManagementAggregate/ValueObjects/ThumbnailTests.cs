#region ========================================================================= USING =====================================================================================
using FluentAssertions;
using Lumina.Contracts.Enums.PhotoLibrary;
using Lumina.Domain.Core.Aggregates.FileManagement.FileManagementAggregate.ValueObjects;
using Lumina.Domain.UnitTests.Core.Aggregates.FileManagementAggregate.ValueObjects.Fixtures;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion


namespace Lumina.Domain.UnitTests.Core.Aggregates.FileManagementAggregate.ValueObjects;

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
    public void Constructor_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        ImageType type = ImageType.PNG;
        byte[] bytes = [1, 2, 3, 4, 5];

        // Act
        Thumbnail thumbnail = new(type, bytes);

        // Assert
        thumbnail.Type.Should().Be(type);
        thumbnail.Bytes.Should().BeEquivalentTo(bytes);
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
        result.Should().BeTrue();
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
        result.Should().BeFalse();
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
        result.Should().BeFalse();
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
        hashCode1.Should().Be(hashCode2);
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
        hashCode1.Should().NotBe(hashCode2);
    }

    [Fact]
    public void GetEqualityComponents_ShouldReturnTypeAndBytes()
    {
        // Arrange
        Thumbnail thumbnail = _thumbnailFixture.CreateThumbnail();

        // Act
        List<object> components = thumbnail.GetEqualityComponents().ToList();

        // Assert
        components.Should().HaveCount(2);
        components[0].Should().Be(thumbnail.Type);
        components[1].Should().BeEquivalentTo(thumbnail.Bytes);
    }
}
