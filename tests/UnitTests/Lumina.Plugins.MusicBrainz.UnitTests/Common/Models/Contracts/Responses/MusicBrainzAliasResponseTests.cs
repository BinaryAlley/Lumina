#region ========================================================================= USING =====================================================================================
using Lumina.Plugins.MusicBrainz.Common.Models.Contracts.Responses;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
#endregion

namespace Lumina.Plugins.MusicBrainz.UnitTests.Common.Models.Contracts.Responses;

/// <summary>
/// Contains unit tests for the <see cref="MusicBrainzAliasResponse"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MusicBrainzAliasResponseTests
{
    [Fact]
    public void IsEnded_WhenTheMusicBrainzApiReturnsNull_ShouldDeserializeAsNull()
    {
        // Arrange
        // the MusicBrainz search API can return a null ended flag for aliases, which used to break deserialization into a non-nullable boolean
        const string JSON = """{"name":"Rammstein","ended":null}""";

        // Act
        MusicBrainzAliasResponse? response = JsonSerializer.Deserialize<MusicBrainzAliasResponse>(JSON);

        // Assert
        Assert.NotNull(response);
        Assert.Null(response.IsEnded);
    }

    [Fact]
    public void IsEnded_WhenTheMusicBrainzApiReturnsTrue_ShouldDeserializeAsTrue()
    {
        // Arrange
        const string JSON = """{"name":"Rammstein","ended":true}""";

        // Act
        MusicBrainzAliasResponse? response = JsonSerializer.Deserialize<MusicBrainzAliasResponse>(JSON);

        // Assert
        Assert.NotNull(response);
        Assert.True(response.IsEnded);
    }

    [Fact]
    public void Deserialize_WhenEveryFieldIsPresent_ShouldDeserializeEveryProperty()
    {
        // Arrange
        const string JSON = """
        {
            "name": "Alias Name",
            "sort-name": "Alias Sort Name",
            "type": "Artist name",
            "locale": "en",
            "primary": true,
            "begin": "1994-01-01",
            "end": "2020-12-31",
            "ended": true
        }
        """;

        // Act
        MusicBrainzAliasResponse? response = JsonSerializer.Deserialize<MusicBrainzAliasResponse>(JSON);

        // Assert
        Assert.NotNull(response);
        Assert.Equal("Alias Name", response.Name);
        Assert.Equal("Alias Sort Name", response.SortName);
        Assert.Equal("Artist name", response.Type);
        Assert.Equal("en", response.Locale);
        Assert.True(response.IsPrimary);
        Assert.Equal("1994-01-01", response.Begin);
        Assert.Equal("2020-12-31", response.End);
        Assert.True(response.IsEnded);
    }
}
