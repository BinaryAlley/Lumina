#region ========================================================================= USING =====================================================================================
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using Lumina.Application.Common.Mapping.FileSystemManagement.Thumbnails;
using Lumina.Contracts.Responses.FileSystemManagement.Thumbnails;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.PhotoLibrary;
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
    private readonly ThumbnailFixture _thumbnailFixture = new();

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
        Thumbnail domainModel = _thumbnailFixture.Create();

        // Act
        ThumbnailResponse result = domainModel.ToResponse();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(domainModel.Type, result.Type);
        Assert.Equal(domainModel.Bytes, result.Bytes);
    }

    [Theory]
    [InlineData(ImageType.JPEG)]
    [InlineData(ImageType.PNG)]
    [InlineData(ImageType.GIF)]
    public void ToResponse_WhenMappingDifferentImageTypes_ShouldMapCorrectly(ImageType imageType)
    {
        // Arrange
        byte[] bytes = [.. _fixture.CreateMany<byte>(100)];
        Thumbnail domainModel = new(imageType, bytes);

        // Act
        ThumbnailResponse result = domainModel.ToResponse();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(imageType, result.Type);
        Assert.Equal(bytes, result.Bytes);
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
        Assert.NotNull(result);
        Assert.Equal(imageType, result.Type);
        Assert.Empty(result.Bytes);
    }

    [Fact]
    public void ToResponse_WhenMappingMultipleThumbnails_ShouldMapAllCorrectly()
    {
        // Arrange
        List<Thumbnail> thumbnails = _thumbnailFixture.CreateMany(3);

        // Act
        List<ThumbnailResponse> results = [.. thumbnails.Select(t => t.ToResponse())];

        // Assert
        Assert.NotNull(results);
        Assert.Equal(thumbnails.Count, results.Count);
        for (int i = 0; i < thumbnails.Count; i++)
        {
            Assert.Equal(thumbnails[i].Type, results[i].Type);
            Assert.Equal(thumbnails[i].Bytes, results[i].Bytes);
        }
    }
}
