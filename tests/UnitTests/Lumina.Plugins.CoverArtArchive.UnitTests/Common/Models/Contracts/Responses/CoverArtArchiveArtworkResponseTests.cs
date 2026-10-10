#region ========================================================================= USING =====================================================================================
using Lumina.Plugins.CoverArtArchive.Common.Models.Contracts.Responses;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
#endregion

namespace Lumina.Plugins.CoverArtArchive.UnitTests.Common.Models.Contracts.Responses;

/// <summary>
/// Contains unit tests for the <see cref="CoverArtArchiveArtworkResponse"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class CoverArtArchiveArtworkResponseTests
{
    [Fact]
    public void Deserialize_WhenTheApiReturnsImages_ShouldDeserializeEveryImage()
    {
        // Arrange
        const string JSON = """
        {
            "images": [
                { "image": "https://coverartarchive.org/release/11111111-1111-1111-1111-111111111111/1.jpg", "front": true, "types": [ "Front" ] },
                { "image": "https://coverartarchive.org/release/11111111-1111-1111-1111-111111111111/2.jpg", "back": true, "types": [ "Back" ] }
            ]
        }
        """;

        // Act
        CoverArtArchiveArtworkResponse? response = JsonSerializer.Deserialize<CoverArtArchiveArtworkResponse>(JSON);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Images);
        Assert.Equal(2, response.Images.Count);
        Assert.Equal("https://coverartarchive.org/release/11111111-1111-1111-1111-111111111111/1.jpg", response.Images[0]!.Image);
        Assert.Equal(["Front"], response.Images[0]!.Types);
        Assert.Equal("https://coverartarchive.org/release/11111111-1111-1111-1111-111111111111/2.jpg", response.Images[1]!.Image);
        Assert.Equal(["Back"], response.Images[1]!.Types);
    }

    [Fact]
    public void Deserialize_WhenTheApiReturnsANullImagesCollection_ShouldDeserializeAsNull()
    {
        // Arrange
        // The API can answer with a null images collection, which must not be turned into anything else by the deserializer.
        const string JSON = """{"images":null}""";

        // Act
        CoverArtArchiveArtworkResponse? response = JsonSerializer.Deserialize<CoverArtArchiveArtworkResponse>(JSON);

        // Assert
        Assert.NotNull(response);
        Assert.Null(response.Images);
    }

    [Fact]
    public void Deserialize_WhenTheImagesCollectionContainsNullElements_ShouldKeepThem()
    {
        // Arrange
        const string JSON = """{"images":[null,{"image":"https://coverartarchive.org/release/11111111-1111-1111-1111-111111111111/1.jpg"}]}""";

        // Act
        CoverArtArchiveArtworkResponse? response = JsonSerializer.Deserialize<CoverArtArchiveArtworkResponse>(JSON);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Images);
        Assert.Equal(2, response.Images.Count);
        Assert.Null(response.Images[0]);
        Assert.NotNull(response.Images[1]);
    }

    [Fact]
    public void Deserialize_WhenTheImagesCollectionIsEmpty_ShouldDeserializeAsEmpty()
    {
        // Arrange
        const string JSON = """{"images":[]}""";

        // Act
        CoverArtArchiveArtworkResponse? response = JsonSerializer.Deserialize<CoverArtArchiveArtworkResponse>(JSON);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Images);
        Assert.Empty(response.Images);
    }

    [Fact]
    public void Deserialize_WhenThePropertyIsMissing_ShouldDefaultToNull()
    {
        // Arrange
        const string JSON = "{}";

        // Act
        CoverArtArchiveArtworkResponse? response = JsonSerializer.Deserialize<CoverArtArchiveArtworkResponse>(JSON);

        // Assert
        Assert.NotNull(response);
        Assert.Null(response.Images);
    }
}
