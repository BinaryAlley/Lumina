#region ========================================================================= USING =====================================================================================
using Lumina.Plugins.MusicBrainz.Common.Models.Contracts.Responses;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
#endregion

namespace Lumina.Plugins.MusicBrainz.UnitTests.Common.Models.Contracts.Responses;

/// <summary>
/// Contains unit tests for the <see cref="MusicBrainzLabelResponse"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MusicBrainzLabelResponseTests
{
    [Fact]
    public void NameDisambiguationAndLabelCode_WhenReturnedByTheMusicBrainzApi_ShouldDeserialize()
    {
        // Arrange
        const string JSON = """{"id":"f0e0e0e0-0000-0000-0000-000000000000","name":"EMI","disambiguation":"EMI Records Ltd.","label-code":1234}""";

        // Act
        MusicBrainzLabelResponse? response = JsonSerializer.Deserialize<MusicBrainzLabelResponse>(JSON);

        // Assert
        Assert.NotNull(response);
        Assert.Equal("EMI", response.Name);
        Assert.Equal("EMI Records Ltd.", response.Disambiguation);
        Assert.Equal(1234, response.LabelCode);
    }

    [Fact]
    public void Deserialize_WhenEveryFieldIsPresent_ShouldDeserializeEveryProperty()
    {
        // Arrange
        const string JSON = """
        {
            "id": "label-id",
            "name": "EMI",
            "disambiguation": "EMI Records Ltd.",
            "label-code": 1234
        }
        """;

        // Act
        MusicBrainzLabelResponse? response = JsonSerializer.Deserialize<MusicBrainzLabelResponse>(JSON);

        // Assert
        Assert.NotNull(response);
        Assert.Equal("label-id", response.Id);
        Assert.Equal("EMI", response.Name);
        Assert.Equal("EMI Records Ltd.", response.Disambiguation);
        Assert.Equal(1234, response.LabelCode);
    }
}
