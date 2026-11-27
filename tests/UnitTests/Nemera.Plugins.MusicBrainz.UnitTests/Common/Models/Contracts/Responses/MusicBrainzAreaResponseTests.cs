#region ========================================================================= USING =====================================================================================
using Lumina.Plugins.MusicBrainz.Common.Models.Contracts.Responses;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
#endregion

namespace Lumina.Plugins.MusicBrainz.UnitTests.Common.Models.Contracts.Responses;

/// <summary>
/// Contains unit tests for the <see cref="MusicBrainzAreaResponse"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MusicBrainzAreaResponseTests
{
    [Fact]
    public void SortNameTypeAndIsoCodes_WhenReturnedByTheMusicBrainzApi_ShouldDeserialize()
    {
        // Arrange
        const string JSON = """
        {
            "id": "c0e0e0e0-0000-0000-0000-000000000000",
            "name": "United Kingdom",
            "sort-name": "United Kingdom",
            "type": "Country",
            "iso-3166-1-codes": [ "GB" ],
            "iso-3166-2-codes": [ "GB-ENG" ]
        }
        """;

        // Act
        MusicBrainzAreaResponse? response = JsonSerializer.Deserialize<MusicBrainzAreaResponse>(JSON);

        // Assert
        Assert.NotNull(response);
        Assert.Equal("United Kingdom", response.SortName);
        Assert.Equal("Country", response.Type);
        Assert.Equal("GB", Assert.Single(response.Iso3166Part1Codes));
        Assert.Equal("GB-ENG", Assert.Single(response.Iso3166Part2Codes));
    }

    [Fact]
    public void Deserialize_WhenEveryFieldIsPresent_ShouldDeserializeEveryProperty()
    {
        // Arrange
        const string JSON = """
        {
            "id": "area-id",
            "name": "United Kingdom",
            "sort-name": "United Kingdom",
            "disambiguation": "The United Kingdom of Great Britain",
            "type": "Country",
            "iso-3166-1-codes": [ "GB" ],
            "iso-3166-2-codes": [ "GB-ENG" ]
        }
        """;

        // Act
        MusicBrainzAreaResponse? response = JsonSerializer.Deserialize<MusicBrainzAreaResponse>(JSON);

        // Assert
        Assert.NotNull(response);
        Assert.Equal("area-id", response.Id);
        Assert.Equal("United Kingdom", response.Name);
        Assert.Equal("United Kingdom", response.SortName);
        Assert.Equal("The United Kingdom of Great Britain", response.Disambiguation);
        Assert.Equal("Country", response.Type);
        Assert.Equal("GB", Assert.Single(response.Iso3166Part1Codes));
        Assert.Equal("GB-ENG", Assert.Single(response.Iso3166Part2Codes));
    }
}
