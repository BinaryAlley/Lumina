#region ========================================================================= USING =====================================================================================
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using FluentAssertions;
using Lumina.Application.Common.Mapping.FileSystemManagement.Thumbnails;
using Lumina.Contracts.Enums.PhotoLibrary;
using Lumina.Contracts.Responses.FileSystemManagement.Thumbnails;
using Lumina.Domain.Core.Aggregates.FileSystemManagement.FileSystemManagementAggregate.ValueObjects;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.FileSystemManagement.Thumbnails;

/// <summary>
/// Contains unit tests for the <see cref="ThumbnailMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ThumbnailMappingTests
{
    private readonly IFixture _fixture;

    /// <summary>
    /// Initializes a new instance of the <see cref="ThumbnailMappingTests"/> class.
    /// </summary>
    public ThumbnailMappingTests()
    {
        _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
    }

    [Fact]
    public void ToResponse_WhenMappingThumbnail_ShouldMapCorrectly()
    {
        // Arrange
        Thumbnail domainModel = _fixture.Create<Thumbnail>();

        // Act
        ThumbnailResponse result = domainModel.ToResponse();

        // Assert
        result.Should().NotBeNull();
        result.Type.Should().Be(domainModel.Type);
        result.Bytes.Should().BeEquivalentTo(domainModel.Bytes);
    }

    [Theory]
    [InlineData(ImageType.JPEG)]
    [InlineData(ImageType.PNG)]
    [InlineData(ImageType.GIF)]
    public void ToResponse_WhenMappingDifferentImageTypes_ShouldMapCorrectly(ImageType imageType)
    {
        // Arrange
        byte[] bytes = _fixture.CreateMany<byte>(100).ToArray();
        Thumbnail domainModel = new(imageType, bytes);

        // Act
        ThumbnailResponse result = domainModel.ToResponse();

        // Assert
        result.Should().NotBeNull();
        result.Type.Should().Be(imageType);
        result.Bytes.Should().BeEquivalentTo(bytes);
    }

    [Fact]
    public void ToResponse_WhenMappingThumbnailWithEmptyBytes_ShouldMapCorrectly()
    {
        // Arrange
        ImageType imageType = _fixture.Create<ImageType>();
        byte[] emptyBytes = [];
        Thumbnail domainModel = new(imageType, emptyBytes);

        // Act
        ThumbnailResponse result = domainModel.ToResponse();

        // Assert
        result.Should().NotBeNull();
        result.Type.Should().Be(imageType);
        result.Bytes.Should().BeEmpty();
    }

    [Fact]
    public void ToResponse_WhenMappingMultipleThumbnails_ShouldMapAllCorrectly()
    {
        // Arrange
        List<Thumbnail> thumbnails = _fixture.CreateMany<Thumbnail>(3).ToList();

        // Act
        List<ThumbnailResponse> results = thumbnails.Select(t => t.ToResponse()).ToList();

        // Assert
        results.Should().NotBeNull();
        results.Should().HaveCount(thumbnails.Count);
        for (int i = 0; i < thumbnails.Count; i++)
        {
            results[i].Type.Should().Be(thumbnails[i].Type);
            results[i].Bytes.Should().BeEquivalentTo(thumbnails[i].Bytes);
        }
    }
}
