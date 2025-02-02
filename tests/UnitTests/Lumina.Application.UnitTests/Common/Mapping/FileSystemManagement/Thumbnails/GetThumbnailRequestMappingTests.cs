#region ========================================================================= USING =====================================================================================
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using Lumina.Application.Common.Mapping.FileSystemManagement.Thumbnails;
using Lumina.Application.Core.FileSystemManagement.Thumbnails.Queries.GetThumbnail;
using Lumina.Contracts.Requests.FileSystemManagement.Thumbnails;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.FileSystemManagement.Thumbnails;

/// <summary>
/// Contains unit tests for the <see cref="GetThumbnailRequestMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetThumbnailRequestMappingTests
{
    private readonly IFixture _fixture;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetThumbnailRequestMappingTests"/> class.
    /// </summary>
    public GetThumbnailRequestMappingTests()
    {
        _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
    }

    [Fact]
    public void ToQuery_WhenMappingGetThumbnailRequest_ShouldMapCorrectly()
    {
        // Arrange
        GetThumbnailRequest request = _fixture.Create<GetThumbnailRequest>();

        // Act
        GetThumbnailQuery result = request.ToQuery();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.Path, result.Path);
        Assert.Equal(request.Quality, result.Quality);
    }

    [Theory]
    [InlineData("C:\\test\\image.jpg", 50)]
    [InlineData("/home/user/image.png", 100)]
    [InlineData("D:\\photos\\vacation\\pic.gif", 75)]
    public void ToQuery_WhenMappingWithDifferentPathsAndQualities_ShouldMapCorrectly(string path, int quality)
    {
        // Arrange
        GetThumbnailRequest request = new(path, quality);

        // Act
        GetThumbnailQuery result = request.ToQuery();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(path, result.Path);
        Assert.Equal(quality, result.Quality);
    }

    [Fact]
    public void ToQuery_WhenMappingMultipleRequests_ShouldMapAllCorrectly()
    {
        // Arrange
        List<GetThumbnailRequest> requests = _fixture.CreateMany<GetThumbnailRequest>().ToList();

        // Act
        List<GetThumbnailQuery> results = requests.Select(r => r.ToQuery()).ToList();

        // Assert
        Assert.NotNull(results);
        Assert.Equal(requests.Count, results.Count);
        for (int i = 0; i < requests.Count; i++)
        {
            Assert.Equal(requests[i].Path, results[i].Path);
            Assert.Equal(requests[i].Quality, results[i].Quality);
        }
    }
}
