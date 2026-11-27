#region ========================================================================= USING =====================================================================================
using Lumina.Plugins.MusicBrainz.Common.Models.Contracts.Responses;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
#endregion

namespace Lumina.Plugins.MusicBrainz.UnitTests.Common.Models.Contracts.Responses;

/// <summary>
/// Contains unit tests for the <see cref="MusicBrainzTagResponse"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MusicBrainzTagResponseTests
{
    [Fact]
    public void NameCountAndDisambiguation_WhenReturnedByTheMusicBrainzApi_ShouldDeserialize()
    {
        // Arrange
        const string JSON = """{"name":"rock","disambiguation":"genre","count":5}""";

        // Act
        MusicBrainzTagResponse? response = JsonSerializer.Deserialize<MusicBrainzTagResponse>(JSON);

        // Assert
        Assert.NotNull(response);
        Assert.Equal("rock", response.Name);
        Assert.Equal("genre", response.Disambiguation);
        Assert.Equal(5, response.Count);
    }

    [Fact]
    public void Deserialize_WhenEveryFieldIsPresent_ShouldDeserializeEveryProperty()
    {
        // Arrange
        const string JSON = """
        {
            "id": "genre-id",
            "name": "rock",
            "disambiguation": "genre",
            "count": 5
        }
        """;

        // Act
        MusicBrainzTagResponse? response = JsonSerializer.Deserialize<MusicBrainzTagResponse>(JSON);

        // Assert
        Assert.NotNull(response);
        Assert.Equal("genre-id", response.Id);
        Assert.Equal("rock", response.Name);
        Assert.Equal("genre", response.Disambiguation);
        Assert.Equal(5, response.Count);
    }
}
