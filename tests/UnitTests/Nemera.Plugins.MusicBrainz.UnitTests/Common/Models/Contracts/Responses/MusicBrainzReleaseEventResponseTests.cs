#region ========================================================================= USING =====================================================================================
using Lumina.Plugins.MusicBrainz.Common.Models.Contracts.Responses;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
#endregion

namespace Lumina.Plugins.MusicBrainz.UnitTests.Common.Models.Contracts.Responses;

/// <summary>
/// Contains unit tests for the <see cref="MusicBrainzReleaseEventResponse"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MusicBrainzReleaseEventResponseTests
{
    [Fact]
    public void DateAndArea_WhenReturnedByTheMusicBrainzApi_ShouldDeserialize()
    {
        // Arrange
        const string JSON = """{"date":"1975-11-21","area":{"id":"c0e0e0e0-0000-0000-0000-000000000000","name":"United Kingdom","iso-3166-1-codes":["GB"]}}""";

        // Act
        MusicBrainzReleaseEventResponse? response = JsonSerializer.Deserialize<MusicBrainzReleaseEventResponse>(JSON);

        // Assert
        Assert.NotNull(response);
        Assert.Equal("1975-11-21", response.Date);
        Assert.NotNull(response.Area);
        Assert.Equal("United Kingdom", response.Area.Name);
        Assert.Equal("GB", Assert.Single(response.Area.Iso3166Part1Codes));
    }

    [Fact]
    public void Deserialize_WhenEveryFieldIsPresent_ShouldDeserializeEveryProperty()
    {
        // Arrange
        const string JSON = """
        {
            "date": "1975-11-21",
            "area": { "id": "area-id", "name": "United Kingdom" }
        }
        """;

        // Act
        MusicBrainzReleaseEventResponse? response = JsonSerializer.Deserialize<MusicBrainzReleaseEventResponse>(JSON);

        // Assert
        Assert.NotNull(response);
        Assert.Equal("1975-11-21", response.Date);
        Assert.NotNull(response.Area);
        Assert.Equal("area-id", response.Area.Id);
    }
}
