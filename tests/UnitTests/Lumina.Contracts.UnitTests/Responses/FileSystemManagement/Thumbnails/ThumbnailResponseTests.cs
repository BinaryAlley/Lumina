#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Fixtures.Core.Responses.FileSystemManagement.Thumbnails;
using Lumina.Contracts.Responses.FileSystemManagement.Thumbnails;
using Lumina.Domain.SharedKernel.Common.Enums.PhotoLibrary;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
#endregion

namespace Lumina.Contracts.UnitTests.Responses.FileSystemManagement.Thumbnails;

/// <summary>
/// Contains unit tests for the <see cref="ThumbnailResponse"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class ThumbnailResponseTests
{
    private readonly ThumbnailResponseFixture _thumbnailResponseFixture = new();

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    [Fact]
    public void Create_WhenCalled_ShouldReturnValidThumbnailResponse()
    {
        // Act
        ThumbnailResponse sut = _thumbnailResponseFixture.Create();

        // Assert
        Assert.NotNull(sut);
        Assert.NotEmpty(sut.Bytes);
    }

    [Fact]
    public void RoundTrip_WhenSerializingThumbnailResponse_ShouldPreserveValues()
    {
        // Arrange
        ThumbnailResponse expected = _thumbnailResponseFixture.Create(type: ImageType.JPEG);

        // Act
        string json = JsonSerializer.Serialize(expected, _jsonOptions);
        ThumbnailResponse? actual = JsonSerializer.Deserialize<ThumbnailResponse>(json, _jsonOptions);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(expected.Type, actual.Type);
        Assert.Equal(expected.Bytes, actual.Bytes);
    }

    [Fact]
    public void Serialize_WhenSerializingThumbnailResponse_ShouldSerializeTypeAsCamelCaseString()
    {
        // Arrange
        ThumbnailResponse sut = _thumbnailResponseFixture.Create(type: ImageType.PNG);

        // Act
        string json = JsonSerializer.Serialize(sut, _jsonOptions);

        // Assert
        Assert.Contains("\"Type\":\"png\"", json, StringComparison.Ordinal);
    }
}
