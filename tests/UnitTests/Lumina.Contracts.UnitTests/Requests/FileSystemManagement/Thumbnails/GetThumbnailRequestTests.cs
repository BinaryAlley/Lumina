#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Fixtures.Core.Requests.FileSystemManagement.Thumbnails;
using Lumina.Contracts.Requests.FileSystemManagement.Thumbnails;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
#endregion

namespace Lumina.Contracts.UnitTests.Requests.FileSystemManagement.Thumbnails;

/// <summary>
/// Contains unit tests for the <see cref="GetThumbnailRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetThumbnailRequestTests
{
    private readonly GetThumbnailRequestFixture _getThumbnailRequestFixture = new();

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public void Create_WhenCalled_ShouldReturnValidGetThumbnailRequest()
    {
        // Act
        GetThumbnailRequest sut = _getThumbnailRequestFixture.Create();

        // Assert
        Assert.NotNull(sut);
        Assert.False(string.IsNullOrWhiteSpace(sut.Path));
        Assert.InRange(sut.Quality, 1, 100);
    }

    [Fact]
    public void RoundTrip_WhenSerializingGetThumbnailRequest_ShouldPreserveValues()
    {
        // Arrange
        GetThumbnailRequest expected = _getThumbnailRequestFixture.Create();

        // Act
        string json = JsonSerializer.Serialize(expected, _jsonOptions);
        GetThumbnailRequest? actual = JsonSerializer.Deserialize<GetThumbnailRequest>(json, _jsonOptions);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Deconstruct_WhenCalled_ShouldReturnAllProperties()
    {
        // Arrange
        GetThumbnailRequest sut = _getThumbnailRequestFixture.Create(path: @"C:\Media\cover.jpg", quality: 80);

        // Act
        (string? path, int quality) = sut;

        // Assert
        Assert.Equal(sut.Path, path);
        Assert.Equal(sut.Quality, quality);
    }
}
