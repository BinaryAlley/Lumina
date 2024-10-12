#region ========================================================================= USING =====================================================================================
using Bogus;
using FluentAssertions;
using Lumina.Application.Common.Mapping.FileManagement;
using Lumina.Contracts.Enums.PhotoLibrary;
using Lumina.Contracts.Responses.FileManagement;
using Lumina.Domain.Core.Aggregates.FileManagement.FileManagementAggregate.ValueObjects;
using Mapster;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.FileManagement;

/// <summary>
/// Contains unit tests for the <see cref="ThumbnailMappingConfig"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ThumbnailMappingConfigTests
{
    private readonly TypeAdapterConfig _config;
    private readonly ThumbnailMappingConfig _thumbnailMappingConfig;
    private readonly Faker _faker;

    public ThumbnailMappingConfigTests()
    {
        _config = new TypeAdapterConfig();
        _thumbnailMappingConfig = new ThumbnailMappingConfig();
        _thumbnailMappingConfig.Register(_config);
        _faker = new Faker();
    }

    [Fact]
    public void Register_WhenMappingThumbnailToThumbnailResponse_ShouldMapCorrectly()
    {
        // Arrange
        ImageType imageType = _faker.PickRandom<ImageType>();
        byte[] bytes = _faker.Random.Bytes(100);
        Thumbnail thumbnail = new(imageType, bytes);

        // Act
        ThumbnailResponse result = thumbnail.Adapt<ThumbnailResponse>(_config);

        // Assert
        result.Should().NotBeNull();
        result.Type.Should().Be(thumbnail.Type);
        result.Bytes.Should().BeEquivalentTo(thumbnail.Bytes);
    }

    [Theory]
    [InlineData(ImageType.JPEG)]
    [InlineData(ImageType.PNG)]
    [InlineData(ImageType.GIF)]
    public void Register_WhenMappingDifferentImageTypes_ShouldMapCorrectly(ImageType imageType)
    {
        // Arrange
        byte[] bytes = _faker.Random.Bytes(100);
        Thumbnail thumbnail = new(imageType, bytes);

        // Act
        ThumbnailResponse result = thumbnail.Adapt<ThumbnailResponse>(_config);

        // Assert
        result.Should().NotBeNull();
        result.Type.Should().Be(imageType);
        result.Bytes.Should().BeEquivalentTo(bytes);
    }

    [Fact]
    public void Register_WhenMappingThumbnailWithEmptyBytes_ShouldMapCorrectly()
    {
        // Arrange
        ImageType imageType = _faker.PickRandom<ImageType>();
        byte[] emptyBytes = [];
        Thumbnail thumbnail = new(imageType, emptyBytes);

        // Act
        ThumbnailResponse result = thumbnail.Adapt<ThumbnailResponse>(_config);

        // Assert
        result.Should().NotBeNull();
        result.Type.Should().Be(imageType);
        result.Bytes.Should().BeEmpty();
    }

    [Fact]
    public void Register_WhenMappingMultipleThumbnails_ShouldMapAllCorrectly()
    {
        // Arrange
        List<Thumbnail> thumbnails =
        [
            new Thumbnail(ImageType.JPEG, _faker.Random.Bytes(100)),
            new Thumbnail(ImageType.PNG, _faker.Random.Bytes(200)),
            new Thumbnail(ImageType.GIF, _faker.Random.Bytes(150))
        ];

        // Act
        List<ThumbnailResponse> results = thumbnails.Adapt<List<ThumbnailResponse>>(_config);

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
