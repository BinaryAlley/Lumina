#region ========================================================================= USING =====================================================================================
using Lumina.Plugins.CoverArtArchive.Common.Models.Contracts.Responses;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
#endregion

namespace Lumina.Plugins.CoverArtArchive.UnitTests.Common.Models.Contracts.Responses;

/// <summary>
/// Contains unit tests for the <see cref="CoverArtArchiveImageResponse"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class CoverArtArchiveImageResponseTests
{
    [Fact]
    public void Deserialize_WhenEveryFieldIsPresent_ShouldDeserializeEveryProperty()
    {
        // Arrange
        const string JSON = """
        {
            "approved": true,
            "back": true,
            "comment": "A comment",
            "front": false,
            "id": 12345,
            "image": "https://coverartarchive.org/release/11111111-1111-1111-1111-111111111111/12345.jpg",
            "thumbnail": "https://coverartarchive.org/release/11111111-1111-1111-1111-111111111111/12345-250.jpg",
            "types": [ "Back", "Booklet" ]
        }
        """;

        // Act
        CoverArtArchiveImageResponse? response = JsonSerializer.Deserialize<CoverArtArchiveImageResponse>(JSON);

        // Assert
        Assert.NotNull(response);
        Assert.Equal("https://coverartarchive.org/release/11111111-1111-1111-1111-111111111111/12345.jpg", response.Image);
        Assert.Equal("https://coverartarchive.org/release/11111111-1111-1111-1111-111111111111/12345-250.jpg", response.Thumbnail);
        Assert.Equal(["Back", "Booklet"], response.Types);
        Assert.False(response.IsFront);
        Assert.True(response.IsBack);
        Assert.Equal("A comment", response.Comment);
    }

    [Fact]
    public void Deserialize_WhenTheApiReturnsANullTypes_ShouldDeserializeAsNull()
    {
        // Arrange
        // The API can answer with a null types collection, which must not be turned into anything else by the deserializer.
        const string JSON = """{"image":"https://coverartarchive.org/release/11111111-1111-1111-1111-111111111111/1.jpg","types":null}""";

        // Act
        CoverArtArchiveImageResponse? response = JsonSerializer.Deserialize<CoverArtArchiveImageResponse>(JSON);

        // Assert
        Assert.NotNull(response);
        Assert.Null(response.Types);
    }

    [Fact]
    public void Deserialize_WhenTheTypesCollectionContainsNullElements_ShouldKeepThem()
    {
        // Arrange
        const string JSON = """{"types":["Front",null,"Booklet"]}""";

        // Act
        CoverArtArchiveImageResponse? response = JsonSerializer.Deserialize<CoverArtArchiveImageResponse>(JSON);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Types);
        Assert.Equal(["Front", null, "Booklet"], response.Types);
    }

    [Fact]
    public void Deserialize_WhenTheApiReturnsANullImage_ShouldDeserializeAsNull()
    {
        // Arrange
        const string JSON = """{"image":null,"types":["Front"]}""";

        // Act
        CoverArtArchiveImageResponse? response = JsonSerializer.Deserialize<CoverArtArchiveImageResponse>(JSON);

        // Assert
        Assert.NotNull(response);
        Assert.Null(response.Image);
    }

    [Fact]
    public void Deserialize_WhenThePropertiesAreMissing_ShouldDefaultToNullAndFalse()
    {
        // Arrange
        const string JSON = "{}";

        // Act
        CoverArtArchiveImageResponse? response = JsonSerializer.Deserialize<CoverArtArchiveImageResponse>(JSON);

        // Assert
        Assert.NotNull(response);
        Assert.Null(response.Image);
        Assert.Null(response.Thumbnail);
        Assert.Null(response.Types);
        Assert.False(response.IsFront);
        Assert.False(response.IsBack);
        Assert.Null(response.Comment);
    }
}
