#region ========================================================================= USING =====================================================================================
using Lumina.Plugins.MusicBrainz.Common.Models.Contracts.Responses;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
#endregion

namespace Lumina.Plugins.MusicBrainz.UnitTests.Common.Models.Contracts.Responses;

/// <summary>
/// Contains unit tests for the <see cref="MusicBrainzUrlResponse"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MusicBrainzUrlResponseTests
{
    [Fact]
    public void Resource_WhenReturnedByTheMusicBrainzApi_ShouldDeserialize()
    {
        // Arrange
        const string JSON = """{"id":"e0e0e0e0-0000-0000-0000-000000000000","resource":"https://www.queenonline.com/"}""";

        // Act
        MusicBrainzUrlResponse? response = JsonSerializer.Deserialize<MusicBrainzUrlResponse>(JSON);

        // Assert
        Assert.NotNull(response);
        Assert.Equal("e0e0e0e0-0000-0000-0000-000000000000", response.Id);
        Assert.Equal("https://www.queenonline.com/", response.Resource);
    }

    [Fact]
    public void Deserialize_WhenEveryFieldIsPresent_ShouldDeserializeEveryProperty()
    {
        // Arrange
        const string JSON = """{"id":"url-id","resource":"https://www.queenonline.com/"}""";

        // Act
        MusicBrainzUrlResponse? response = JsonSerializer.Deserialize<MusicBrainzUrlResponse>(JSON);

        // Assert
        Assert.NotNull(response);
        Assert.Equal("url-id", response.Id);
        Assert.Equal("https://www.queenonline.com/", response.Resource);
    }
}
